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


namespace p2_40_Main_PBA_Tester.Data
{
    internal static class TaskFunctions
    {
        // 메인에서 이 함수를 호출하면, 이름에 맞는 테스트를 찾아서 실행함
        public static async Task<bool> RunTestItem(string taskName, int channelIndex, ChControl control, CancellationToken token, bool totalResult)
        {
            bool result = false;

            control.Logger.Section(taskName);
            // 실제로는 여기서 장비 통신 등을 수행 (Task.Delay는 시뮬레이션용)
            // 나중에 CommManager 등을 인자로 받아서 실제 하드웨어 제어 코드를 넣으면 됨

            switch (taskName)
            {
                case "QR READ":
                    result = await Test_QrRead(channelIndex, control, token);
                    break;
                case "MCU INFO":
                    result = await Test_McuInfo(channelIndex, control, token);
                    break;
                case "OVP":
                    result = await Test_OVP(channelIndex, control, token);
                    break;
                case "LDO":
                    result = await Test_LDO(channelIndex, control, token);
                    break;
                case "CURRENT_SLEEP_SHIP":
                    result = await Test_Current_Sleep_Ship(channelIndex, control, token);
                    break;

                //통신검사
                case "PBA CMD CHECK START":
                    result = await Test_Pba_Cmd_Check_Start(channelIndex, control, token);
                    break;

                case "MOTOR":
                    result = await Test_MOTOR(channelIndex, control, token);
                    break;

                case "FLOODS":
                    result = await Test_FLOODS(channelIndex, control, token);
                    break;

                case "CARTRIDGE":
                    result = await Test_CARTRIDGE(channelIndex, control, token);
                    break;

                case "SUB HEATER":
                    result = await Test_Sub_Heater(channelIndex, control, token);
                    break;

                case "GPAK":
                    result = await Test_GPAK(channelIndex, control, token);
                    break;
                case "ACCELEROMETER":
                    result = await Test_ACCELEROMETER(channelIndex, control, token);
                    break;
                case "FLASH MEMORY":
                    result = await Test_Flash_Memory(channelIndex, control, token);
                    break;

                default:
                    // 정의되지 않은 항목은 일단 PASS 처리하거나 로그 남김
                    control.Logger.Info($"이 작업은 구현되지 않았습니다. : {taskName}");
                    result = false;
                    break;
            }

            // 2. 결과 출력
            control.Logger.ResultSection(taskName, result);

            return result;
        }

      












        // --- 각 개별 테스트 함수들  ---

        private static async Task<bool> Test_QrRead(int ch, ChControl control, CancellationToken token)
        {
            var Qr = CommManager.QrPorts[ch];
            if (!Qr.IsOpen)
            {
                Console.WriteLine($"QR PORT가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("QR PORT is not connected!");
                return false;
            }
            try
            {
                await Task.Delay(Settings.Instance.QR_READ_Step_Delay);

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
                    await Task.Delay(200);
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

                await Qr.SendAsync(Variable.QR_END);

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
                string errorMsg = $"[{ch + 1}CH] QR READ 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                // UI 로그에도 표시
                control.Logger.Fail(errorMsg);
                return false;
            }
            

        }

        private static async Task<bool> Test_McuInfo(int ch, ChControl control, CancellationToken token)
        {
            bool isPass = true;

            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];
            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }
            
            try
            {
                await Task.Delay(Settings.Instance.MCU_INFO_Step_Delay);

                byte[] start_cmd_tx = new TcpProtocol(0xC1, 0x01).GetPacket();
                int start_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.MCU_INFO_Tcp_01_Delay;
                Console.WriteLine($"MCU INFO START CMD RX 수신 대기 [Delay : {start_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] start_cmd_rx = await Board.SendAndReceivePacketAsync(start_cmd_tx, start_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(start_cmd_tx, start_cmd_rx))
                {
                    control.Logger.Fail($"START CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"START CMD 적용 완료");

                //await Task.Delay(Settings.Instance.Pba_On_Delay);
                //Console.WriteLine($"PBA 전원 ON 대기 중... [{Settings.Instance.Pba_On_Delay}ms]");

                Console.WriteLine($"pba 연결 딜레이 : {Settings.Instance.Pba_Connect_Timeout}");
                
                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if(!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }
                //control.Logger.Pass($"PBA connect success [{Return_Pba_Port_Name(ch)}]");

                byte[] MCU_ID_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_MCU_ID).GetPacket();
                int MCU_ID_READ_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.MCU_INFO_Pba_Delay;
                Console.WriteLine($"MCU ID READ CMD RX 수신 대기 [Delay : {MCU_ID_READ_CMD_timeout}ms] [CH{ch + 1}]");

                byte[] MCU_ID_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(MCU_ID_READ_CMD_tx, MCU_ID_READ_CMD_timeout, token);
                if (MCU_ID_READ_CMD_rx == null) { control.Logger.Fail("MCU ID: Rx is NULL"); return false; }

                string McuId = $"{MCU_ID_READ_CMD_rx[7]:X2}{MCU_ID_READ_CMD_rx[6]:X2}" +
                    $"{MCU_ID_READ_CMD_rx[5]:X2}{MCU_ID_READ_CMD_rx[4]:X2}" +
                    $"{MCU_ID_READ_CMD_rx[3]:X2}{MCU_ID_READ_CMD_rx[2]:X2}" +
                    $"{MCU_ID_READ_CMD_rx[1]:X2}{MCU_ID_READ_CMD_rx[0]:X2}"; // hexWords 로직 축약
                bool isPass_McuId = McuId.Length == Settings.Instance.MCU_INFO_Mcu_Id_Len;
                if(!isPass_McuId)
                {
                    control.Logger.Fail($"MCU ID : {McuId} ===> Len : {McuId.Length} [{Settings.Instance.MCU_INFO_Mcu_Id_Len}]");
                    isPass = false;
                }
                control.Logger.Pass($"MCU ID : {McuId} ===> Len : {McuId.Length} [{Settings.Instance.MCU_INFO_Mcu_Id_Len}]");

                byte[] Fw_Ver_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FW_VER).GetPacket();
                int Fw_Ver_READ_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.MCU_INFO_Pba_Delay;
                Console.WriteLine($"FW VER READ CMD RX 수신 대기 [Delay : {Fw_Ver_READ_CMD_timeout}ms] [CH{ch + 1}]");

                byte[] Fw_Ver_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(Fw_Ver_READ_CMD_tx, Fw_Ver_READ_CMD_timeout, token);
                if (Fw_Ver_READ_CMD_rx == null) { control.Logger.Fail("FW Ver: Rx is NULL"); return false; }

                string major = $"{(char)Fw_Ver_READ_CMD_rx[0]}{(char)Fw_Ver_READ_CMD_rx[1]}";
                string minor = $"{(char)Fw_Ver_READ_CMD_rx[2]}{(char)Fw_Ver_READ_CMD_rx[3]}";
                string FwVer = $"{major}.{minor}";

                ushort FwVer_LDC = (ushort)((Fw_Ver_READ_CMD_rx[8] << 8) | Fw_Ver_READ_CMD_rx[9]);

                bool isPass_FwVer = FwVer == Settings.Instance.MCU_INFO_Main_Fw_Ver;
                bool isPass_LdcVer = FwVer_LDC == int.Parse(Settings.Instance.MCU_INFO_LDC_Fw_Ver);

                if(!isPass_FwVer)
                {
                    control.Logger.Fail($"FW VER : {FwVer} [{Settings.Instance.MCU_INFO_Main_Fw_Ver}]");
                    isPass = false;
                }
                control.Logger.Pass($"FW VER : {FwVer} [{Settings.Instance.MCU_INFO_Main_Fw_Ver}]");
                if (!isPass_LdcVer)
                {
                    control.Logger.Fail($"LDC FW VER : {FwVer_LDC} [{int.Parse(Settings.Instance.MCU_INFO_LDC_Fw_Ver)}]");
                    isPass = false;
                }
                control.Logger.Pass($"LDC FW VER : {FwVer_LDC} [{int.Parse(Settings.Instance.MCU_INFO_LDC_Fw_Ver)}]");

                byte[] IMAGE_FW_VER_READ_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_IMAGE_FW_VER).GetPacket();
                int IMAGE_FW_VER_READ_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.MCU_INFO_Pba_Delay;
                Console.WriteLine($"IMAGE FW READ CMD RX 수신 대기 [Delay : {IMAGE_FW_VER_READ_CMD_timeout}ms] [CH{ch + 1}]");

                byte[] IMAGE_FW_VER_READ_CMD_rx = await Pba.SendAndReceivePacketAsync_OnlyData(IMAGE_FW_VER_READ_CMD_tx, IMAGE_FW_VER_READ_CMD_timeout, token);
                if (IMAGE_FW_VER_READ_CMD_rx == null) { control.Logger.Fail("IMAGE FW VER : Rx is NULL"); return false; }

                string ImageVer = $"{(char)IMAGE_FW_VER_READ_CMD_rx[0]}{(char)IMAGE_FW_VER_READ_CMD_rx[1]}." +
                    $"{(char)IMAGE_FW_VER_READ_CMD_rx[2]}{(char)IMAGE_FW_VER_READ_CMD_rx[3]}";

                bool isPass_ImageVer = ImageVer == Settings.Instance.MCU_INFO_Image_Fw_Ver;

                if (!isPass_ImageVer)
                {
                    control.Logger.Fail($"IMAGE FW VER : {ImageVer} [{Settings.Instance.MCU_INFO_Image_Fw_Ver}]");
                    isPass = false;
                }
                control.Logger.Pass($"IMAGE FW VER : {ImageVer} [{Settings.Instance.MCU_INFO_Image_Fw_Ver}]");

                byte[] end_cmd_tx = new TcpProtocol(0xC1, 0x02).GetPacket();
                int end_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.MCU_INFO_Tcp_02_Delay;
                Console.WriteLine($"MCU INFO END CMD RX 수신 대기 [Delay : {end_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] end_cmd_rx = await Board.SendAndReceivePacketAsync(end_cmd_tx, end_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(end_cmd_tx, end_cmd_rx))
                {
                    control.Logger.Fail($"END CMD RX 에러");
                    return false;
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
                string errorMsg = $"[{ch + 1}CH] MCU INFO 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                // UI 로그에도 표시
                control.Logger.Fail(errorMsg);
                return false;
            }
        }

        private static async Task<bool> Test_OVP(int ch, ChControl control, CancellationToken token)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.OVP_Step_Delay);

                byte[] ovp_cmd_tx = new TcpProtocol(0xC2, 0x01).GetPacket();
                int ovp_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.OVP_TCP_01_Delay;
                Console.WriteLine($"OVP CMD RX 수신 대기 [Delay : {ovp_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] ovp_cmd_rx = await Board.SendAndReceivePacketAsync(ovp_cmd_tx, ovp_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(ovp_cmd_tx, ovp_cmd_rx))
                {
                    control.Logger.Fail($"OVP CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"OVP CMD 적용 완료");

                byte[] ovp_volt_byte = new byte[] { ovp_cmd_rx[7], ovp_cmd_rx[8], ovp_cmd_rx[9], ovp_cmd_rx[10] };
                float ovp_volt = BitConverter.ToSingle(ovp_volt_byte, 0);

                bool isPass_ovp = ovp_volt >= Settings.Instance.OVP_VBUS_Min && ovp_volt <= Settings.Instance.OVP_VBUS_Max;

                if(!isPass_ovp)
                {
                    control.Logger.Fail($"OVP : {ovp_volt}V [{Settings.Instance.OVP_VBUS_Min} ~ {Settings.Instance.OVP_VBUS_Max}]");
                    isPass = false;
                }
                control.Logger.Pass($"OVP : {ovp_volt}V [{Settings.Instance.OVP_VBUS_Min} ~ {Settings.Instance.OVP_VBUS_Max}]");

                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);

                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] MCU INFO 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                // UI 로그에도 표시
                control.Logger.Fail(errorMsg);
                return false;
            }
            
        }

        private static async Task<bool> Test_LDO(int ch, ChControl control, CancellationToken token)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];
            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.LDO_Step_Delay);

                byte[] Ldo_start_cmd_tx = new TcpProtocol(0xC3, 0x01).GetPacket();
                int Ldo_start_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.LDO_TCP_01_Delay;
                Console.WriteLine($"LDO START CMD RX 수신 대기 [Delay : {Ldo_start_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] Ldo_start_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_start_cmd_tx, Ldo_start_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(Ldo_start_cmd_tx, Ldo_start_cmd_rx))
                {
                    control.Logger.Fail($"LDO START CMD RX 에러");
                    return false;
                }
                control.Logger.Pass($"LDO START CMD 적용 완료");
                byte[] vsys_volt_byte = new byte[] { Ldo_start_cmd_rx[7], Ldo_start_cmd_rx[8], Ldo_start_cmd_rx[9], Ldo_start_cmd_rx[10] };
                byte[] vsys_3V3_volt_off_byte = new byte[] { Ldo_start_cmd_rx[11], Ldo_start_cmd_rx[12], Ldo_start_cmd_rx[13], Ldo_start_cmd_rx[14] };
                float vsys_volt = BitConverter.ToSingle(vsys_volt_byte, 0);
                float vsys_3V3_off_volt = BitConverter.ToSingle(vsys_3V3_volt_off_byte, 0);
                bool isPass_vsys_volt = vsys_volt >= Settings.Instance.LDO_VSYS_Min && vsys_volt <= Settings.Instance.LDO_VSYS_Max;
                bool isPass_vsys_3V3_volt_off = vsys_3V3_off_volt >= Settings.Instance.LDO_VSYS_3V3_OFF_Min &&
                    vsys_3V3_off_volt <= Settings.Instance.LDO_VSYS_3V3_OFF_Max;

                if (isPass_vsys_volt) control.Logger.Pass($"VSYS : {vsys_volt} [{Settings.Instance.LDO_VSYS_Min} ~ {Settings.Instance.LDO_VSYS_Max}]");
                else control.Logger.Fail($"VSYS : {vsys_volt} [{Settings.Instance.LDO_VSYS_Min} ~ {Settings.Instance.LDO_VSYS_Max}]");

                if (isPass_vsys_3V3_volt_off) control.Logger.Pass($"VSYS_3V3 OFF : {vsys_3V3_off_volt} [{Settings.Instance.LDO_VSYS_3V3_OFF_Min} ~ {Settings.Instance.LDO_VSYS_3V3_OFF_Max}]");
                else control.Logger.Fail($"VSYS_3V3 OFF : {vsys_3V3_off_volt} [{Settings.Instance.LDO_VSYS_3V3_OFF_Min} ~ {Settings.Instance.LDO_VSYS_3V3_OFF_Max}]");

                if (!(isPass_vsys_volt && isPass_vsys_3V3_volt_off)) isPass = false;

                

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }
                control.Logger.Pass($"PBA connect success [{Return_Pba_Port_Name(ch)}]");

                byte[] VSYS_EN_PIN_ON_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VSYS_EN_PIN_ON).GetPacket();
                int VSYS_EN_PIN_ON_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.LDO_Pba_Delay;
                Console.WriteLine($"VSYS_EN_PIN_ON_CMD RX 수신 대기 [Delay : {VSYS_EN_PIN_ON_CMD_timeout}ms] [CH{ch + 1}]");
                byte[] VSYS_EN_PIN_ON_CMD_rx = await Pba.SendAndReceivePacketAsync(VSYS_EN_PIN_ON_CMD_tx, VSYS_EN_PIN_ON_CMD_timeout, token);
                if (!UtilityFunctions.CheckEchoAck(VSYS_EN_PIN_ON_CMD_tx, VSYS_EN_PIN_ON_CMD_rx)) 
                { 
                    control.Logger.Fail("VSYS_EN_PIN_ON_CMD RX : Rx is NULL"); 
                    return false;
                }
                //control.Logger.Pass($"VSYS_EN_PIN_ON_CMD 적용 완료");

                byte[] VDD_3V3_ON_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VDD_3V3_ON).GetPacket();
                int VDD_3V3_ON_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.LDO_Pba_Delay;
                Console.WriteLine($"VDD_3V3_ON_CMD RX 수신 대기 [Delay : {VDD_3V3_ON_CMD_timeout}ms] [CH{ch + 1}]");
                byte[] VDD_3V3_ON_CMD_rx = await Pba.SendAndReceivePacketAsync(VDD_3V3_ON_CMD_tx, VDD_3V3_ON_CMD_timeout, token);
                if (!UtilityFunctions.CheckEchoAck(VDD_3V3_ON_CMD_tx,VDD_3V3_ON_CMD_rx))
                { 
                    control.Logger.Fail("VDD_3V3_ON_CMD RX : Rx is NULL"); 
                    return false;
                }
                //control.Logger.Pass($"VDD_3V3_ON_CMD 적용 완료");

                byte[] LCD_3V0_ON_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LCD_3V0_ON).GetPacket();
                int LCD_3V0_ON_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.LDO_Pba_Delay;
                Console.WriteLine($"LCD_3V0_ON_CMD RX 수신 대기 [Delay : {LCD_3V0_ON_CMD_timeout}ms] [CH{ch + 1}]");
                byte[] LCD_3V0_ON_CMD_rx = await Pba.SendAndReceivePacketAsync(LCD_3V0_ON_CMD_tx, LCD_3V0_ON_CMD_timeout, token);
                if (!UtilityFunctions.CheckEchoAck(LCD_3V0_ON_CMD_tx, LCD_3V0_ON_CMD_rx)) 
                { 
                    control.Logger.Fail("LCD_3V0_ON_CMD RX : Rx is NULL"); 
                    return false;
                }
                //control.Logger.Pass($"LCD_3V0_ON_CMD 적용 완료");

                byte[] DC_BOOST_ON_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_DC_BOOST_ON).GetPacket();
                int DC_BOOST_ON_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.LDO_Pba_Delay;
                Console.WriteLine($"DC_BOOST_ON_CMD RX 수신 대기 [Delay : {LCD_3V0_ON_CMD_timeout}ms] [CH{ch + 1}]");
                byte[] DC_BOOST_ON_CMD_rx = await Pba.SendAndReceivePacketAsync(DC_BOOST_ON_CMD_tx, DC_BOOST_ON_CMD_timeout, token);
                if (!UtilityFunctions.CheckEchoAck(DC_BOOST_ON_CMD_tx,DC_BOOST_ON_CMD_rx))
                { 
                    control.Logger.Fail("DC_BOOST_ON_CMD RX : Rx is NULL"); 
                    return false; 
                }
                //control.Logger.Pass($"DC_BOOST_ON_CMD 적용 완료");

                byte[] Ldo_second_cmd_tx = new TcpProtocol(0xC3, 0x02).GetPacket();
                int Ldo_second_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.LDO_TCP_02_Delay;
                Console.WriteLine($"LDO SECOND CMD RX 수신 대기 [Delay : {Ldo_second_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] Ldo_second_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_second_cmd_tx, Ldo_second_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(Ldo_second_cmd_tx, Ldo_second_cmd_rx))
                {
                    control.Logger.Fail($"LDO SECOND CMD RX 에러");
                    return false;
                }
                byte[] mcu_3V0_byte = new byte[] { Ldo_start_cmd_rx[7], Ldo_start_cmd_rx[8], Ldo_start_cmd_rx[9], Ldo_start_cmd_rx[10] };
                byte[] sys_3V3_byte = new byte[] { Ldo_start_cmd_rx[11], Ldo_start_cmd_rx[12], Ldo_start_cmd_rx[13], Ldo_start_cmd_rx[14] };
                byte[] vdd_3V0_byte = new byte[] { Ldo_start_cmd_rx[15], Ldo_start_cmd_rx[16], Ldo_start_cmd_rx[17], Ldo_start_cmd_rx[18] };
                byte[] lcd_3V0_byte = new byte[] { Ldo_start_cmd_rx[19], Ldo_start_cmd_rx[20], Ldo_start_cmd_rx[21], Ldo_start_cmd_rx[22] };
                float mcu_3V0 = BitConverter.ToSingle(mcu_3V0_byte, 0);
                float sys_3V3 = BitConverter.ToSingle(sys_3V3_byte, 0);
                float vdd_3V0 = BitConverter.ToSingle(vdd_3V0_byte, 0);
                float lcd_3V0 = BitConverter.ToSingle(lcd_3V0_byte, 0);
                bool isPass_mcu_3V0 = mcu_3V0 >= Settings.Instance.LDO_MCU_3V0_Min && mcu_3V0 <= Settings.Instance.LDO_MCU_3V0_Max;
                bool isPass_sys_3V3 = sys_3V3 >= Settings.Instance.LDO_VSYS_3V3_Min && sys_3V3 <= Settings.Instance.LDO_VSYS_3V3_Max;
                bool isPass_vdd_3V0 = vdd_3V0 >= Settings.Instance.LDO_VDD_3V0_Min && vdd_3V0 <= Settings.Instance.LDO_VDD_3V0_Max;
                bool isPass_lcd_3V0 = lcd_3V0 >= Settings.Instance.LDO_LCD_3V0_Min && lcd_3V0 <= Settings.Instance.LDO_LCD_3V0_Max;

                if (isPass_sys_3V3) control.Logger.Pass($"VSYS_3V3 : {sys_3V3} [{Settings.Instance.LDO_VSYS_3V3_Min} ~ {Settings.Instance.LDO_VSYS_3V3_Max}]");
                else control.Logger.Fail($"VSYS_3V3 : {sys_3V3} [{Settings.Instance.LDO_VSYS_3V3_Min} ~ {Settings.Instance.LDO_VSYS_3V3_Max}]");

                if (isPass_mcu_3V0) control.Logger.Pass($"MCU_3V0 : {mcu_3V0} [{Settings.Instance.LDO_MCU_3V0_Min} ~ {Settings.Instance.LDO_MCU_3V0_Max}]");
                else control.Logger.Fail($"MCU_3V0 : {mcu_3V0} [{Settings.Instance.LDO_MCU_3V0_Min} ~ {Settings.Instance.LDO_MCU_3V0_Max}]");

                if (isPass_vdd_3V0) control.Logger.Pass($"VDD_3V0 : {vdd_3V0} [{Settings.Instance.LDO_VDD_3V0_Min} ~ {Settings.Instance.LDO_VDD_3V0_Min}]");
                else control.Logger.Fail($"VDD_3V0 : {vdd_3V0} [{Settings.Instance.LDO_VDD_3V0_Min} ~ {Settings.Instance.LDO_VDD_3V0_Min}]");

                if (isPass_lcd_3V0) control.Logger.Pass($"LCD_3V0 : {lcd_3V0} [{Settings.Instance.LDO_LCD_3V0_Min} ~ {Settings.Instance.LDO_LCD_3V0_Max}]");
                else control.Logger.Fail($"LCD_3V0 : {lcd_3V0} [{Settings.Instance.LDO_LCD_3V0_Min} ~ {Settings.Instance.LDO_LCD_3V0_Max}]");

                //DC BOOST 추가 예정
                //절차도 바뀔 예정

                if (!(isPass_sys_3V3 && isPass_mcu_3V0 && isPass_vdd_3V0 && isPass_lcd_3V0)) isPass = false;

                byte[] LDO_OFF_CMD_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_LDO_OFF).GetPacket();
                int LDO_OFF_CMD_timeout = Settings.Instance.Pba_Read_Timeout + Settings.Instance.LDO_Pba_Delay;
                Console.WriteLine($"LDO_OFF_CMD RX 수신 대기 [Delay : {LDO_OFF_CMD_timeout}ms] [CH{ch + 1}]");
                byte[] LDO_OFF_CMD_rx = await Pba.SendAndReceivePacketAsync(LDO_OFF_CMD_tx, LDO_OFF_CMD_timeout, token);

                if (!UtilityFunctions.CheckEchoAck(LDO_OFF_CMD_tx, LDO_OFF_CMD_rx))
                {
                    control.Logger.Fail($"LDO_OFF_CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"LDO OFF CMD 적용 완료");

                byte[] Ldo_end_cmd_tx = new TcpProtocol(0xC3, 0x03).GetPacket();
                int Ldo_end_cmd_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.LDO_TCP_01_Delay;
                Console.WriteLine($"LDO END CMD RX 수신 대기 [Delay : {Ldo_end_cmd_timeout}ms] [CH{ch + 1}]");
                byte[] Ldo_end_cmd_rx = await Board.SendAndReceivePacketAsync(Ldo_end_cmd_tx, Ldo_end_cmd_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(Ldo_end_cmd_tx, Ldo_end_cmd_rx))
                {
                    control.Logger.Fail($"LDO END CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"LDO END CMD 적용 완료");


                return isPass;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);

                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] LDO 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                // UI 로그에도 표시
                control.Logger.Fail(errorMsg);
                return false;
            }
            
        }

        private static async Task<bool> Test_Current_Sleep_Ship(int ch, ChControl control, CancellationToken token)
        {
            bool isPass = true;
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.CURRENT_SLEEP_SHIP_Step_Delay);

                // 1. CURRENT_SLEEP_SHIP START CMD (TCP 0xC4, 0x01)
                byte[] start_tx = new TcpProtocol(0xC4, 0x01).GetPacket();
                int start_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_01_Delay;
                Console.WriteLine($"START CMD RX 수신 대기 [Delay : {start_timeout}ms] [CH{ch + 1}]");
                byte[] start_rx = await Board.SendAndReceivePacketAsync(start_tx, start_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(start_tx, start_rx))
                {
                    control.Logger.Fail($"START CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"START CMD 적용 완료");
                // 2. PBA 연결
                Console.WriteLine($"pba 연결 딜레이 : {Settings.Instance.Pba_Connect_Timeout}");

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }
                //control.Logger.Pass($"PBA connect success [{Return_Pba_Port_Name(ch)}]");

                // 3. Sleep CMD 전송 (PBA 0x0006)
                byte[] sleep_cmd_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SLEEP_CMD).GetPacket(); // Sleep 커맨드
                int pba_delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_Pba_Delay; // 통신 대기 Delay
                Console.WriteLine($"SLEEP CMD RX 수신 대기 [Delay : {pba_delay}ms] [CH{ch + 1}]");

                byte[] sleep_cmd_rx = await Pba.SendAndReceivePacketAsync(sleep_cmd_tx, pba_delay, token);
                if (!UtilityFunctions.CheckEchoAck(sleep_cmd_tx, sleep_cmd_rx))
                {
                    control.Logger.Fail("SLEEP CMD 에러");
                    return false;
                }
                //control.Logger.Pass("SLEEP CMD 적용 완료");

                // 4. CURRENT_SLEEP_SHIP SLEEP 측정 CMD (TCP 0xC4, 0x02)
                byte[] sleep_meas_tx = new TcpProtocol(0xC4, 0x02).GetPacket();
                int sleep_meas_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_02_Delay;
                Console.WriteLine($"SLEEP 측정 RX 수신 대기 [Delay : {sleep_meas_timeout}ms] [CH{ch + 1}]");

                byte[] sleep_meas_rx = await Board.SendAndReceivePacketAsync(sleep_meas_tx, sleep_meas_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(sleep_meas_tx, sleep_meas_rx))
                {
                    control.Logger.Fail("Sleep 측정 데이터 수신 실패");
                    return false;
                }

                byte[] sleep_curr_byte = new byte[] { sleep_meas_rx[7], sleep_meas_rx[8], sleep_meas_rx[9], sleep_meas_rx[10] };
                float sleep_curr = BitConverter.ToSingle(sleep_curr_byte, 0);
                bool isPass_sleep = sleep_curr >= Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min &&
                    sleep_curr <= Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max;

                if (isPass_sleep) control.Logger.Pass($"Sleep Curr : {sleep_curr:F6} mA ({Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min}" +
                    $"~{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max})");
                else
                {
                    control.Logger.Fail($"Sleep Curr : {sleep_curr:F6} mA ({Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Min}" +
                      $"~{Settings.Instance.CURRENT_SLEEP_SHIP_Sleep_Curr_Max})");
                    isPass = false;
                }

                //Ship CMD 전송 (PBA 0x0001)
                byte[] ship_cmd_tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SHIP_CMD).GetPacket(); // Ship 커맨드
                Console.WriteLine($"SHIP CMD RX 수신 대기 [Delay : {pba_delay}ms] [CH{ch + 1}]");
                byte[] ship_cmd_rx = await Pba.SendAndReceivePacketAsync(ship_cmd_tx, pba_delay, token);
                if (!UtilityFunctions.CheckEchoAck(ship_cmd_tx, ship_cmd_rx))
                {
                    control.Logger.Fail("SHIP CMD 에러");
                    return false;
                }
                control.Logger.Pass("SHIP CMD 적용 완료");

                //CURRENT_SLEEP_SHIP SHIP 측정 CMD (TCP 0xC4, 0x03)
                byte[] ship_meas_tx = new TcpProtocol(0xC4, 0x03).GetPacket();
                int ship_meas_timeout = Settings.Instance.Board_Read_Timeout + Settings.Instance.CURRENT_SLEEP_SHIP_TCP_03_Delay;
                Console.WriteLine($"SHIP 측정 RX 수신 대기 [Delay : {ship_meas_timeout}ms] [CH{ch + 1}]");
                byte[] ship_meas_rx = await Board.SendAndReceivePacketAsync(ship_meas_tx, ship_meas_timeout, token);

                if (!UtilityFunctions.CheckTcpRxData(ship_meas_tx, ship_meas_rx))
                {
                    control.Logger.Fail("Ship 측정 데이터 수신 실패");
                    return false;
                }

                byte[] ship_curr_byte = new byte[] { ship_meas_rx[7], ship_meas_rx[8], ship_meas_rx[9], ship_meas_rx[10] };
                float ship_curr = BitConverter.ToSingle(ship_curr_byte, 0); // 단위 uA 여부 확인 필요
                bool isPass_ship = ship_curr >= Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min
                    && ship_curr <= Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max;

                if (isPass_ship)
                {
                    control.Logger.Pass($"Ship Curr : {ship_curr:F6} uA ({Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min}~" +
                        $"{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max})");
                }
                else
                {
                    control.Logger.Fail($"Ship Curr : {ship_curr:F6} uA ({Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Min}~" +
                        $"{Settings.Instance.CURRENT_SLEEP_SHIP_Ship_Curr_Max})");
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
                string errorMsg = $"[{ch + 1}CH] CURRENT 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
        }


        #region 통신 검사
        private static async Task<bool> Test_Pba_Cmd_Check_Start(int ch, ChControl control, CancellationToken token)
        {
            
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.PBA_CMD_CHECK_START_Step_Delay);

                byte[] tx = new TcpProtocol(0xD1, 0x01).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout + Settings.Instance.PBA_CMD_CHECK_START_TCP_01_Delay;
                Console.WriteLine($"PBA CMD CHECK START CMD RX 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);
                if(!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"PBA CMD CHECK START CMD RX 에러");
                    return false;
                }

                //await Task.Delay(Settings.Instance.Pba_On_Delay);
                //Console.WriteLine($"PBA 전원 ON 대기 중... [{Settings.Instance.Pba_On_Delay}ms]");

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }
                //control.Logger.Pass($"PBA connect success [{Return_Pba_Port_Name(ch)}]");

                return true;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] PBA CMD CHECK START 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }

        }

        private static async Task<bool> Test_MOTOR(int ch, ChControl control, CancellationToken token)
        {
            
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.MOTOR_Step_Delay);

                byte[] tx = new TcpProtocol(0xC9, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.MOTOR_TCP_01_Delay;
                Console.WriteLine($"MOTOR START CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay1, token);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"MOTOR START CMD RX 에러");
                    return false;
                }
                //control.Logger.Pass($"MOTOR START CMD 적용 완료");

                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_VIB_TEST_START).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.MOTOR_PBA_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync(tx2, delay2, token);
                if (!UtilityFunctions.CheckEchoAck(tx2, rx2))
                {
                    control.Logger.Fail($"VID Motor Test start cmd rx 에러");
                    return false;
                }

                byte[] tx3 = new TcpProtocol(0xC9, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.MOTOR_TCP_02_Delay;
                Console.WriteLine($"MOTOR END CMD RX 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");

                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"MOTOR END CMD RX 에러");
                    return false;
                }
                short motor_pwm = (short)((rx3[7] << 8) | rx3[8]);
                bool ispass = motor_pwm >= Settings.Instance.MOTOR_PWM_Min && motor_pwm <= Settings.Instance.MOTOR_PWM_Max;

                if(ispass)
                {
                    control.Logger.Pass($"MOTOR PWM : {motor_pwm} [{Settings.Instance.MOTOR_PWM_Min} ~ {Settings.Instance.MOTOR_PWM_Max}]");
                }
                else
                {
                    control.Logger.Fail($"MOTOR PWM : {motor_pwm} [{Settings.Instance.MOTOR_PWM_Min} ~ {Settings.Instance.MOTOR_PWM_Max}]");
                }

                return ispass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] MOTOR 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }

        }

        private static async Task<bool> Test_FLOODS(int ch, ChControl control, CancellationToken token)
        {
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.FLOODS_Step_Delay);

                byte[] tx = new TcpProtocol(0xCA, 0x01).GetPacket();
                int delay1 = Settings.Instance.Board_Read_Timeout + Settings.Instance.FLOODS_TCP_01_Delay;
                Console.WriteLine($"FLOODS START CMD RX 수신 대기 [Delay : {delay1}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay1, token);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    control.Logger.Fail($"FLOODS START CMD RX 에러");
                    return false;
                }

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FLOOD_STATE).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLOODS_PBA_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay2, token);
                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"LDC Flood State Read cmd rx 에러");
                    return false;
                }

                short flood_state = (short)((rx2[0] << 8) | (rx2[1]));
                bool isPass = flood_state == Settings.Instance.FLOODS_STATE;
                if (isPass) control.Logger.Pass($"FLOOD STATE : {flood_state} [{Settings.Instance.FLOODS_STATE}]");
                else control.Logger.Fail($"FLOOD STATE : {flood_state} [{Settings.Instance.FLOODS_STATE}]");

                byte[] tx3 = new TcpProtocol(0xCA, 0x02).GetPacket();
                int delay3 = Settings.Instance.Board_Read_Timeout + Settings.Instance.FLOODS_TCP_02_Delay;
                Console.WriteLine($"FLOODS END CMD RX 수신 대기 [Delay : {delay3}ms] [CH{ch + 1}]");

                byte[] rx3 = await Board.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckTcpRxData(tx3, rx3))
                {
                    control.Logger.Fail($"FLOODS END CMD RX 에러");
                    return false;
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
                string errorMsg = $"[{ch + 1}CH] FLOODS 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
        }

        private static async Task<bool> Test_CARTRIDGE(int ch, ChControl control, CancellationToken token)
        {
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.CARTRIDGE_Step_Delay);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_ON).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.CARTRIDGE_PBA_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"Cartridge Boost on CMD 에러");
                    return false;
                }

                byte[] tx2 = new TcpProtocol(0xCC, 0x01).GetPacket();
                int delay2 = Settings.Instance.Board_Read_Timeout + Settings.Instance.CARTRIDGE_TCP_01_Delay;
                Console.WriteLine($"CARTRIDGE START 수신 대기 [Delay : {delay2}ms] [CH{ch + 1}]");

                byte[] rx2 = await Board.SendAndReceivePacketAsync(tx2, delay2, token);
                if (!UtilityFunctions.CheckTcpRxData(tx2, rx2))
                {
                    control.Logger.Fail($"CARTRIDGE START CMD RX 에러");
                    return false;
                }

                byte[] cartridge_byte = new byte[] { rx2[7], rx2[8], rx2[9], rx2[10] };
                float cartridge_result = BitConverter.ToSingle(cartridge_byte, 0);
                bool isPass = cartridge_result >= Settings.Instance.CARTRIDGE_Load_Switch_Min &&
                    cartridge_result <= Settings.Instance.CARTRIDGE_Load_Switch_Max;

                if (isPass) control.Logger.Pass($"CARTRIDGE : {cartridge_result} [{Settings.Instance.CARTRIDGE_Load_Switch_Min}" +
                    $" ~ {Settings.Instance.CARTRIDGE_Load_Switch_Max}]");
                else control.Logger.Fail($"CARTRIDGE : {cartridge_result} [{Settings.Instance.CARTRIDGE_Load_Switch_Min}" +
                    $" ~ {Settings.Instance.CARTRIDGE_Load_Switch_Max}]");

                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_CARTRIDGE_BOOST_OFF).GetPacket();
                int delay3 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.CARTRIDGE_PBA_Delay;
                byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                {
                    control.Logger.Fail($"Cartridge Boost off CMD 에러");
                    return false;
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
                string errorMsg = $"[{ch + 1}CH] CARTRIDGE 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
        }

        private static async Task<bool> Test_Sub_Heater(int ch, ChControl control, CancellationToken token)
        {
            var Board = CommManager.Boards[ch];
            var Pba = CommManager.Pbas[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                control.Logger.Fail("TCP is not connected!");
                return false;
            }

            try
            {
                await Task.Delay(Settings.Instance.SUB_HEATER_Step_Delay);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_ON).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.SUB_HEATER_PBA_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"SUB HEATER Boost on CMD 에러");
                    return false;
                }

                byte[] tx2 = new TcpProtocol(0xCD, 0x01).GetPacket();
                int delay2 = Settings.Instance.Board_Read_Timeout + Settings.Instance.SUB_HEATER_TCP_01_Delay;
                Console.WriteLine($"SUB HEATER START 수신 대기 [Delay : {delay2}ms] [CH{ch + 1}]");

                byte[] rx2 = await Board.SendAndReceivePacketAsync(tx2, delay2, token);
                if (!UtilityFunctions.CheckTcpRxData(tx2, rx2))
                {
                    control.Logger.Fail($"SUB HEATER START CMD RX 에러");
                    return false;
                }

                byte[] sub_heater_byte = new byte[] { rx2[7], rx2[8], rx2[9], rx2[10] };
                float sub_heater_result = BitConverter.ToSingle(sub_heater_byte, 0);
                bool isPass = sub_heater_result >= Settings.Instance.SUB_HEATER_Load_Switch_Min &&
                    sub_heater_result <= Settings.Instance.SUB_HEATER_Load_Switch_Max;

                if (isPass) control.Logger.Pass($"SUB HEATER : {sub_heater_result} [{Settings.Instance.SUB_HEATER_Load_Switch_Min}" +
                    $" ~ {Settings.Instance.SUB_HEATER_Load_Switch_Max}]");
                else control.Logger.Fail($"SUB HEATER : {sub_heater_result} [{Settings.Instance.SUB_HEATER_Load_Switch_Min}" +
                    $" ~ {Settings.Instance.SUB_HEATER_Load_Switch_Max}]");

                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_SUB_HEATER_BOOST_OFF).GetPacket();
                int delay3 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.SUB_HEATER_PBA_Delay;
                byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                {
                    control.Logger.Fail($"SUB HEATER Boost off CMD 에러");
                    return false;
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
                string errorMsg = $"[{ch + 1}CH] SUB HEATER 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }

        }

        private static async Task<bool> Test_ACCELEROMETER(int ch, ChControl control, CancellationToken token)
        {
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.ACCELEROMETER_Step_Delay);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_ACCEL_IC_TEST_START).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.ACCELEROMETER_PBA_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"ACCEL IC TEST START CMD 에러");
                    return false;
                }

                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_ACCEL_IC_TEST_RESULT).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.ACCELEROMETER_PBA_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay2, token);
                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"ACCEL IC TEST START CMD 에러 : {rx2}");
                    return false;
                }

                short Accel_result = (short)((rx2[0] << 8) | rx2[1]);
                bool isPass = Accel_result == Settings.Instance.ACCELEROMETER_Result;

                if (isPass) control.Logger.Pass($"ACCELEROMETER : {Accel_result} [{Settings.Instance.ACCELEROMETER_Result}]");
                else control.Logger.Fail($"ACCELEROMETER : {Accel_result} [{Settings.Instance.ACCELEROMETER_Result}]");

                return isPass;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] ACCELEROMETER 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }

        }

        private static async Task<bool> Test_GPAK(int ch, ChControl control, CancellationToken token)
        {
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.GPAK_Step_Delay);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                // 1~2: GPAK Test Start (Write) 0050 0001, PBA 응답 대기
                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_START).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.GPAK_Pba_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"GPAK Test Start CMD 에러");
                    return false;
                }

                // 3~4: GPAK Test Result (read) 0026, PBA 응답 대기
                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_GPAK_TEST_RESULT).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.GPAK_Pba_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay2, token);
                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"GPAK Test Result Read 에러");
                    return false;
                }

                // 5: Response Data 확인 → 결과값 표시 및 판단
                short gpakResult = (short)((rx2[0] << 8) | rx2[1]);
                bool isPass = gpakResult == Settings.Instance.GPAK_Result;
                if (isPass)
                    control.Logger.Pass($"GPAK : {gpakResult} [{Settings.Instance.GPAK_Result}]");
                else
                    control.Logger.Fail($"GPAK : {gpakResult} [{Settings.Instance.GPAK_Result}]");
                

                // 6~7: GPAK Test end (Write) 0050 0000, PBA 응답 대기
                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_GPAK_TEST_END).GetPacket();
                int delay3 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.GPAK_Pba_Delay;
                byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                {
                    control.Logger.Fail($"GPAK Test End CMD 에러");
                    return false;
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
                string errorMsg = $"[{ch + 1}CH] GPAK 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
            }
        }

        private static async Task<bool> Test_Flash_Memory(int ch, ChControl control, CancellationToken token)
        {
            var Pba = CommManager.Pbas[ch];

            try
            {
                await Task.Delay(Settings.Instance.FLASH_MEMORY_Step_Delay);

                bool connectOk = await Pba.ConnectAsync(Return_Pba_Port_Name(ch), Return_Pba_Port_Baudrate(ch), Settings.Instance.Pba_Connect_Timeout, token);
                if (!connectOk)
                {
                    control.Logger.Fail($"PBA connect fail [{Return_Pba_Port_Name(ch)}]");
                    return false;
                }

                byte[] tx = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_MCU_FLASH_INTEGRITY_CHECK_START).GetPacket();
                int delay = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLASH_MEMORY_Pba_Delay;
                byte[] rx = await Pba.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckEchoAck(tx, rx))
                {
                    control.Logger.Fail($"MCU Flash Integrity Check Start CMD 에러");
                    return false;
                }
                await Task.Delay(Settings.Instance.FLASH_MEMORY_MCU_FLASH_WAIT);
                control.Logger.Info($"MCU FLASH WAIT 대기...[{Settings.Instance.FLASH_MEMORY_MCU_FLASH_WAIT}ms]");

                byte[] tx2 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                int delay2 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLASH_MEMORY_Pba_Delay;
                byte[] rx2 = await Pba.SendAndReceivePacketAsync_OnlyData(tx2, delay2, token);
                if (rx2 == null || rx2.Length < 2)
                {
                    control.Logger.Fail($"MCU Flash Integrity Result Read 에러");
                    return false;
                }
                short mcuResult = (short)((rx2[0] << 8) | rx2[1]);
                bool mcuPass = mcuResult == Settings.Instance.FLASH_MEMORY_MCU_MEMORY;
                if (mcuPass)
                    control.Logger.Pass($"MCU Memory : {mcuResult} [{Settings.Instance.FLASH_MEMORY_MCU_MEMORY}]");
                else
                    control.Logger.Fail($"MCU Memory : {mcuResult} [{Settings.Instance.FLASH_MEMORY_MCU_MEMORY}]");



                // Ext Flash Integrity Check (7~12)
                byte[] tx3 = new CDCProtocol(Variable.SLAVE, Variable.WRITE, Variable.WRITE_EXT_FLASH_INTEGRITY_CHECK_START).GetPacket();
                int delay3 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLASH_MEMORY_Pba_Delay;
                byte[] rx3 = await Pba.SendAndReceivePacketAsync(tx3, delay3, token);
                if (!UtilityFunctions.CheckEchoAck(tx3, rx3))
                {
                    control.Logger.Fail($"Ext Flash Integrity Check Start CMD 에러");
                    return false;
                }
                await Task.Delay(Settings.Instance.FLASH_MEMORY_MCU_EXT_WAIT);
                control.Logger.Info($"EXT FLASH WAIT 대기...[{Settings.Instance.FLASH_MEMORY_MCU_EXT_WAIT}ms]");

                byte[] tx4 = new CDCProtocol(Variable.SLAVE, Variable.READ, Variable.READ_EXT_FLASH_INTEGRITY_CHECK_RESULT).GetPacket();
                int delay4 = Settings.Instance.Pba_Read_Timeout + Settings.Instance.FLASH_MEMORY_Pba_Delay;
                byte[] rx4 = await Pba.SendAndReceivePacketAsync_OnlyData(tx4, delay4, token);
                if (rx4 == null || rx4.Length < 2)
                {
                    control.Logger.Fail($"Ext Flash Integrity Result Read 에러");
                    return false;
                }
                short extResult = (short)((rx4[0] << 8) | rx4[1]);
                bool extPass = extResult == Settings.Instance.FLASH_MEMORY_EXT_MEMORY;
                if (extPass)
                    control.Logger.Pass($"EXT Memory : {extResult} [{Settings.Instance.FLASH_MEMORY_EXT_MEMORY}]");
                else
                    control.Logger.Fail($"EXT Memory : {extResult} [{Settings.Instance.FLASH_MEMORY_EXT_MEMORY}]");

                bool result = mcuPass && extPass;

                return result;
            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] FLASH MEMORY 예외: {ex.Message}";
                control.Logger.Fail(errorMsg);
                return false;
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

        #endregion
    }
}
