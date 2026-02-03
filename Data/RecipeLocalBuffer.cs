using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace p2_40_Main_PBA_Tester.Data
{
    public sealed class RecipeLocalBuffer
    {
        //여기에 적힌 값들이 레시피 디폴트 값
        #region INTERLOCK 
        public bool INTERLOCK_Enable { get; set; } = true;
        #endregion

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
        public string MCU_INFO_LDC_Fw_Ver { get; set; } = "1,1";
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
        public int USB_CHECK_TOP { get; set; } = 3;
        public int USB_CHECK_BOTTOM { get; set; } = 3;
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

        #region MES
        public bool MES_Enable { get; set; } = true;
        #endregion
    }
}
