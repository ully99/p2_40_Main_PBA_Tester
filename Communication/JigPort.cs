using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using p2_40_Main_PBA_Tester.Data; // Settings, LogManager 등이 있다고 가정

namespace p2_40_Main_PBA_Tester.Communication
{
    public class JigPort
    {
        // 원본 TcpProtocol의 헤더 정의를 그대로 가져옴
        private readonly byte HEADER1 = 0x49; // 'I'
        private readonly byte HEADER2 = 0x54; // 'T'
        private readonly byte HEADER3 = 0x4D; // 'M'

        // 최소 패킷 길이: HDR(3) + LEN(2) + CMD(1) + ITEM(1) + CRC(1) = 8 bytes
        private const int MIN_PACKET_LENGTH = 8;

        private const int reconnect_delay = 100;

        public SerialPort Port { get; private set; }
        public bool IsOpen => Port != null && Port.IsOpen;

        // UI Logging 처리를 위한 대리자 및 타겟 (SerialChannelPort 형식 유지)
        public RichTextBox LogTarget { get; set; }
        // string: 데이터(Hex), bool: true=TX / false=RX
        public Action<RichTextBox, string, bool> LogCommToUI { get; set; }

        private readonly List<byte> _receiveBuffer = new List<byte>();
        private readonly object _bufferLock = new object();

        // 비동기 수신 대기를 위한 TaskCompletionSource
        private TaskCompletionSource<byte[]> _packetTcs;

        // 요청 간 최소 인터벌 보장 (SerialChannelPort 형식 유지)
        private DateTime _lastTxUtc = DateTime.MinValue;
        public int MinIntervalMs => Settings.Instance?.Pba_Min_Interval ?? 10;

        public event Action<byte[]> OnPacketReceived;

        #region Connect / Disconnect (작성하신 부분 유지 및 보완)
        public bool Connect(string portName, int baudRate = 115200)
        {
            try
            {
                if (Port != null)
                {
                    CleanupPort();
                }

                if (string.IsNullOrEmpty(portName) || portName.ToUpper() == "NONE")
                    return false;

                // JIG 사양에 맞게 설정 (제공된 소스 기준)
                Port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

                // 바이너리 데이터를 주고받으므로 Encoding 설정은 크게 의미 없으나 기본값 유지
                Port.Encoding = Encoding.Default;
                Port.ReadTimeout = 1000;
                Port.WriteTimeout = 1000;

                Port.DataReceived += OnDataReceived;
                Port.Open();

                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();

                Console.WriteLine($"[JIG] 연결 성공: {portName}, {baudRate}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JIG] 연결 실패 : {ex.Message}");
                CleanupPort();
                return false;
            }
        }

        public async Task<bool> ConnectAsync(string portName, int baudRate = 115200, int timeoutMs = 3000, CancellationToken token = default)
        {
            if (IsOpen && Port.PortName == portName && Port.BaudRate == baudRate)
            {
                Console.WriteLine($"[JIG] 이미 동일한 포트로 연결되어 있습니다. ({portName})");
                return true;
            }

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (token != default) token.ThrowIfCancellationRequested();

                try
                {
                    CleanupPort();

                    Port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                    Port.ReadTimeout = 1000;
                    Port.WriteTimeout = 1000;
                    Port.DataReceived += OnDataReceived;

                    // UI 블로킹 방지를 위한 Task.Run
                    bool openSuccess = await Task.Run(() =>
                    {
                        try
                        {
                            Port.Open();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[JIG] Open 시도 중 에러: {ex.Message}");
                            return false;
                        }
                    }, token);

                    if (!openSuccess) throw new Exception("Port Open Failed");

                    Port.DiscardInBuffer();
                    Port.DiscardOutBuffer();

                    Console.WriteLine($"[JIG] 새 연결 성공 ({portName})");
                    return true;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JIG] 연결 시도 중... ({ex.Message})");
                    await Task.Delay(reconnect_delay, token);
                }
            }

            Console.WriteLine($"[JIG] 연결 최종 실패 (타임아웃 {timeoutMs}ms)");
            return false;
        }

        public void Disconnect()
        {
            if (Port != null)
            {
                if (Port.IsOpen)
                {
                    Port.DataReceived -= OnDataReceived; // 이벤트 해제 중요!
                    try { Port.Close(); } catch { }
                }
                Port.Dispose();
                Port = null;
            }
            lock (_bufferLock) { _receiveBuffer.Clear(); }
        }

        private void CleanupPort()
        {
            try
            {
                if (Port != null)
                {
                    Port.DataReceived -= OnDataReceived;
                    if (Port.IsOpen)
                    {
                        Port.DiscardInBuffer();
                        Port.DiscardOutBuffer();
                        Port.Close();
                    }
                    Port.Dispose();
                    Port = null;
                }
            }
            catch { }
            finally
            {
                lock (_bufferLock)
                {
                    _receiveBuffer.Clear();
                }
                // 대기 중인 Task가 있다면 취소 처리
                _packetTcs?.TrySetCanceled();
            }
        }
        #endregion

        #region Send / Receive (Core Logic - SerialChannelPort 형식 유지)

        /// <summary>
        /// 데이터를 송신합니다. 최소 인터벌을 보장합니다.
        /// </summary>
        public async Task<bool> SendAsync(byte[] data, int timeoutMs = 2000)
        {
            if (!IsOpen) return false;

            int remain = MinIntervalMs - (int)(DateTime.UtcNow - _lastTxUtc).TotalMilliseconds;
            if (remain > 0) await Task.Delay(remain).ConfigureAwait(false);

            using (var cts = new CancellationTokenSource(timeoutMs))
            {
                try
                {
                    await Port.BaseStream.WriteAsync(data, 0, data.Length, cts.Token).ConfigureAwait(false);
                    _lastTxUtc = DateTime.UtcNow;

                    string hexStr = ToHex(data);
                    if (LogCommToUI != null) LogCommToUI(LogTarget, hexStr, true);
                    Console.WriteLine($"TX => {hexStr} [JIG]");



                    return true;
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// 명령을 전송하고 응답 패킷을 기다립니다. (Retry 로직 포함)
        /// </summary>
        /// <param name="cmd">Command Byte</param>
        /// <param name="item">Item Byte</param>
        /// <param name="data">Data Payload</param>
        /// <param name="timeoutMs">타임아웃</param>
        /// <returns>완전한 응답 패킷 byte[], 실패 시 null</returns>
        public async Task<byte[]> SendAndReceivePacketAsync(byte[] txData, int timeoutMs, CancellationToken token = default)
        {
            int maxAttempts = 1;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (token != default) token.ThrowIfCancellationRequested();

                DiscardBuffers();
                _packetTcs = new TaskCompletionSource<byte[]>();

                if (await SendAsync(txData).ConfigureAwait(false))
                {
                    Task timeoutTask = Task.Delay(timeoutMs);
                    Task completed = await Task.WhenAny(_packetTcs.Task, timeoutTask).ConfigureAwait(false);

                    if (completed != timeoutTask)
                    {
                        if (_packetTcs.Task.IsCompleted && !_packetTcs.Task.IsFaulted)
                            return await _packetTcs.Task.ConfigureAwait(false);
                    }
                    else
                    {
                        _packetTcs.TrySetCanceled();
                    }
                }

                if (attempt < maxAttempts)
                {
                    Console.WriteLine($"[Retry] JIG ({attempt}/{maxAttempts})");
                    await Task.Delay(200, token).ConfigureAwait(false);
                }
            }
            return null;
        }

        /// <summary>
        /// 명령 전송 후 응답 패킷에서 DATA 영역만 추출하여 반환합니다.
        /// </summary>
        
        #endregion

        #region SerialPort Event & Parsing (ITM 프로토콜 맞게 수정)

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Port == null || !Port.IsOpen) return;

            try
            {
                int bytesToRead = Port.BytesToRead;
                if (bytesToRead <= 0) return;

                byte[] buffer = new byte[bytesToRead];
                int readCount = Port.Read(buffer, 0, bytesToRead);

                if (readCount > 0)
                {
                    lock (_bufferLock)
                    {
                        // 수신된 데이터를 내부 버퍼에 추가
                        for (int i = 0; i < readCount; i++) _receiveBuffer.Add(buffer[i]);
                    }
                    // 패킷 추출 시도
                    TryExtractPacket();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JIG] OnDataReceived 예외: {ex.Message}");
            }
        }

        /// <summary>
        /// 내부 버퍼에서 "ITM" 기반의 완전한 패킷을 추출합니다.
        /// </summary>
        private void TryExtractPacket()
        {
            if (_packetTcs == null || _packetTcs.Task.IsCompleted) return;

            while (true)
            {
                lock (_bufferLock)
                {
                    if (_receiveBuffer.Count < MIN_PACKET_LENGTH) return; // 최소 길이 미달

                    // 1. 헤더 "ITM" 찾기
                    int headerIdx = -1;
                    for (int i = 0; i <= _receiveBuffer.Count - 3; i++)
                    {
                        if (_receiveBuffer[i] == HEADER1 && _receiveBuffer[i + 1] == HEADER2 && _receiveBuffer[i + 2] == HEADER3)
                        {
                            headerIdx = i;
                            break;
                        }
                    }

                    // 헤더를 못 찾음
                    if (headerIdx == -1)
                    {
                        // 버퍼 뒷부분에 "I"나 "IT"가 걸려있을 수 있으므로 마지막 2바이트 남기고 삭제
                        int removeLen = Math.Max(0, _receiveBuffer.Count - 2);
                        if (removeLen > 0) _receiveBuffer.RemoveRange(0, removeLen);
                        return;
                    }

                    // 헤더 앞의 쓰레기 데이터 삭제
                    if (headerIdx > 0)
                    {
                        _receiveBuffer.RemoveRange(0, headerIdx);
                    }

                    // 다시 길이 체크 (헤더 "ITM" 3바이트 + 길이 2바이트는 최소한 있어야 함)
                    if (_receiveBuffer.Count < 5) return;

                    // 2. 길이 필드 확인 (Big Endian)
                    // LEN = CMD(1) + ITEM(1) + DATA.Length
                    ushort lenField = (ushort)((_receiveBuffer[3] << 8) | _receiveBuffer[4]);

                    // 전체 패킷 예상 길이 = HDR(3) + LEN(2) + lenField + CRC(1)
                    int totalPacketLen = 3 + 2 + lenField + 1;

                    // 버퍼에 전체 패킷이 아직 다 안 들어옴
                    if (_receiveBuffer.Count < totalPacketLen) return;

                    // 3. 패킷 데이터 복사
                    byte[] packet = new byte[totalPacketLen];
                    _receiveBuffer.CopyTo(0, packet, 0, totalPacketLen);

                    // 사용한 만큼 버퍼에서 제거
                    _receiveBuffer.RemoveRange(0, totalPacketLen);

                    // 4. CRC 검증
                    if (!VerifyCRC(packet))
                    {
                        Console.WriteLine("[JIG] 수신 패킷 CRC Error! Discarding.");
                        // 다음 패킷 탐색을 위해 루프 계속
                        continue;
                    }

                    // 5. 최종 패킷 반환
                    if (LogCommToUI != null) LogCommToUI(LogTarget, SafeAscii(packet), false);
                    Console.WriteLine($"RX => {SafeAscii(packet).Replace("\r", "").Replace("\n", "")}");

                    _packetTcs.TrySetResult(packet);
                    
                    OnPacketReceived?.Invoke(packet);
                    return; // 패킷 하나 찾았으므로 루프 종료 (다음 요청 시 다시 동작)
                }
            }
        }

        private bool VerifyCRC(byte[] packet)
        {
            if (packet == null || packet.Length < MIN_PACKET_LENGTH) return false;

            int len = packet.Length;
            byte rxCrc = packet[len - 1]; // 마지막 바이트

            // 계산 대상: CMD, ITEM, DATA 영역
            // 시작 인덱스 5 (HDR 3 + LEN 2), 길이 = len - HDR(3) - LEN(2) - CRC(1)
            int calcLen = len - 6;
            byte[] calcData = new byte[calcLen];
            Array.Copy(packet, 5, calcData, 0, calcLen);

            // 원본 TcpProtocol의 CalculateCRC 로직 그대로 사용
            int sum = calcData.Sum(d => (int)d);
            byte calcCrc = (byte)(sum ^ 0x55); // XOR 0x55

            return rxCrc == calcCrc;
        }
        #endregion

        #region Helpers
        private static string SafeAscii(byte[] buf)
        {
            try { return Encoding.ASCII.GetString(buf); }
            catch { return BitConverter.ToString(buf); }
        }
        private string ToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", " ");
        }
        public void DiscardBuffers()
        {
            try
            {
                if (IsOpen)
                {
                    Port.DiscardInBuffer();
                    Port.DiscardOutBuffer();
                }
            }
            catch { }
            lock (_bufferLock)
            {
                _receiveBuffer.Clear();
            }
        }

        
        #endregion

        
        
    }
}