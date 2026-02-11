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

        #endregion


        #region Init
        public RecipeSettingForm(MainForm parentform)
        {
            this.mainform = parentform;
            InitializeComponent();
        }

        private void RecipeSettingForm_Load(object sender, EventArgs e)
        {
            dgViewTaskList.DataSource = CreateTaskTable();
        }

        

        private DataTable CreateTaskTable()
        {
            DataTable table = new DataTable();

            DataColumn colNum = table.Columns.Add("Num", typeof(int));
            DataColumn colItem = table.Columns.Add("Item", typeof(string));
            DataColumn colEnable = table.Columns.Add("Enable", typeof(bool));

            colNum.ReadOnly = true;
            colItem.ReadOnly = true;

            table.Rows.Add(1, "INTERLOCK", _local.INTERLOCK_Enable);
            table.Rows.Add(2, "QR READ", _local.QR_READ_Enable);
            table.Rows.Add(3, "MCU INFO", _local.MCU_INFO_Enable);
            table.Rows.Add(4, "OVP", _local.OVP_Enable);
            table.Rows.Add(5, "LDO", _local.LDO_Enable);
            table.Rows.Add(6, "CURRENT_SLEEP_SHIP", _local.CURRENT_SLEEP_SHIP_Enable);
            table.Rows.Add(7, "CHARGE", _local.CHARGE_Enable);
            table.Rows.Add(8, "GPAK", _local.GPAK_Enable);
            table.Rows.Add(9, "USB CHECK", _local.USB_CHECK_Enable);
            table.Rows.Add(10, "FLASH MEMORY", _local.FLASH_MEMORY_Enable);
            table.Rows.Add(11, "MOTOR", _local.MOTOR_Enable);
            table.Rows.Add(12, "FLOODS", _local.FLOODS_Enable);
            table.Rows.Add(13, "HEATER", _local.HEATER_Enable);
            table.Rows.Add(14, "CARTRIDGE", _local.CARTRIDGE_Enable);
            table.Rows.Add(15, "SUB HEATER", _local.SUB_HEATER_Enable);
            table.Rows.Add(16, "ACCELEROMETER", _local.ACCELEROMETER_Enable);
            table.Rows.Add(17, "PRESSURE SENSOR", _local.PRESSURE_SENSOR_Enable);
            table.Rows.Add(18, "PBA FLAG", _local.PBA_FLAG_Enable);
            table.Rows.Add(19, "PBA CMD CHECK START", _local.PBA_CMD_CHECK_START_Enable);
            table.Rows.Add(20, "TEST END", _local.TEST_END_Enable);
            table.Rows.Add(21, "MES", _local.MES_Enable);

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
                case "INTERLOCK":
                    break;

                case "QR READ":
                    table.Rows.Add("[Delay] Step", _local.QR_READ_Step_Delay);
                    table.Rows.Add("[판정] 조건 자릿수", _local.QR_READ_Len);
                    break;

                case "MCU INFO":
                    table.Rows.Add("[Delay] Step", _local.MCU_INFO_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.MCU_INFO_Pba_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.MCU_INFO_Tcp_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.MCU_INFO_Tcp_02_Delay);

                    table.Rows.Add("[판정] MCU ID 조건 자릿수", _local.MCU_INFO_Mcu_Id_Len);
                    table.Rows.Add("[판정] Main FW Ver", _local.MCU_INFO_Main_Fw_Ver);
                    table.Rows.Add("[판정] LDC FW Ver", _local.MCU_INFO_LDC_Fw_Ver);
                    table.Rows.Add("[판정] Image FW Ver", _local.MCU_INFO_Image_Fw_Ver);
                    break;

                case "OVP":
                    table.Rows.Add("[Delay] Step", _local.OVP_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.OVP_TCP_01_Delay);
                    table.Rows.Add("[판정] VBUS Min", _local.OVP_VBUS_Min);
                    table.Rows.Add("[판정] VBUS Max", _local.OVP_VBUS_Max);
                    break;

                case "LDO":
                    table.Rows.Add("[Delay] Step", _local.LDO_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.LDO_Pba_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.LDO_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.LDO_TCP_02_Delay);

                    table.Rows.Add("[판정] VSYS Min", _local.LDO_VSYS_Min);
                    table.Rows.Add("[판정] VSYS Max", _local.LDO_VSYS_Max);
                    table.Rows.Add("[판정] VSYS_3V3 OFF Min", _local.LDO_VSYS_3V3_OFF_Min);
                    table.Rows.Add("[판정] VSYS_3V3 OFF Max", _local.LDO_VSYS_3V3_OFF_Max);
                    table.Rows.Add("[판정] VSYS_3V3 Min", _local.LDO_VSYS_3V3_Min);
                    table.Rows.Add("[판정] VSYS_3V3 Max", _local.LDO_VSYS_3V3_Max);
                    table.Rows.Add("[판정] MCU_3V0 Min", _local.LDO_MCU_3V0_Min);
                    table.Rows.Add("[판정] MCU_3V0 Max", _local.LDO_MCU_3V0_Max);
                    table.Rows.Add("[판정] VDD_3V0 Min", _local.LDO_VDD_3V0_Min);
                    table.Rows.Add("[판정] VDD_3V0 Max", _local.LDO_VDD_3V0_Max);
                    table.Rows.Add("[판정] LCD_3V0 Min", _local.LDO_LCD_3V0_Min);
                    table.Rows.Add("[판정] LCD_3V0 Max", _local.LDO_LCD_3V0_Max);
                    table.Rows.Add("[판정] DC_BOOST Min", _local.LDO_DC_BOOST_Min);
                    table.Rows.Add("[판정] DC_BOOST Max", _local.LDO_DC_BOOST_Max);
                    break;

                case "CURRENT_SLEEP_SHIP":
                    table.Rows.Add("[Delay] Step", _local.CURRENT_SLEEP_SHIP_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.CURRENT_SLEEP_SHIP_Pba_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.CURRENT_SLEEP_SHIP_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.CURRENT_SLEEP_SHIP_TCP_02_Delay);
                    table.Rows.Add("[Delay] TCP 03", _local.CURRENT_SLEEP_SHIP_TCP_03_Delay);

                    table.Rows.Add("[판정] Sleep Mode Min", _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min);
                    table.Rows.Add("[판정] Sleep Mode Max", _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max);
                    table.Rows.Add("[판정] Ship Mode Min", _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min);
                    table.Rows.Add("[판정] Ship Mode Max", _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max);
                    
                    break;

                case "CHARGE":
                    table.Rows.Add("[Delay] Processor Step", _local.CHARGE_Processor_Step_Delay);
                    table.Rows.Add("[Delay] TCP1", _local.CHARGE_TCP1_Delay);
                    table.Rows.Add("[판정] Current Min", _local.CHARGE_Current_Min);
                    table.Rows.Add("[판정] Current Max", _local.CHARGE_Current_Max);
                    break;

                case "GPAK":
                    table.Rows.Add("[Delay] Step", _local.GPAK_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.GPAK_Pba_Delay);
                    table.Rows.Add("[판정] GPAK", _local.GPAK_Result);
                    break;

                case "USB CHECK":
                    table.Rows.Add("Delay", _local.USB_CHECK_Delay);
                    table.Rows.Add("[Delay] Processor Step", _local.USB_CHECK_Processor_Step_Delay);
                    table.Rows.Add("[Delay] TCP1", _local.USB_CHECK_TCP1_Delay);
                    table.Rows.Add("[Delay] TCP2", _local.USB_CHECK_TCP2_Delay);
                    table.Rows.Add("[판정] TOP", _local.USB_CHECK_TOP);
                    table.Rows.Add("[판정] BOTTOM", _local.USB_CHECK_BOTTOM);
                    break;

                case "FLASH MEMORY":
                    table.Rows.Add("[Delay] Step", _local.FLASH_MEMORY_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.FLASH_MEMORY_Pba_Delay);
                    table.Rows.Add("[Delay] MCU FLASH WAIT", _local.FLASH_MEMORY_MCU_FLASH_WAIT);
                    table.Rows.Add("[Delay] EXT FLASH WAIT ", _local.FLASH_MEMORY_MCU_EXT_WAIT);
                    table.Rows.Add("[판정] MCU Memory", _local.FLASH_MEMORY_MCU_MEMORY);
                    table.Rows.Add("[판정] EXT Memory", _local.FLASH_MEMORY_EXT_MEMORY);
                    break;

                case "MOTOR":
                    table.Rows.Add("[Delay] Step", _local.MOTOR_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.MOTOR_PBA_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.MOTOR_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.MOTOR_TCP_02_Delay);
                    table.Rows.Add("[판정] PWM Min", _local.MOTOR_PWM_Min);
                    table.Rows.Add("[판정] PWM Max", _local.MOTOR_PWM_Max);
                    break;

                case "FLOODS":
                    table.Rows.Add("[Delay] Step", _local.FLOODS_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.FLOODS_PBA_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.FLOODS_TCP_01_Delay);
                    table.Rows.Add("[Delay] TCP 02", _local.FLOODS_TCP_02_Delay);
                    table.Rows.Add("[판정] FLOOD STATE", _local.FLOODS_STATE);
                    break;

                case "HEATER":
                    table.Rows.Add("[Delay] PBA Delay", _local.HEATER_PBA_Delay);
                    table.Rows.Add("[Delay] Processor Step", _local.HEATER_Processor_Step_Delay);
                    table.Rows.Add("[판정] PWM Min", _local.HEATER_PWM_Min);
                    table.Rows.Add("[판정] PWM Max", _local.HEATER_PWM_Max);
                    table.Rows.Add("[판정] Load Switch Min", _local.HEATER_Load_Switch_Min);
                    table.Rows.Add("[판정] Load Switch Max", _local.HEATER_Load_Switch_Max);
                    break;

                case "CARTRIDGE":
                    table.Rows.Add("[Delay] Step", _local.CARTRIDGE_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.CARTRIDGE_PBA_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.CARTRIDGE_TCP_01_Delay);
                    table.Rows.Add("[판정] CARTRIDGE Min", _local.CARTRIDGE_Min);
                    table.Rows.Add("[판정] CARTRIDGE Max", _local.CARTRIDGE_Max);
                    break;

                case "SUB HEATER":
                    table.Rows.Add("[Delay] Step", _local.SUB_HEATER_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.SUB_HEATER_PBA_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.SUB_HEATER_TCP_01_Delay);
                    table.Rows.Add("[판정] Load Switch Min", _local.SUB_HEATER_Load_Switch_Min);
                    table.Rows.Add("[판정] Load Switch Max", _local.SUB_HEATER_Load_Switch_Max);
                    break;

                case "ACCELEROMETER":
                    table.Rows.Add("[Delay] Step", _local.ACCELEROMETER_Step_Delay);
                    table.Rows.Add("[Delay] PBA Delay", _local.ACCELEROMETER_PBA_Delay);
                    table.Rows.Add("[판정] ACCELEROMETER", _local.ACCELEROMETER_Result);
                    break;

                case "PRESSURE SENSOR":
                    table.Rows.Add("[Delay] PBA Delay", _local.PRESSURE_SENSOR_PBA_Delay);
                    table.Rows.Add("[Delay] Processor Delay", _local.PRESSURE_SENSOR_Processor_Delay);
                    table.Rows.Add("[판정] COM_CHECK", _local.PRESSURE_SENSOR_COM_CHECK);
                    break;

                case "PBA FLAG":
                    table.Rows.Add("[Delay] PBA Delay", _local.PBA_FLAG_PBA_Delay);
                    table.Rows.Add("[Delay] Processor Step", _local.PBA_FLAG_Processor_Step);
                    table.Rows.Add("[판정] Test", _local.PBA_FLAG_Test);
                    break;

                case "PBA CMD CHECK START":
                    table.Rows.Add("[Delay] Step", _local.PBA_CMD_CHECK_START_Step_Delay);
                    table.Rows.Add("[Delay] TCP 01", _local.PBA_CMD_CHECK_START_TCP_01_Delay);
                    break;

                case "TEST END":
                    break;

                case "MES":
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
                    if (paramName.Contains("Delay") || paramName.Contains("딜레이") || paramName.Contains("설정"))
                    {
                        dgViewSetValue.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 224, 192); // 주황
                    }
                    else if (paramName.Contains("판정") || paramName.Contains("사용") || paramName.Contains("판단"))
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
                    case "INTERLOCK":
                        break;

                    case "QR READ":
                        if (param.Contains("Step")) _local.QR_READ_Step_Delay = int.Parse(value);
                        else if (param.Contains("자릿수")) _local.QR_READ_Len = int.Parse(value);
                        break;

                    case "MCU INFO":
                        if (param.Contains("Step")) _local.MCU_INFO_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.MCU_INFO_Pba_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.MCU_INFO_Tcp_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.MCU_INFO_Tcp_02_Delay = int.Parse(value);
                        else if (param.Contains("MCU ID")) _local.MCU_INFO_Mcu_Id_Len = int.Parse(value);
                        else if (param.Contains("Main FW")) _local.MCU_INFO_Main_Fw_Ver = value;
                        else if (param.Contains("LDC FW")) _local.MCU_INFO_LDC_Fw_Ver = value;
                        else if (param.Contains("Image FW")) _local.MCU_INFO_Image_Fw_Ver = value;
                        break;

                    case "OVP":
                        if (param.Contains("Step")) _local.OVP_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.OVP_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("VBUS Min")) _local.OVP_VBUS_Min = float.Parse(value);
                        else if (param.Contains("VBUS Max")) _local.OVP_VBUS_Max = float.Parse(value);
                        break;

                    case "LDO":
                        if (param.Contains("Step")) _local.LDO_Step_Delay= int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.LDO_Pba_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.LDO_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.LDO_TCP_02_Delay = int.Parse(value);

                        else if (param.Contains("VSYS Min")) _local.LDO_VSYS_Min = float.Parse(value);
                        else if (param.Contains("VSYS Max")) _local.LDO_VSYS_Max = float.Parse(value);
                        else if (param.Contains("VSYS_3V3 OFF Min")) _local.LDO_VSYS_3V3_OFF_Min = float.Parse(value);
                        else if (param.Contains("VSYS_3V3 OFF Max")) _local.LDO_VSYS_3V3_OFF_Max = float.Parse(value);
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
                        break;

                    case "CURRENT_SLEEP_SHIP":
                        if (param.Contains("Step")) _local.CURRENT_SLEEP_SHIP_Step_Delay = int.Parse(value);
                        if (param.Contains("PBA Delay")) _local.CURRENT_SLEEP_SHIP_Pba_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.CURRENT_SLEEP_SHIP_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.CURRENT_SLEEP_SHIP_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("TCP 03")) _local.CURRENT_SLEEP_SHIP_TCP_03_Delay = int.Parse(value);
                        else if (param.Contains("Sleep Mode Min")) _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min = float.Parse(value);
                        else if (param.Contains("Sleep Mode Max")) _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max = float.Parse(value);
                        else if (param.Contains("Ship Mode Min")) _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min = float.Parse(value);
                        else if (param.Contains("Ship Mode Max")) _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max = float.Parse(value);
                        break;

                    case "CHARGE":
                        if (param.Contains("Processor Step")) _local.CHARGE_Processor_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP1")) _local.CHARGE_TCP1_Delay = int.Parse(value);
                        else if (param.Contains("Current Min")) _local.CHARGE_Current_Min = float.Parse(value);
                        else if (param.Contains("Current Max")) _local.CHARGE_Current_Max = float.Parse(value);
                        break;

                    case "GPAK":
                        if (param.Contains("Step") && !param.Contains("PBA")) _local.GPAK_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.GPAK_Pba_Delay = int.Parse(value);
                        else if (param.Contains("GPAK")) _local.GPAK_Result = short.Parse(value);
                        break;

                    case "USB CHECK":
                        if (param.Equals("Delay")) _local.USB_CHECK_Delay = int.Parse(value);
                        else if (param.Contains("Processor Step")) _local.USB_CHECK_Processor_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP1")) _local.USB_CHECK_TCP1_Delay = int.Parse(value);
                        else if (param.Contains("TCP2")) _local.USB_CHECK_TCP2_Delay = int.Parse(value);
                        else if (param.Contains("TOP")) _local.USB_CHECK_TOP = int.Parse(value);
                        else if (param.Contains("BOTTOM")) _local.USB_CHECK_BOTTOM = int.Parse(value);
                        break;

                    case "FLASH MEMORY":
                        if (param.Contains("Step")) _local.FLASH_MEMORY_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.FLASH_MEMORY_Pba_Delay = int.Parse(value);
                        else if (param.Contains("MCU FLASH WAIT")) _local.FLASH_MEMORY_MCU_FLASH_WAIT = int.Parse(value);
                        else if (param.Contains("EXT FLASH WAIT")) _local.FLASH_MEMORY_MCU_EXT_WAIT = int.Parse(value);
                        else if (param.Contains("MCU Memory")) _local.FLASH_MEMORY_MCU_MEMORY = short.Parse(value);
                        else if (param.Contains("EXT Memory")) _local.FLASH_MEMORY_EXT_MEMORY = short.Parse(value);
                        break;

                    case "MOTOR":
                        if (param.Contains("Step")) _local.MOTOR_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.MOTOR_PBA_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.MOTOR_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.MOTOR_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("PWM Min")) _local.MOTOR_PWM_Min = short.Parse(value);
                        else if (param.Contains("PWM Max")) _local.MOTOR_PWM_Max = short.Parse(value);
                        break;

                    case "FLOODS":
                        if (param.Contains("Step")) _local.FLOODS_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.FLOODS_PBA_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.FLOODS_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("TCP 02")) _local.FLOODS_TCP_02_Delay = int.Parse(value);
                        else if (param.Contains("FLOOD STATE")) _local.FLOODS_STATE = short.Parse(value);
                        break;

                    case "HEATER":
                        if (param.Contains("PBA Delay")) _local.HEATER_PBA_Delay = int.Parse(value);
                        else if (param.Contains("Processor Step")) _local.HEATER_Processor_Step_Delay = int.Parse(value);
                        else if (param.Contains("PWM Min")) _local.HEATER_PWM_Min = int.Parse(value);
                        else if (param.Contains("PWM Max")) _local.HEATER_PWM_Max = int.Parse(value);
                        else if (param.Contains("Load Switch Min")) _local.HEATER_Load_Switch_Min = float.Parse(value);
                        else if (param.Contains("Load Switch Max")) _local.HEATER_Load_Switch_Max = float.Parse(value);
                        break;

                    case "CARTRIDGE":
                        if (param.Contains("Step")) _local.CARTRIDGE_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.CARTRIDGE_PBA_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.CARTRIDGE_TCP_01_Delay = int.Parse(value);
                        else if (param.Contains("CARTRIDGE Min")) _local.CARTRIDGE_Min = float.Parse(value);
                        else if (param.Contains("CARTRIDGE Max")) _local.CARTRIDGE_Max = float.Parse(value);
                        break;

                    case "SUB HEATER":
                        if (param.Contains("Step")) _local.SUB_HEATER_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.SUB_HEATER_PBA_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.SUB_HEATER_TCP_01_Delay= int.Parse(value);
                        else if (param.Contains("Load Switch Min")) _local.SUB_HEATER_Load_Switch_Min = float.Parse(value);
                        else if (param.Contains("Load Switch Max")) _local.SUB_HEATER_Load_Switch_Max = float.Parse(value);
                        break;

                    case "ACCELEROMETER":
                        if (param.Contains("Step")) _local.ACCELEROMETER_Step_Delay = int.Parse(value);
                        else if (param.Contains("PBA Delay")) _local.ACCELEROMETER_PBA_Delay = int.Parse(value);
                        else if (param.Contains("ACCELEROMETER")) _local.ACCELEROMETER_Result = short.Parse(value);
                        break;

                    case "PRESSURE SENSOR":
                        if (param.Contains("PBA Delay")) _local.PRESSURE_SENSOR_PBA_Delay = int.Parse(value);
                        else if (param.Contains("Processor Delay")) _local.PRESSURE_SENSOR_Processor_Delay = int.Parse(value);
                        else if (param.Contains("COM_CHECK")) _local.PRESSURE_SENSOR_COM_CHECK = int.Parse(value);
                        break;

                    case "PBA FLAG":
                        if (param.Contains("PBA Delay")) _local.PBA_FLAG_PBA_Delay = int.Parse(value);
                        else if (param.Contains("Processor Step")) _local.PBA_FLAG_Processor_Step = int.Parse(value);
                        else if (param.Contains("Test")) _local.PBA_FLAG_Test = int.Parse(value);
                        break;

                    case "PBA CMD CHECK START":
                        if (param.Contains("Step") || param.Equals("[Delay] Step")) _local.PBA_CMD_CHECK_START_Step_Delay = int.Parse(value);
                        else if (param.Contains("TCP 01")) _local.PBA_CMD_CHECK_START_TCP_01_Delay = int.Parse(value);
                        break;

                    case "TEST END":
                        break;

                    case "MES":
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
                    case "INTERLOCK": _local.INTERLOCK_Enable = enabled; break;
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
                    case "PRESSURE SENSOR": _local.PRESSURE_SENSOR_Enable = enabled; break;
                    case "PBA FLAG": _local.PBA_FLAG_Enable = enabled; break;
                    case "PBA CMD CHECK START": _local.PBA_CMD_CHECK_START_Enable = enabled; break;
                    case "TEST END": _local.TEST_END_Enable = enabled; break;
                    case "MES": _local.MES_Enable = enabled; break; 
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
                case "INTERLOCK": _local.INTERLOCK_Enable = enabled; break;
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
                case "PRESSURE SENSOR": _local.PRESSURE_SENSOR_Enable = enabled; break;
                case "PBA FLAG": _local.PBA_FLAG_Enable = enabled; break;
                case "PBA CMD CHECK START": _local.PBA_CMD_CHECK_START_Enable = enabled; break;
                case "TEST END": _local.TEST_END_Enable = enabled; break;
                case "MES": _local.MES_Enable = enabled; break;
                default:
                    // 정의되지 않은 Task인 경우 로그를 남기거나 처리
                    Console.WriteLine($"알 수 없는 Task: {task}");
                    break;
            }
        }

        #endregion

        #region JSON
        private void SaveRecipeToJson(string filePath)
        {
            List<string> taskOrder = new List<string>();
            foreach (DataGridViewRow row in dgViewTaskList.Rows)
            {
                if (row.IsNewRow) continue;
                string task = row.Cells["Item"].Value?.ToString();
                if (!string.IsNullOrEmpty(task))
                    taskOrder.Add(task);
            }

            JObject settings = new JObject
            {
                // Enable
                ["INTERLOCK_Enable"] = _local.INTERLOCK_Enable,
                ["QR_READ_Enable"] = _local.QR_READ_Enable,
                ["MCU_INFO_Enable"] = _local.MCU_INFO_Enable,
                ["OVP_Enable"] = _local.OVP_Enable,
                ["LDO_Enable"] = _local.LDO_Enable,
                ["CURRENT_SLEEP_SHIP_Enable"] = _local.CURRENT_SLEEP_SHIP_Enable,
                ["CHARGE_Enable"] = _local.CHARGE_Enable,
                ["GPAK_Enable"] = _local.GPAK_Enable,
                ["USB_CHECK_Enable"] = _local.USB_CHECK_Enable,
                ["FLASH_MEMORY_Enable"] = _local.FLASH_MEMORY_Enable,
                ["MOTOR_Enable"] = _local.MOTOR_Enable,
                ["FLOODS_Enable"] = _local.FLOODS_Enable,
                ["HEATER_Enable"] = _local.HEATER_Enable,
                ["CARTRIDGE_Enable"] = _local.CARTRIDGE_Enable,
                ["SUB_HEATER_Enable"] = _local.SUB_HEATER_Enable,
                ["ACCELEROMETER_Enable"] = _local.ACCELEROMETER_Enable,
                ["PRESSURE_SENSOR_Enable"] = _local.PRESSURE_SENSOR_Enable,
                ["PBA_FLAG_Enable"] = _local.PBA_FLAG_Enable,
                ["PBA_CMD_CHECK_START_Enable"] = _local.PBA_CMD_CHECK_START_Enable,
                ["TEST_END_Enable"] = _local.TEST_END_Enable,
                ["MES_Enable"] = _local.MES_Enable,

                // QR READ
                ["QR_READ_Step_Delay"] = _local.QR_READ_Step_Delay,
                ["QR_READ_Len"] = _local.QR_READ_Len,

                // MCU INFO
                ["MCU_INFO_Step_Delay"] = _local.MCU_INFO_Step_Delay,
                ["MCU_INFO_Pba_Delay"] = _local.MCU_INFO_Pba_Delay,
                ["MCU_INFO_Tcp_01_Delay"] = _local.MCU_INFO_Tcp_01_Delay,
                ["MCU_INFO_Tcp_02_Delay"] = _local.MCU_INFO_Tcp_02_Delay,

                ["MCU_INFO_Mcu_Id_Len"] = _local.MCU_INFO_Mcu_Id_Len,
                ["MCU_INFO_Main_Fw_Ver"] = _local.MCU_INFO_Main_Fw_Ver,
                ["MCU_INFO_LDC_Fw_Ver"] = _local.MCU_INFO_LDC_Fw_Ver,
                ["MCU_INFO_Image_Fw_Ver"] = _local.MCU_INFO_Image_Fw_Ver,

                // OVP
                ["OVP_Step_Delay"] = _local.OVP_Step_Delay,
                ["OVP_TCP_01_Delay"] = _local.OVP_TCP_01_Delay,
                ["OVP_VBUS_Min"] = _local.OVP_VBUS_Min,
                ["OVP_VBUS_Max"] = _local.OVP_VBUS_Max,

                // LDO
                ["LDO_Step_Delay"] = _local.LDO_Step_Delay,
                ["LDO_Pba_Delay"] = _local.LDO_Pba_Delay,
                ["LDO_TCP_01_Delay"] = _local.LDO_TCP_01_Delay,
                ["LDO_TCP_02_Delay"] = _local.LDO_TCP_02_Delay,
                ["LDO_VSYS_Min"] = _local.LDO_VSYS_Min,
                ["LDO_VSYS_Max"] = _local.LDO_VSYS_Max,
                ["LDO_VSYS_3V3_OFF_Min"] = _local.LDO_VSYS_3V3_OFF_Min,
                ["LDO_VSYS_3V3_OFF_Max"] = _local.LDO_VSYS_3V3_OFF_Max,
                ["LDO_VSYS_3V3_Min"] = _local.LDO_VSYS_3V3_Min,
                ["LDO_VSYS_3V3_Max"] = _local.LDO_VSYS_3V3_Max,
                ["LDO_MCU_3V0_Min"] = _local.LDO_MCU_3V0_Min,
                ["LDO_MCU_3V0_Max"] = _local.LDO_MCU_3V0_Max,
                ["LDO_VDD_3V0_Min"] = _local.LDO_VDD_3V0_Min,
                ["LDO_VDD_3V0_Max"] = _local.LDO_VDD_3V0_Max,
                ["LDO_LCD_3V0_Min"] = _local.LDO_LCD_3V0_Min,
                ["LDO_LCD_3V0_Max"] = _local.LDO_LCD_3V0_Max,
                ["LDO_DC_BOOST_Min"] = _local.LDO_DC_BOOST_Min,
                ["LDO_DC_BOOST_Max"] = _local.LDO_DC_BOOST_Max,

                // CURRENT_SLEEP_SHIP
                ["CURRENT_SLEEP_SHIP_Step_Delay"] = _local.CURRENT_SLEEP_SHIP_Step_Delay,
                ["CURRENT_SLEEP_SHIP_Pba_Delay"] = _local.CURRENT_SLEEP_SHIP_Pba_Delay,
                ["CURRENT_SLEEP_SHIP_TCP_01_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_01_Delay,
                ["CURRENT_SLEEP_SHIP_TCP_02_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_02_Delay,
                ["CURRENT_SLEEP_SHIP_TCP_03_Delay"] = _local.CURRENT_SLEEP_SHIP_TCP_03_Delay,

                ["CURRENT_SLEEP_SHIP_Sleep_Curr_Min"] = _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min, 
                ["CURRENT_SLEEP_SHIP_Sleep_Curr_Max"] = _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max,
                ["CURRENT_SLEEP_SHIP_Ship_Curr_Min"] = _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min,
                ["CURRENT_SLEEP_SHIP_Ship_Curr_Max"] = _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max,
                
                // CHARGE
                ["CHARGE_Processor_Step_Delay"] = _local.CHARGE_Processor_Step_Delay,
                ["CHARGE_TCP1_Delay"] = _local.CHARGE_TCP1_Delay,
                ["CHARGE_Current_Min"] = _local.CHARGE_Current_Min,
                ["CHARGE_Current_Max"] = _local.CHARGE_Current_Max,

                // GPAK
                ["GPAK_Step_Delay"] = _local.GPAK_Step_Delay,
                ["GPAK_Pba_Delay"] = _local.GPAK_Pba_Delay,
                ["GPAK_Result"] = _local.GPAK_Result,

                // USB CHECK
                ["USB_CHECK_Delay"] = _local.USB_CHECK_Delay,
                ["USB_CHECK_Processor_Step_Delay"] = _local.USB_CHECK_Processor_Step_Delay,
                ["USB_CHECK_TCP1_Delay"] = _local.USB_CHECK_TCP1_Delay,
                ["USB_CHECK_TCP2_Delay"] = _local.USB_CHECK_TCP2_Delay,
                ["USB_CHECK_TOP"] = _local.USB_CHECK_TOP,
                ["USB_CHECK_BOTTOM"] = _local.USB_CHECK_BOTTOM,

                // FLASH MEMORY
                ["FLASH_MEMORY_Step_Delay"] = _local.FLASH_MEMORY_Step_Delay,
                ["FLASH_MEMORY_Pba_Delay"] = _local.FLASH_MEMORY_Pba_Delay,
                ["FLASH_MEMORY_MCU_FLASH_WAIT"] = _local.FLASH_MEMORY_MCU_FLASH_WAIT,
                ["FLASH_MEMORY_MCU_EXT_WAIT"] = _local.FLASH_MEMORY_MCU_EXT_WAIT,
                ["FLASH_MEMORY_MCU_MEMORY"] = _local.FLASH_MEMORY_MCU_MEMORY,
                ["FLASH_MEMORY_EXT_MEMORY"] = _local.FLASH_MEMORY_EXT_MEMORY,

         

                // MOTOR
                ["MOTOR_Step_Delay"] = _local.MOTOR_Step_Delay,
                ["MOTOR_PBA_Delay"] = _local.MOTOR_PBA_Delay,
                ["MOTOR_TCP_01_Delay"] = _local.MOTOR_TCP_01_Delay,
                ["MOTOR_TCP_02_Delay"] = _local.MOTOR_TCP_02_Delay,
                ["MOTOR_PWM_Min"] = _local.MOTOR_PWM_Min,
                ["MOTOR_PWM_Max"] = _local.MOTOR_PWM_Max,

                // FLOODS
                ["FLOODS_Step_Delay"] = _local.FLOODS_Step_Delay,
                ["FLOODS_PBA_Delay"] = _local.FLOODS_PBA_Delay,
                ["FLOODS_TCP_01_Delay"] = _local.FLOODS_TCP_01_Delay,
                ["FLOODS_TCP_02_Delay"] = _local.FLOODS_TCP_02_Delay,
                ["FLOODS_STATE"] = _local.FLOODS_STATE,


                // HEATER
                ["HEATER_PBA_Delay"] = _local.HEATER_PBA_Delay,
                ["HEATER_Processor_Step_Delay"] = _local.HEATER_Processor_Step_Delay,
                ["HEATER_PWM_Min"] = _local.HEATER_PWM_Min,
                ["HEATER_PWM_Max"] = _local.HEATER_PWM_Max,
                ["HEATER_Load_Switch_Min"] = _local.HEATER_Load_Switch_Min,
                ["HEATER_Load_Switch_Max"] = _local.HEATER_Load_Switch_Max,

                // CARTRIDGE
                ["CARTRIDGE_Step_Delay"] = _local.CARTRIDGE_Step_Delay,
                ["CARTRIDGE_PBA_Delay"] = _local.CARTRIDGE_PBA_Delay,
                ["CARTRIDGE_TCP_01_Delay"] = _local.CARTRIDGE_TCP_01_Delay,
                ["CARTRIDGE_Min"] = _local.CARTRIDGE_Min,
                ["CARTRIDGE_Max"] = _local.CARTRIDGE_Max,

                // SUB HEATER
                ["SUB_HEATER_Step_Delay"] = _local.SUB_HEATER_Step_Delay,
                ["SUB_HEATER_PBA_Delay"] = _local.SUB_HEATER_PBA_Delay,
                ["SUB_HEATER_TCP_01_Delay"] = _local.SUB_HEATER_TCP_01_Delay,
                ["SUB_HEATER_Load_Switch_Min"] = _local.SUB_HEATER_Load_Switch_Min,
                ["SUB_HEATER_Load_Switch_Max"] = _local.SUB_HEATER_Load_Switch_Max,

                // ACCELEROMETER
                ["ACCELEROMETER_Step_Delay"] = _local.ACCELEROMETER_Step_Delay,
                ["ACCELEROMETER_PBA_Delay"] = _local.ACCELEROMETER_PBA_Delay,
                ["ACCELEROMETER_Result"] = _local.ACCELEROMETER_Result,

                // PRESSURE SENSOR
                ["PRESSURE_SENSOR_PBA_Delay"] = _local.PRESSURE_SENSOR_PBA_Delay,
                ["PRESSURE_SENSOR_Processor_Delay"] = _local.PRESSURE_SENSOR_Processor_Delay,
                ["PRESSURE_SENSOR_COM_CHECK"] = _local.PRESSURE_SENSOR_COM_CHECK,

                // PBA FLAG
                ["PBA_FLAG_PBA_Delay"] = _local.PBA_FLAG_PBA_Delay,
                ["PBA_FLAG_Processor_Step"] = _local.PBA_FLAG_Processor_Step,
                ["PBA_FLAG_Test"] = _local.PBA_FLAG_Test,

                // PBA CMD CHECK START
                ["PBA_CMD_CHECK_START_Step_Delay"] = _local.PBA_CMD_CHECK_START_Step_Delay,
                ["PBA_CMD_CHECK_START_TCP_01_Delay"] = _local.PBA_CMD_CHECK_START_TCP_01_Delay,
            };

            JObject recipe = new JObject
            {
                ["Type"] = "Recipe",
                ["Settings"] = settings,
                ["TaskOrder"] = JArray.FromObject(taskOrder)
            };

            string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
            File.WriteAllText(filePath, json);
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

        private void LoadRecipeFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                dynamic recipe = JsonConvert.DeserializeObject(json);

                if (recipe.Type == null || recipe.Type.ToString() != "Recipe")
                {
                    MessageBox.Show("유효하지 않은 레시피 파일입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dynamic settings = recipe.Settings;

                // Enable
                _local.INTERLOCK_Enable = settings.INTERLOCK_Enable;
                _local.QR_READ_Enable = settings.QR_READ_Enable;
                _local.MCU_INFO_Enable = settings.MCU_INFO_Enable;
                _local.OVP_Enable = settings.OVP_Enable;
                _local.LDO_Enable = settings.LDO_Enable;
                _local.CURRENT_SLEEP_SHIP_Enable = settings.CURRENT_SLEEP_SHIP_Enable;
                _local.CHARGE_Enable = settings.CHARGE_Enable;
                _local.GPAK_Enable = settings.GPAK_Enable;
                _local.USB_CHECK_Enable = settings.USB_CHECK_Enable;
                _local.FLASH_MEMORY_Enable = settings.FLASH_MEMORY_Enable;
                _local.MOTOR_Enable = settings.MOTOR_Enable;
                _local.FLOODS_Enable = settings.FLOODS_Enable;
                _local.HEATER_Enable = settings.HEATER_Enable;
                _local.CARTRIDGE_Enable = settings.CARTRIDGE_Enable;
                _local.SUB_HEATER_Enable = settings.SUB_HEATER_Enable;
                _local.ACCELEROMETER_Enable = settings.ACCELEROMETER_Enable;
                _local.PRESSURE_SENSOR_Enable = settings.PRESSURE_SENSOR_Enable;
                _local.PBA_FLAG_Enable = settings.PBA_FLAG_Enable;
                _local.PBA_CMD_CHECK_START_Enable = settings.PBA_CMD_CHECK_START_Enable;
                _local.TEST_END_Enable = settings.TEST_END_Enable;
                _local.MES_Enable = settings.MES_Enable;

                // Values
                _local.QR_READ_Step_Delay = settings.QR_READ_Step_Delay;
                _local.QR_READ_Len = settings.QR_READ_Len;
                _local.MCU_INFO_Step_Delay = settings.MCU_INFO_Step_Delay;
                _local.MCU_INFO_Pba_Delay = settings.MCU_INFO_Pba_Delay;
                _local.MCU_INFO_Tcp_01_Delay = settings.MCU_INFO_Tcp_01_Delay;
                _local.MCU_INFO_Tcp_02_Delay = settings.MCU_INFO_Tcp_02_Delay;

                _local.MCU_INFO_Mcu_Id_Len = settings.MCU_INFO_Mcu_Id_Len;
                _local.MCU_INFO_Main_Fw_Ver = settings.MCU_INFO_Main_Fw_Ver;
                _local.MCU_INFO_LDC_Fw_Ver = settings.MCU_INFO_LDC_Fw_Ver;
                _local.MCU_INFO_Image_Fw_Ver = settings.MCU_INFO_Image_Fw_Ver;

                _local.OVP_Step_Delay = settings.OVP_Step_Delay;
                _local.OVP_TCP_01_Delay = settings.OVP_TCP_01_Delay;
                _local.OVP_VBUS_Min = settings.OVP_VBUS_Min;
                _local.OVP_VBUS_Max = settings.OVP_VBUS_Max;

                _local.LDO_Step_Delay = settings.LDO_Step_Delay;
                _local.LDO_Pba_Delay = settings.LDO_Pba_Delay;
                _local.LDO_TCP_01_Delay = settings.LDO_TCP_01_Delay;
                _local.LDO_TCP_02_Delay = settings.LDO_TCP_02_Delay;

                _local.LDO_VSYS_Min = settings.LDO_VSYS_Min;
                _local.LDO_VSYS_Max = settings.LDO_VSYS_Max;
                _local.LDO_VSYS_3V3_OFF_Min = settings.LDO_VSYS_3V3_OFF_Min;
                _local.LDO_VSYS_3V3_OFF_Max = settings.LDO_VSYS_3V3_OFF_Max;
                _local.LDO_VSYS_3V3_Min = settings.LDO_VSYS_3V3_Min;
                _local.LDO_VSYS_3V3_Max = settings.LDO_VSYS_3V3_Max;
                _local.LDO_MCU_3V0_Min = settings.LDO_MCU_3V0_Min;
                _local.LDO_MCU_3V0_Max = settings.LDO_MCU_3V0_Max;
                _local.LDO_VDD_3V0_Min = settings.LDO_VDD_3V0_Min;
                _local.LDO_VDD_3V0_Max = settings.LDO_VDD_3V0_Max;
                _local.LDO_LCD_3V0_Min = settings.LDO_LCD_3V0_Min;
                _local.LDO_LCD_3V0_Max = settings.LDO_LCD_3V0_Max;
                _local.LDO_DC_BOOST_Min = settings.LDO_DC_BOOST_Min;
                _local.LDO_DC_BOOST_Max = settings.LDO_DC_BOOST_Max;

                _local.CURRENT_SLEEP_SHIP_Step_Delay = settings.CURRENT_SLEEP_SHIP_Step_Delay;
                _local.CURRENT_SLEEP_SHIP_Pba_Delay = settings.CURRENT_SLEEP_SHIP_Pba_Delay;
                _local.CURRENT_SLEEP_SHIP_TCP_01_Delay = settings.CURRENT_SLEEP_SHIP_TCP_01_Delay;
                _local.CURRENT_SLEEP_SHIP_TCP_02_Delay = settings.CURRENT_SLEEP_SHIP_TCP_02_Delay;
                _local.CURRENT_SLEEP_SHIP_TCP_03_Delay = settings.CURRENT_SLEEP_SHIP_TCP_03_Delay;

                _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Min = settings.CURRENT_SLEEP_SHIP_Sleep_Curr_Min;
                _local.CURRENT_SLEEP_SHIP_Sleep_Curr_Max = settings.CURRENT_SLEEP_SHIP_Sleep_Curr_Max;
                _local.CURRENT_SLEEP_SHIP_Ship_Curr_Min = settings.CURRENT_SLEEP_SHIP_Ship_Curr_Min;
                _local.CURRENT_SLEEP_SHIP_Ship_Curr_Max = settings.CURRENT_SLEEP_SHIP_Ship_Curr_Max;
                
                _local.CHARGE_Processor_Step_Delay = settings.CHARGE_Processor_Step_Delay;
                _local.CHARGE_TCP1_Delay = settings.CHARGE_TCP1_Delay;
                _local.CHARGE_Current_Min = settings.CHARGE_Current_Min;
                _local.CHARGE_Current_Max = settings.CHARGE_Current_Max;

                _local.GPAK_Step_Delay = settings.GPAK_Step_Delay;
                _local.GPAK_Pba_Delay = settings.GPAK_Pba_Delay;
                _local.GPAK_Result = settings.GPAK_Result;

                _local.USB_CHECK_Delay = settings.USB_CHECK_Delay;
                _local.USB_CHECK_Processor_Step_Delay = settings.USB_CHECK_Processor_Step_Delay;
                _local.USB_CHECK_TCP1_Delay = settings.USB_CHECK_TCP1_Delay;
                _local.USB_CHECK_TCP2_Delay = settings.USB_CHECK_TCP2_Delay;
                _local.USB_CHECK_TOP = settings.USB_CHECK_TOP;
                _local.USB_CHECK_BOTTOM = settings.USB_CHECK_BOTTOM;



                _local.FLASH_MEMORY_Step_Delay = settings.FLASH_MEMORY_Step_Delay;
                _local.FLASH_MEMORY_Pba_Delay = settings.FLASH_MEMORY_Pba_Delay;
                _local.FLASH_MEMORY_MCU_FLASH_WAIT = settings.FLASH_MEMORY_MCU_FLASH_WAIT;
                _local.FLASH_MEMORY_MCU_EXT_WAIT = settings.FLASH_MEMORY_MCU_EXT_WAIT;
                _local.FLASH_MEMORY_MCU_MEMORY = settings.FLASH_MEMORY_MCU_MEMORY;
                _local.FLASH_MEMORY_EXT_MEMORY = settings.FLASH_MEMORY_EXT_MEMORY;


                _local.MOTOR_Step_Delay = settings.MOTOR_Step_Delay;
                _local.MOTOR_PBA_Delay = settings.MOTOR_PBA_Delay;
                _local.MOTOR_TCP_01_Delay = settings.MOTOR_TCP_01_Delay;
                _local.MOTOR_TCP_02_Delay = settings.MOTOR_TCP_02_Delay;
                _local.MOTOR_PWM_Min = settings.MOTOR_PWM_Min;
                _local.MOTOR_PWM_Max = settings.MOTOR_PWM_Max;

                _local.FLOODS_Step_Delay = settings.FLOODS_Step_Delay;
                _local.FLOODS_PBA_Delay = settings.FLOODS_PBA_Delay;
                _local.FLOODS_TCP_01_Delay = settings.FLOODS_TCP_01_Delay;
                _local.FLOODS_TCP_02_Delay = settings.FLOODS_TCP_02_Delay;
                _local.FLOODS_STATE = settings.FLOODS_STATE;


                _local.HEATER_PBA_Delay = settings.HEATER_PBA_Delay;
                _local.HEATER_Processor_Step_Delay = settings.HEATER_Processor_Step_Delay;
                _local.HEATER_PWM_Min = settings.HEATER_PWM_Min;
                _local.HEATER_PWM_Max = settings.HEATER_PWM_Max;
                _local.HEATER_Load_Switch_Min = settings.HEATER_Load_Switch_Min;
                _local.HEATER_Load_Switch_Max = settings.HEATER_Load_Switch_Max;

                _local.CARTRIDGE_Step_Delay = settings.CARTRIDGE_Step_Delay;
                _local.CARTRIDGE_PBA_Delay = settings.CARTRIDGE_PBA_Delay;
                _local.CARTRIDGE_TCP_01_Delay = settings.CARTRIDGE_TCP_01_Delay;
                _local.CARTRIDGE_Min = settings.CARTRIDGE_Min;
                _local.CARTRIDGE_Max = settings.CARTRIDGE_Max;

                _local.SUB_HEATER_Step_Delay = settings.SUB_HEATER_Step_Delay;
                _local.SUB_HEATER_PBA_Delay = settings.SUB_HEATER_PBA_Delay;
                _local.SUB_HEATER_TCP_01_Delay = settings.SUB_HEATER_TCP_01_Delay;
                _local.SUB_HEATER_Load_Switch_Min = settings.SUB_HEATER_Load_Switch_Min;
                _local.SUB_HEATER_Load_Switch_Max = settings.SUB_HEATER_Load_Switch_Max;

                _local.ACCELEROMETER_Step_Delay = settings.ACCELEROMETER_Step_Delay;
                _local.ACCELEROMETER_PBA_Delay = settings.ACCELEROMETER_PBA_Delay;
                _local.ACCELEROMETER_Result = settings.ACCELEROMETER_Result;

                _local.PRESSURE_SENSOR_PBA_Delay = settings.PRESSURE_SENSOR_PBA_Delay;
                _local.PRESSURE_SENSOR_Processor_Delay = settings.PRESSURE_SENSOR_Processor_Delay;
                _local.PRESSURE_SENSOR_COM_CHECK = settings.PRESSURE_SENSOR_COM_CHECK;

                _local.PBA_FLAG_PBA_Delay = settings.PBA_FLAG_PBA_Delay;
                _local.PBA_FLAG_Processor_Step = settings.PBA_FLAG_Processor_Step;
                _local.PBA_FLAG_Test = settings.PBA_FLAG_Test;

                _local.PBA_CMD_CHECK_START_Step_Delay = settings.PBA_CMD_CHECK_START_Step_Delay;
                _local.PBA_CMD_CHECK_START_TCP_01_Delay = settings.PBA_CMD_CHECK_START_TCP_01_Delay;
                

                dgViewTaskList.DataSource = null;
                dgViewSetValue.DataSource = null;

                // TaskOrder 반영
                List<string> taskOrder = new List<string>();
                if (recipe.TaskOrder != null)
                {
                    foreach (var task in recipe.TaskOrder)
                        taskOrder.Add(task.ToString());
                }

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

                MessageBox.Show("Recipe Load Success!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


        #endregion

        
    }
}
