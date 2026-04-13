using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using p2_40_Main_PBA_Tester.Communication;
using p2_40_Main_PBA_Tester.LIB;
using p2_40_Main_PBA_Tester.Forms;
using p2_40_Main_PBA_Tester.UserControls;
using System.Reflection;
using System.Windows.Forms;


namespace p2_40_Main_PBA_Tester.Data
{
    internal static class TaskFunctions
    {
        

        // 메인에서 이 함수를 호출하면, 이름에 맞는 테스트를 찾아서 실행함
        public static async Task<bool> RunTestItem(string taskName, int channelIndex, ChControl control, CancellationToken token,
                                            bool totalResult, MesData mesData)
        {
            bool result = false;

            control.Logger.Section(taskName);
            // 실제로는 여기서 장비 통신 등을 수행 (Task.Delay는 시뮬레이션용)
            // 나중에 CommManager 등을 인자로 받아서 실제 하드웨어 제어 코드를 넣으면 됨

            switch (taskName)
            {
                case "QR READ":
                    result = await Test_QrRead(channelIndex, control, token, mesData);
                    break;
                case "MCU INFO":
                    result = await Test_McuInfo(channelIndex, control, token, mesData);
                    break;
                case "OVP":
                    result = await Test_OVP(channelIndex, control, token, mesData);
                    break;
                case "LDO":
                    result = await Test_LDO(channelIndex, control, token, mesData);
                    break;
                case "CURRENT_SLEEP_SHIP":
                    result = await Test_Current_Sleep_Ship(channelIndex, control, token, mesData);
                    break;
                case "CHARGE":
                    result = await Test_CHARGE(channelIndex, control, token, mesData);
                    break;
                case "USB CHECK":
                    result = await Test_USB_CHECK(channelIndex, control, token, mesData);
                    break;

                //통신검사
                case "PBA CMD CHECK START":
                    result = await Test_Pba_Cmd_Check_Start(channelIndex, control, token, mesData);
                    break;

                case "FLAG INIT":
                    result = await Test_Flag_Init(channelIndex, control, token, mesData);
                    break;

                case "MOTOR":
                    result = await Test_MOTOR(channelIndex, control, token, mesData);
                    break;

                case "FLOODS":
                    result = await Test_FLOODS(channelIndex, control, token, mesData);
                    break;

                case "HEATER":
                    result = await Test_HEATER(channelIndex, control, token, mesData);
                    break;

                case "CARTRIDGE":
                    result = await Test_CARTRIDGE(channelIndex, control, token, mesData);
                    break;

                case "SUB HEATER":
                    result = await Test_Sub_Heater(channelIndex, control, token, mesData);
                    break;

                case "GPAK":
                    result = await Test_GPAK(channelIndex, control, token, mesData);
                    break;
                case "ACCELEROMETER":
                    result = await Test_ACCELEROMETER(channelIndex, control, token, mesData);
                    break;
                case "FLASH MEMORY":
                    result = await Test_Flash_Memory(channelIndex, control, token, mesData);
                    break;
                case "PBA FLAG":
                    result = await Test_PBA_FLAG(channelIndex, control, token, totalResult, mesData);
                    break;

                case "PBA TEST END":
                    result = await Test_PBA_TEST_END(channelIndex, control, token, mesData);
                    break;

                case "TEST1":
                    result = await Test_HEATER_debug(channelIndex, control, token);
                    break;
                case "TEST2":
                    result = await Test_HEATER_debug(channelIndex, control, token);
                    break;
                case "TEST3":
                    result = await Test_HEATER_debug(channelIndex, control, token);
                    break;


                default:
                    // 정의되지 않은 항목은 일단 PASS 처리하거나 로그 남김
                    control.Logger.Info($"This task is not implemented : {taskName}");
                    result = false;
                    break;
            }

            // 2. 결과 출력
            control.Logger.ResultSection(taskName, result);

            return result;
        }

        








        // --- 각 개별 테스트 함수들  ---

        private static async Task<bool> Test_QrRead(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            var Qr = CommManager.QrPorts[ch];
            bool isPass = false;
            
            try
            {
                if (!Qr.IsOpen)
                {
                    Console.WriteLine($"QR PORT가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("QR PORT is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.QR_READ_Step_Delay, token);

                string rx = null;
                const int retryMax = 3;
                for (int retry = 1; retry <= retryMax; retry++)
                {
                    // 1. 중단 체크 (예외 발생 시 catch (OperationCanceledException)로 이동)                 
                    token.ThrowIfCancellationRequested();

                    Console.WriteLine($"CH{ch + 1} 스캐너 시작 명령 전송 [QR READ] (시도 {retry})");

                    rx = await Qr.SendAndReceiveAsync(Variable.QR_START, 5000, token);
                    if (rx != null)
                    {
                        Console.WriteLine($"CH{ch + 1} QR 수신 완료: {rx}");
                        break;
                    }

                    await Qr.SendAsync(Variable.QR_END);
                    await Task.Delay(200, token);
                }

                if (rx == null)
                {
                    control.Logger.Fail("QR Read Fail: null");
                    await Qr.SendAsync(Variable.QR_END);
                    return false;
                }

                bool isQrReadOk = rx.Length == Settings.Instance.QR_READ_Len;
                if (isQrReadOk) control.Logger.Pass($"QR READ : {rx} ===> Len : {rx.Length} [{Settings.Instance.QR_READ_Len}]");
                else control.Logger.Fail($"QR READ : {rx} ===> Len : {rx.Length} [{Settings.Instance.QR_READ_Len}]");

                mesData.PBA_QR_CODE = rx;

                await Qr.SendAsync(Variable.QR_END);
                isPass = true;
                return isQrReadOk;
            }
            catch (OperationCanceledException)
            {
                // 1. UI 상태를 STOP으로 변경 (ChControl의 메서드 사용)
                control.UpdateNowStatus(ChControl.NowStatus.STOP);

                // 2. 실패 로그를 남기지 않고 상위 호출부(MainForm)로 예외를 다시 던짐
                // 이렇게 해야 MainForm의 시퀀스 루프도 즉시 중단됩니다.
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] QR READ exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                // UI 로그에도 표시
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("QR READ", true);
                else
                    mesData.SetResultColumn("QR READ", false);  
            }
            

        }

        private static async Task<bool> Test_McuInfo(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }


                await Task.Delay(Settings.Instance.MCU_INFO_Step_Delay, token);

                // --- 1. MCU INFO START CMD (TCP 0xC1 0x01) ---
                byte[] start_cmd_tx = new TcpProtocol(0xC1, 0x01).GetPacket();
                int start_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.MCU_INFO_Tcp_01_Delay;
                Console.WriteLine($"MCU INFO START CMD RX 수신 대기 [Delay : {start_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] start_cmd_rx = await Board.SendAndReceivePacketAsync(start_cmd_tx, start_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(start_cmd_tx, start_cmd_rx))
                {
                    control.Logger.Fail($"START CMD RX read fail");
                    isPass = false;
                }

                await Task.Delay(Settings.Instance.MCU_INFO_Booting_01_Delay, token);

                // --- 2. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }

                // --- 3. MCU ID READ ---
                byte[] MCU_ID_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_MCU_ID).GetPacket();
                byte[] MCU_ID_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(MCU_ID_READ_CMD_tx, Settings.Instance.Pba_Read_Timeout, token);

                if (MCU_ID_READ_CMD_rx == null || MCU_ID_READ_CMD_rx.Length < 8)
                {
                    control.Logger.Fail("MCU ID: Rx is NULL or Invalid Length");
                    isPass = false;
                }
                else
                {
                    string McuId = $"{MCU_ID_READ_CMD_rx[7]:X2}{MCU_ID_READ_CMD_rx[6]:X2}" +
                                   $"{MCU_ID_READ_CMD_rx[5]:X2}{MCU_ID_READ_CMD_rx[4]:X2}" +
                                   $"{MCU_ID_READ_CMD_rx[3]:X2}{MCU_ID_READ_CMD_rx[2]:X2}" +
                                   $"{MCU_ID_READ_CMD_rx[1]:X2}{MCU_ID_READ_CMD_rx[0]:X2}";

                    mesData.MCU_ID = McuId;
                    if (McuId.Length != Settings.Instance.MCU_INFO_Mcu_Id_Len)
                    {
                        control.Logger.Fail($"MCU ID : {McuId} ===> Len : {McuId.Length} [{Settings.Instance.MCU_INFO_Mcu_Id_Len}]");
                        isPass = false;
                    }
                    else
                    {
                        control.Logger.Pass($"MCU ID : {McuId} ===> Len : {McuId.Length} [{Settings.Instance.MCU_INFO_Mcu_Id_Len}]");
                    }

                    // INTERLOCK (MCU ID 수신 성공 시에만 수행)
                    if (Settings.Instance.USE_INTERLOCK)
                    {
                        var (interlockOk, interlockResult) = await MesData.CheckInterlockAsync(McuId, token);
                        if (interlockOk) control.Logger.Pass($"INTERLOCK PASS : {McuId}");
                        else
                        {
                            control.Logger.Fail($"INTERLOCK {interlockResult} : {McuId}");
                            ShowInterlockFailPopup(control, ch);
                            isPass = false; // 인터LOCK 실패 시에도 로직은 계속 진행
                        }
                    }
                }

                // --- 4. FW VER READ ---
                byte[] Fw_Ver_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FW_VER).GetPacket();
                byte[] Fw_Ver_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(Fw_Ver_READ_CMD_tx, Settings.Instance.Pba_Read_Timeout, token);

                if (Fw_Ver_READ_CMD_rx == null || Fw_Ver_READ_CMD_rx.Length < 10)
                {
                    control.Logger.Fail("FW Ver: Rx is NULL or Invalid Length");
                    isPass = false;
                }
                else
                {
                    string major = $"{(char)Fw_Ver_READ_CMD_rx[0]}{(char)Fw_Ver_READ_CMD_rx[1]}";
                    string minor = $"{(char)Fw_Ver_READ_CMD_rx[2]}{(char)Fw_Ver_READ_CMD_rx[3]}";
                    string debugging = $"{(char)Fw_Ver_READ_CMD_rx[4]}{(char)Fw_Ver_READ_CMD_rx[5]}";
                    string FwVer = $"{int.Parse(major)}.{int.Parse(minor)}.{int.Parse(debugging)}";
                    ushort FwVer_LDC = (ushort)((Fw_Ver_READ_CMD_rx[8] << 8) | Fw_Ver_READ_CMD_rx[9]);

                    mesData.MAIN_INFO_FW_VER = FwVer;
                    mesData.MAIN_INFO_FW_VER_LDC = FwVer_LDC.ToString();

                    // Spec 검사 (Main FW)
                    string normalizedSpecVer = string.Join(".", Settings.Instance.MCU_INFO_Main_Fw_Ver.Split('.').Select(s => int.Parse(s).ToString()));
                    if (FwVer != normalizedSpecVer)
                    {
                        control.Logger.Fail($"FW VER : {FwVer} [{normalizedSpecVer}]");
                        isPass = false;
                    }
                    else control.Logger.Pass($"FW VER : {FwVer} [{normalizedSpecVer}]");

                    // Spec 검사 (LDC FW)
                    if (FwVer_LDC != int.Parse(Settings.Instance.MCU_INFO_LDC_Fw_Ver))
                    {
                        control.Logger.Fail($"LDC FW VER : {FwVer_LDC} [{Settings.Instance.MCU_INFO_LDC_Fw_Ver}]");
                        isPass = false;
                    }
                    else control.Logger.Pass($"LDC FW VER : {FwVer_LDC} [{Settings.Instance.MCU_INFO_LDC_Fw_Ver}]");
                }

                // --- 5. IMAGE FW VER READ ---
                byte[] IMAGE_FW_VER_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_IMAGE_FW_VER).GetPacket();
                byte[] IMAGE_FW_VER_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(IMAGE_FW_VER_READ_CMD_tx, Settings.Instance.Pba_Read_Timeout, token);

                if (IMAGE_FW_VER_READ_CMD_rx == null || IMAGE_FW_VER_READ_CMD_rx.Length < 4)
                {
                    control.Logger.Fail("IMAGE FW VER : Rx is NULL or Invalid Length");
                    isPass = false;
                }
                else
                {
                    string imgMajor = $"{(char)IMAGE_FW_VER_READ_CMD_rx[0]}{(char)IMAGE_FW_VER_READ_CMD_rx[1]}";
                    string imgMinor = $"{(char)IMAGE_FW_VER_READ_CMD_rx[2]}{(char)IMAGE_FW_VER_READ_CMD_rx[3]}";
                    string ImageVer = $"{int.Parse(imgMajor)}.{int.Parse(imgMinor)}";
                    mesData.MAIN_INFO_IMAGE_VER = ImageVer;

                    string normalizedImageSpecVer = string.Join(".", Settings.Instance.MCU_INFO_Image_Fw_Ver.Split('.').Select(s => int.Parse(s).ToString()));
                    if (ImageVer != normalizedImageSpecVer)
                    {
                        control.Logger.Fail($"IMAGE FW VER : {ImageVer} [{normalizedImageSpecVer}]");
                        isPass = false;
                    }
                    else control.Logger.Pass($"IMAGE FW VER : {ImageVer} [{normalizedImageSpecVer}]");
                }

                // --- 6. MCU INFO END CMD (TCP 0xC1 0x02) ---
                byte[] end_cmd_tx = new TcpProtocol(0xC1, 0x02).GetPacket();
                int end_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.MCU_INFO_Tcp_02_Delay;
                Console.WriteLine($"MCU INFO END CMD RX 수신 대기 [Delay : {end_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] end_cmd_rx = await Board.SendAndReceivePacketAsync(end_cmd_tx, end_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(end_cmd_tx, end_cmd_rx))
                {
                    control.Logger.Fail($"END CMD RX read fail");
                    isPass = false;
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] MCU INFO exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("MCU INFO", true);
                else
                    mesData.SetResultColumn("MCU INFO", false);
            }
        }

        private static async Task<bool> Test_OVP(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.OVP_Step_Delay, token);

                // --- 1. OVP CMD 전송 (0xC2 0x01) ---
                byte[] ovp_cmd_tx = new TcpProtocol(0xC2, 0x01).GetPacket();
                int ovp_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.OVP_TCP_01_Delay;
                Console.WriteLine($"OVP CMD RX 수신 대기 [Delay : {ovp_cmd_timeout}ms] [CH{ch + 1}]");

                byte[] ovp_cmd_rx = await Board.SendAndReceivePacketAsync(ovp_cmd_tx, ovp_cmd_timeout, token);

                // --- 2. 데이터 유효성 검사 및 파싱 ---
                if (!UtilityFunctions.CheckTcpRxData(ovp_cmd_tx, ovp_cmd_rx))
                {
                    control.Logger.Fail($"OVP CMD RX read fail");
                    isPass = false;
                }
                else
                {
                    // 데이터 수신 성공 시 float 변환 (Index 7부터 4바이트)
                    if (ovp_cmd_rx != null && ovp_cmd_rx.Length >= 15)
                    {

                        byte[] vbus_volt_byte = new byte[] { ovp_cmd_rx[7], ovp_cmd_rx[8], ovp_cmd_rx[9], ovp_cmd_rx[10] };
                        float vbus_volt = BitConverter.ToSingle(vbus_volt_byte, 0);

                        byte[] ovp_volt_byte = new byte[] { ovp_cmd_rx[11], ovp_cmd_rx[12], ovp_cmd_rx[13], ovp_cmd_rx[14] };
                        float ovp_volt = BitConverter.ToSingle(ovp_volt_byte, 0);

                        mesData.OVP_CHG_IN = ovp_volt.ToString();
                        mesData.VBUS_CHG_IN = vbus_volt.ToString(); 

                        // 전압 범위 판정

                        if (vbus_volt >= Settings.Instance.OVP_VBUS_Min && vbus_volt <= Settings.Instance.OVP_VBUS_Max)
                        {
                            control.Logger.Pass($"VBUS : {vbus_volt:F3}V [{Settings.Instance.OVP_VBUS_Min} ~ {Settings.Instance.OVP_VBUS_Max}]");
                        }
                        else
                        {
                            control.Logger.Fail($"VBUS : {vbus_volt:F3}V [{Settings.Instance.OVP_VBUS_Min} ~ {Settings.Instance.OVP_VBUS_Max}]");
                            isPass = false;
                        }

                        if (ovp_volt >= Settings.Instance.OVP_OVP_Min && ovp_volt <= Settings.Instance.OVP_OVP_Max)
                        {
                            control.Logger.Pass($"OVP : {ovp_volt:F3}V [{Settings.Instance.OVP_OVP_Min} ~ {Settings.Instance.OVP_OVP_Max}]");
                        }
                        else
                        {
                            control.Logger.Fail($"OVP : {ovp_volt:F3}V [{Settings.Instance.OVP_OVP_Min} ~ {Settings.Instance.OVP_OVP_Max}]");
                            isPass = false;
                        }

                        
                    }
                    else
                    {
                        control.Logger.Fail("OVP RX data length is insufficient");
                        isPass = false;
                    }
                }

                // 이후 과정이 있다면 여기에 추가 (현재는 바로 반환)
                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] OVP exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("OVP", true);
                else
                    mesData.SetResultColumn("OVP", false);
            }
        }

        private static async Task<bool> Test_LDO(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            
            
            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.LDO_Step_Delay, token);

                // --- 1. LDO START CMD (0xC3 0x01) ---
                byte[] Ldo_start_cmd_tx = new TcpProtocol(0xC3, 0x01).GetPacket();
                int Ldo_start_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.LDO_TCP_01_Delay;
                Console.WriteLine($"LDO START CMD RX 수신 대기 [Delay : {Ldo_start_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] Ldo_start_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_start_cmd_tx, Ldo_start_cmd_timeout, token);

                float dcboost_off = -1000;

                if (!UtilityFunctions.CheckTcpRxData(Ldo_start_cmd_tx, Ldo_start_cmd_rx))
                {
                    control.Logger.Fail($"LDO START CMD RX read fail");
                    isPass = false;
                }
                else if (Ldo_start_cmd_rx.Length >= 19) // 전압 데이터 확인을 위한 길이 체크
                {
                    byte[] vsys_volt_byte = new byte[] { Ldo_start_cmd_rx[7], Ldo_start_cmd_rx[8], Ldo_start_cmd_rx[9], Ldo_start_cmd_rx[10] };
                    byte[] vsys_3V3_volt_byte = new byte[] { Ldo_start_cmd_rx[11], Ldo_start_cmd_rx[12], Ldo_start_cmd_rx[13], Ldo_start_cmd_rx[14] };

                    float vsys_volt = BitConverter.ToSingle(vsys_volt_byte, 0);
                    float vsys_3V3_volt = BitConverter.ToSingle(vsys_3V3_volt_byte, 0);
                    dcboost_off = BitConverter.ToSingle(Ldo_start_cmd_rx, 15);

                    mesData.VSYS_VOLT = vsys_volt.ToString();
                    mesData.VSYS_3V3 = vsys_3V3_volt.ToString();
                    mesData.DC_BOOST_OFF = dcboost_off.ToString();

                    // VSYS 판정
                    if (vsys_volt >= Settings.Instance.LDO_VSYS_Min && vsys_volt <= Settings.Instance.LDO_VSYS_Max)
                        control.Logger.Pass($"VSYS : {vsys_volt:F3}V [{Settings.Instance.LDO_VSYS_Min}~{Settings.Instance.LDO_VSYS_Max}]");
                    else { control.Logger.Fail($"VSYS : {vsys_volt:F3}V [{Settings.Instance.LDO_VSYS_Min}~{Settings.Instance.LDO_VSYS_Max}]"); isPass = false; }

                    // VSYS_3V3 OFF 판정
                    if (vsys_3V3_volt >= Settings.Instance.LDO_VSYS_3V3_Min && vsys_3V3_volt <= Settings.Instance.LDO_VSYS_3V3_Max)
                        control.Logger.Pass($"VSYS_3V3 : {vsys_3V3_volt:F3}V [{Settings.Instance.LDO_VSYS_3V3_Min}~{Settings.Instance.LDO_VSYS_3V3_Max}]");
                    else { control.Logger.Fail($"VSYS_3V3 : {vsys_3V3_volt:F3}V [{Settings.Instance.LDO_VSYS_3V3_Min}~{Settings.Instance.LDO_VSYS_3V3_Max}]"); isPass = false; }
                }
                else
                {
                    control.Logger.Fail("Unexpected LDO START CMD return data => data length is insufficient");
                    isPass = false;
                }

                await Task.Delay(Settings.Instance.LDO_Booting_01_Delay, token);

                // --- 2. PBA 연결 및 LDO ALL ON ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }
                else
                {
                    byte[] LDO_ALL_ON_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LDO_ALL_ON).GetPacket();
                    byte[] LDO_ALL_ON_rx = await Pba.SendAndReceivePacketAsync(LDO_ALL_ON_tx, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(LDO_ALL_ON_tx, LDO_ALL_ON_rx))
                    {
                        control.Logger.Fail("LDO_ALL_ON RX read fail");
                        isPass = false;
                    }
                }



                // --- 3. LDO SECOND CMD (각 채널 전압 측정 0xC3 0x02) ---
                byte[] Ldo_second_cmd_tx = new TcpProtocol(0xC3, 0x02).GetPacket();
                int Ldo_second_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.LDO_TCP_02_Delay;
                byte[] Ldo_second_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_second_cmd_tx, Ldo_second_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(Ldo_second_cmd_tx, Ldo_second_cmd_rx))
                {
                    control.Logger.Fail($"LDO SECOND CMD RX read fail");
                    isPass = false;
                }
                else if (Ldo_second_cmd_rx.Length >= 23)
                {
                    float mcu_3V0 = BitConverter.ToSingle(Ldo_second_cmd_rx, 7);
                    float vdd_3V0 = BitConverter.ToSingle(Ldo_second_cmd_rx, 11);
                    float lcd_3V0 = BitConverter.ToSingle(Ldo_second_cmd_rx, 15);
                    float dcboost = BitConverter.ToSingle(Ldo_second_cmd_rx, 19);

                    //mesData.VSYS_3V3 = sys_3V3.ToString();
                    mesData.MCU_3V0 = mcu_3V0.ToString();
                    mesData.VDD_3V0 = vdd_3V0.ToString();
                    mesData.LCD_3V0 = lcd_3V0.ToString();
                    mesData.DC_BOOST = dcboost.ToString();

                    // 개별 전압 판정
                    if (mcu_3V0 >= Settings.Instance.LDO_MCU_3V0_Min && mcu_3V0 <= Settings.Instance.LDO_MCU_3V0_Max)
                        control.Logger.Pass($"MCU_3V0 : {mcu_3V0:F3}V [{Settings.Instance.LDO_MCU_3V0_Min} ~ {Settings.Instance.LDO_MCU_3V0_Max}]");
                    else
                    {
                        control.Logger.Fail($"MCU_3V0 : {mcu_3V0:F3}V [{Settings.Instance.LDO_MCU_3V0_Min} ~ {Settings.Instance.LDO_MCU_3V0_Max}]");
                        isPass = false;
                    }

                    if (vdd_3V0 >= Settings.Instance.LDO_VDD_3V0_Min && vdd_3V0 <= Settings.Instance.LDO_VDD_3V0_Max)
                        control.Logger.Pass($"VDD_3V0 : {vdd_3V0:F3}V [{Settings.Instance.LDO_VDD_3V0_Min} ~ {Settings.Instance.LDO_VDD_3V0_Max}]");
                    else
                    {
                        control.Logger.Fail($"VDD_3V0 : {vdd_3V0:F3}V [{Settings.Instance.LDO_VDD_3V0_Min} ~ {Settings.Instance.LDO_VDD_3V0_Max}]");
                        isPass = false;
                    }

                    if (lcd_3V0 >= Settings.Instance.LDO_LCD_3V0_Min && lcd_3V0 <= Settings.Instance.LDO_LCD_3V0_Max)
                        control.Logger.Pass($"LCD_3V0 : {lcd_3V0:F3}V [{Settings.Instance.LDO_LCD_3V0_Min} ~ {Settings.Instance.LDO_LCD_3V0_Max}]");
                    else 
                    {
                        control.Logger.Fail($"LCD_3V0 : {lcd_3V0:F3}V [{Settings.Instance.LDO_LCD_3V0_Min} ~ {Settings.Instance.LDO_LCD_3V0_Max}]");
                        isPass = false;
                    }

                    if (dcboost >= Settings.Instance.LDO_DC_BOOST_Min && dcboost <= Settings.Instance.LDO_DC_BOOST_Max)
                        control.Logger.Pass($"DC_BOOST : {dcboost:F3}V [{Settings.Instance.LDO_DC_BOOST_Min} ~ {Settings.Instance.LDO_DC_BOOST_Max}]");
                    else 
                    {
                        control.Logger.Fail($"DC_BOOST : {dcboost:F3}V [{Settings.Instance.LDO_DC_BOOST_Min} ~ {Settings.Instance.LDO_DC_BOOST_Max}]");
                        isPass = false; 
                    }
                }

                // --- 4. 복구 단계: LDO OFF 및 LDO END CMD (실패와 무관하게 수행) ---
                if (Pba.IsConnected())
                {
                    byte[] LDO_OFF_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LDO_OFF).GetPacket();
                    byte[] LDO_OFF_CMD_rx = await Pba.SendAndReceivePacketAsync(LDO_OFF_CMD_tx, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(LDO_OFF_CMD_tx, LDO_OFF_CMD_rx)) control.Logger.Fail($"LDO_OFF_CMD 에러");
                }

                byte[] Ldo_end_cmd_tx = new TcpProtocol(0xC3, 0x03).GetPacket();
                byte[] Ldo_end_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_end_cmd_tx, Ldo_start_cmd_timeout, token);
                if (!UtilityFunctions.CheckTcpRxData(Ldo_end_cmd_tx, Ldo_end_cmd_rx))
                {
                    control.Logger.Fail($"LDO END CMD RX read fail");
                    isPass = false;
                }
                else if (Ldo_end_cmd_rx.Length >= 15)
                {
                    float vdd_3V0_off = BitConverter.ToSingle(Ldo_end_cmd_rx, 7);
                    float lcd_3V0_off = BitConverter.ToSingle(Ldo_end_cmd_rx, 11);
                    //float dcboost_off = BitConverter.ToSingle(Ldo_end_cmd_rx, 15);

                    mesData.VDD_3V0_OFF = vdd_3V0_off.ToString();
                    mesData.LCD_3V0_OFF = lcd_3V0_off.ToString();
                    //mesData.DC_BOOST_OFF = dcboost_off.ToString();

                    if (vdd_3V0_off >= Settings.Instance.LDO_VDD_3V0_OFF_Min && vdd_3V0_off <= Settings.Instance.LDO_VDD_3V0_OFF_Max)
                        control.Logger.Pass($"VDD_3V0_OFF : {vdd_3V0_off:F3}V [{Settings.Instance.LDO_VDD_3V0_OFF_Min} ~ {Settings.Instance.LDO_VDD_3V0_OFF_Max}]");
                    else
                    {
                        control.Logger.Fail($"VDD_3V0_OFF : {vdd_3V0_off:F3}V [{Settings.Instance.LDO_VDD_3V0_OFF_Min} ~ {Settings.Instance.LDO_VDD_3V0_OFF_Max}]");
                        isPass = false;
                    }

                    if (lcd_3V0_off >= Settings.Instance.LDO_LCD_3V0_OFF_Min && lcd_3V0_off <= Settings.Instance.LDO_LCD_3V0_OFF_Max)
                        control.Logger.Pass($"LCD_3V0_OFF : {lcd_3V0_off:F3}V [{Settings.Instance.LDO_LCD_3V0_OFF_Min} ~ {Settings.Instance.LDO_LCD_3V0_OFF_Max}]");
                    else
                    {
                        control.Logger.Fail($"LCD_3V0_OFF : {lcd_3V0_off:F3}V [{Settings.Instance.LDO_LCD_3V0_OFF_Min} ~ {Settings.Instance.LDO_LCD_3V0_OFF_Max}]");
                        isPass = false;
                    }

                    if (dcboost_off == -1000)
                    {
                        control.Logger.Fail($"DC_BOOST_OFF : NULL [{Settings.Instance.LDO_DC_BOOST_OFF_Min} ~ {Settings.Instance.LDO_DC_BOOST_OFF_Max}]");
                        isPass = false;
                    }

                    if (dcboost_off >= Settings.Instance.LDO_DC_BOOST_OFF_Min && dcboost_off <= Settings.Instance.LDO_DC_BOOST_OFF_Max)
                        control.Logger.Pass($"DC_BOOST_OFF : {dcboost_off:F3}V [{Settings.Instance.LDO_DC_BOOST_OFF_Min} ~ {Settings.Instance.LDO_DC_BOOST_OFF_Max}]");
                    else
                    {
                        control.Logger.Fail($"DC_BOOST_OFF : {dcboost_off:F3}V [{Settings.Instance.LDO_DC_BOOST_OFF_Min} ~ {Settings.Instance.LDO_DC_BOOST_OFF_Max}]");
                        isPass = false;
                    }
                }
                else
                {
                    control.Logger.Fail("Unexpected LDO END CMD return data => data length is insufficient");
                    isPass = false;
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] LDO exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("LDO", true);
                else
                    mesData.SetResultColumn("LDO", false);
            }
        }

        private static async Task<bool> Test_Current_Sleep_Ship(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.CURRENT_SLEEP_SHIP_Step_Delay, token);

                // --- 1. CURRENT_SLEEP_SHIP 시작 커맨드 (TCP 0xC4, 0x01) ---
                byte[] start_tx = new TcpProtocol(0xC4, 0x01).GetPacket();
                int start_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_01_Delay;
                Console.WriteLine($"START CMD RX 수신 대기 [Delay : {start_timeout}ms] [CH{ch + 1}]");
                byte[] start_rx = await Board.SendAndReceivePacketAsync(start_tx, start_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(start_tx, start_rx))
                {
                    control.Logger.Fail($"START CMD RX read fail");
                    isPass = false;
                }

                // --- 2. PBA 연결 및 Sleep CMD 전송 ---
                await Task.Delay(Settings.Instance.CURRENT_SLEEP_SHIP_Booting_01_Delay, token);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail (Sleep Step) [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }
                else
                {
                    byte[] sleep_cmd_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SLEEP_CMD).GetPacket();
                    byte[] sleep_cmd_rx = await Pba.SendAndReceivePacketAsync(sleep_cmd_tx, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(sleep_cmd_tx, sleep_cmd_rx))
                    {
                        control.Logger.Fail("SLEEP CMD read fail");
                        isPass = false;
                    }
                }

                // --- 3. Sleep 전류 측정 (TCP 0xC4, 0x02) ---
                byte[] sleep_meas_tx = new TcpProtocol(0xC4, 0x02).GetPacket();
                int sleep_meas_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_02_Delay;
                byte[] sleep_meas_rx = await Board.SendAndReceivePacketAsync(sleep_meas_tx, sleep_meas_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(sleep_meas_tx, sleep_meas_rx))
                {
                    control.Logger.Fail("Sleep data read fail");
                    isPass = false;
                }
                else if (sleep_meas_rx.Length >= 11)
                {
                    float sleep_curr = BitConverter.ToSingle(sleep_meas_rx, 7);
                    mesData.SLEEP_CURR = sleep_curr.ToString();

                    if (sleep_curr >= Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min &&
                        sleep_curr <= Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max)
                    {
                        control.Logger.Pass($"Sleep Curr : {sleep_curr:F3}mA [{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min}~{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"Sleep Curr : {sleep_curr:F3}mA [{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min}~{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max}]");
                        isPass = false;
                    }
                }

                // --- 4. PBA 재연결 및 Ship CMD 전송 ---
                await Task.Delay(Settings.Instance.CURRENT_SLEEP_SHIP_Booting_02_Delay, token);

                bool connectOk2 = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk2)
                {
                    control.Logger.Fail($"PBA connect fail (Ship Step) [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }
                else
                {
                    byte[] ship_cmd_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SHIP_CMD).GetPacket();
                    byte[] ship_cmd_rx = await Pba.SendAndReceivePacketAsync(ship_cmd_tx, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(ship_cmd_tx, ship_cmd_rx))
                    {
                        control.Logger.Fail("SHIP CMD read fail");
                        isPass = false;
                    }
                }

                // --- 5. Ship 전류 측정 (TCP 0xC4, 0x03) ---
                byte[] ship_meas_tx = new TcpProtocol(0xC4, 0x03).GetPacket();
                int ship_meas_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_03_Delay;
                byte[] ship_meas_rx = await Board.SendAndReceivePacketAsync(ship_meas_tx, ship_meas_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(ship_meas_tx, ship_meas_rx))
                {
                    control.Logger.Fail("Ship data read fail");
                    isPass = false;
                }
                else if (ship_meas_rx.Length >= 11)
                {
                    float ship_curr = BitConverter.ToSingle(ship_meas_rx, 7);
                    mesData.SHIP_CURR = ship_curr.ToString();

                    if (ship_curr >= Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min &&
                        ship_curr <= Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max)
                    {
                        control.Logger.Pass($"Ship Curr : {ship_curr:F3}uA [{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min}~{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"Ship Curr : {ship_curr:F3}uA [{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min}~{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max}]");
                        isPass = false;
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] CURRENT exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("CURRENT_SLEEP_SHIP", true);
                else
                    mesData.SetResultColumn("CURRENT_SLEEP_SHIP", false);
            }
        }
        private static async Task<bool> Test_CHARGE(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_hvdcp = true;
            bool isPass_pps = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.CHARGE_Step_Delay, token);

                // --- 1. Charging HVDCP CMD (TCP 0xC5, 0x01) ---
                byte[] tx1 = new TcpProtocol(0xC5, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.CHARGE_TCP_01_Delay;
                Console.WriteLine($"Charging HVDCP CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");
                byte[] rx1 = await Board.SendAndReceivePacketAsync(tx1, delay1, token);

                if (!UtilityFunctions.CheckTcpRxData(tx1, rx1))
                {
                    control.Logger.Fail($"Charging HVDCP CMD RX read fail");
                    isPass_hvdcp = false;
                }
                else if (rx1.Length >= 11) // float 데이터 파싱 안전 거리 확보
                {
                    float charge_hvdcp = (int)(BitConverter.ToSingle(rx1, 7) * 1000);
                    mesData.CHARGE_HVDCP = charge_hvdcp.ToString();

                    if (charge_hvdcp >= Settings.Instance.CHARGE_HVDCP_Min && charge_hvdcp <= Settings.Instance.CHARGE_HVDCP_Max)
                    {
                        control.Logger.Pass($"CHARGING HVDCP : {charge_hvdcp}mA [{Settings.Instance.CHARGE_HVDCP_Min} ~ {Settings.Instance.CHARGE_HVDCP_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"CHARGING HVDCP : {charge_hvdcp}mA [{Settings.Instance.CHARGE_HVDCP_Min} ~ {Settings.Instance.CHARGE_HVDCP_Max}]");
                        isPass_hvdcp = false;
                    }
                }

                // --- 2. Charging PPS CMD (TCP 0xC5, 0x02) ---
                byte[] tx2 = new TcpProtocol(0xC5, 0x02).GetPacket();
                int delay2 = Settings.Instance.Board_Read_Timeout + Settings.Instance.CHARGE_TCP_02_Delay;
                Console.WriteLine($"Charging PPS CMD RX 수신 대기 [Delay : {delay2}ms] [CH{ch + 1}]");
                byte[] rx2 = await Board.SendAndReceivePacketAsync(tx2, delay2, token);

                if (!UtilityFunctions.CheckTcpRxData(tx2, rx2))
                {
                    control.Logger.Fail($"Charging PPS CMD RX read fail");
                    isPass_pps = false;
                }

                // --- 3. PBA 연결 및 충전 이력 확인 ---
                await Task.Delay(Settings.Instance.CHARGE_Booting_01_Delay, token);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass_pps = false;
                }
                else
                {
                    // PPS 충전전류 이력 WRITE
                    byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_PPS_CHARGE_CURR_RECORD).GetPacket();
                    byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                    {
                        control.Logger.Fail("PPS CHARGE CURR RECORD write fail");
                        isPass_pps = false;
                    }

                    // PPS 충전전류 이력 READ
                    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_PPS_CHARGE_CURR_RECORD).GetPacket();
                    byte[] rx4 = await Pba.SendAndReceivePacketAsync_OnlyData(tx4, Settings.Instance.Pba_Read_Timeout, token);

                    if (rx4 == null || rx4.Length < 8)
                    {
                        control.Logger.Fail("PPS CHARGE CURR RECORD read fail");
                        isPass_pps = false;
                    }
                    else
                    {
                        short charge_record_soc = (short)((rx4[0] << 8) | rx4[1]);
                        short charge_record_vbus = (short)((rx4[2] << 8) | rx4[3]);
                        short charge_record_vbat = (short)((rx4[4] << 8) | rx4[5]);
                        short charge_record_ibat = (short)((rx4[6] << 8) | rx4[7]);

                        control.Logger.Info("CHARGING PPS :");
                        control.Logger.Info($"SOC : {charge_record_soc}% / VBUS : {charge_record_vbus}mV / VBAT : {charge_record_vbat}mV");
                        mesData.CHARGE_PPS = charge_record_ibat.ToString();
                        mesData.CHARGE_PPS_VBUS = charge_record_vbus.ToString();
                        mesData.CHARGE_PPS_VBAT = charge_record_vbat.ToString();

                        if (charge_record_ibat >= Settings.Instance.CHARGE_PPS_Min && charge_record_ibat <= Settings.Instance.CHARGE_PPS_Max)
                        {
                            control.Logger.Pass($"IBAT : {charge_record_ibat}mA [{Settings.Instance.CHARGE_PPS_Min} ~ {Settings.Instance.CHARGE_PPS_Max}]");
                        }
                        else
                        {
                            control.Logger.Fail($"IBAT : {charge_record_ibat}mA [{Settings.Instance.CHARGE_PPS_Min} ~ {Settings.Instance.CHARGE_PPS_Max}]");
                            isPass_pps = false;
                        }
                    }
                }

                // 전체 판정 종합
                isPass = isPass_hvdcp && isPass_pps;

                // --- 4. 복구 단계: CHARGE PPS END CMD (TCP 0xC5, 0x03) ---
                // 앞선 과정이 실패했더라도 장비를 초기 상태로 되돌리기 위해 반드시 수행합니다.
                byte[] tx5 = new TcpProtocol(0xC5, 0x03).GetPacket();
                int delay5 = Settings.Instance.Board_Read_Timeout + Settings.Instance.CHARGE_TCP_03_Delay;
                Console.WriteLine($"CHARGE PPS END CMD [Delay : {delay5}ms] [CH{ch + 1}]");
                byte[] rx5 = await Board.SendAndReceivePacketAsync(tx5, delay5, token);

                if (!UtilityFunctions.CheckTcpRxData(tx5, rx5))
                {
                    control.Logger.Fail("CHARGE PPS END CMD read fail");
                    isPass = false;
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] CHARGE exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("CHARGE", true);
                else
                    mesData.SetResultColumn("CHARGE", false);
            }

        }

        private static async Task<bool> Test_USB_CHECK(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_top = true;
            bool isPass_bottom = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.USB_CHECK_Step_Delay, token);

                // --- 1. USB TOP HVDCP 시작 (0xC7, 0x01) ---
                byte[] tx1 = new TcpProtocol(0xC7, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.USB_CHECK_TCP_01_Delay;
                Console.WriteLine($"USB TOP HVDCP CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");
                byte[] rx1 = await Board.SendAndReceivePacketAsync(tx1, delay1, token);

                if (!UtilityFunctions.CheckTcpRxData(tx1, rx1))
                {
                    control.Logger.Fail($"USB TOP HVDCP RX read fail");
                    isPass_top = false;
                }

                // --- 2. PBA 연결 및 TOP TA 체크 ---
                await Task.Delay(Settings.Instance.USB_CHECK_Booting_01_Delay, token);
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);

                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail (TOP) [{Return_Pba_Port_Name(ch)}]");
                    isPass_top = false;
                }
                else
                {
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_TA_CHECK).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, Settings.Instance.Pba_Read_Timeout, token);

                    if (rx2 == null || rx2.Length < 2)
                    {
                        control.Logger.Fail($"USB TOP TA check fail");
                        isPass_top = false;
                    }
                    else
                    {
                        short ta_type = (short)((rx2[0] << 8) | rx2[1]);
                        mesData.USB_CHECK_TOP = ta_type.ToString();
                        if (ta_type == Settings.Instance.USB_CHECK_TOP)
                        {
                            control.Logger.Pass($"USB TOP : {ta_type} [{Settings.Instance.USB_CHECK_TOP}]");
                        }
                        else
                        {
                            control.Logger.Fail($"USB TOP : {ta_type} [{Settings.Instance.USB_CHECK_TOP}]");
                            isPass_top = false;
                        }
                    }
                }

                // --- 3. USB BOTTOM HVDCP 전환 (0xC7, 0x02) ---
                byte[] tx3 = new TcpProtocol(0xC7, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.USB_CHECK_TCP_02_Delay;
                Console.WriteLine($"USB BOTTOM HVDCP RX 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");
                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);

                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"USB BOTTOM HVDCP RX read fail");
                    isPass_bottom = false;
                }

                // --- 4. PBA 재연결 및 BOTTOM TA 체크 ---
                await Task.Delay(Settings.Instance.USB_CHECK_Booting_02_Delay, token);
                bool connectOk2 = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);

                if (!connectOk2)
                {
                    control.Logger.Fail($"PBA connect fail (BOTTOM) [{Return_Pba_Port_Name(ch)}]");
                    isPass_bottom = false;
                }
                else
                {
                    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_TA_CHECK).GetPacket();
                    byte[] rx4 = await Pba.SendAndReceivePacketAsync_OnlyData(tx4, Settings.Instance.Pba_Read_Timeout, token);

                    if (rx4 == null || rx4.Length < 2)
                    {
                        control.Logger.Fail($"USB BOTTOM TA check fail");
                        isPass_bottom = false;
                    }
                    else
                    {
                        short ta_type2 = (short)((rx4[0] << 8) | rx4[1]);
                        mesData.USB_CHECK_BOT = ta_type2.ToString();
                        if (ta_type2 == Settings.Instance.USB_CHECK_BOTTOM)
                        {
                            control.Logger.Pass($"USB BOTTOM : {ta_type2} [{Settings.Instance.USB_CHECK_BOTTOM}]");
                        }
                        else
                        {
                            control.Logger.Fail($"USB BOTTOM : {ta_type2} [{Settings.Instance.USB_CHECK_BOTTOM}]");
                            isPass_bottom = false;
                        }
                    }
                }

                // 최종 결과 합산
                isPass = isPass_top && isPass_bottom;

                // --- 5. USB 검사 종료 커맨드 (0xC7, 0x03) ---
                // 이전 단계 실패와 무관하게 장비 복구(USB 경로 원복 등)를 위해 반드시 수행
                byte[] tx5 = new TcpProtocol(0xC7, 0x03).GetPacket();
                int delay5 = Settings.Instance.Board_Read_Timeout + Settings.Instance.USB_CHECK_TCP_03_Delay;
                Console.WriteLine($"USB END RX 수신 대기 [Delay : {delay5}ms] [CH{ch + 1}]");
                byte[] rx5 = await Board.SendAndReceivePacketAsync(tx5, delay5, token);

                if (!UtilityFunctions.CheckTcpRxData(tx5, rx5))
                {
                    control.Logger.Fail($"USB END RX read fail");
                    isPass = false;
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] USB CHECK exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("USB CHECK", true);
                else
                    mesData.SetResultColumn("USB CHECK", false);
            }
        }
        #region 통신 검사
        private static async Task<bool> Test_Pba_Cmd_Check_Start(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.PBA_CMD_CHECK_START_Step_Delay, token);

                byte[] tx = new TcpProtocol(0xD1, 0x01).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.PBA_CMD_CHECK_START_TCP_01_Delay;
                Console.WriteLine($"PBA CMD CHECK START CMD RX 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);
                if(!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"PBA CMD CHECK START CMD RX read fail");
                    isPass = false;
                }

                await Task.Delay(Settings.Instance.PBA_CMD_CHECK_START_Booting_01_Delay, token);
                Console.WriteLine($"PBA 전원 ON 대기 중... [{Settings.Instance.PBA_CMD_CHECK_START_Booting_01_Delay}ms]");

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }
                //control.Logger.Pass($"PBA connect success [{Return_Pba_Port_Name(ch)}]");

                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] PBA CMD CHECK START exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("PBA CMD CHECK START", true);
                else
                    mesData.SetResultColumn("PBA CMD CHECK START", false);

            }


        }

        private static async Task<bool> Test_Flag_Init(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.FLAG_INIT_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                    return false; // 연결 실패 시 이후 Read/Write가 불가능하므로 중단
                }

                int delay = Settings.Instance.Pba_Read_Timeout;

                // --- 2. 현재 프로세스 플래그 읽기 (Before Init) ---
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_PROCESS_FLAG).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, delay, token);

                if (rx == null || rx.Length < 8) // 4개 워드 데이터이므로 최소 8바이트 확인
                {
                    control.Logger.Fail($"FLAG READ CMD read fail (Before Init)");
                    return false; // 초기 데이터 확인 불가 시 초기화 진행 불가로 판단하여 중단
                }

                byte[] byte_now_flag = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    byte_now_flag[i] = rx[2 * i + 1];
                }

                string str_now_flag = BitConverter.ToString(byte_now_flag).Replace("-", "");
                control.Logger.Info($"PROCESS FLAG (Before Init) : {str_now_flag}");

                // --- 3. 플래그 초기화 (PBA FLAG 비트 0으로 설정) ---
                byte_now_flag[3] &= 0xFE;
                byte[] init_flag_data = new byte[] {
                    0x0B, 0xB9, 0x00, 0x04, 0x08,
                    0x00, byte_now_flag[0], 0x00, byte_now_flag[1], 0x00, byte_now_flag[2], 0x00, byte_now_flag[3]};

                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.MULTI_WRITE, init_flag_data).GetPacket();
                byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, delay);

                if (rx2 == null || !UtilityFunctions.CheckWriteMultiAck(tx2, rx2))
                {
                    control.Logger.Fail("INIT FLAG WRITE FAIL");
                    isPass = false; // 쓰기 실패 시 플래그만 변경하고 재확인 단계로 진행
                }

                //flash update
                byte[] tx_update = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_FLASH_UPDATE).GetPacket();
                byte[] rx_update = await Pba.SendAndReceivePacketAsync(tx_update, delay);
                if (rx_update == null || !UtilityFunctions.CheckEchoAck(tx_update, rx_update))
                {
                    control.Logger.Fail("FLASH UPDATE FAIL");
                    isPass = false;
                }

                await Task.Delay(Settings.Instance.FLAG_INIT_Update_Delay); //업데이트 적용 대기

                // --- 4. 초기화 결과 재확인 (After Init) ---
                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_PROCESS_FLAG).GetPacket();
                byte[] rx3 = await Pba.SendAndReceivePacketAsync_OnlyData(tx3, delay, token);

                if (rx3 == null || rx3.Length < 8)
                {
                    control.Logger.Fail($"FLAG READ CMD read fail (After Init)");
                    isPass = false;
                }
                else
                {
                    byte[] byte_init_flag = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        byte_init_flag[i] = rx3[2 * i + 1];
                    }

                    string str_init_flag = BitConverter.ToString(byte_init_flag).Replace("-", "");

                    // PBA 비트(Bit 0)가 0으로 초기화 되었는지 확인
                    if ((byte_init_flag[3] & 0x01) == 0)
                    {
                        control.Logger.Pass($"PROCESS FLAG (After Init) : {str_init_flag}");
                    }
                    else
                    {
                        control.Logger.Fail($"PROCESS FLAG (After Init) : {str_init_flag} (Bit 0 is not cleared)");
                        isPass = false;
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] FLAG INIT exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("FLAG INIT", true);
                else
                    mesData.SetResultColumn("FLAG INIT", false);

            }
        }

        private static async Task<bool> Test_MOTOR(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.MOTOR_Step_Delay, token);

                // --- 1. MOTOR START CMD (TCP 0xC9, 0x01) ---
                byte[] tx = new TcpProtocol(0xC9, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.MOTOR_TCP_01_Delay;
                Console.WriteLine($"MOTOR START CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay1, token);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"MOTOR START CMD RX read fail");
                    isPass = false;
                }

                // --- 2. PBA 진동 테스트 시작 명령 (CDC WRITE) ---
                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VIB_TEST_START).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.MOTOR_PBA_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, delay2, token);

                if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                {
                    control.Logger.Fail($"VID Motor Test start cmd rx read fail");
                    isPass = false;
                }

                // --- 3. MOTOR END 및 PWM 측정 CMD (TCP 0xC9, 0x02) ---
                // 앞선 단계에서 실패했더라도 모터 구동 정지를 위해 반드시 수행
                byte[] tx3 = new TcpProtocol(0xC9, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.MOTOR_TCP_02_Delay;
                Console.WriteLine($"MOTOR END CMD RX 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");

                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);

                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"MOTOR END CMD RX read fail");
                    isPass = false;
                }
                else
                {
                    // 데이터 파싱 전 수신 데이터 길이 확인 (최소 11바이트 이상 필요)
                    if (rx3 != null && rx3.Length >= 11)
                    {
                        // 인덱스 7부터 4바이트 추출하여 PWM 값 계산
                        byte[] byte_motor_pwm = rx3.Skip(7).Take(4).Reverse().ToArray();
                        uint motor_pwm = BitConverter.ToUInt32(byte_motor_pwm, 0);

                        mesData.MOTOR_PWM = motor_pwm.ToString();

                        // 판정 범위 확인
                        if (motor_pwm >= Settings.Instance.MOTOR_PWM_Min && motor_pwm <= Settings.Instance.MOTOR_PWM_Max)
                        {
                            control.Logger.Pass($"MOTOR PWM : {motor_pwm} [{Settings.Instance.MOTOR_PWM_Min} ~ {Settings.Instance.MOTOR_PWM_Max}]");
                        }
                        else
                        {
                            control.Logger.Fail($"MOTOR PWM : {motor_pwm} [{Settings.Instance.MOTOR_PWM_Min} ~ {Settings.Instance.MOTOR_PWM_Max}]");
                            isPass = false;
                        }
                    }
                    else
                    {
                        control.Logger.Fail("MOTOR PWM data len is insufficient");
                        isPass = false;
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] MOTOR exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("MOTOR", true);
                else
                    mesData.SetResultColumn("MOTOR", false);

            }
        }

        private static async Task<bool> Test_FLOODS(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_board = true;
            bool isPass_usb = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;

                }
                await Task.Delay(Settings.Instance.FLOODS_Step_Delay, token);

                // --- 1. BOARD FLOODS 시작 설정 (TCP 0xCA, 0x01) ---
                byte[] tx = new TcpProtocol(0xCA, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.FLOODS_TCP_01_Delay;
                Console.WriteLine($"BOARD FLOODS CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay1, token);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"BOARD FLOODS CMD RX read fail");
                    isPass_board = false;
                }

                // --- 2. PBA 연결 및 Board Flood 상태 확인 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass_board = false;
                    isPass_usb = false; // 연결 실패 시 이후 USB 테스트도 진행 불가 판정
                }
                else
                {
                    // Board Flood State Read
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_BOARD_FLOOD_STATE).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, Settings.Instance.Pba_Read_Timeout, token);

                    if (rx2 == null || rx2.Length < 2)
                    {
                        control.Logger.Fail($"LDC Flood State Read cmd rx read fail");
                        isPass_board = false;
                    }
                    else
                    {
                        short board_floods = (short)((rx2[0] << 8) | rx2[1]);
                        mesData.FLOOD_BOARD = board_floods.ToString();

                        if (board_floods == Settings.Instance.FLOODS_Board_Floods)
                        {
                            control.Logger.Pass($"BOARD FLOODS : {board_floods} [{Settings.Instance.FLOODS_Board_Floods}]");
                        }
                        else
                        {
                            control.Logger.Fail($"BOARD FLOODS : {board_floods} [{Settings.Instance.FLOODS_Board_Floods}]");
                            isPass_board = false;
                        }
                    }
                }

                // --- 3. USB FLOODS 설정 전환 (TCP 0xCA, 0x02) ---
                byte[] tx3 = new TcpProtocol(0xCA, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.FLOODS_TCP_02_Delay;
                Console.WriteLine($"USB FLOODS CMD RX 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");

                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"USB FLOODS CMD RX read fail");
                    isPass_usb = false;
                }

                // --- 4. USB Flood 상태 확인 ---
                if (Pba.IsConnected())
                {
                    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_USB_FLOOD_STATE).GetPacket();
                    byte[] rx4 = await Pba.SendAndReceivePacketAsync_OnlyData(tx4, Settings.Instance.Pba_Read_Timeout, token);

                    if (rx4 == null || rx4.Length < 2)
                    {
                        control.Logger.Fail($"USB FLOODS cmd rx read fail");
                        isPass_usb = false;
                    }
                    else
                    {
                        short usb_floods = (short)((rx4[0] << 8) | rx4[1]);
                        mesData.FLOOD_USB = usb_floods.ToString();

                        if (usb_floods == Settings.Instance.FLOODS_USB_Floods)
                        {
                            control.Logger.Pass($"USB FLOODS : {usb_floods} [{Settings.Instance.FLOODS_USB_Floods}]");
                        }
                        else
                        {
                            control.Logger.Fail($"USB FLOODS : {usb_floods} [{Settings.Instance.FLOODS_USB_Floods}]");
                            isPass_usb = false;
                        }
                    }
                }

                // 최종 판정 합산
                isPass = isPass_board && isPass_usb;

                // --- 5. FLOODS 종료 커맨드 (TCP 0xCA, 0x03) ---
                // 이전 단계의 성공 여부와 관계없이 장비 초기화를 위해 반드시 전송
                byte[] tx5 = new TcpProtocol(0xCA, 0x03).GetPacket();
                int delay5 = Settings.Instance.Board_Read_Timeout + Settings.Instance.FLOODS_TCP_02_Delay;
                Console.WriteLine($"FLOODS END CMD RX 수신 대기 [Delay : {delay5}ms] [CH{ch + 1}]");

                byte[] rx5 = await Board.SendAndReceivePacketAsync(tx5, delay5, token);
                if (!UtilityFunctions.CheckTcpRxData(tx5, rx5))
                {
                    control.Logger.Fail($"FLOODS END CMD RX read fail");
                    isPass = false;
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] FLOODS exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("FLOODS", true);
                else
                    mesData.SetResultColumn("FLOODS", false);

            }
        }

        private static async Task<bool> Test_HEATER(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_pinOff = true;
            bool isPass_pinOn = true;
            bool isPass_pwm = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.HEATER_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false; // 연결 실패 시 이후 PBA 명령 블록들을 건너뜀
                }

                // --- 2. HEATER START (TCP 0xCB, 0x01) ---
                byte[] tx = new TcpProtocol(0xCB, 0x01).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.HEATER_TCP_01_Delay;
                Console.WriteLine($"HEATER START 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");
                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"HEATER START CMD RX read fail");
                    isPass = false;
                }

                // --- 3. SENSING PIN OFF 및 저항 측정 ---
                if (Pba.IsConnected())
                {
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_OFF).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                    {
                        control.Logger.Fail($"SENSING PIN OFF fail");
                        isPass_pinOff = false;
                    }

                    byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                    byte[] rx3 = await Pba.SendAndReceivePacketAsync_OnlyData(tx3, Settings.Instance.Pba_Read_Timeout, token);
                    if (rx3 == null || rx3.Length < 2)
                    {
                        control.Logger.Fail($"READ HEATER NOW RESIST (OFF) RX read fail");
                        isPass_pinOff = false;
                    }
                    else
                    {
                        short pinOff_resist_short = (short)((rx3[0] << 8) | rx3[1]);
                        float pinOff_resist = (float)pinOff_resist_short / 1000;
                        mesData.HEATER_PIN_OFF = pinOff_resist.ToString();

                        if (pinOff_resist >= Settings.Instance.HEATER_Sensing_Pin_Off_Min && pinOff_resist <= Settings.Instance.HEATER_Sensing_Pin_Off_Max)
                            control.Logger.Pass($"HEATER SENSING PIN OFF : {pinOff_resist:F3} [{Settings.Instance.HEATER_Sensing_Pin_Off_Min}~{Settings.Instance.HEATER_Sensing_Pin_Off_Max}]");
                        else 
                        {
                            control.Logger.Fail($"HEATER SENSING PIN OFF : {pinOff_resist:F3} [{Settings.Instance.HEATER_Sensing_Pin_Off_Min}~{Settings.Instance.HEATER_Sensing_Pin_Off_Max}]");
                            isPass_pinOff = false; 
                        }
                    }

                    // --- 4. SENSING PIN ON 및 저항 측정 ---
                    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_ON).GetPacket();
                    byte[] rx4 = await Pba.SendAndReceivePacketAsync(tx4, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx4, rx4))
                    {
                        control.Logger.Fail($"SENSING PIN ON fail");
                        Console.Write($"fail log => tx : {BitConverter.ToString(tx4)}");
                        Console.Write($"fail log => rx : {BitConverter.ToString(rx4)}");
                        isPass_pinOn = false;
                    }

                    byte[] tx5 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                    byte[] rx5 = await Pba.SendAndReceivePacketAsync_OnlyData(tx5, Settings.Instance.Pba_Read_Timeout, token);
                    if (rx5 == null || rx5.Length < 2)
                    {
                        control.Logger.Fail($"READ HEATER NOW RESIST (ON) RX read fail");
                        isPass_pinOn = false;
                    }
                    else
                    {
                        short pinOn_resist_short = (short)((rx5[0] << 8) | rx5[1]);
                        float pinOn_resist = (float)pinOn_resist_short / 1000 + Settings.Instance.HEATER_Offsets[ch]; //오프셋 기능 추가
                        Console.WriteLine($"적용된 오프셋 : {Settings.Instance.HEATER_Offsets[ch]} [CH{ch + 1}]");

                        if (pinOn_resist >= Settings.Instance.HEATER_Sensing_Pin_On_Min && pinOn_resist <= Settings.Instance.HEATER_Sensing_Pin_On_Max)
                        {
                            control.Logger.Pass($"HEATER SENSING PIN ON : {pinOn_resist:F3} [{Settings.Instance.HEATER_Sensing_Pin_On_Min}~{Settings.Instance.HEATER_Sensing_Pin_On_Max}]");
                            mesData.HEATER_PIN_ON = pinOn_resist.ToString();
                        }
                        else 
                        {
                            control.Logger.Fail($"HEATER SENSING PIN ON : {pinOn_resist:F3} [{Settings.Instance.HEATER_Sensing_Pin_On_Min}~{Settings.Instance.HEATER_Sensing_Pin_On_Max}]");

                            bool retry_pass = true;
                            isPass_pinOn = false;

                            uint MaxRetryCount = Settings.Instance.HEATER_Retry_Count;
                            for (int i = 0; i < MaxRetryCount; i++)
                            {
                                Console.WriteLine($"HEATER RETRY ({i + 1}/{MaxRetryCount}) [CH{ch + 1}]");
                                control.Logger.Info($"HEATER RETRY ({i + 1}/{MaxRetryCount})");

                                byte[] tx_retry = new TcpProtocol(0xCB, 0x03).GetPacket();
                                byte[] rx_retry = await Board.SendAndReceivePacketAsync(tx_retry, Settings.Instance.Board_Read_Timeout, token);
                                if (!UtilityFunctions.CheckTcpRxData(tx_retry, rx_retry))
                                {
                                    control.Logger.Fail($"HEATER RETRY CMD RX read fail");
                                    retry_pass = false;
                                    continue;
                                }

                                byte[] tx_PinOn_retry = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_ON).GetPacket();
                                byte[] rx_PinOn_retry = await Pba.SendAndReceivePacketAsync(tx_PinOn_retry, Settings.Instance.Pba_Read_Timeout, token);
                                if (!UtilityFunctions.CheckEchoAck(tx_PinOn_retry, rx_PinOn_retry))
                                {
                                    control.Logger.Fail($"SENSING PIN ON fail");
                                    Console.Write($"fail log => tx : {BitConverter.ToString(tx_PinOn_retry)}");
                                    Console.Write($"fail log => rx : {BitConverter.ToString(rx_PinOn_retry)}");
                                    retry_pass = false;
                                    continue;
                                }
                                byte[] tx_read_resist = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                                byte[] rx_read_resist = await Pba.SendAndReceivePacketAsync_OnlyData(tx_read_resist, Settings.Instance.Pba_Read_Timeout, token);
                                if (rx_read_resist == null || rx_read_resist.Length < 2)
                                {
                                    control.Logger.Fail($"READ HEATER NOW RESIST (ON) RX read fail");
                                    retry_pass = false;
                                    continue;
                                }
                                else
                                {
                                    short pinOn_resist_short_retry = (short)((rx_read_resist[0] << 8) | rx_read_resist[1]);
                                    float pinOn_resist_retry = (float)pinOn_resist_short_retry / 1000 + Settings.Instance.HEATER_Offsets[ch]; //오프셋 기능 추가
                                    Console.WriteLine($"적용된 오프셋 : {Settings.Instance.HEATER_Offsets[ch]} [CH{ch + 1}]");

                                    if (pinOn_resist_retry >= Settings.Instance.HEATER_Sensing_Pin_On_Min && pinOn_resist_retry <= Settings.Instance.HEATER_Sensing_Pin_On_Max)
                                    {
                                        control.Logger.Pass($"HEATER SENSING PIN ON : {pinOn_resist_retry:F3} [{Settings.Instance.HEATER_Sensing_Pin_On_Min}~{Settings.Instance.HEATER_Sensing_Pin_On_Max}]");
                                        mesData.HEATER_PIN_ON = pinOn_resist_retry.ToString();
                                        retry_pass = true;
                                        break;
                                    }
                                    else
                                    {
                                        control.Logger.Fail($"HEATER SENSING PIN ON : {pinOn_resist_retry:F3} [{Settings.Instance.HEATER_Sensing_Pin_On_Min}~{Settings.Instance.HEATER_Sensing_Pin_On_Max}]");
                                        mesData.HEATER_PIN_ON = pinOn_resist_retry.ToString();
                                        retry_pass = false;
                                        continue;
                                    }
                                }
                            }

                            isPass_pinOn |= retry_pass;
                        }
                        
                    }
                    

                    // --- 5. HEATER PWM 설정 ---
                    byte[] tx6 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_HEATER_PWM).GetPacket();
                    byte[] rx6 = await Pba.SendAndReceivePacketAsync(tx6, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx6, rx6))
                    {
                        control.Logger.Fail($"WRITE HEATER PWM read fail");
                        isPass_pwm = false;
                    }
                }
                else
                {
                    control.Logger.Fail("PBA is not connected");
                    isPass_pinOff = false;
                    isPass_pinOn = false;
                }

                // --- 6. HEATER PWM 측정 (TCP 0xCB, 0x02) ---
                byte[] tx7 = new TcpProtocol(0xCB, 0x02).GetPacket();
                int delay7 = Settings.Instance.Board_Read_Timeout + Settings.Instance.HEATER_TCP_02_Delay;
                Console.WriteLine($"HEATER PWM 수신 대기 [Delay : {delay7}ms] [CH{ch + 1}]");
                byte[] rx7 = await Board.SendAndReceivePacketAsync(tx7, delay7, token);

                if (!UtilityFunctions.CheckTcpRxData(tx7, rx7))
                {
                    control.Logger.Fail($"HEATER PWM RX read fail");
                    isPass_pwm = false;
                }
                else if (rx7.Length >= 11)
                {
                    byte[] byte_heater_pwm = rx7.Skip(7).Take(4).Reverse().ToArray();
                    uint heater_pwm = BitConverter.ToUInt32(byte_heater_pwm, 0);
                    mesData.HEATER_PWM = heater_pwm.ToString();

                    if (heater_pwm >= Settings.Instance.HEATER_PWM_Min && heater_pwm <= Settings.Instance.HEATER_PWM_Max)
                        control.Logger.Pass($"HEATER PWM : {heater_pwm} [{Settings.Instance.HEATER_PWM_Min}~{Settings.Instance.HEATER_PWM_Max}]");
                    else
                    {
                        control.Logger.Fail($"HEATER PWM : {heater_pwm} [{Settings.Instance.HEATER_PWM_Min}~{Settings.Instance.HEATER_PWM_Max}]");
                        isPass_pwm = false; 
                    }
                }

                isPass = isPass && isPass_pinOff && isPass_pinOn && isPass_pwm;
                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] HEATER exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("HEATER", true);
                else
                    mesData.SetResultColumn("HEATER", false);

            }
        }

        private static async Task<bool> Test_CARTRIDGE(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_kato = true;
            bool isPass_pwm = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.CARTRIDGE_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }

                // --- 2. CARTRIDGE 시작 명령 (TCP 0xCC, 0x01) ---
                byte[] tx = new TcpProtocol(0xCC, 0x01).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.CARTRIDGE_TCP_01_Delay;
                Console.WriteLine($"CARTRIDGE START 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");
                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"CARTRIDGE START CMD RX read fail");
                    isPass = false;
                }

                // --- 3. Cartridge Boost ON (CDC WRITE) ---
                if (Pba.IsConnected())
                {
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_ON).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                    {
                        control.Logger.Fail($"Cartridge Boost on CMD read fail");
                        isPass = false;
                    }
                }

                // --- 4. CARTRIDGE PWM 및 BOOST 측정 (TCP 0xCC, 0x02) ---
                byte[] tx3 = new TcpProtocol(0xCC, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.CARTRIDGE_TCP_02_Delay;
                Console.WriteLine($"CARTRIDGE PWM CMD 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");
                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);

                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"CARTRIDGE PWM CMD RX read fail");
                    isPass = false;
                }
                else if (rx3.Length >= 15) // 데이터 인덱스 14까지 존재해야 함
                {
                    // KATO BOOST 측정 판정
                    byte[] kato_boost_byte = new byte[] { rx3[7], rx3[8], rx3[9], rx3[10] };
                    float kato_boost_result = BitConverter.ToSingle(kato_boost_byte, 0);
                    mesData.CARTRIDGE_BOOST = kato_boost_result.ToString();

                    if (kato_boost_result >= Settings.Instance.CARTRIDGE_KATO_BOOST_Min &&
                        kato_boost_result <= Settings.Instance.CARTRIDGE_KATO_BOOST_Max)
                    {
                        control.Logger.Pass($"KATO BOOST : {kato_boost_result} [{Settings.Instance.CARTRIDGE_KATO_BOOST_Min} ~ {Settings.Instance.CARTRIDGE_KATO_BOOST_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"KATO BOOST : {kato_boost_result} [{Settings.Instance.CARTRIDGE_KATO_BOOST_Min} ~ {Settings.Instance.CARTRIDGE_KATO_BOOST_Max}]");
                        isPass_kato = false;
                    }

                    // CARTRIDGE PWM 측정 판정
                    byte[] byte_cart_pwm = rx3.Skip(11).Take(4).Reverse().ToArray();
                    uint cartridge_pwm_result = BitConverter.ToUInt32(byte_cart_pwm, 0);
                    mesData.CARTRIDGE_PWM = cartridge_pwm_result.ToString();

                    if (cartridge_pwm_result >= Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Min &&
                        cartridge_pwm_result <= Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Max)
                    {
                        control.Logger.Pass($"CARTRIDGE PWM : {cartridge_pwm_result} [{Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Min} ~ {Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"CARTRIDGE PWM : {cartridge_pwm_result} [{Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Min} ~ {Settings.Instance.CARTRIDGE_CARTRIDGE_PWM_Max}]");
                        isPass_pwm = false;
                    }
                }
                else
                {
                    control.Logger.Fail("CARTRIDGE data len is insufficient");
                    isPass = false;
                }

                // --- 5. 복구 단계: Cartridge Boost OFF (실패 여부와 관계없이 수행) ---
                //if (Pba.IsConnected())
                //{
                //    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_OFF).GetPacket();
                //    byte[] rx4 = await Pba.SendAndReceivePacketAsync(tx4, Settings.Instance.Pba_Read_Timeout, token);
                //    if (!UtilityFunctions.CheckEchoAck(tx4, rx4))
                //    {
                //        control.Logger.Fail($"Cartridge Boost off CMD 에러");
                //        isPass = false;
                //    }
                //} 끄는 명령 삭제

                // 전체 판정 합산
                isPass = isPass && isPass_kato && isPass_pwm;
                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] CARTRIDGE exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("CARTRIDGE", true);
                else
                    mesData.SetResultColumn("CARTRIDGE", false);

            }
        }

        private static async Task<bool> Test_Sub_Heater(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool isPass_boost = true;
            bool isPass_pwm = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            

            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.SUB_HEATER_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                }

                // --- 2. SUB HEATER 시작 명령 (TCP 0xCD, 0x01) ---
                byte[] tx1 = new TcpProtocol(0xCD, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.SUB_HEATER_TCP_01_Delay;
                Console.WriteLine($"SUB HEATER START 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");

                byte[] rx1 = await Board.SendAndReceivePacketAsync(tx1, delay1, token);
                if (!UtilityFunctions.CheckTcpRxData(tx1, rx1))
                {
                    control.Logger.Fail($"SUB HEATER START CMD RX read fail");
                    isPass = false;
                }

                // --- 3. SUB HEATER Boost ON (CDC WRITE) ---
                if (Pba.IsConnected())
                {
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_ON).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                    {
                        control.Logger.Fail($"SUB HEATER Boost on CMD read fail");
                        isPass = false;
                    }
                }

                // --- 4. SUB HEATER PWM 및 BOOST 측정 (TCP 0xCD, 0x02) ---
                byte[] tx3 = new TcpProtocol(0xCD, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.SUB_HEATER_TCP_02_Delay;
                Console.WriteLine($"SUB HEATER PWM CMD 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");
                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);

                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"SUB HEATER PWM CMD RX read fail");
                    isPass = false;
                }
                else if (rx3.Length >= 15) // 데이터 파싱을 위해 인덱스 14까지 확보 확인
                {
                    // BOOST 전압 판정
                    float boost_result = BitConverter.ToSingle(rx3, 7);
                    mesData.SUB_HEATER_BOOST = boost_result.ToString();

                    if (boost_result >= Settings.Instance.SUB_HEATER_BOOST_Min && boost_result <= Settings.Instance.SUB_HEATER_BOOST_Max)
                    {
                        control.Logger.Pass($"SUB HEATER BOOST : {boost_result:F3} [{Settings.Instance.SUB_HEATER_BOOST_Min}~{Settings.Instance.SUB_HEATER_BOOST_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"SUB HEATER BOOST : {boost_result:F3} [{Settings.Instance.SUB_HEATER_BOOST_Min}~{Settings.Instance.SUB_HEATER_BOOST_Max}]");
                        isPass_boost = false;
                    }

                    // PWM 판정
                    byte[] byte_pwm_result = rx3.Skip(11).Take(4).Reverse().ToArray();
                    uint pwm_result = BitConverter.ToUInt32(byte_pwm_result, 0);
                    mesData.SUB_HEATER_PWM = pwm_result.ToString();

                    if (pwm_result >= Settings.Instance.SUB_HEATER_PWM_Min && pwm_result <= Settings.Instance.SUB_HEATER_PWM_Max)
                    {
                        control.Logger.Pass($"SUB HEATER PWM : {pwm_result} [{Settings.Instance.SUB_HEATER_PWM_Min}~{Settings.Instance.SUB_HEATER_PWM_Max}]");
                    }
                    else
                    {
                        control.Logger.Fail($"SUB HEATER PWM : {pwm_result} [{Settings.Instance.SUB_HEATER_PWM_Min}~{Settings.Instance.SUB_HEATER_PWM_Max}]");
                        isPass_pwm = false;
                    }
                }
                else
                {
                    control.Logger.Fail("SUB HEATER data len is insufficient");
                    isPass = false;
                }

                // --- 5. 복구 단계: SUB HEATER Boost OFF (실패 여부와 관계없이 수행) ---
                //if (Pba.IsConnected())
                //{
                //    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_OFF).GetPacket();
                //    byte[] rx4 = await Pba.SendAndReceivePacketAsync(tx4, Settings.Instance.Pba_Read_Timeout, token);
                //    if (!UtilityFunctions.CheckEchoAck(tx4, rx4))
                //    {
                //        control.Logger.Fail($"SUB HEATER Boost off CMD 에러");
                //        isPass = false;
                //    }
                //} 끄는 명령 삭제

                isPass = isPass && isPass_boost && isPass_pwm;
                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] SUB HEATER exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("SUB HEATER", true);
                else
                    mesData.SetResultColumn("SUB HEATER", false);

            }
        }

        private static async Task<bool> Test_ACCELEROMETER(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.ACCELEROMETER_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                    return false; // 연결 자체가 실패하면 이후 Read/Write가 불가능하므로 중단
                }

                // --- 2. ACCEL IC TEST START (CDC WRITE) ---
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_ACCEL_IC_TEST_START).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.ACCELEROMETER_PBA_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"ACCEL IC TEST START CMD read fail");
                    isPass = false;
                }

                // --- 3. ACCEL IC TEST RESULT READ (CDC READ) ---
                // 앞 단계에서 에러가 났더라도 결과 읽기를 시도하여 상태를 확인합니다.
                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_ACCEL_IC_TEST_RESULT).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.ACCELEROMETER_PBA_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay2, token);

                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"ACCEL IC TEST RESULT READ FAIL : Rx is NULL or Invalid");
                    isPass = false;
                }
                else
                {
                    // 결과 파싱 (2바이트 short)
                    short Accel_result = (short)((rx2[0] << 8) | rx2[1]);
                    mesData.ACCELEROMETER = Accel_result.ToString();

                    // 설정값과 비교 판정
                    if (Accel_result == Settings.Instance.ACCELEROMETER_Result)
                    {
                        control.Logger.Pass($"ACCELEROMETER : {Accel_result} [{Settings.Instance.ACCELEROMETER_Result}]");
                    }
                    else
                    {
                        control.Logger.Fail($"ACCELEROMETER : {Accel_result} [{Settings.Instance.ACCELEROMETER_Result}]");
                        isPass = false;
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] ACCELEROMETER exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("ACCELEROMETER", true);
                else
                    mesData.SetResultColumn("ACCELEROMETER", false);

            }
        }

        private static async Task<bool> Test_GPAK(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.GPAK_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                    return false; // 초기 연결 실패 시에는 이후 통신이 불가하므로 중단
                }

                // --- 2. GPAK Test Start (Write) ---
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_START).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.GPAK_Pba_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"GPAK Test Start CMD read fail");
                    isPass = false;
                }

                // --- 3. GPAK Test Result (Read) ---
                // 시작 커맨드가 실패했더라도 장비 상태 확인을 위해 결과 읽기를 시도합니다.
                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_GPAK_TEST_RESULT).GetPacket();
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay, token);

                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"GPAK Test Result Read fail");
                    isPass = false;
                }
                else
                {
                    // 결과 파싱 및 판단
                    short gpakResult = (short)((rx2[0] << 8) | rx2[1]);
                    mesData.GPAK_CHECK = gpakResult.ToString();

                    if (gpakResult == Settings.Instance.GPAK_Result)
                    {
                        control.Logger.Pass($"GPAK : {gpakResult} [{Settings.Instance.GPAK_Result}]");
                    }
                    else
                    {
                        control.Logger.Fail($"GPAK : {gpakResult} [{Settings.Instance.GPAK_Result}]");
                        isPass = false;
                    }
                }

                // --- 4. GPAK Test End (Write) ---
                // 앞선 판정 결과(isPass)와 관계없이 테스트 모드 종료를 위해 반드시 수행
                if (Pba.IsConnected())
                {
                    byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_END).GetPacket();
                    byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, delay, token);

                    if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                    {
                        control.Logger.Fail($"GPAK Test End CMD read fail");
                        isPass = false;
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] GPAK exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("GPAK", true);
                else
                    mesData.SetResultColumn("GPAK", false);

            }
        }

        private static async Task<bool> Test_Flash_Memory(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            bool mcuPass = true;
            bool extPass = true;
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.FLASH_MEMORY_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                    return false; // 연결 자체가 실패하면 이후 Read/Write가 불가능하므로 중단
                }

                int pba_delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLASH_MEMORY_Pba_Delay;

                // --- 2. MCU Flash Integrity Check 시작 ---
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_MCU_FLASH_INTEGRITY_CHECK_START).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, pba_delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"MCU Flash Integrity Check Start CMD read fail");
                    mcuPass = false;
                }

                // --- 3. MCU Flash Integrity 결과 읽기 ---
                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, pba_delay, token);
                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"MCU Flash Integrity Result Read read fail read fail");
                    mcuPass = false;
                }
                else
                {
                    short mcuResult = (short)((rx2[0] << 8) | rx2[1]);
                    mesData.MCU_MEMORY = mcuResult.ToString();
                    if (mcuResult == Settings.Instance.FLASH_MEMORY_MCU_MEMORY)
                    {
                        control.Logger.Pass($"MCU Memory : {mcuResult} [{Settings.Instance.FLASH_MEMORY_MCU_MEMORY}]");
                    }
                    else
                    {
                        control.Logger.Fail($"MCU Memory : {mcuResult} [{Settings.Instance.FLASH_MEMORY_MCU_MEMORY}]");
                        mcuPass = false;
                    }
                }

                // --- 4. Ext Flash Integrity Check 시작 ---
                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_EXT_FLASH_INTEGRITY_CHECK_START).GetPacket();
                byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, pba_delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                {
                    control.Logger.Fail($"Ext Flash Integrity Check Start CMD read fail");
                    extPass = false;
                }

                // --- 5. Ext Flash Integrity 결과 읽기 ---
                byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_EXT_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                byte[] rx4 = await Pba.SendAndReceivePacketAsync_OnlyData(tx4, pba_delay, token);
                if (rx4 == null || rx4.Length < 2)
                {
                    control.Logger.Fail($"Ext Flash Integrity Result Read fail");
                    extPass = false;
                }
                else
                {
                    short extResult = (short)((rx4[0] << 8) | rx4[1]);
                    mesData.EXT_MEMORY = extResult.ToString();
                    if (extResult == Settings.Instance.FLASH_MEMORY_EXT_MEMORY)
                    {
                        control.Logger.Pass($"EXT Memory : {extResult} [{Settings.Instance.FLASH_MEMORY_EXT_MEMORY}]");
                    }
                    else
                    {
                        control.Logger.Fail($"EXT Memory : {extResult} [{Settings.Instance.FLASH_MEMORY_EXT_MEMORY}]");
                        extPass = false;
                    }
                }

                // --- 6. 최종 결과 종합 ---
                isPass = mcuPass && extPass;
                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] FLASH MEMORY exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("FLASH MEMORY", true);
                else
                    mesData.SetResultColumn("FLASH MEMORY", false);

            }
        }

        private static async Task<bool> Test_PBA_FLAG(int ch, ChControl control, CancellationToken token, bool totalResult, MesData mesData)
        {
            bool isPass = true;
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.PBA_FLAG_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false;
                    return false; // 연결 실패 시 이후 쓰기/읽기가 불가능하므로 즉시 중단
                }

                int delay = Settings.Instance.Pba_Read_Timeout;

                // --- 2. 현재 플래그 읽기 (Before Write) ---
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_PROCESS_FLAG).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, delay, token);

                if (rx == null || rx.Length < 8) // 4워드 = 8바이트 확인
                {
                    control.Logger.Fail($"FLAG READ CMD read fail");
                    isPass = false;
                }
                else
                {
                    byte[] byte_now_flag = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        byte_now_flag[i] = rx[2 * i + 1];
                    }

                    string str_now_flag = BitConverter.ToString(byte_now_flag).Replace("-", "");
                    control.Logger.Info($"PROCESS FLAG (Before Write) : {str_now_flag}");

                    // totalResult에 따른 비트 연산 (성공 시 Bit 0을 1로, 실패 시 0으로)
                    if (totalResult && isPass)
                    {
                        byte_now_flag[3] |= 0x01;
                        control.Logger.Info("Writing PASS flag to PBA...");
                    }
                    else
                    {
                        byte_now_flag[3] &= 0xFE;
                        control.Logger.Info("Writing FAIL flag to PBA...");
                    }

                    // --- 3. 플래그 쓰기 수행 ---
                    byte[] write_flag_data = new byte[] {
                        0x0B, 0xB9, 0x00, 0x04, 0x08,
                        0x00, byte_now_flag[0], 0x00, byte_now_flag[1], 0x00, byte_now_flag[2], 0x00, byte_now_flag[3]};

                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.MULTI_WRITE, write_flag_data).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, delay);

                    if (rx2 == null || !UtilityFunctions.CheckWriteMultiAck(tx2, rx2))
                    {
                        control.Logger.Fail("WRITE PBA FLAG FAIL");
                        isPass = false;
                    }

                    //flash update
                    byte[] tx_update = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_FLASH_UPDATE).GetPacket();
                    byte[] rx_update = await Pba.SendAndReceivePacketAsync(tx_update, delay);
                    if (rx_update == null || !UtilityFunctions.CheckEchoAck(tx_update, rx_update))
                    {
                        control.Logger.Fail("FLASH UPDATE FAIL");
                        isPass = false;
                    }

                    await Task.Delay(Settings.Instance.PBA_FLAG_Update_Delay); //업데이트 적용 대기

                    // --- 4. 쓰기 결과 재확인 (After Write) ---
                    byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_PROCESS_FLAG).GetPacket();
                    byte[] rx3 = await Pba.SendAndReceivePacketAsync_OnlyData(tx3, delay, token);

                    if (rx3 == null || rx3.Length < 8)
                    {
                        control.Logger.Fail($"FLAG READ CMD read fail (After Write)");
                        isPass = false;
                    }
                    else
                    {
                        byte[] byte_written_flag = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            byte_written_flag[i] = rx3[2 * i + 1];
                        }
                        str_now_flag = BitConverter.ToString(byte_written_flag).Replace("-", "");
                        string str_written_flag = BitConverter.ToString(byte_written_flag).Replace("-", "");

                        // 최종 판정 (Bit 0 확인)
                        short verdict_flag = (short)((byte_written_flag[3] & 0x01) == 1 ? 1 : 0);
                        mesData.PBA_FLAG = verdict_flag.ToString();

                        isPass = (verdict_flag == Settings.Instance.PBA_FLAG_FLAG);

                        if (isPass)
                        {
                            control.Logger.Info($"PROCESS FLAG (After Write) : {str_now_flag}");
                            control.Logger.Pass($"PBA FLAG : {verdict_flag} [{Settings.Instance.PBA_FLAG_FLAG}]");
                        }
                        else
                        {
                            control.Logger.Info($"PROCESS FLAG (After Write) : {str_now_flag}");
                            control.Logger.Fail($"PBA FLAG : {verdict_flag} [{Settings.Instance.PBA_FLAG_FLAG}]");
                        }
                    }
                }

                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] PBA FLAG 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("PBA FLAG", true);
                else
                    mesData.SetResultColumn("PBA FLAG", false);

            }
        }

        private static async Task<bool> Test_PBA_TEST_END(int ch, ChControl control, CancellationToken token, MesData mesData)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];

            

            try
            {
                // 1. 기본 연결 확인
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.PBA_TEST_END_Step_Delay, token);

                // --- 2. TESTER INITIALIZE CMD (TCP 0x01, 0x00) ---
                // 모든 검사 시퀀스를 종료하고 장비를 초기화하는 명령
                byte[] tx = new TcpProtocol(0x01, 0x00).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.PBA_TEST_END_TCP_01_Delay;
                Console.WriteLine($"PBA CMD TEST END (Initialize) CMD RX 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    // 초기화 명령 응답이 비정상이더라도 로그만 남기고 true를 반환하여 
                    // 소프트웨어 측면의 시퀀스 종료를 보장하거나, 
                    // 상황에 따라 false를 반환해 사용자에게 알림을 줍니다.
                    control.Logger.Fail($"TESTER INITIALIZE CMD RX read fail");
                    isPass = false;
                }
                else
                {
                    control.Logger.Info("TESTER INITIALIZE SUCCESS");
                }

                // 시퀀스의 마지막이므로 최종 성공 여부를 리턴
                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] PBA TEST END exception: {ex.Message}";
                Console.WriteLine(errorMsg);
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                if (isPass)
                    mesData.SetResultColumn("PBA TEST END", true);
                else
                    mesData.SetResultColumn("PBA TEST END", false);

            }

        }

        private static async Task<bool> Test_HEATER_debug(int ch, ChControl control, CancellationToken token)
        {
            bool isPass = true;
            bool isPass_pinOff = true;
            bool isPass_pinOn = true;
            bool isPass_pwm = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];



            try
            {
                if (!Board.IsConnected())
                {
                    Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                    control.Logger.Fail("TCP is not connected!");
                    isPass = false;
                    return false;
                }

                await Task.Delay(Settings.Instance.TEST_1_HEATER_Step_Delay, token);

                // --- 1. PBA 연결 ---
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    isPass = false; // 연결 실패 시 이후 PBA 명령 블록들을 건너뜀
                }

                // --- 2. HEATER START (TCP 0xCB, 0x01) ---
                byte[] tx = new TcpProtocol(0xCB, 0x01).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.TEST_1_HEATER_TCP_01_Delay;
                Console.WriteLine($"HEATER START 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");
                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"HEATER START CMD RX read fail");
                    isPass = false;
                }

                // --- 3. SENSING PIN OFF 및 저항 측정 ---
                if (Pba.IsConnected())
                {
                    byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_OFF).GetPacket();
                    byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                    {
                        control.Logger.Fail($"SENSING PIN OFF fail");
                        isPass_pinOff = false;
                    }

                    byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                    byte[] rx3 = await Pba.SendAndReceivePacketAsync_OnlyData(tx3, Settings.Instance.Pba_Read_Timeout, token);
                    if (rx3 == null || rx3.Length < 2)
                    {
                        control.Logger.Fail($"READ HEATER NOW RESIST (OFF) RX read fail");
                        isPass_pinOff = false;
                    }
                    else
                    {
                        short pinOff_resist_short = (short)((rx3[0] << 8) | rx3[1]);
                        float pinOff_resist = (float)pinOff_resist_short / 1000;
                        

                        if (pinOff_resist >= Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Min && pinOff_resist <= Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Max)
                            control.Logger.Pass($"HEATER SENSING PIN OFF : {pinOff_resist:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Max}]");
                        else
                        {
                            control.Logger.Fail($"HEATER SENSING PIN OFF : {pinOff_resist:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_Off_Max}]");
                            isPass_pinOff = false;
                        }
                    }

                    // --- 4. SENSING PIN ON 및 저항 측정 ---
                    byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_ON).GetPacket();
                    byte[] rx4 = await Pba.SendAndReceivePacketAsync(tx4, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx4, rx4))
                    {
                        control.Logger.Fail($"SENSING PIN ON fail");
                        isPass_pinOn = false;
                    }

                    byte[] tx5 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                    byte[] rx5 = await Pba.SendAndReceivePacketAsync_OnlyData(tx5, Settings.Instance.Pba_Read_Timeout, token);
                    if (rx5 == null || rx5.Length < 2)
                    {
                        control.Logger.Fail($"READ HEATER NOW RESIST (ON) RX read fail");
                        isPass_pinOn = false;
                    }
                    else
                    {
                        short pinOn_resist_short = (short)((rx5[0] << 8) | rx5[1]);
                        float pinOn_resist = (float)pinOn_resist_short / 1000 + Settings.Instance.TEST_1_HEATER_Offsets[ch]; //오프셋 기능 추가
                        Console.WriteLine($"적용된 오프셋 : {Settings.Instance.TEST_1_HEATER_Offsets[ch]} [CH{ch + 1}]");

                        if (pinOn_resist >= Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min && pinOn_resist <= Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max)
                        {
                            control.Logger.Pass($"HEATER SENSING PIN ON : {pinOn_resist:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max}]");
                            
                        }
                        else
                        {
                            control.Logger.Fail($"HEATER SENSING PIN ON : {pinOn_resist:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max}]");

                            bool retry_pass = true;
                            isPass_pinOn = false;

                            uint MaxRetryCount = Settings.Instance.TEST_1_HEATER_Retry_Count;
                            for (int i = 0; i < MaxRetryCount; i++)
                            {
                                Console.WriteLine($"HEATER RETRY ({i + 1}/{MaxRetryCount}) [CH{ch + 1}]");
                                control.Logger.Info($"HEATER RETRY ({i + 1}/{MaxRetryCount})");

                                byte[] tx_retry = new TcpProtocol(0xCB, 0x03).GetPacket();
                                byte[] rx_retry = await Board.SendAndReceivePacketAsync(tx_retry, Settings.Instance.Board_Read_Timeout, token);
                                if (!UtilityFunctions.CheckTcpRxData(tx_retry, rx_retry))
                                {
                                    control.Logger.Fail($"HEATER RETRY CMD RX read fail");
                                    retry_pass = false;
                                    continue;
                                }

                                byte[] tx_PinOn_retry = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SENSING_PIN_ON).GetPacket();
                                byte[] rx_PinOn_retry = await Pba.SendAndReceivePacketAsync(tx_PinOn_retry, Settings.Instance.Pba_Read_Timeout, token);
                                if (!UtilityFunctions.CheckEchoAck(tx_PinOn_retry, rx_PinOn_retry))
                                {
                                    control.Logger.Fail($"SENSING PIN ON fail");
                                    retry_pass = false;
                                    continue;
                                }
                                byte[] tx_read_resist = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_HEATER_NOW_RESIST).GetPacket();
                                byte[] rx_read_resist = await Pba.SendAndReceivePacketAsync_OnlyData(tx_read_resist, Settings.Instance.Pba_Read_Timeout, token);
                                if (rx_read_resist == null || rx_read_resist.Length < 2)
                                {
                                    control.Logger.Fail($"READ HEATER NOW RESIST (ON) RX read fail");
                                    retry_pass = false;
                                    continue;
                                }
                                else
                                {
                                    short pinOn_resist_short_retry = (short)((rx_read_resist[0] << 8) | rx_read_resist[1]);
                                    float pinOn_resist_retry = (float)pinOn_resist_short_retry / 1000 + Settings.Instance.TEST_1_HEATER_Offsets[ch]; //오프셋 기능 추가
                                    Console.WriteLine($"적용된 오프셋 : {Settings.Instance.TEST_1_HEATER_Offsets[ch]} [CH{ch + 1}]");

                                    if (pinOn_resist_retry >= Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min && pinOn_resist_retry <= Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max)
                                    {
                                        control.Logger.Pass($"HEATER SENSING PIN ON : {pinOn_resist_retry:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max}]");
                                        
                                        retry_pass = true;
                                        break;
                                    }
                                    else
                                    {
                                        control.Logger.Fail($"HEATER SENSING PIN ON : {pinOn_resist_retry:F3} [{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Min}~{Settings.Instance.TEST_1_HEATER_Sensing_Pin_On_Max}]");
                                        retry_pass = false;
                                        continue;
                                    }
                                }
                            }

                            isPass_pinOn |= retry_pass;
                        }
                    }

                    // --- 5. HEATER PWM 설정 ---
                    byte[] tx6 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_HEATER_PWM).GetPacket();
                    byte[] rx6 = await Pba.SendAndReceivePacketAsync(tx6, Settings.Instance.Pba_Read_Timeout, token);
                    if (!UtilityFunctions.CheckEchoAck(tx6, rx6))
                    {
                        control.Logger.Fail($"WRITE HEATER PWM read fail");
                        isPass_pwm = false;
                    }
                }

                // --- 6. HEATER PWM 측정 (TCP 0xCB, 0x02) ---
                byte[] tx7 = new TcpProtocol(0xCB, 0x02).GetPacket();
                int delay7 = Settings.Instance.Board_Read_Timeout + Settings.Instance.TEST_1_HEATER_TCP_02_Delay;
                Console.WriteLine($"HEATER PWM 수신 대기 [Delay : {delay7}ms] [CH{ch + 1}]");
                byte[] rx7 = await Board.SendAndReceivePacketAsync(tx7, delay7, token);

                if (!UtilityFunctions.CheckTcpRxData(tx7, rx7))
                {
                    control.Logger.Fail($"HEATER PWM RX read fail");
                    isPass_pwm = false;
                }
                else if (rx7.Length >= 11)
                {
                    byte[] byte_heater_pwm = rx7.Skip(7).Take(4).Reverse().ToArray();
                    uint heater_pwm = BitConverter.ToUInt32(byte_heater_pwm, 0);

                    if (heater_pwm >= Settings.Instance.TEST_1_HEATER_PWM_Min && heater_pwm <= Settings.Instance.TEST_1_HEATER_PWM_Max)
                        control.Logger.Pass($"HEATER PWM : {heater_pwm} [{Settings.Instance.TEST_1_HEATER_PWM_Min}~{Settings.Instance.TEST_1_HEATER_PWM_Max}]");
                    else
                    {
                        control.Logger.Fail($"HEATER PWM : {heater_pwm} [{Settings.Instance.TEST_1_HEATER_PWM_Min}~{Settings.Instance.TEST_1_HEATER_PWM_Max}]");
                        isPass_pwm = false;
                    }
                }

                isPass = isPass && isPass_pinOff && isPass_pinOn && isPass_pwm;
                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] HEATER exception: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
            finally
            {
                

            }
        }
        #endregion


        #region Utility
        private static string Return_Pba_Port_Name (int ch)
        {
            string portName = "";
            switch (ch)
            {
                case 0:
                    portName = Settings.Instance.Device_Port_CH1;
                    break;
                case 1:
                    portName = Settings.Instance.Device_Port_CH2;
                    break;
                case 2:
                    portName = Settings.Instance.Device_Port_CH3;
                    break;
                case 3:
                    portName = Settings.Instance.Device_Port_CH4;
                    break;

            }

            return portName;
        }

        private static int Return_Pba_Port_Baudrate(int ch)
        {
            int Baudrate = 115200;
            switch (ch)
            {
                case 0:
                    Baudrate = Settings.Instance.Device_BaudRate_CH1;
                    break;
                case 1:
                    Baudrate = Settings.Instance.Device_BaudRate_CH2;
                    break;
                case 2:
                    Baudrate = Settings.Instance.Device_BaudRate_CH3;
                    break;
                case 3:
                    Baudrate = Settings.Instance.Device_BaudRate_CH4;
                    break;

            }

            return Baudrate;
        }

        private static void ShowInterlockFailPopup(ChControl control, int channelIndex)
        {
            string message = $"{channelIndex + 1}CH INTERLOCK FAIL!";

            void showAction()
            {
                using (var form = new InterLockForm(message))
                {
                    Form owner = control.FindForm();
                    if (owner != null && !owner.IsDisposed)
                        form.ShowDialog(owner);
                    else
                        form.ShowDialog();
                }
            }

            if (control.IsHandleCreated && control.InvokeRequired)
                control.Invoke((Action)showAction);
            else
                showAction();
        }

        #endregion
    }
}
