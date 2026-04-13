using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using p2_40_Main_PBA_Tester.Data;
using p2_40_Main_PBA_Tester.LIB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace p2_40_Main_PBA_Tester.Forms
{
    public partial class RecipeSettingForm : Form
    {
        #region Field
        public MainForm mainform;
        private RecipeLocalBuffer _local = new RecipeLocalBuffer();
        private string currentTask = "";
        private bool AdminMode = false;

        // 메인폼에서 넘겨받을 초기 레시피 경로 저장용 프로퍼티 
        public string InitialRecipePath { get; set; } = string.Empty;
        #endregion


        #region Init
        public RecipeSettingForm(MainForm parentform)
        {
            this.mainform = parentform;
            InitializeComponent();
            AdminMode = false;
        }

        private void RecipeSettingForm_Load(object sender, EventArgs e)
        {
            dgViewTaskList.DataSource = CreateTaskTable();

            // 메인폼에서 넘겨받은 레시피 파일이 있다면 덮어쓰기 로드
            if (!string.IsNullOrWhiteSpace(InitialRecipePath) && File.Exists(InitialRecipePath))
            {
                // 두 번째 인자 true는 "성공 팝업을 띄우지 마라"는 뜻
                LoadRecipeFromJson(InitialRecipePath, true);
            }
        }

        

        private DataTable CreateTaskTable()
        {
            DataTable table = new DataTable();

            DataColumn colNum = table.Columns.Add("Num", typeof(int));
            DataColumn colItem = table.Columns.Add("Item", typeof(string));
            DataColumn colEnable = table.Columns.Add("Enable", typeof(bool));

            colNum.ReadOnly = true;
            colItem.ReadOnly = true;

            table.Rows.Add(1, "QR READ", _local.QR_READ_Enable);
            table.Rows.Add(2, "MCU INFO", _local.MCU_INFO_Enable);
            table.Rows.Add(3, "OVP", _local.OVP_Enable);
            table.Rows.Add(4, "LDO", _local.LDO_Enable);
            table.Rows.Add(5, "CURRENT_SLEEP_SHIP", _local.CURRENT_SLEEP_SHIP_Enable);
            table.Rows.Add(6, "CHARGE", _local.CHARGE_Enable);
            table.Rows.Add(7, "GPAK", _local.GPAK_Enable);
            table.Rows.Add(8, "USB CHECK", _local.USB_CHECK_Enable);
            table.Rows.Add(9, "FLASH MEMORY", _local.FLASH_MEMORY_Enable);
            table.Rows.Add(10, "MOTOR", _local.MOTOR_Enable);
            table.Rows.Add(11, "FLOODS", _local.FLOODS_Enable);
            table.Rows.Add(12, "HEATER", _local.HEATER_Enable);
            table.Rows.Add(13, "CARTRIDGE", _local.CARTRIDGE_Enable);
            table.Rows.Add(14, "SUB HEATER", _local.SUB_HEATER_Enable);
            table.Rows.Add(15, "ACCELEROMETER", _local.ACCELEROMETER_Enable);
            table.Rows.Add(16, "PBA FLAG", _local.PBA_FLAG_Enable);
            table.Rows.Add(17, "PBA CMD CHECK START", _local.PBA_CMD_CHECK_START_Enable);
            table.Rows.Add(18, "FLAG INIT", _local.FLAG_INIT_Enable);
            table.Rows.Add(19, "PBA TEST END", _local.PBA_TEST_END_Enable);
            table.Rows.Add(20, "TEST1", _local.TEST1_Enable);
            table.Rows.Add(21, "TEST2", _local.TEST2_Enable);
            table.Rows.Add(22, "TEST3", _local.TEST3_Enable);

            return table;
        }

        

        #endregion

        #region Event
        private void dgViewTaskList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string selectedTask = dgViewTaskList.Rows[e.RowIndex].Cells["Item"].Value.ToString();
                currentTask = selectedTask;
                dgViewSetValue.DataSource = LoadTaskSettingsToRightTable(selectedTask);
            }
        }
        private DataTable LoadTaskSettingsToRightTable(string task)
        {
            DataTable table = new DataTable();
            DataColumn colParam = table.Columns.Add("Parameter", typeof(string));
            DataColumn colValue = table.Columns.Add("Value", typeof(object));

            colParam.ReadOnly = true;

            switch (task)
            {
                
                   

                case "QR READ":
                    table.Rows.Add("[Delay] Step", _local.QR_READ_Step_Delay);
                    table.Rows.Add("[Spec] 조건 자릿수", _local.QR_READ_Len);
                    break;

                case "MCU INFO":
                    table.Rows.Add("[Delay] Step", _local.MCU_INFO_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.MCU_INFO_Tcp_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.MCU_INFO_Tcp_02_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.MCU_INFO_Booting_01_Delay);

                    table.Rows.Add("[Spec] MCU ID 조건 자릿수", _local.MCU_INFO_Mcu_Id_Len);
                    table.Rows.Add("[Spec] Main FW Ver", _local.MCU_INFO_Main_Fw_Ver);
                    table.Rows.Add("[Spec] LDC FW Ver", _local.MCU_INFO_LDC_Fw_Ver);
                    table.Rows.Add("[Spec] Image FW Ver", _local.MCU_INFO_Image_Fw_Ver);
                    break;

                case "OVP":
                    table.Rows.Add("[Delay] Step", _local.OVP_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.OVP_TCP_01_Delay);
                    table.Rows.Add("[Spec] OVP Min", _local.OVP_OVP_Min);
                    table.Rows.Add("[Spec] OVP Max", _local.OVP_OVP_Max);
                    table.Rows.Add("[Spec] VBUS Min", _local.OVP_VBUS_Min);
                    table.Rows.Add("[Spec] VBUS Max", _local.OVP_VBUS_Max);
                    break;

                case "LDO":
                    table.Rows.Add("[Delay] Step", _local.LDO_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.LDO_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.LDO_TCP_02_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.LDO_Booting_01_Delay);


                    table.Rows.Add("[Spec] VSYS Min", _local.LDO_VSYS_Min);
                    table.Rows.Add("[Spec] VSYS Max", _local.LDO_VSYS_Max);
                    table.Rows.Add("[Spec] VSYS_3V3 Min", _local.LDO_VSYS_3V3_Min);
                    table.Rows.Add("[Spec] VSYS_3V3 Max", _local.LDO_VSYS_3V3_Max);
                    table.Rows.Add("[Spec] MCU_3V0 Min", _local.LDO_MCU_3V0_Min);
                    table.Rows.Add("[Spec] MCU_3V0 Max", _local.LDO_MCU_3V0_Max);
                    table.Rows.Add("[Spec] VDD_3V0 Min", _local.LDO_VDD_3V0_Min);
                    table.Rows.Add("[Spec] VDD_3V0 Max", _local.LDO_VDD_3V0_Max);
                    table.Rows.Add("[Spec] LCD_3V0 Min", _local.LDO_LCD_3V0_Min);
                    table.Rows.Add("[Spec] LCD_3V0 Max", _local.LDO_LCD_3V0_Max);
                    table.Rows.Add("[Spec] DC_BOOST Min", _local.LDO_DC_BOOST_Min);
                    table.Rows.Add("[Spec] DC_BOOST Max", _local.LDO_DC_BOOST_Max);
                    table.Rows.Add("[Spec] VDD_3V0_OFF Min", _local.LDO_VDD_3V0_OFF_Min);
                    table.Rows.Add("[Spec] VDD_3V0_OFF Max", _local.LDO_VDD_3V0_OFF_Max);
                    table.Rows.Add("[Spec] LCD_3V0_OFF Min", _local.LDO_LCD_3V0_OFF_Min);
                    table.Rows.Add("[Spec] LCD_3V0_OFF Max", _local.LDO_LCD_3V0_OFF_Max);
                    table.Rows.Add("[Spec] DC_BOOST_OFF Min", _local.LDO_DC_BOOST_OFF_Min);
                    table.Rows.Add("[Spec] DC_BOOST_OFF Max", _local.LDO_DC_BOOST_OFF_Max);

                    break;

                case "CURRENT_SLEEP_SHIP":
                    table.Rows.Add("[Delay] Step", _local.CURRENT_SLEEP_SHIP_Step_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.CURRENT_SLEEP_SHIP_Booting_01_Delay);
                    table.Rows.Add("[Delay] Booting Delay 02", _local.CURRENT_SLEEP_SHIP_Booting_02_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.CURRENT_SLEEP_SHIP_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.CURRENT_SLEEP_SHIP_TCP_02_Delay);
                    table.Rows.Add("[Delay] TCP 03", _local.CURRENT_SLEEP_SHIP_TCP_03_Delay);
                    table.Rows.Add("[Setting] Retry Count 01", _local.CURRENT_SLEEP_SHIP_Retry_Count_01);


                    table.Rows.Add("[Spec] Sleep Mode Min", _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min);
                    table.Rows.Add("[Spec] Sleep Mode Max", _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max);
                    table.Rows.Add("[Spec] Ship Mode Min", _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min);
                    table.Rows.Add("[Spec] Ship Mode Max", _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max);
                    
                    break;

                case "CHARGE":
                    table.Rows.Add("[Delay] Step", _local.CHARGE_Step_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.CHARGE_Booting_01_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.CHARGE_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.CHARGE_TCP_02_Delay);
                    table.Rows.Add("[Delay] TCP 03", _local.CHARGE_TCP_03_Delay);

                    table.Rows.Add("[Spec] CHARGING HVDCP Min", _local.CHARGE_HVDCP_Min);
                    table.Rows.Add("[Spec] CHARGING HVDCP Max", _local.CHARGE_HVDCP_Max);
                    table.Rows.Add("[Spec] CHARGING PPS Min", _local.CHARGE_PPS_Min);
                    table.Rows.Add("[Spec] CHARGING PPS Max", _local.CHARGE_PPS_Max);

                    break;

                case "GPAK":
                    table.Rows.Add("[Delay] Step", _local.GPAK_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.GPAK_Pba_Delay);
                    table.Rows.Add("[Spec] GPAK", _local.GPAK_Result);
                    break;

                case "USB CHECK":
                    table.Rows.Add("[Delay] Step", _local.USB_CHECK_Step_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.USB_CHECK_Booting_01_Delay);
                    table.Rows.Add("[Delay] Booting Delay 02", _local.USB_CHECK_Booting_02_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.USB_CHECK_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.USB_CHECK_TCP_02_Delay);
                    table.Rows.Add("[Delay] TCP 03", _local.USB_CHECK_TCP_03_Delay);
                    table.Rows.Add("[Spec] USB TOP", _local.USB_CHECK_TOP);
                    table.Rows.Add("[Spec] USB BOTTOM", _local.USB_CHECK_BOTTOM);


                    break;

                case "FLASH MEMORY":
                    table.Rows.Add("[Delay] Step", _local.FLASH_MEMORY_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.FLASH_MEMORY_Pba_Delay);
                    table.Rows.Add("[Spec] MCU Memory", _local.FLASH_MEMORY_MCU_MEMORY);
                    table.Rows.Add("[Spec] EXT Memory", _local.FLASH_MEMORY_EXT_MEMORY);
                    break;

                case "MOTOR":
                    table.Rows.Add("[Delay] Step", _local.MOTOR_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.MOTOR_PBA_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.MOTOR_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.MOTOR_TCP_02_Delay);
                    table.Rows.Add("[Spec] PWM Min", _local.MOTOR_PWM_Min);
                    table.Rows.Add("[Spec] PWM Max", _local.MOTOR_PWM_Max);
                    break;

                case "FLOODS":
                    table.Rows.Add("[Delay] Step", _local.FLOODS_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.FLOODS_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.FLOODS_TCP_02_Delay);
                    table.Rows.Add("[Spec] USB FLOODS", _local.FLOODS_USB_Floods);
                    table.Rows.Add("[Spec] BOARD FLOODS", _local.FLOODS_Board_Floods);

                    break;

                case "HEATER":
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch1", _local.HEATER_Offsets[0]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch2", _local.HEATER_Offsets[1]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch3", _local.HEATER_Offsets[2]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch4", _local.HEATER_Offsets[3]);
                    table.Rows.Add("[Setting] Heater Retry Count", _local.HEATER_Retry_Count);
                    table.Rows.Add("[Delay] Step", _local.HEATER_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.HEATER_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.HEATER_TCP_02_Delay);
                    table.Rows.Add("[Spec] PWM Min", _local.HEATER_PWM_Min);
                    table.Rows.Add("[Spec] PWM Max", _local.HEATER_PWM_Max);
                    table.Rows.Add("[Spec] Sensing Pin Off Min", _local.HEATER_Sensing_Pin_Off_Min);
                    table.Rows.Add("[Spec] Sensing Pin Off Max", _local.HEATER_Sensing_Pin_Off_Max);
                    table.Rows.Add("[Spec] Sensing Pin On Min", _local.HEATER_Sensing_Pin_On_Min);
                    table.Rows.Add("[Spec] Sensing Pin On Max", _local.HEATER_Sensing_Pin_On_Max);

                    break;

                case "CARTRIDGE":
                    table.Rows.Add("[Delay] Step", _local.CARTRIDGE_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.CARTRIDGE_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.CARTRIDGE_TCP_02_Delay);

                    table.Rows.Add("[Spec] CARTRIDGE PWM Min", _local.CARTRIDGE_CARTRIDGE_PWM_Min);
                    table.Rows.Add("[Spec] CARTRIDGE PWM Max", _local.CARTRIDGE_CARTRIDGE_PWM_Max);
                    table.Rows.Add("[Spec] KATO_BOOST Min", _local.CARTRIDGE_KATO_BOOST_Min);
                    table.Rows.Add("[Spec] KATO_BOOST Max", _local.CARTRIDGE_KATO_BOOST_Max);

                    break;

                case "SUB HEATER":
                    table.Rows.Add("[Delay] Step", _local.SUB_HEATER_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.SUB_HEATER_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.SUB_HEATER_TCP_02_Delay);

                    table.Rows.Add("[Spec] SUB HEATER PWM Min", _local.SUB_HEATER_PWM_Min);
                    table.Rows.Add("[Spec] SUB HEATER PWM Max", _local.SUB_HEATER_PWM_Max);
                    table.Rows.Add("[Spec] SUB HEATER BOOST Min", _local.SUB_HEATER_BOOST_Min);
                    table.Rows.Add("[Spec] SUB HEATER BOOST Max", _local.SUB_HEATER_BOOST_Max);

                    break;

                case "ACCELEROMETER":
                    table.Rows.Add("[Delay] Step", _local.ACCELEROMETER_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.ACCELEROMETER_PBA_Delay);
                    table.Rows.Add("[Spec] ACCELEROMETER", _local.ACCELEROMETER_Result);
                    break;

                

                case "PBA FLAG":
                    table.Rows.Add("[Delay] Step", _local.PBA_FLAG_Step_Delay);
                    table.Rows.Add("[Delay] Update", _local.PBA_FLAG_Update_Delay);
                    table.Rows.Add("[Spec] FLAG", _local.PBA_FLAG_FLAG);
                    
                    break;

                case "PBA CMD CHECK START":
                    table.Rows.Add("[Delay] Step", _local.PBA_CMD_CHECK_START_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.PBA_CMD_CHECK_START_TCP_01_Delay);
                    table.Rows.Add("[Delay] Booting Delay 01", _local.PBA_CMD_CHECK_START_Booting_01_Delay);

                    break;

                case "FLAG INIT":
                    table.Rows.Add("[Delay] Step", _local.FLAG_INIT_Step_Delay);
                    table.Rows.Add("[Delay] Update", _local.FLAG_INIT_Update_Delay);
                    break;

                case "PBA TEST END":
                    table.Rows.Add("[Delay] Step", _local.PBA_TEST_END_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.PBA_TEST_END_TCP_01_Delay);

                    break;

                case "TEST1":
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch1", _local.TEST_1_HEATER_Offsets[0]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch2", _local.TEST_1_HEATER_Offsets[1]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch3", _local.TEST_1_HEATER_Offsets[2]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch4", _local.TEST_1_HEATER_Offsets[3]);
                    table.Rows.Add("[Setting] Heater Retry Count", _local.TEST_1_HEATER_Retry_Count);
                    table.Rows.Add("[Delay] Step", _local.TEST_1_HEATER_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.TEST_1_HEATER_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.TEST_1_HEATER_TCP_02_Delay);
                    table.Rows.Add("[Spec] PWM Min", _local.TEST_1_HEATER_PWM_Min);
                    table.Rows.Add("[Spec] PWM Max", _local.TEST_1_HEATER_PWM_Max);
                    table.Rows.Add("[Spec] Sensing Pin Off Min", _local.TEST_1_HEATER_Sensing_Pin_Off_Min);
                    table.Rows.Add("[Spec] Sensing Pin Off Max", _local.TEST_1_HEATER_Sensing_Pin_Off_Max);
                    table.Rows.Add("[Spec] Sensing Pin On Min", _local.TEST_1_HEATER_Sensing_Pin_On_Min);
                    table.Rows.Add("[Spec] Sensing Pin On Max", _local.TEST_1_HEATER_Sensing_Pin_On_Max);
                    break;

                case "TEST2":
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch1", _local.TEST_2_HEATER_Offsets[0]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch2", _local.TEST_2_HEATER_Offsets[1]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch3", _local.TEST_2_HEATER_Offsets[2]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch4", _local.TEST_2_HEATER_Offsets[3]);
                    table.Rows.Add("[Setting] Heater Retry Count", _local.TEST_2_HEATER_Retry_Count);
                    table.Rows.Add("[Delay] Step", _local.TEST_2_HEATER_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.TEST_2_HEATER_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.TEST_2_HEATER_TCP_02_Delay);
                    table.Rows.Add("[Spec] PWM Min", _local.TEST_2_HEATER_PWM_Min);
                    table.Rows.Add("[Spec] PWM Max", _local.TEST_2_HEATER_PWM_Max);
                    table.Rows.Add("[Spec] Sensing Pin Off Min", _local.TEST_2_HEATER_Sensing_Pin_Off_Min);
                    table.Rows.Add("[Spec] Sensing Pin Off Max", _local.TEST_2_HEATER_Sensing_Pin_Off_Max);
                    table.Rows.Add("[Spec] Sensing Pin On Min", _local.TEST_2_HEATER_Sensing_Pin_On_Min);
                    table.Rows.Add("[Spec] Sensing Pin On Max", _local.TEST_2_HEATER_Sensing_Pin_On_Max);
                    break;

                case "TEST3":
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch1", _local.TEST_3_HEATER_Offsets[0]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch2", _local.TEST_3_HEATER_Offsets[1]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch3", _local.TEST_3_HEATER_Offsets[2]);
                    if (AdminMode) table.Rows.Add("[Setting] Heater Offset_Ch4", _local.TEST_3_HEATER_Offsets[3]);
                    table.Rows.Add("[Setting] Heater Retry Count", _local.TEST_3_HEATER_Retry_Count);
                    table.Rows.Add("[Delay] Step", _local.TEST_3_HEATER_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.TEST_3_HEATER_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.TEST_3_HEATER_TCP_02_Delay);
                    table.Rows.Add("[Spec] PWM Min", _local.TEST_3_HEATER_PWM_Min);
                    table.Rows.Add("[Spec] PWM Max", _local.TEST_3_HEATER_PWM_Max);
                    table.Rows.Add("[Spec] Sensing Pin Off Min", _local.TEST_3_HEATER_Sensing_Pin_Off_Min);
                    table.Rows.Add("[Spec] Sensing Pin Off Max", _local.TEST_3_HEATER_Sensing_Pin_Off_Max);
                    table.Rows.Add("[Spec] Sensing Pin On Min", _local.TEST_3_HEATER_Sensing_Pin_On_Min);
                    table.Rows.Add("[Spec] Sensing Pin On Max", _local.TEST_3_HEATER_Sensing_Pin_On_Max);
                    break;


            }

            return table;
        }

        private void dgViewSetValue_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgViewSetValue.Columns[e.ColumnIndex].Name == "Parameter")
            {
                string paramName = e.Value?.ToString();

                if (paramName != null)
                {
                    if (paramName.Contains("Delay") || paramName.Contains("딜레이") || paramName.Contains("Setting"))
                    {
                        dgViewSetValue.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 224, 192); // 주황
                    }
                    else if (paramName.Contains("Spec") || paramName.Contains("판정") || paramName.Contains("판단"))
                    {
                        dgViewSetValue.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(192, 255, 255); // 하늘
                    }
                }
            }
        }

        private void dgViewSetValue_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgViewSetValue.IsCurrentCellDirty)
            {
                dgViewSetValue.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        private void dgViewSetValue_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;

            string param = dgViewSetValue.Rows[e.RowIndex].Cells[0].Value?.ToString();
            string value = dgViewSetValue.Rows[e.RowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrWhiteSpace(param) || string.IsNullOrWhiteSpace(currentTask)) return;

            try
            {
                switch (currentTask)
                {
                    

                    case "QR READ":
                        if (param.Contains("Step")) _local.QR_READ_Step_Delay = int.Parse(value);
                        else if (param.Contains("자릿수")) _local.QR_READ_Len = int.Parse(value);
                        break;

                    case "MCU INFO":
                        if (param.Contains("Step")) _local.MCU_INFO_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.MCU_INFO_Tcp_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.MCU_INFO_Tcp_02_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.MCU_INFO_Booting_01_Delay = int.Parse(value);

                        else if (param.Contains("MCU ID")) _local.MCU_INFO_Mcu_Id_Len = int.Parse(value);
                        else if (param.Contains("Main FW")) _local.MCU_INFO_Main_Fw_Ver = value;
                        else if (param.Contains("LDC FW")) _local.MCU_INFO_LDC_Fw_Ver = value;
                        else if (param.Contains("Image FW")) _local.MCU_INFO_Image_Fw_Ver = value;
                        break;

                    case "OVP":
                        if (param.Contains("Step")) _local.OVP_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.OVP_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("OVP Min")) _local.OVP_OVP_Min = float.Parse(value);
                        else if (param.Contains("OVP Max")) _local.OVP_OVP_Max = float.Parse(value);
                        else if (param.Contains("VBUS Min")) _local.OVP_VBUS_Min = float.Parse(value);
                        else if (param.Contains("VBUS Max")) _local.OVP_VBUS_Max = float.Parse(value);
                        break;

                    case "LDO":
                        if (param.Contains("Step")) _local.LDO_Step_Delay= int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.LDO_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.LDO_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.LDO_Booting_01_Delay = int.Parse(value);


                        else if (param.Contains("VSYS Min")) _local.LDO_VSYS_Min = float.Parse(value);
                        else if (param.Contains("VSYS Max")) _local.LDO_VSYS_Max = float.Parse(value);
                       
                        else if (param.Contains("VSYS_3V3 Min")) _local.LDO_VSYS_3V3_Min = float.Parse(value);
                        else if (param.Contains("VSYS_3V3 Max")) _local.LDO_VSYS_3V3_Max = float.Parse(value);
                        else if (param.Contains("MCU_3V0 Min")) _local.LDO_MCU_3V0_Min = float.Parse(value);
                        else if (param.Contains("MCU_3V0 Max")) _local.LDO_MCU_3V0_Max = float.Parse(value);
                        else if (param.Contains("VDD_3V0 Min")) _local.LDO_VDD_3V0_Min = float.Parse(value);
                        else if (param.Contains("VDD_3V0 Max")) _local.LDO_VDD_3V0_Max = float.Parse(value);
                        else if (param.Contains("LCD_3V0 Min")) _local.LDO_LCD_3V0_Min = float.Parse(value);
                        else if (param.Contains("LCD_3V0 Max")) _local.LDO_LCD_3V0_Max = float.Parse(value);
                        else if (param.Contains("DC_BOOST Min")) _local.LDO_DC_BOOST_Min = float.Parse(value);
                        else if (param.Contains("DC_BOOST Max")) _local.LDO_DC_BOOST_Max = float.Parse(value);
                        else if (param.Contains("VDD_3V0_OFF Min")) _local.LDO_VDD_3V0_OFF_Min = float.Parse(value);
                        else if (param.Contains("VDD_3V0_OFF Max")) _local.LDO_VDD_3V0_OFF_Max = float.Parse(value);
                        else if (param.Contains("LCD_3V0_OFF Min")) _local.LDO_LCD_3V0_OFF_Min = float.Parse(value);
                        else if (param.Contains("LCD_3V0_OFF Max")) _local.LDO_LCD_3V0_OFF_Max = float.Parse(value);
                        else if (param.Contains("DC_BOOST_OFF Min")) _local.LDO_DC_BOOST_OFF_Min = float.Parse(value);
                        else if (param.Contains("DC_BOOST_OFF Max")) _local.LDO_DC_BOOST_OFF_Max = float.Parse(value);
                        break;

                    case "CURRENT_SLEEP_SHIP":
                        if (param.Contains("Step")) _local.CURRENT_SLEEP_SHIP_Step_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.CURRENT_SLEEP_SHIP_Booting_01_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 02")) _local.CURRENT_SLEEP_SHIP_Booting_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.CURRENT_SLEEP_SHIP_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.CURRENT_SLEEP_SHIP_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 03")) _local.CURRENT_SLEEP_SHIP_TCP_03_Delay = int.Parse(value);
                        else if (param.Contains("Retry Count 01")) _local.CURRENT_SLEEP_SHIP_Retry_Count_01 = ushort.Parse(value);
                        else if (param.Contains("Sleep Mode Min")) _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min = float.Parse(value);
                        else if (param.Contains("Sleep Mode Max")) _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max = float.Parse(value);
                        else if (param.Contains("Ship Mode Min")) _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min = float.Parse(value);
                        else if (param.Contains("Ship Mode Max")) _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max = float.Parse(value);
                        break;

                    case "CHARGE":
                        if (param.Contains("Step")) _local.CHARGE_Step_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.CHARGE_Booting_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.CHARGE_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.CHARGE_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 03")) _local.CHARGE_TCP_03_Delay = int.Parse(value);

                        else if (param.Contains("CHARGING HVDCP Min")) _local.CHARGE_HVDCP_Min = float.Parse(value);
                        else if (param.Contains("CHARGING HVDCP Max")) _local.CHARGE_HVDCP_Max = float.Parse(value);
                        else if (param.Contains("CHARGING PPS Min")) _local.CHARGE_PPS_Min = short.Parse(value);
                        else if (param.Contains("CHARGING PPS Max")) _local.CHARGE_PPS_Max = short.Parse(value);
                        break;

                    case "GPAK":
                        if (param.Contains("Step") && !param.Contains("PBA")) _local.GPAK_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.GPAK_Pba_Delay = int.Parse(value);
                        else if (param.Contains("GPAK")) _local.GPAK_Result = short.Parse(value);
                        break;

                    case "USB CHECK":
                        if (param.Contains("Step")) _local.USB_CHECK_Step_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.USB_CHECK_Booting_01_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 02")) _local.USB_CHECK_Booting_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.USB_CHECK_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.USB_CHECK_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 03")) _local.USB_CHECK_TCP_03_Delay = int.Parse(value);

                        else if (param.Contains("USB TOP")) _local.USB_CHECK_TOP = short.Parse(value);
                        else if (param.Contains("USB BOTTOM")) _local.USB_CHECK_BOTTOM = short.Parse(value);
                        break;

                    case "FLASH MEMORY":
                        if (param.Contains("Step")) _local.FLASH_MEMORY_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.FLASH_MEMORY_Pba_Delay = int.Parse(value);
                        else if (param.Contains("MCU Memory")) _local.FLASH_MEMORY_MCU_MEMORY = short.Parse(value);
                        else if (param.Contains("EXT Memory")) _local.FLASH_MEMORY_EXT_MEMORY = short.Parse(value);
                        break;

                    case "MOTOR":
                        if (param.Contains("Step")) _local.MOTOR_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.MOTOR_PBA_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.MOTOR_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.MOTOR_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("PWM Min")) _local.MOTOR_PWM_Min = uint.Parse(value);
                        else if (param.Contains("PWM Max")) _local.MOTOR_PWM_Max = uint.Parse(value);
                        break;

                    case "FLOODS":
                        if (param.Contains("Step")) _local.FLOODS_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.FLOODS_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.FLOODS_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("USB FLOODS")) _local.FLOODS_USB_Floods = short.Parse(value);
                        else if (param.Contains("BOARD FLOODS")) _local.FLOODS_Board_Floods = short.Parse(value);

                        break;

                    case "HEATER":
                        if (param.Contains("Heater Offset_Ch1")) _local.HEATER_Offsets[0] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch2")) _local.HEATER_Offsets[1] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch3")) _local.HEATER_Offsets[2] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch4")) _local.HEATER_Offsets[3] = float.Parse(value);
                        else if (param.Contains("Step")) _local.HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.HEATER_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.HEATER_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("Heater Retry Count")) _local.HEATER_Retry_Count = uint.Parse(value);
                        else if (param.Contains("PWM Min")) _local.HEATER_PWM_Min = uint.Parse(value);
                        else if (param.Contains("PWM Max")) _local.HEATER_PWM_Max = uint.Parse(value);
                        else if (param.Contains("Sensing Pin Off Min")) _local.HEATER_Sensing_Pin_Off_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin Off Max")) _local.HEATER_Sensing_Pin_Off_Max = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Min")) _local.HEATER_Sensing_Pin_On_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Max")) _local.HEATER_Sensing_Pin_On_Max = float.Parse(value);
                        break;

                    case "CARTRIDGE":
                        if (param.Contains("Step")) _local.CARTRIDGE_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.CARTRIDGE_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.CARTRIDGE_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("CARTRIDGE PWM Min")) _local.CARTRIDGE_CARTRIDGE_PWM_Min = uint.Parse(value);
                        else if (param.Contains("CARTRIDGE PWM Max")) _local.CARTRIDGE_CARTRIDGE_PWM_Max = uint.Parse(value);

                        else if (param.Contains("KATO_BOOST Min")) _local.CARTRIDGE_KATO_BOOST_Min = float.Parse(value);
                        else if (param.Contains("KATO_BOOST Max")) _local.CARTRIDGE_KATO_BOOST_Max = float.Parse(value);
                        break;

                    case "SUB HEATER":
                        if (param.Contains("Step")) _local.SUB_HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.SUB_HEATER_TCP_01_Delay= int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.SUB_HEATER_TCP_02_Delay = int.Parse(value);

                        else if (param.Contains("SUB HEATER PWM Min")) _local.SUB_HEATER_PWM_Min = uint.Parse(value);
                        else if (param.Contains("SUB HEATER PWM Max")) _local.SUB_HEATER_PWM_Max = uint.Parse(value);

                        else if (param.Contains("SUB HEATER BOOST Min")) _local.SUB_HEATER_BOOST_Min = float.Parse(value);
                        else if (param.Contains("SUB HEATER BOOST Max")) _local.SUB_HEATER_BOOST_Max = float.Parse(value);

                        break;

                    case "ACCELEROMETER":
                        if (param.Contains("Step")) _local.ACCELEROMETER_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.ACCELEROMETER_PBA_Delay = int.Parse(value);
                        else if (param.Contains("ACCELEROMETER")) _local.ACCELEROMETER_Result = short.Parse(value);
                        break;

                    

                    case "PBA FLAG":
                        if (param.Contains("Step")) _local.PBA_FLAG_Step_Delay = int.Parse(value);
                        else if (param.Contains("Update")) _local.PBA_FLAG_Update_Delay = int.Parse(value);
                        else if (param.Contains("FLAG")) _local.PBA_FLAG_FLAG = short.Parse(value);

                        break;

                    case "PBA CMD CHECK START":
                        if (param.Contains("Step") || param.Equals("[Delay] Step")) _local.PBA_CMD_CHECK_START_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.PBA_CMD_CHECK_START_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("Booting Delay 01")) _local.PBA_CMD_CHECK_START_Booting_01_Delay = int.Parse(value);

                        break;

                    case "FLAG INIT":
                        if (param.Contains("Step") || param.Equals("[Delay] Step")) _local.FLAG_INIT_Step_Delay = int.Parse(value);
                        else if (param.Contains("Update")) _local.FLAG_INIT_Update_Delay = int.Parse(value);
                        break;

                    case "PBA TEST END":
                        if (param.Contains("Step")) _local.PBA_TEST_END_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.PBA_TEST_END_TCP_01_Delay = int.Parse(value);

                        break;

                    case "TEST1":
                        if (param.Contains("Heater Offset_Ch1")) _local.TEST_1_HEATER_Offsets[0] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch2")) _local.TEST_1_HEATER_Offsets[1] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch3")) _local.TEST_1_HEATER_Offsets[2] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch4")) _local.TEST_1_HEATER_Offsets[3] = float.Parse(value);
                        else if (param.Contains("Step")) _local.TEST_1_HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.TEST_1_HEATER_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.TEST_1_HEATER_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("Heater Retry Count")) _local.TEST_1_HEATER_Retry_Count = uint.Parse(value);
                        else if (param.Contains("PWM Min")) _local.TEST_1_HEATER_PWM_Min = uint.Parse(value);
                        else if (param.Contains("PWM Max")) _local.TEST_1_HEATER_PWM_Max = uint.Parse(value);
                        else if (param.Contains("Sensing Pin Off Min")) _local.TEST_1_HEATER_Sensing_Pin_Off_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin Off Max")) _local.TEST_1_HEATER_Sensing_Pin_Off_Max = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Min")) _local.TEST_1_HEATER_Sensing_Pin_On_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Max")) _local.TEST_1_HEATER_Sensing_Pin_On_Max = float.Parse(value);
                        break;

                    case "TEST2":
                        if (param.Contains("Heater Offset_Ch1")) _local.TEST_2_HEATER_Offsets[0] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch2")) _local.TEST_2_HEATER_Offsets[1] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch3")) _local.TEST_2_HEATER_Offsets[2] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch4")) _local.TEST_2_HEATER_Offsets[3] = float.Parse(value);
                        else if (param.Contains("Step")) _local.TEST_2_HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.TEST_2_HEATER_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.TEST_2_HEATER_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("Heater Retry Count")) _local.TEST_2_HEATER_Retry_Count = uint.Parse(value);
                        else if (param.Contains("PWM Min")) _local.TEST_2_HEATER_PWM_Min = uint.Parse(value);
                        else if (param.Contains("PWM Max")) _local.TEST_2_HEATER_PWM_Max = uint.Parse(value);
                        else if (param.Contains("Sensing Pin Off Min")) _local.TEST_2_HEATER_Sensing_Pin_Off_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin Off Max")) _local.TEST_2_HEATER_Sensing_Pin_Off_Max = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Min")) _local.TEST_2_HEATER_Sensing_Pin_On_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Max")) _local.TEST_2_HEATER_Sensing_Pin_On_Max = float.Parse(value);
                        break;

                    case "TEST3":
                        if (param.Contains("Heater Offset_Ch1")) _local.TEST_3_HEATER_Offsets[0] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch2")) _local.TEST_3_HEATER_Offsets[1] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch3")) _local.TEST_3_HEATER_Offsets[2] = float.Parse(value);
                        else if (param.Contains("Heater Offset_Ch4")) _local.TEST_3_HEATER_Offsets[3] = float.Parse(value);
                        else if (param.Contains("Step")) _local.TEST_3_HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.TEST_3_HEATER_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.TEST_3_HEATER_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("Heater Retry Count")) _local.TEST_3_HEATER_Retry_Count = uint.Parse(value);
                        else if (param.Contains("PWM Min")) _local.TEST_3_HEATER_PWM_Min = uint.Parse(value);
                        else if (param.Contains("PWM Max")) _local.TEST_3_HEATER_PWM_Max = uint.Parse(value);
                        else if (param.Contains("Sensing Pin Off Min")) _local.TEST_3_HEATER_Sensing_Pin_Off_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin Off Max")) _local.TEST_3_HEATER_Sensing_Pin_Off_Max = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Min")) _local.TEST_3_HEATER_Sensing_Pin_On_Min = float.Parse(value);
                        else if (param.Contains("Sensing Pin On Max")) _local.TEST_3_HEATER_Sensing_Pin_On_Max = float.Parse(value);
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로컬 변수에 값 저장 실패 {ex.Message}");
            }
        }

        private void dgViewTaskList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgViewTaskList.Columns["Enable"].Index)
            {
                string task = dgViewTaskList.Rows[e.RowIndex].Cells["Item"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(task)) return;

                bool enabled = Convert.ToBoolean(dgViewTaskList.Rows[e.RowIndex].Cells["Enable"].Value);

                switch (task)
                {
                    case "QR READ": _local.QR_READ_Enable = enabled; break;
                    case "MCU INFO": _local.MCU_INFO_Enable = enabled; break;
                    case "OVP": _local.OVP_Enable = enabled; break;
                    case "LDO": _local.LDO_Enable = enabled; break;
                    case "CURRENT_SLEEP_SHIP": _local.CURRENT_SLEEP_SHIP_Enable = enabled; break;
                    case "CHARGE": _local.CHARGE_Enable = enabled; break;
                    case "GPAK": _local.GPAK_Enable = enabled; break;
                    case "USB CHECK": _local.USB_CHECK_Enable = enabled; break;
                    case "FLASH MEMORY": _local.FLASH_MEMORY_Enable = enabled; break;
                    case "MOTOR": _local.MOTOR_Enable = enabled; break;
                    case "FLOODS": _local.FLOODS_Enable = enabled; break;
                    case "HEATER": _local.HEATER_Enable = enabled; break;
                    case "CARTRIDGE": _local.CARTRIDGE_Enable = enabled; break;
                    case "SUB HEATER": _local.SUB_HEATER_Enable = enabled; break;
                    case "ACCELEROMETER": _local.ACCELEROMETER_Enable = enabled; break;
                    case "PBA FLAG": _local.PBA_FLAG_Enable = enabled; break;
                    case "PBA CMD CHECK START": _local.PBA_CMD_CHECK_START_Enable = enabled; break;
                    case "FLAG INIT": _local.FLAG_INIT_Enable = enabled; break;
                    case "PBA TEST END": _local.PBA_TEST_END_Enable = enabled; break;
                    case "TEST1": _local.TEST1_Enable = enabled; break;
                    case "TEST2": _local.TEST2_Enable = enabled; break;
                    case "TEST3": _local.TEST3_Enable = enabled; break;
                }
            }
        }

        private void dgViewTaskList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgViewTaskList.IsCurrentCellDirty)
                dgViewTaskList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void btnAllCheck_Click(object sender, EventArgs e)
        {
            SetAllEnableStatus(true);
        }

        private void btnClearCheck_Click(object sender, EventArgs e)
        {
            SetAllEnableStatus(false);
        }

        private void SetAllEnableStatus(bool isChecked)
        {
            DataTable dt = dgViewTaskList.DataSource as DataTable;
            if (dt == null) return;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Enable"] = isChecked;

                // 중요: 이벤트를 안 타니까, 강제로 로직을 실행시킴
                // e 인자에 null을 넣어도 현재 핸들러 로직상 문제는 없지만, 
                // 안전하게 인덱스를 지정해서 호출할 수 있습니다.
                UpdateLocalVariableFromRow(i, isChecked);
            }
            dgViewTaskList.Refresh();
        }

        private void UpdateLocalVariableFromRow(int rowIndex, bool enabled)
        {
            // 1. 행 인덱스 유효성 검사
            if (rowIndex < 0 || rowIndex >= dgViewTaskList.Rows.Count) return;

            // 2. Task 이름 가져오기
            string task = dgViewTaskList.Rows[rowIndex].Cells["Item"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(task)) return;

            // 3. Task 이름에 따른 _local 버퍼 업데이트
            switch (task)
            {
                case "QR READ": _local.QR_READ_Enable = enabled; break;
                case "MCU INFO": _local.MCU_INFO_Enable = enabled; break;
                case "OVP": _local.OVP_Enable = enabled; break;
                case "LDO": _local.LDO_Enable = enabled; break;
                case "CURRENT_SLEEP_SHIP": _local.CURRENT_SLEEP_SHIP_Enable = enabled; break;
                case "CHARGE": _local.CHARGE_Enable = enabled; break;
                case "GPAK": _local.GPAK_Enable = enabled; break;
                case "USB CHECK": _local.USB_CHECK_Enable = enabled; break;
                case "FLASH MEMORY": _local.FLASH_MEMORY_Enable = enabled; break;
                case "MOTOR": _local.MOTOR_Enable = enabled; break;
                case "FLOODS": _local.FLOODS_Enable = enabled; break;
                case "HEATER": _local.HEATER_Enable = enabled; break;
                case "CARTRIDGE": _local.CARTRIDGE_Enable = enabled; break;
                case "SUB HEATER": _local.SUB_HEATER_Enable = enabled; break;
                case "ACCELEROMETER": _local.ACCELEROMETER_Enable = enabled; break;
                case "PBA FLAG": _local.PBA_FLAG_Enable = enabled; break;
                case "PBA CMD CHECK START": _local.PBA_CMD_CHECK_START_Enable = enabled; break;
                case "FLAG INIT": _local.FLAG_INIT_Enable = enabled; break;
                case "PBA TEST END": _local.PBA_TEST_END_Enable = enabled; break;
                case "TEST1": _local.TEST1_Enable = enabled; break;
                case "TEST2": _local.TEST2_Enable = enabled; break;
                case "TEST3": _local.TEST3_Enable = enabled; break;
                default:
                    // 정의되지 않은 Task인 경우 로그를 남기거나 처리
                    Console.WriteLine($"알 수 없는 Task: {task}");
                    break;
            }
        }

        #endregion

        #region JSON
        //private void SaveRecipeToJson(string filePath)
        //{
        //    List<string> taskOrder = new List<string>();
        //    foreach (DataGridViewRow row in dgViewTaskList.Rows)
        //    {
        //        if (row.IsNewRow) continue;
        //        string task = row.Cells["Item"].Value?.ToString();
        //        if (!string.IsNullOrEmpty(task))
        //            taskOrder.Add(task);
        //    }

        //    JObject settings = new JObject
        //    {
        //        // Enable
        //        ["QR_READ_Enable"] = _local.QR_READ_Enable,
        //        ["MCU_INFO_Enable"] = _local.MCU_INFO_Enable,
        //        ["OVP_Enable"] = _local.OVP_Enable,
        //        ["LDO_Enable"] = _local.LDO_Enable,
        //        ["CURRENT_SLEEP_SHIP_Enable"] = _local.CURRENT_SLEEP_SHIP_Enable,
        //        ["CHARGE_Enable"] = _local.CHARGE_Enable,
        //        ["GPAK_Enable"] = _local.GPAK_Enable,
        //        ["USB_CHECK_Enable"] = _local.USB_CHECK_Enable,
        //        ["FLASH_MEMORY_Enable"] = _local.FLASH_MEMORY_Enable,
        //        ["MOTOR_Enable"] = _local.MOTOR_Enable,
        //        ["FLOODS_Enable"] = _local.FLOODS_Enable,
        //        ["HEATER_Enable"] = _local.HEATER_Enable,
        //        ["CARTRIDGE_Enable"] = _local.CARTRIDGE_Enable,
        //        ["SUB_HEATER_Enable"] = _local.SUB_HEATER_Enable,
        //        ["ACCELEROMETER_Enable"] = _local.ACCELEROMETER_Enable,
        //        ["PBA_FLAG_Enable"] = _local.PBA_FLAG_Enable,
        //        ["PBA_CMD_CHECK_START_Enable"] = _local.PBA_CMD_CHECK_START_Enable,
        //        ["FLAG_INIT_Enable"] = _local.FLAG_INIT_Enable,
        //        ["PBA_TEST_END_Enable"] = _local.PBA_TEST_END_Enable,

        //        // QR READ
        //        ["QR_READ_Step_Delay"] = _local.QR_READ_Step_Delay,
        //        ["QR_READ_Len"] = _local.QR_READ_Len,

        //        // MCU INFO
        //        ["MCU_INFO_Step_Delay"] = _local.MCU_INFO_Step_Delay,
        //        ["MCU_INFO_Tcp_01_Delay"] = _local.MCU_INFO_Tcp_01_Delay,
        //        ["MCU_INFO_Tcp_02_Delay"] = _local.MCU_INFO_Tcp_02_Delay,
        //        ["MCU_INFO_Booting_01_Delay"] = _local.MCU_INFO_Booting_01_Delay,


        //        ["MCU_INFO_Mcu_Id_Len"] = _local.MCU_INFO_Mcu_Id_Len,
        //        ["MCU_INFO_Main_Fw_Ver"] = _local.MCU_INFO_Main_Fw_Ver,
        //        ["MCU_INFO_LDC_Fw_Ver"] = _local.MCU_INFO_LDC_Fw_Ver,
        //        ["MCU_INFO_Image_Fw_Ver"] = _local.MCU_INFO_Image_Fw_Ver,

        //        // OVP
        //        ["OVP_Step_Delay"] = _local.OVP_Step_Delay,
        //        ["OVP_TCP_01_Delay"] = _local.OVP_TCP_01_Delay,
        //        ["OVP_OVP_Min"] = _local.OVP_OVP_Min,
        //        ["OVP_OVP_Max"] = _local.OVP_OVP_Max,
        //        ["OVP_VBUS_Min"] = _local.OVP_VBUS_Min,
        //        ["OVP_VBUS_Max"] = _local.OVP_VBUS_Max,

        //        // LDO
        //        ["LDO_Step_Delay"] = _local.LDO_Step_Delay,
        //        ["LDO_TCP_01_Delay"] = _local.LDO_TCP_01_Delay,
        //        ["LDO_TCP_02_Delay"] = _local.LDO_TCP_02_Delay,
        //        ["LDO_Booting_01_Delay"] = _local.LDO_Booting_01_Delay,

        //        ["LDO_VSYS_Min"] = _local.LDO_VSYS_Min,
        //        ["LDO_VSYS_Max"] = _local.LDO_VSYS_Max,
                
        //        ["LDO_VSYS_3V3_Min"] = _local.LDO_VSYS_3V3_Min,
        //        ["LDO_VSYS_3V3_Max"] = _local.LDO_VSYS_3V3_Max,
        //        ["LDO_MCU_3V0_Min"] = _local.LDO_MCU_3V0_Min,
        //        ["LDO_MCU_3V0_Max"] = _local.LDO_MCU_3V0_Max,
        //        ["LDO_VDD_3V0_Min"] = _local.LDO_VDD_3V0_Min,
        //        ["LDO_VDD_3V0_Max"] = _local.LDO_VDD_3V0_Max,
        //        ["LDO_LCD_3V0_Min"] = _local.LDO_LCD_3V0_Min,
        //        ["LDO_LCD_3V0_Max"] = _local.LDO_LCD_3V0_Max,
        //        ["LDO_DC_BOOST_Min"] = _local.LDO_DC_BOOST_Min,
        //        ["LDO_DC_BOOST_Max"] = _local.LDO_DC_BOOST_Max,
        //        ["LDO_VDD_3V0_OFF_Min"] = _local.LDO_VDD_3V0_OFF_Min,
        //        ["LDO_VDD_3V0_OFF_Max"] = _local.LDO_VDD_3V0_OFF_Max,
        //        ["LDO_LCD_3V0_OFF_Min"] = _local.LDO_LCD_3V0_OFF_Min,
        //        ["LDO_LCD_3V0_OFF_Max"] = _local.LDO_LCD_3V0_OFF_Max,
        //        ["LDO_DC_BOOST_OFF_Min"] = _local.LDO_DC_BOOST_OFF_Min,
        //        ["LDO_DC_BOOST_OFF_Max"] = _local.LDO_DC_BOOST_OFF_Max,

        //        // CURRENT_SLEEP_SHIP
        //        ["CURRENT_SLEEP_SHIP_Step_Delay"] = _local.CURRENT_SLEEP_SHIP_Step_Delay,
        //        ["CURRENT_SLEEP_SHIP_Booting_01_Delay"] = _local.CURRENT_SLEEP_SHIP_Booting_01_Delay,
        //        ["CURRENT_SLEEP_SHIP_Booting_02_Delay"] = _local.CURRENT_SLEEP_SHIP_Booting_02_Delay,
        //        ["CURRENT_SLEEP_SHIP_TCP_01_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_01_Delay,
        //        ["CURRENT_SLEEP_SHIP_TCP_02_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_02_Delay,
        //        ["CURRENT_SLEEP_SHIP_TCP_03_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_03_Delay,
        //        ["CURRENT_SLEEP_SHIP_Retry_Count_01"] = _local.CURRENT_SLEEP_SHIP_Retry_Count_01,
        //        ["CURRENT_SLEEP_SHIP_Sleep_Curr_Min"] = _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min, 
        //        ["CURRENT_SLEEP_SHIP_Sleep_Curr_Max"] = _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max,
        //        ["CURRENT_SLEEP_SHIP_Ship_Curr_Min"] = _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min,
        //        ["CURRENT_SLEEP_SHIP_Ship_Curr_Max"] = _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max,
                
        //        // CHARGE
        //        ["CHARGE_Step_Delay"] = _local.CHARGE_Step_Delay,
        //        ["CHARGE_TCP_01_Delay"] = _local.CHARGE_TCP_01_Delay,
        //        ["CHARGE_TCP_02_Delay"] = _local.CHARGE_TCP_02_Delay,
        //        ["CHARGE_TCP_03_Delay"] = _local.CHARGE_TCP_03_Delay,

        //        ["CHARGE_HVDCP_Min"] = _local.CHARGE_HVDCP_Min,
        //        ["CHARGE_HVDCP_Max"] = _local.CHARGE_HVDCP_Max,
        //        ["CHARGE_PPS_Min"] = _local.CHARGE_PPS_Min,
        //        ["CHARGE_PPS_Max"] = _local.CHARGE_PPS_Max,


        //        // GPAK
        //        ["GPAK_Step_Delay"] = _local.GPAK_Step_Delay,
        //        ["GPAK_Pba_Delay"] = _local.GPAK_Pba_Delay,
        //        ["GPAK_Result"] = _local.GPAK_Result,

        //        // USB CHECK
        //        ["USB_CHECK_Step_Delay"] = _local.USB_CHECK_Step_Delay,
        //        ["USB_CHECK_Booting_01_Delay"] = _local.USB_CHECK_Booting_01_Delay,
        //        ["USB_CHECK_Booting_02_Delay"] = _local.USB_CHECK_Booting_02_Delay,
        //        ["USB_CHECK_TCP_01_Delay"] = _local.USB_CHECK_TCP_01_Delay,
        //        ["USB_CHECK_TCP_02_Delay"] = _local.USB_CHECK_TCP_02_Delay,
        //        ["USB_CHECK_TCP_03_Delay"] = _local.USB_CHECK_TCP_03_Delay,
        //        ["USB_CHECK_TOP"] = _local.USB_CHECK_TOP,
        //        ["USB_CHECK_BOTTOM"] = _local.USB_CHECK_BOTTOM,

        //        // FLASH MEMORY
        //        ["FLASH_MEMORY_Step_Delay"] = _local.FLASH_MEMORY_Step_Delay,
        //        ["FLASH_MEMORY_Pba_Delay"] = _local.FLASH_MEMORY_Pba_Delay,
        //        ["FLASH_MEMORY_MCU_MEMORY"] = _local.FLASH_MEMORY_MCU_MEMORY,
        //        ["FLASH_MEMORY_EXT_MEMORY"] = _local.FLASH_MEMORY_EXT_MEMORY,

         

        //        // MOTOR
        //        ["MOTOR_Step_Delay"] = _local.MOTOR_Step_Delay,
        //        ["MOTOR_PBA_Delay"] = _local.MOTOR_PBA_Delay,
        //        ["MOTOR_TCP_01_Delay"] = _local.MOTOR_TCP_01_Delay,
        //        ["MOTOR_TCP_02_Delay"] = _local.MOTOR_TCP_02_Delay,
        //        ["MOTOR_PWM_Min"] = _local.MOTOR_PWM_Min,
        //        ["MOTOR_PWM_Max"] = _local.MOTOR_PWM_Max,

        //        // FLOODS
        //        ["FLOODS_Step_Delay"] = _local.FLOODS_Step_Delay,
        //        ["FLOODS_TCP_01_Delay"] = _local.FLOODS_TCP_01_Delay,
        //        ["FLOODS_TCP_02_Delay"] = _local.FLOODS_TCP_02_Delay,
        //        ["FLOODS_USB_Floods"] = _local.FLOODS_USB_Floods,
        //        ["FLOODS_Board_Floods"] = _local.FLOODS_Board_Floods,


        //        // HEATER

        //        //["HEATER_Offset"] = _local.HEATER_Offsets[0],
        //        ["HEATER_Step_Delay"] = _local.HEATER_Step_Delay,
        //        ["HEATER_TCP_01_Delay"] = _local.HEATER_TCP_01_Delay,
        //        ["HEATER_TCP_02_Delay"] = _local.HEATER_TCP_02_Delay,
        //        ["HEATER_Retry_Count"] = _local.HEATER_Retry_Count,
        //        ["HEATER_PWM_Min"] = _local.HEATER_PWM_Min,
        //        ["HEATER_PWM_Max"] = _local.HEATER_PWM_Max,
        //        ["HEATER_Sensing_Pin_Off_Min"] = _local.HEATER_Sensing_Pin_Off_Min,
        //        ["HEATER_Sensing_Pin_Off_Max"] = _local.HEATER_Sensing_Pin_Off_Max,
        //        ["HEATER_Sensing_Pin_On_Min"] = _local.HEATER_Sensing_Pin_On_Min,
        //        ["HEATER_Sensing_Pin_On_Max"] = _local.HEATER_Sensing_Pin_On_Max,

        //        // CARTRIDGE
        //        ["CARTRIDGE_Step_Delay"] = _local.CARTRIDGE_Step_Delay,
        //        ["CARTRIDGE_TCP_01_Delay"] = _local.CARTRIDGE_TCP_01_Delay,
        //        ["CARTRIDGE_TCP_02_Delay"] = _local.CARTRIDGE_TCP_02_Delay,

        //        ["CARTRIDGE_CARTRIDGE_PWM_Min"] = _local.CARTRIDGE_CARTRIDGE_PWM_Min,
        //        ["CARTRIDGE_CARTRIDGE_PWM_Max"] = _local.CARTRIDGE_CARTRIDGE_PWM_Max,
        //        ["CARTRIDGE_KATO_BOOST_Min"] = _local.CARTRIDGE_KATO_BOOST_Min,
        //        ["CARTRIDGE_KATO_BOOST_Max"] = _local.CARTRIDGE_KATO_BOOST_Max,

        //        // SUB HEATER
        //        ["SUB_HEATER_Step_Delay"] = _local.SUB_HEATER_Step_Delay,
        //        ["SUB_HEATER_TCP_01_Delay"] = _local.SUB_HEATER_TCP_01_Delay,
        //        ["SUB_HEATER_TCP_02_Delay"] = _local.SUB_HEATER_TCP_02_Delay,

        //        ["SUB_HEATER_PWM_Min"] = _local.SUB_HEATER_PWM_Min,
        //        ["SUB_HEATER_PWM_Max"] = _local.SUB_HEATER_PWM_Max,
        //        ["SUB_HEATER_BOOST_Min"] = _local.SUB_HEATER_BOOST_Min,
        //        ["SUB_HEATER_BOOST_Max"] = _local.SUB_HEATER_BOOST_Max,


        //        // ACCELEROMETER
        //        ["ACCELEROMETER_Step_Delay"] = _local.ACCELEROMETER_Step_Delay,
        //        ["ACCELEROMETER_PBA_Delay"] = _local.ACCELEROMETER_PBA_Delay,
        //        ["ACCELEROMETER_Result"] = _local.ACCELEROMETER_Result,

                

        //        // PBA FLAG
        //        ["PBA_FLAG_Step_Delay"] = _local.PBA_FLAG_Step_Delay,
        //        ["PBA_FLAG_Update_Delay"] = _local.PBA_FLAG_Update_Delay,
        //        ["PBA_FLAG_FLAG"] = _local.PBA_FLAG_FLAG,
                

        //        // PBA CMD CHECK START
        //        ["PBA_CMD_CHECK_START_Step_Delay"] = _local.PBA_CMD_CHECK_START_Step_Delay,
        //        ["PBA_CMD_CHECK_START_TCP_01_Delay"] = _local.PBA_CMD_CHECK_START_TCP_01_Delay,
        //        ["PBA_CMD_CHECK_START_Booting_01_Delay"] = _local.PBA_CMD_CHECK_START_Booting_01_Delay,

        //        // PBA CMD CHECK START
        //        ["FLAG_INIT_Step_Delay"] = _local.FLAG_INIT_Step_Delay,
        //        ["FLAG_INIT_Update_Delay"] = _local.FLAG_INIT_Update_Delay,

        //        // PBA TEST END
        //        ["PBA_TEST_END_Step_Delay"] = _local.PBA_TEST_END_Step_Delay,
        //        ["PBA_TEST_END_TCP_01_Delay"] = _local.PBA_TEST_END_TCP_01_Delay,


        //    };

        //    JObject recipe = new JObject
        //    {
        //        ["Type"] = "Recipe",
        //        ["Settings"] = settings,
        //        ["TaskOrder"] = JArray.FromObject(taskOrder)
        //    };

        //    string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
        //    File.WriteAllText(filePath, json);
        //}

        private void SaveRecipeToJson(string filePath)
        {
            try
            {
                // 1. TaskOrder 리스트 생성 (기존 로직 유지)
                List<string> taskOrder = new List<string>();
                foreach (DataGridViewRow row in dgViewTaskList.Rows)
                {
                    if (row.IsNewRow) continue;
                    string task = row.Cells["Item"].Value?.ToString();
                    if (!string.IsNullOrEmpty(task))
                        taskOrder.Add(task);
                }

                // 2. _local 객체를 단 한 줄로 변환 (배열 포함 모든 필드 자동 매핑)
                JObject settings = JObject.FromObject(_local);

                // 3. 전체 레시피 구조 생성
                JObject recipe = new JObject
                {
                    ["Type"] = "Recipe",
                    ["Settings"] = settings,
                    ["TaskOrder"] = JArray.FromObject(taskOrder)
                };

                // 4. 저장
                string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
                File.WriteAllText(filePath, json);

                MessageBox.Show("레시피 저장 성공!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Recipe Save Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnSaveRecipe_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "레시피 저장";
            saveFileDialog.FileName = "Recipe.json";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveRecipeToJson(saveFileDialog.FileName);
            }
        }

        private void LoadRecipeFromJson(string filePath, bool isSilentMode = false)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject recipe = JObject.Parse(json);

                if ((string)recipe["Type"] != "Recipe")
                {
                    MessageBox.Show("유효하지 않은 레시피 파일입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 누락된 값은 RecipeLocalBuffer의 기본값을 유지하고,
                // 알 수 없는 예전 필드는 자동으로 무시한다.
                _local = recipe["Settings"]?.ToObject<RecipeLocalBuffer>() ?? new RecipeLocalBuffer();

                dgViewTaskList.DataSource = null;
                dgViewSetValue.DataSource = null;

                // TaskOrder 반영
                List<string> taskOrder = recipe["TaskOrder"]?.ToObject<List<string>>() ?? new List<string>();

                DataTable table = CreateTaskTable();
                DataTable reorderedTable = table.Clone();

                foreach (string taskName in taskOrder)
                {
                    DataRow[] found = table.Select($"Item = '{taskName.Replace("'", "''")}'");
                    if (found.Length > 0)
                        reorderedTable.ImportRow(found[0]);
                }

                foreach (DataRow row in table.Rows)
                {
                    string item = row["Item"].ToString();
                    if (!taskOrder.Contains(item))
                        reorderedTable.ImportRow(row);
                }

                dgViewTaskList.DataSource = null;
                dgViewTaskList.DataSource = reorderedTable;

                if (!isSilentMode)
                {
                    MessageBox.Show("레시피 불러오기 성공!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Recipe Load Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadRecipe_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            openFileDialog.Title = "레시피 불러오기";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadRecipeFromJson(openFileDialog.FileName);
            }
        }

        #endregion

        #region Order Move

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            int index = dgViewTaskList.CurrentCell?.RowIndex ?? -1;
            if (index > 0)
                MoveRow(dgViewTaskList, index, index - 1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int index = dgViewTaskList.CurrentCell?.RowIndex ?? -1;
            var table = dgViewTaskList.DataSource as DataTable;
            if (table != null && index < table.Rows.Count - 1)
                MoveRow(dgViewTaskList, index, index + 1);
        }
        private void MoveRow(DataGridView grid, int fromIndex, int toIndex)
        {
            var table = grid.DataSource as DataTable;
            if (table == null || fromIndex < 0 || toIndex < 0 || fromIndex >= table.Rows.Count || toIndex >= table.Rows.Count)
                return;

            // 데이터 복사
            var temp = table.NewRow();
            temp.ItemArray = table.Rows[fromIndex].ItemArray;

            // 삭제 → 재삽입
            table.Rows.RemoveAt(fromIndex);
            table.Rows.InsertAt(temp, toIndex);

            // 선택 줄 갱신
            grid.ClearSelection();
            grid.CurrentCell = grid.Rows[toIndex].Cells[0];
            grid.Rows[toIndex].Selected = true;
        }
        private void btnPwChange_Click(object sender, EventArgs e)
        {
            var form = new ChangePasswordForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblAdminMode_DoubleClick(object sender, EventArgs e)
        {
            if (!RequireLogin(this)) return;

            if (!AdminMode)
            {
                AdminMode = true;
                lblAdminMode.Text = "Admin Mode";
            }
            else
            {
                AdminMode = false;
                lblAdminMode.Text = "";
            }

            if (currentTask == "HEATER")
                dgViewSetValue.DataSource = LoadTaskSettingsToRightTable("HEATER");
            else if (currentTask == "TEST1")
                dgViewSetValue.DataSource = LoadTaskSettingsToRightTable("TEST1");
            else if (currentTask == "TEST2")
                dgViewSetValue.DataSource = LoadTaskSettingsToRightTable("TEST2");
            else if (currentTask == "TEST2")
                dgViewSetValue.DataSource = LoadTaskSettingsToRightTable("TEST3");

        }

        private bool RequireLogin(IWin32Window owner)
        {
            using (var dlg = new LoginForm())
            {
                dlg.StartPosition = FormStartPosition.CenterScreen;
                return dlg.ShowDialog(owner) == DialogResult.OK;
            }
        }

        #endregion

        
    }
}
