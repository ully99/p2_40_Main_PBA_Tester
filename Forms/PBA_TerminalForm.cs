using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using p2_40_Main_PBA_Tester.Data;
using p2_40_Main_PBA_Tester.Communication;
using p2_40_Main_PBA_Tester.UI;
using p2_40_Main_PBA_Tester.LIB;

namespace p2_40_Main_PBA_Tester.Forms
{
    public partial class PBA_TerminalForm : Form
    {
        #region Field
        public ManualForm manualform;
        private SerialChannelPort Pba;
        RichTextLogger DeviceTx;
        RichTextLogger DeviceRx;
        private bool StatusPortOpen = false;
        #endregion

        #region Init
        public PBA_TerminalForm(ManualForm parentform)
        {
            this.manualform = parentform;
            InitializeComponent();
            ConnectEvent();
            RescanSerialPort();
            DeviceTx = new RichTextLogger(tboxTx);
            DeviceRx = new RichTextLogger(tboxRx);
        }

        private void RescanSerialPort()
        {
            try
            {
                // 현재 PC에 연결된 모든 COM 포트 가져오기
                string[] portNames = SerialPort.GetPortNames();

                cboxPbaCom.Items.Clear();
                cboxPbaCom.Items.Add("None");
                cboxPbaCom.Items.AddRange(portNames);
                cboxPbaCom.SelectedIndex = 0;

                // 새로운 COM 포트 목록 추가
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while searching for port " + ex.Message);
            }
        }

        private void InitControlData()
        {
            lblMcuId.Text = "-";
            lblSn.Text = "-";
            lblFwVer.Text = "-";
            lblLdcFwVer.Text= "-";
            lblImageFwVer.Text = "-";
        }

        #endregion

        #region Event
        private void ConnectEvent()
        {



            //READ
            btnRead_LdcFloodState.Click += btnRead_LdcFloodState_Click; //009C 0001
            btnRead_AccelResult.Click += btnRead_AccelResult_Click; //0025 0001
            btnRead_McuFlashRead.Click += btnRead_McuFlashRead_Click; //0087 0001
            btnRead_ExtFlashRead.Click += btnRead_ExtFlashRead_Click; //0089 0001
            btnRead_GpakResult.Click += btnRead_GpakResult_Click; //0026 0001

            btnRead_BatSoc.Click += btnRead_BatSoc_Click; //000C 0001
            btnRead_TaType.Click += btnRead_TaType_Click; //007F 0001
            btnRead_BatCurrent.Click += btnRead_BatCurrent_Click; //000A 0001
            btnRead_ChargeCount.Click += btnRead_ChargeCount_Click; //021C 0004
            btnRead_FlagRead.Click += btnRead_FlagRead_Click; //0BBC 0001

            btnReadSend.Click += btnReadSend_Click;

            //WRITE
            btnWrite_VsysEnPinOn.Click += btnWrite_VsysEnPinOn_Click; //0005 0008
            btnWrite_Vdd3V3On.Click += btnWrite_Vdd3V3On_Click; //0005 0002
            btnWrite_Lcd3V0On.Click += btnWrite_Lcd3V0On_Click; //0005 0004
            btnWrite_DcBoostOn.Click += btnWrite_DcBoostOn_Click; //0005 0001
            btnWrite_LdoOff.Click += btnWrite_LdoOff_Click; //0005 0000
            btnWrite_Sleep.Click += btnWrite_Sleep_Click; //0006 0001?
            btnWrite_Ship.Click += btnWrite_Ship_Click; //0001 0001
            btnWrite_VidMotorTest.Click += btnWrite_VidMotorTest_Click; //0009 0001
            btnWrite_CartBoostOn.Click += btnWrite_CartBoostOn_Click; //0007 0001
            btnWrite_CartBoostOff.Click += btnWrite_CartBoostOff_Click; //0007 0000
            btnWrite_SubHeaterOn.Click += btnWrite_SubHeaterOn_Click; //0007 0002
            btnWrite_SubHeaterOff.Click += btnWrite_SubHeaterOff_Click; //0007 0000
            btnWrite_AccelStart.Click += btnWrite_AccelStart_Click; //0052 0001
            btnWrite_McuFlashCheck.Click += btnWrite_McuFlashCheck_Click; //000C 0000
            btnWrite_ExtFlashCheck.Click += btnWrite_ExtFlashCheck_Click; //000D 0000
            btnWrite_GpakStart.Click += btnWrite_GpakStart_Click; //0050 0001
            btnWrite_GpakEnd.Click += btnWrite_GpakEnd_Click; //0050 0000
            btnWrite_FlagPass.Click += btnWrite_FlagPass_Click; //0BBC 0007
            btnWrite_FlagFail.Click += btnWrite_FlagFail_Click; //0BBC 0003

            btnWriteSend.Click += btnWriteSend_Click;

            //MULTI WRITE
            btnWrite_ChargeCount.Click += btnWrite_ChargeCount_Click; //0x02, 0x1C, 0x00, 0x04, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            btnMultiWriteSend.Click += btnMultiWriteSend_Click;
        }
        private void btnRescan_Click(object sender, EventArgs e)
        {
            RescanSerialPort();
        }
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string portName = cboxPbaCom.SelectedItem?.ToString();
            btnConnect.Enabled = false;
            try
            {
                if (StatusPortOpen)
                {
                    ToggleLogComm_Device(Pba, true, false); //tx log 해제
                    ToggleLogComm_Device(Pba, false, false); //rx log 해제
                    Pba.Disconnect();
                    //InitControlData();
                    btnConnect.BackColor = Color.White;
                    btnConnect.Text = "연결";
                    StatusPortOpen = false;
                    
                    Console.WriteLine($"{portName} 해제 [Channel {Pba.ChannelNo}]");
                    return;
                }

                if (Pba == null)
                    Pba = new SerialChannelPort(ch: 101); // 채널 번호 적절히

                if (Pba.IsConnected())
                    Pba.Disconnect();

                bool ok = Pba.Connect(portName);
                if (!ok)
                {
                    StatusPortOpen = false;
                    btnConnect.BackColor = Color.White;
                    btnConnect.Text = "연결";
                    return;
                }

                StatusPortOpen = true;
                btnConnect.BackColor = Color.Green;
                btnConnect.Text = "해제";
                Console.WriteLine($"{portName} 연결 완료 [Channel {Pba.ChannelNo}]");

                ToggleLogComm_Device(Pba, true, true); //tx log 연결
                ToggleLogComm_Device(Pba, false, true); //rx log 연결

                await Task.Delay(100);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine("PBA 연결 중 예외 발생!" + ex.Message);
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void PBA_TerminalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Pba != null && Pba.IsConnected())
            {
                ToggleLogComm_Device(Pba, true, false); //tx log 해제
                ToggleLogComm_Device(Pba, false, false); //rx log 해제
                Pba.Disconnect();
                Console.WriteLine($"PBA Port 해제");
            }
        }
        #endregion

        #region Utility
        private void ToggleLogComm_Device(SerialChannelPort device, bool Istx, bool enable)
        {
            if (device == null) return;

            if (enable)
            {
                if (Istx)
                {
                    device.LogTxToUI = (tbox, msg) => DeviceTx.Tx(msg);
                }
                else
                {
                    device.LogRxToUI = (tbox, msg) => DeviceRx.Rx(msg);
                }
                
            }
            else
            {
                if (Istx)
                {
                    device.LogTxToUI = null;
                }
                else
                {
                    device.LogRxToUI = null;
                }
            }
        }


        #endregion

        #region Task
        private async void btnReadSend_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }

            btnReadSend.Enabled = false;

            try
            {
                // 전부 HEX로 해석
                ushort addr = UtilityFunctions.ParseU16HexLoose(tboxReadAddr.Text);   // ex: "1"  -> 0x0001
                ushort qty = UtilityFunctions.ParseU16HexLoose(tboxReadQuantity.Text);    // ex: "17" -> 0x0017

                // payload = [AddrHi AddrLo QtyHi QtyLo]
                byte[] payload = new byte[4];
                payload[0] = (byte)(addr >> 8);
                payload[1] = (byte)(addr & 0xFF);
                payload[2] = (byte)(qty >> 8);
                payload[3] = (byte)(qty & 0xFF);

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, payload).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);

                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }

            }
            catch (Exception ex)
            {
                DeviceRx.Fail("Read 실패: " + ex.Message);
            }
            finally
            {
                btnReadSend.Enabled = true;
            }
        }

        private async void btnWriteSend_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }

            btnWriteSend.Enabled = false;

            try
            {
                ushort addr = UtilityFunctions.ParseU16HexLoose(tboxWriteAddr.Text); // 예: "1" -> 0x0001
                ushort data = UtilityFunctions.ParseU16HexLoose(tboxWriteData.Text); // 예: "A" -> 0x000A

                // payload = [AddrHi AddrLo DataHi DataLo]
                byte[] payload = new byte[4];
                payload[0] = (byte)(addr >> 8);
                payload[1] = (byte)(addr & 0xFF);
                payload[2] = (byte)(data >> 8);
                payload[3] = (byte)(data & 0xFF);

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, payload).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);

                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    if (rx == null) DeviceRx.Fail("RECV = null");
                    else DeviceRx.Fail("RX is not unexpected value");
                    return;
                }

            }
            catch (Exception ex)
            {
                DeviceRx.Fail("Write 실패: " + ex.Message);
            }
            finally
            {
                btnWriteSend.Enabled = true;
            }
        }

        private async void btnMultiWriteSend_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }

            btnMultiWriteSend.Enabled = false;

            try
            {
                ushort addr = UtilityFunctions.ParseU16HexLoose(tboxMultiWriteAddr.Text);

                // Datas(HEX) -> ushort[] 레지스터 값들로 파싱
                ushort[] regs = UtilityFunctions.ParseMultiWriteRegs(tboxMultiWriteDatas.Text);
                if (regs == null || regs.Length == 0)
                    throw new FormatException("Datas(HEX)가 비어있음");

                // payload = [Addr(2)][Qty(2)][ByteCount(1)][Data(2*Qty)]
                byte[] payload = UtilityFunctions.BuildMultiWritePayload(addr, regs);

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.MULTI_WRITE, payload).GetPacket();

                // MultiWrite는 응답이 "요약 ACK"라 프레임 그대로 받아야 안전
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);

                //if (!UtilityFunctions.CheckWriteMultiAck(tx,rx))
                //{
                //    if (rx == null) DeviceRx.Fail("RECV = null");
                //    DeviceRx.Fail("RECV = null");
                //    return;
                //}

                bool ok = UtilityFunctions.CheckWriteMultiAck(tx, rx);
                if (ok)
                {
                    DeviceRx.Info($"(ADDR=0x{addr:X4}, QTY={regs.Length})");
                }
                else
                {
                    DeviceRx.Fail($"MULTI WRITE ACK FAIL (ADDR=0x{addr:X4}, QTY={regs.Length})");
                    DeviceRx.Info("TX=" + Encoding.ASCII.GetString(tx).Trim());
                    DeviceRx.Info("RX=" + Encoding.ASCII.GetString(rx).Trim());
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail("MultiWrite 실패: " + ex.Message);
            }
            finally
            {
                btnMultiWriteSend.Enabled = true;
            }
        }

        private async Task GetMcuId()
        {
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_MCU_ID).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);

                if (rx == null) { DeviceRx.Fail("ID Check: Rx is NULL"); return; }

                string DeviceId = $"{rx[6]:X2}{rx[7]:X2}{rx[4]:X2}{rx[5]:X2}{rx[2]:X2}{rx[3]:X2}{rx[0]:X2}{rx[1]:X2}";
                lblMcuId.Text = DeviceId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("MCU ID 못가져옴" + ex.Message);
            }

        }

        private async Task GetSn()
        {
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_SERIAL_NO).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);

                if (rx == null) { DeviceRx.Fail("SN Check: Rx is NULL"); return; }

                var sb = new StringBuilder();
                for (int i = 1; i < rx.Length; i += 2) sb.Append((char)rx[i]);
                string SerialNo = sb.ToString();
                lblSn.Text = SerialNo;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SN 못가져옴" + ex.Message);
            }
        }

        private async Task GetFwVer()
        {
            try
            {
                byte[] Fw_Ver_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FW_VER).GetPacket();
                int timeout = Settings.Instance.Pba_Read_Timeout;
                

                byte[] Fw_Ver_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(Fw_Ver_READ_CMD_tx, timeout);
                if (Fw_Ver_READ_CMD_rx == null) { DeviceRx.Fail("FW Ver Check : RX is NULL"); return; }

                string major = $"{(char)Fw_Ver_READ_CMD_rx[0]}{(char)Fw_Ver_READ_CMD_rx[1]}";
                string minor = $"{(char)Fw_Ver_READ_CMD_rx[2]}{(char)Fw_Ver_READ_CMD_rx[3]}";
                string FwVer = $"{major}.{minor}";

                ushort FwVer_LDC = (ushort)((Fw_Ver_READ_CMD_rx[8] << 8) | Fw_Ver_READ_CMD_rx[9]);

                lblFwVer.Text = FwVer;
                lblLdcFwVer.Text = FwVer_LDC.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FW Ver 못가져옴" + ex.Message);
            }
        }

        private async Task GetImageFwVer()
        {
            try
            {
                byte[] IMAGE_FW_VER_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_IMAGE_FW_VER).GetPacket();
                byte[] IMAGE_FW_VER_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(IMAGE_FW_VER_READ_CMD_tx, Settings.Instance.Pba_Read_Timeout);
                if (IMAGE_FW_VER_READ_CMD_rx == null) { DeviceRx.Fail("LDC FW Check: Rx is NULL"); return; }

                string ImageVer = $"{(char)IMAGE_FW_VER_READ_CMD_rx[0]}{(char)IMAGE_FW_VER_READ_CMD_rx[1]}." +
                    $"{(char)IMAGE_FW_VER_READ_CMD_rx[2]}{(char)IMAGE_FW_VER_READ_CMD_rx[3]}";
                lblImageFwVer.Text = ImageVer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LDC FW Ver 못가져옴" + ex.Message);
            }
        }

        

        private void lblClearTxRx_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tboxTx.Clear();
            tboxRx.Clear();
        }

        // READ 핸들러들
        private async void btnRead_LdcFloodState_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FLOOD_STATE).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"LDC Flood State Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_AccelResult_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_ACCEL_IC_TEST_RESULT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Accel Result Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_McuFlashRead_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"MCU Flash Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_ExtFlashRead_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_EXT_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Ext Flash Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_GpakResult_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_GPAK_TEST_RESULT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"GPAK Result Read 실패: {ex.Message}");
            }
        }

        // WRITE 핸들러들
        private async void btnWrite_VsysEnPinOn_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VSYS_EN_PIN_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("VSYS EN PIN ON Write 실패");
                    return;
                }
                DeviceRx.Pass("VSYS EN PIN ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"VSYS EN PIN ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_Vdd3V3On_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VDD_3V3_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("VDD 3V3 ON Write 실패");
                    return;
                }
                DeviceRx.Pass("VDD 3V3 ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"VDD 3V3 ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_Lcd3V0On_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LCD_3V0_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("LCD 3V0 ON Write 실패");
                    return;
                }
                DeviceRx.Pass("LCD 3V0 ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"LCD 3V0 ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_DcBoostOn_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_DC_BOOST_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("DC Boost ON Write 실패");
                    return;
                }
                DeviceRx.Pass("DC Boost ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"DC Boost ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_LdoOff_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LDO_OFF).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("LDO OFF Write 실패");
                    return;
                }
                DeviceRx.Pass("LDO OFF Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"LDO OFF Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_Sleep_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SLEEP_CMD).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Sleep Write 실패");
                    return;
                }
                DeviceRx.Pass("Sleep Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Sleep Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_Ship_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SHIP_CMD).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Ship Write 실패");
                    return;
                }
                DeviceRx.Pass("Ship Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Ship Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_VidMotorTest_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VIB_TEST_START).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("VID Motor Test Write 실패");
                    return;
                }
                DeviceRx.Pass("VID Motor Test Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"VID Motor Test Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_CartBoostOn_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Cartridge Boost ON Write 실패");
                    return;
                }
                DeviceRx.Pass("Cartridge Boost ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Cartridge Boost ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_CartBoostOff_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_OFF).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Cartridge Boost OFF Write 실패");
                    return;
                }
                DeviceRx.Pass("Cartridge Boost OFF Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Cartridge Boost OFF Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_SubHeaterOn_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_ON).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Sub Heater ON Write 실패");
                    return;
                }
                DeviceRx.Pass("Sub Heater ON Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Sub Heater ON Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_SubHeaterOff_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_OFF).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Sub Heater OFF Write 실패");
                    return;
                }
                DeviceRx.Pass("Sub Heater OFF Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Sub Heater OFF Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_AccelStart_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_ACCEL_IC_TEST_START).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Accel Start Write 실패");
                    return;
                }
                DeviceRx.Pass("Accel Start Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Accel Start Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_McuFlashCheck_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_MCU_FLASH_INTEGRITY_CHECK_START).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("MCU Flash Check Write 실패");
                    return;
                }
                DeviceRx.Pass("MCU Flash Check Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"MCU Flash Check Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_ExtFlashCheck_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_EXT_FLASH_CHECK).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Ext Flash Check Write 실패");
                    return;
                }
                DeviceRx.Pass("Ext Flash Check Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Ext Flash Check Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_GpakStart_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_START).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("GPAK Start Write 실패");
                    return;
                }
                DeviceRx.Pass("GPAK Start Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"GPAK Start Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_GpakEnd_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_END).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("GPAK End Write 실패");
                    return;
                }
                DeviceRx.Pass("GPAK End Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"GPAK End Write 실패: {ex.Message}");
            }
        }

        private async void btnRead_BatSoc_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_SOC).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Bat SOC Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_TaType_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_TA_CHECK).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"TA Type Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_BatCurrent_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_BAT_CURRENT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Bat Current Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_ChargeCount_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FIFG_CHARGE_COUNT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Charge Count Read 실패: {ex.Message}");
            }
        }

        private async void btnRead_FlagRead_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_CHARGE_FLAG).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync_OnlyData(tx, Settings.Instance.Pba_Read_Timeout);
                if (rx == null)
                {
                    DeviceRx.Fail("RECV = null");
                    return;
                }

                var regs = UtilityFunctions.ParseRegistersBigEndian(rx);
                for (int i = 0; i < regs.Length; i++)
                {
                    DeviceRx.Info(
                        $"RECV[{i}] ( DEC = {regs[i]}, HEX = {regs[i]:X4} )"
                    );
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Flag Read 실패: {ex.Message}");
            }
        }

        private async void btnWrite_FlagPass_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CHARGE_FLAG_PASS).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Flag Pass Write 실패");
                    return;
                }
                DeviceRx.Pass("Flag Pass Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Flag Pass Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_FlagFail_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CHARGE_FLAG_FAIL).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    DeviceRx.Fail("Flag Fail Write 실패");
                    return;
                }
                DeviceRx.Pass("Flag Fail Write 성공");
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Flag Fail Write 실패: {ex.Message}");
            }
        }

        private async void btnWrite_ChargeCount_Click(object sender, EventArgs e)
        {
            if (Pba == null || !Pba.IsConnected())
            {
                DeviceRx.Fail("포트 연결 안됨");
                return;
            }
            try
            {
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.MULTI_WRITE, Variable.MULTI_WRITE_FIFG_CHARGE_COUNT).GetPacket();
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, Settings.Instance.Pba_Read_Timeout);
                bool ok = UtilityFunctions.CheckWriteMultiAck(tx, rx);
                if (ok)
                {
                    DeviceRx.Pass("Charge Count MultiWrite 성공");
                }
                else
                {
                    DeviceRx.Fail("Charge Count MultiWrite 실패");
                }
            }
            catch (Exception ex)
            {
                DeviceRx.Fail($"Charge Count MultiWrite 실패: {ex.Message}");
            }
        }

        #endregion

        

        

        #region Parsing
        private void btnIntToHex_Click(object sender, EventArgs e)
        {
            try
            {
                ushort val = UtilityFunctions.ParseU16DecLoose(tboxInt.Text); // 0~65535

                byte hi = (byte)(val >> 8);
                byte lo = (byte)(val & 0xFF);

                tboxHexHi.Text = hi.ToString("X2"); // 항상 2자리
                tboxHexLo.Text = lo.ToString("X2");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Int → Hex 변환 실패: " + ex.Message);
            }
        }

        private void btnHexToInt_Click(object sender, EventArgs e)
        {
            try
            {
                byte hi = UtilityFunctions.ParseU8HexLoose(tboxHexHi.Text); // 00~FF
                byte lo = UtilityFunctions.ParseU8HexLoose(tboxHexLo.Text);

                ushort val = (ushort)((hi << 8) | lo);
                tboxInt.Text = val.ToString(); // 10진수로 표시
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hex → Int 변환 실패: " + ex.Message);
            }
        }

        private void btnFloatToHex_Click(object sender, EventArgs e)
        {
            try
            {
                float f = UtilityFunctions.ParseFloatLoose(tboxFloat.Text);

                // float -> bytes (BitConverter는 시스템 엔디안(대부분 Little))
                byte[] bytes = BitConverter.GetBytes(f); // [b0,b1,b2,b3] = Little-endian

                // UI: hex1=LSB, hex4=MSB (32 E6 2E 3E 같은 형태)
                tboxHex1.Text = bytes[0].ToString("X2");
                tboxHex2.Text = bytes[1].ToString("X2");
                tboxHex3.Text = bytes[2].ToString("X2");
                tboxHex4.Text = bytes[3].ToString("X2");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Float → Hex 변환 실패: " + ex.Message);
            }
        }

        private void btnHexToFloat_Click(object sender, EventArgs e)
        {
            try
            {
                byte b0 = UtilityFunctions.ParseU8HexLoose(tboxHex1.Text);
                byte b1 = UtilityFunctions.ParseU8HexLoose(tboxHex2.Text);
                byte b2 = UtilityFunctions.ParseU8HexLoose(tboxHex3.Text);
                byte b3 = UtilityFunctions.ParseU8HexLoose(tboxHex4.Text);

                byte[] bytes = new byte[] { b0, b1, b2, b3 }; // UI가 LSB->MSB 순서라고 가정

                float f = BitConverter.ToSingle(bytes, 0);

                // 표시 포맷은 취향. 정확히 쓰고 싶으면 "R" 추천
                tboxFloat.Text = f.ToString("0.####", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hex → Float 변환 실패: " + ex.Message);
            }
        }








        #endregion

        private void btnPbaInfoAllClear_Click(object sender, EventArgs e)
        {
            InitControlData();
        }

        private async void btnPbaInfoAllRead_Click(object sender, EventArgs e)
        {
            if (!Pba.IsConnected()) return;

            await GetMcuId();
            await GetSn();
            await GetFwVer();
            await GetImageFwVer();
        }

     
    }
}
