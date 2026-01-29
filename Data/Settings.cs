using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;


namespace p2_40_Main_PBA_Tester.Data
{
    public class Settings : INotifyPropertyChanged
    {
        private readonly object _lock = new object();


        public string Password { get; set; } = "ITM";

        #region MCU LOT MODEL 
        public string Mcu_No { get; set; } = "1";
        public string Lot_No { get; set; } = "1";
        public string Model_No { get; set; } = "P2 4.0";
        public List<string> ModelList { get; set; } = new List<string>() {"P2 4.0", "Model A", "Model B"};
        #endregion

        public bool Use_Write_Log { get; set; }

        public string SavedCurrentDate { get; set; }
        public int LogFileStack { get; set; } = 1;

        public int Board_Connect_Timeout { get; set; } = 3000;

        public int Board_Read_Timeout { get; set; } = 4500;
        public int Pba_Read_Timeout { get; set; } = 4500;

        public bool Use_Maurer { get; set; } = false;
        public string Maurer_Port { get; set; } = "";

        #region CH1
        private bool _use_ch1;
        public bool Use_CH1
        {
            get => _use_ch1;
            set
            {
                if (_use_ch1 == value) return; // 값이 같으면 무시 (무한 루프 방지)
                _use_ch1 = value;
                OnPropertyChanged("Use_CH1"); // UI에 변경 사실을 알림
            }
        }
        public string Board_IP_CH1 { get; set; } = "192.168.0.101";
        public int Board_Port_CH1 { get; set; } = 5000;
        public string Device_Port_CH1 { get; set; } = "None";
        public int Device_BaudRate_CH1 { get; set; } = 115200;
        public string Qr_Port_CH1 { get; set; } = "None";
        public int Qr_BaudRate_CH1 { get; set; } = 9600;





        #endregion

        #region CH2

        private bool _use_ch2;
        public bool Use_CH2
        {
            get => _use_ch2;
            set
            {
                if (_use_ch2 == value) return; // 값이 같으면 무시 (무한 루프 방지)
                _use_ch2 = value;
                OnPropertyChanged("Use_CH2"); // UI에 변경 사실을 알림
            }
        }
        public string Board_IP_CH2 { get; set; } = "192.168.0.102";
        public int Board_Port_CH2 { get; set; } = 5001;
        public string Device_Port_CH2 { get; set; } = "None";
        public int Device_BaudRate_CH2 { get; set; } = 115200;
        public string Qr_Port_CH2 { get; set; } = "None";
        public int Qr_BaudRate_CH2 { get; set; } = 9600;



        #endregion

        #region CH3

        private bool _use_ch3;
        public bool Use_CH3
        {
            get => _use_ch3;
            set
            {
                if (_use_ch3 == value) return; // 값이 같으면 무시 (무한 루프 방지)
                _use_ch3 = value;
                OnPropertyChanged("Use_CH3"); // UI에 변경 사실을 알림
            }
        }
        public string Board_IP_CH3 { get; set; } = "192.168.0.102";
        public int Board_Port_CH3 { get; set; } = 5002;
        public string Device_Port_CH3 { get; set; } = "None";
        public int Device_BaudRate_CH3 { get; set; } = 115200;
        public string Qr_Port_CH3 { get; set; } = "None";
        public int Qr_BaudRate_CH3 { get; set; } = 9600;




        #endregion

        #region CH4
        private bool _use_ch4;
        public bool Use_CH4
        {
            get => _use_ch4;
            set
            {
                if (_use_ch4 == value) return; // 값이 같으면 무시 (무한 루프 방지)
                _use_ch4 = value;
                OnPropertyChanged("Use_CH4"); // UI에 변경 사실을 알림
            }
        }
        public string Board_IP_CH4 { get; set; } = "192.168.0.103";
        public int Board_Port_CH4 { get; set; } = 5003;
        public string Device_Port_CH4 { get; set; } = "None";
        public int Device_BaudRate_CH4 { get; set; } = 115200;
        public string Qr_Port_CH4 { get; set; } = "None";
        public int Qr_BaudRate_CH4 { get; set; } = 9600;

        #endregion

        public string Jig_Port { get; set; } = "None";
        public int Jig_BaudRate { get; set; } = 9600;
        public string Recipe_Qr_Port { get; set; } = "None";
        public int Recipe_Qr_BaudRate { get; set; } = 9600;
        public bool Use_1CH_Only { get; set; } = false;

        public bool Use_TxRx_Console_Board { get; set; } = false;
        public bool Use_TxRx_Console_Pba { get; set; } = false;





        //production count 




        // --- 핵심 데이터 (이 값들이 바뀌면 나머지는 자동 계산됨) ---
        private int _okCount_Ch1; public int OkCount_Ch1 { get => _okCount_Ch1; set { lock (_lock) { _okCount_Ch1 = value; } NotifyUpdates(); } }
        private int _ngCount_Ch1; public int NgCount_Ch1 { get => _ngCount_Ch1; set { lock (_lock) { _ngCount_Ch1 = value; } NotifyUpdates(); } }

        private int _okCount_Ch2; public int OkCount_Ch2 { get => _okCount_Ch2; set { lock (_lock) { _okCount_Ch2 = value; } NotifyUpdates(); } }
        private int _ngCount_Ch2; public int NgCount_Ch2 { get => _ngCount_Ch2; set { lock (_lock) { _ngCount_Ch2 = value; } NotifyUpdates(); } }

        private int _okCount_Ch3; public int OkCount_Ch3 { get => _okCount_Ch3; set { lock (_lock) { _okCount_Ch3 = value; } NotifyUpdates(); } }
        private int _ngCount_Ch3; public int NgCount_Ch3 { get => _ngCount_Ch3; set { lock (_lock) { _ngCount_Ch3 = value; } NotifyUpdates(); } }

        private int _okCount_Ch4; public int OkCount_Ch4 { get => _okCount_Ch4; set { lock (_lock) { _okCount_Ch4 = value; } NotifyUpdates(); } }
        private int _ngCount_Ch4; public int NgCount_Ch4 { get => _ngCount_Ch4; set { lock (_lock) { _ngCount_Ch4 = value; } NotifyUpdates(); } }


        // --- [자동 계산 영역] 채널별 Total ---
        public int TotalCount_Ch1 => OkCount_Ch1 + NgCount_Ch1;
        public int TotalCount_Ch2 => OkCount_Ch2 + NgCount_Ch2;
        public int TotalCount_Ch3 => OkCount_Ch3 + NgCount_Ch3;
        public int TotalCount_Ch4 => OkCount_Ch4 + NgCount_Ch4;


        // --- [자동 계산 영역] 전체 생산 합계 ---
        public int OkCount => OkCount_Ch1 + OkCount_Ch2 + OkCount_Ch3 + OkCount_Ch4;
        public int NgCount => NgCount_Ch1 + NgCount_Ch2 + NgCount_Ch3 + NgCount_Ch4;
        public int TotalCount => OkCount + NgCount;


        // --- [자동 계산 영역] 불량률 및 양품률 ---
        public double OkRate => TotalCount == 0 ? 0 : (double)OkCount / TotalCount;
        public double NgRate => TotalCount == 0 ? 0 : (double)NgCount / TotalCount;


        // --- 핵심 로직: 값 변경 알림 ---
        private void NotifyUpdates()
        {
            // 인자값으로 string.Empty를 보내면 이 클래스에 바인딩된 모든 UI가 갱신됩니다.
            // 개별적으로 OnPropertyChanged를 수십번 호출할 필요가 없어 매우 효율적입니다.
            OnPropertyChanged(string.Empty);
        }

        public void ResetAllCounts()
        {
            _okCount_Ch1 = 0; _ngCount_Ch1 = 0;
            _okCount_Ch2 = 0; _ngCount_Ch2 = 0;
            _okCount_Ch3 = 0; _ngCount_Ch3 = 0;
            _okCount_Ch4 = 0; _ngCount_Ch4 = 0;

            NotifyUpdates(); // 모든 UI를 0으로 갱신
        }




        //판정 값

        #region QR READ (0)
        public bool QR_READ_Enable { get; set; } = true;
        public int QR_READ_Len { get; set; } = 16;
        #endregion 

        #region MCU INFO (1)
        public bool MCU_INFO_Enable { get; set; } = true;
        public int MCU_INFO_Processor_Step_Delay { get; set; } = 200;
        public int MCU_INFO_TCP1_Delay { get; set; } = 1000;
        public int MCU_INFO_Mcu_Id_Len { get; set; } = 16;
        public string MCU_INFO_Main_Fw_Ver { get; set; } = "1.1";
        public string MCU_INFO_LDC_Fw_Ver { get; set; } = "1.1";
        public string MCU_INFO_Image_Fw_Ver { get; set; } = "1.1";

        #endregion

        #region OVP (2)
        public bool OVP_Enable { get; set; } = true;
        public int OVP_Processor_Step_Delay { get; set; } = 10;
        public int OVP_TCP1_Delay { get; set; } = 3000;

        public float OVP_VBUS_Min { get; set; } = 13.7F;
        public float OVP_VBUS_Max { get; set; } = 14.3F;

        #endregion

        #region LDO (3)
        public bool LDO_Enable { get; set; } = true;
        public int LDO_Processor_Step_Delay { get; set; } = 200;
        public int LDO_TCP1_Delay { get; set; } = 1000;

        public float LDO_MCU_3V_Min { get; set; } = 2.950F;
        public float LDO_MCU_3V_Max { get; set; } = 3.050F;
        public float LDO_VDD_3V_Min { get; set; } = 2.950F;
        public float LDO_VDD_3V_Max { get; set; } = 3.050F;
        public float LDO_LCD_3V_Min { get; set; } = 2.950F;
        public float LDO_LCD_3V_Max { get; set; } = 3.050F;
        public float LDO_Vboost_Min { get; set; } = 4.650F;
        public float LDO_Vboost_Max { get; set; } = 4.950F;
        public float LDO_VSYS_Min { get; set; } = 1.111F;
        public float LDO_VSYS_Max { get; set; } = 1.111F;
        public float LDO_SYS_3V3_Min { get; set; } = 1.111F;
        public float LDO_SYS_3V3_Max { get; set; } = 1.111F;
        public float LDO_DC_BOOST_Min { get; set; } = 1.111F;
        public float LDO_DC_BOOST_Max { get; set; } = 1.111F;


        #endregion

        #region CURRENT_SLEEP_SHIP (4)
        public bool CURRENT_SLEEP_SHIP_Enable { get; set; } = true;
        public int CURRENT_SLEEP_SHIP_Step_Delay { get; set; } = 200;
        public int CURRENT_SLEEP_SHIP_TCP1_Delay { get; set; } = 1000;
        public float CURRENT_SLEEP_SHIP_Sleep_Mode_Min { get; set; } = 0.000F;//mAh
        public float CURRENT_SLEEP_SHIP_Sleep_Mode_Max { get; set; } = 1.000F; //mAh
        public float CURRENT_SLEEP_SHIP_Ship_Mode_Min { get; set; } = 20F; //uA
        public float CURRENT_SLEEP_SHIP_Ship_Mode_Max { get; set; } = 50F; //uA
        public float CURRENT_SLEEP_SHIP_HVDCP_Min { get; set; } = 1.111F;
        public float CURRENT_SLEEP_SHIP_HVDCP_Max { get; set; } = 1.111F;
        public float CURRENT_SLEEP_SHIP_PPS_Min { get; set; } = 1.111F;
        public float CURRENT_SLEEP_SHIP_PPS_Max { get; set; } = 1.111F;




        #endregion

        #region CHARGE (5)
        public bool CHARGE_Enable { get; set; } = true;
        public int CHARGE_Processor_Step_Delay { get; set; } = 10;
        public int CHARGE_TCP1_Delay { get; set; } = 5000;
        public float CHARGE_Current_Min { get; set; } = 1.111F;
        public float CHARGE_Current_Max { get; set; } = 1.111F;

        #endregion

        #region GPAK (6)

        public bool GPAK_Enable { get; set; } = true;
        public int GPAK_Processor_Step_Delay { get; set; } = 10;
        public int GPAK_TCP1_Delay { get; set; } = 5000;
        public int GPAK_Switching { get; set; } = 1;
        #endregion

        #region USB CHECK (7)
        public bool USB_CHECK_Enable { get; set; } = true;
        public int USB_CHECK_Delay { get; set; } = 300;
        public int USB_CHECK_Processor_Step_Delay { get; set; } = 10;
        public int USB_CHECK_TCP1_Delay { get; set; } = 5000;
        public int USB_CHECK_TCP2_Delay { get; set; } = 5000;
        public int USB_CHECK_TOP { get; set; } = 3; //float형 일수도 있음
        public int USB_CHECK_BOTTOM { get; set; } = 3; //float형 일수도 있음
        #endregion

        #region FLASH MEMORY (8)
        public bool FLASH_MEMORY_Enable { get; set; } = true;
        public int FLASH_MEMORY_Processor_Step_Delay { get; set; } = 10;
        public int FLASH_MEMORY_TCP1_Delay { get; set; } = 5000;
        public int FLASH_INTEGRITY { get; set; } = 1;

        #endregion

        #region MOTOR (9)
        public bool MOTOR_Enable { get; set; } = true;
        public int MOTOR_PBA_Delay { get; set; } = 100;
        public int MOTOR_Processor_Step { get; set; } = 10;
        public int MOTOR_TCP1_Delay { get; set; } = 1000;
        public int MOTOR_TCP2_Delay { get; set; } = 1000;
        public int MOTOR_PWM_Min { get; set; } = 240;
        public int MOTOR_PWM_Max { get; set; } = 255;
        #endregion

        #region FLOODS (10)
        public bool FLOODS_Enable { get; set; } = true;
        public int FLOODS_PBA_Delay { get; set; } = 100;
        public int FLOODS_Processor_Step_Delay { get; set; } = 10;

        #endregion

        #region HEATER (11)
        public bool HEATER_Enable { get; set; } = true;
        public int HEATER_PBA_Delay { get; set; } = 100;
        public int HEATER_Processor_Step_Delay { get; set; } = 10;
        public int HEATER_PWM_Min { get; set; } = 9200;
        public int HEATER_PWM_Max { get; set; } = 9408;
        public float HEATER_Load_Switch_Min { get; set; } = 4.650F;
        public float HEATER_Load_Switch_Max { get; set; } = 4.950F;

        #endregion

        #region CARTRIDGE (12)
        public bool CARTRIDGE_Enable { get; set; } = true;
        public int CARTRIDGE_PBA_Delay { get; set; } = 100;
        public int CARTRIDGE_Processor_Step_Delay { get; set; } = 10;

        public float CARTRIDGE_Load_Switch_Min { get; set; } = 4.650F;
        public float CARTRIDGE_Load_Switch_Max { get; set; } = 4.950F;

        #endregion

        #region SUB HEATER (13)
        public bool SUB_HEATER_Enable { get; set; } = true;
        public int SUB_HEATER_PBA_Delay { get; set; } = 100;
        public int SUB_HEATER_Processor_Step_Delay { get; set; } = 10;
        public float SUB_HEATER_Load_Switch_Min { get; set; } = 4.650F;
        public float SUB_HEATER_Load_Switch_Max { get; set; } = 4.950F;

        #endregion

        #region ACCELEROMETER (14)
        public bool ACCELEROMETER_Enable { get; set; } = true;
        public int ACCELEROMETER_PBA_Delay { get; set; } = 100;
        public int ACCELEROMETER_Processor_Delay { get; set; } = 100;
        public int ACCELEROMETER_COM_CHECK { get; set; } = 1;
        #endregion

        #region PRESSURE SENSOR (15) 
        public bool PRESSURE_SENSOR_Enable { get; set; } = true;
        public int PRESSURE_SENSOR_PBA_Delay { get; set; } = 100;
        public int PRESSURE_SENSOR_Processor_Delay { get; set; } = 100;
        public int PRESSURE_SENSOR_COM_CHECK { get; set; } = 1;

        #endregion

        #region PBA FLAG (16)
        public bool PBA_FLAG_Enable { get; set; } = true;
        public int PBA_FLAG_PBA_Delay { get; set; } = 100;
        public int PBA_FLAG_Processor_Step { get; set; } = 10;
        public int PBA_FLAG_Test { get; set; } = 0;

        #endregion

        #region PBA CMD CHECK START (17) 
        public bool PBA_CMD_CHECK_START_Enable { get; set; } = true;
        public int PBA_CMD_CHECK_START_Delay { get; set; } = 100;
        public int PBA_CMD_CHECK_START_Processor_Step { get; set; } = 10;
        public int PBA_CMD_CHECK_START_Test { get; set; } = 0;

        #endregion

        #region TEST END (18)
        public bool TEST_END_Enable { get; set; } = true;
        #endregion

        //-------------------------------------



        //DB
        public bool USE_MES { get; set; }
        public string DB_IP { get; set; }
        public string DB_PORT { get; set; }
        public string DB_NAME { get; set; }
        public string DB_TABLE { get; set; }
        public string DB_USER { get; set; }
        public string DB_PW { get; set; }

        //FTP
        public bool USE_FTP { get; set; }
        public string FTP_HOST { get; set; }
        public string FTP_PORT { get; set; }
        public string FTP_USER { get; set; }
        public string FTP_PW { get; set; }
        public string FTP_BASE_DIR { get; set; }
        public bool FTP_USE_SSL { get; set; }



        private string _nowTime = DateTime.Now.ToString("yyyy-MM-dd : HH:mm:ss");
        public string NowTime
        {
            get => _nowTime;
            set
            {
                if (_nowTime != value)
                {
                    _nowTime = value;
                    OnPropertyChanged(nameof(NowTime));
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null) return;

            // 현재 열려있는 폼 중 Invoke가 가능한 아무 폼이나 찾음
            Form target = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.IsHandleCreated);

            if (target != null && target.InvokeRequired)
            {
                target.BeginInvoke(new Action(() => handler.Invoke(this, new PropertyChangedEventArgs(propertyName))));
            }
            else
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //전역 단일 인스턴스
        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }

        private static readonly string filePath = Path.Combine(
            System.Windows.Forms.Application.StartupPath, "config.json");

        


        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(filePath);
                if (dir != null)
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"설정 저장 오류: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public static Settings Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var serializerSettings = new JsonSerializerSettings
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    };
                    return JsonConvert.DeserializeObject<Settings>(json, serializerSettings) ?? new Settings();
                }
                else
                {
                    return new Settings();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"설정 불러오기 오류: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return new Settings();
            }
        }
    }
}
