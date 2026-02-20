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
        public int QR_READ_Step_Delay { get; set; } = 30;

        public int QR_READ_Len { get; set; } = 16;
        #endregion

        #region MCU INFO (1)
        public bool MCU_INFO_Enable { get; set; } = true;
        public int MCU_INFO_Step_Delay { get; set; } = 200;
        //public int MCU_INFO_Pba_Delay { get; set; } = 200;
        public int MCU_INFO_Tcp_01_Delay { get; set; } = 100;
        public int MCU_INFO_Tcp_02_Delay { get; set; } = 100;
        public int MCU_INFO_Booting_01_Delay { get; set; } = 6000;
        public int MCU_INFO_Mcu_Id_Len { get; set; } = 16;
        public string MCU_INFO_Main_Fw_Ver { get; set; } = "0.1.0";
        public string MCU_INFO_LDC_Fw_Ver { get; set; } = "1";
        public string MCU_INFO_Image_Fw_Ver { get; set; } = "1.0";
        #endregion

        #region OVP (2)
        public bool OVP_Enable { get; set; } = true;
        public int OVP_Step_Delay { get; set; } = 200;
        public int OVP_TCP_01_Delay { get; set; } = 3000;
        public float OVP_VBUS_Min { get; set; } = 13.7F;
        public float OVP_VBUS_Max { get; set; } = 14.3F;
        #endregion

        #region LDO (3)
        public bool LDO_Enable { get; set; } = true;
        public int LDO_Step_Delay { get; set; } = 200;

        //public int LDO_Pba_Delay { get; set; } = 200;
        public int LDO_TCP_01_Delay { get; set; } = 1000;
        public int LDO_TCP_02_Delay { get; set; } = 1000;
        public int LDO_Booting_01_Delay { get; set; } = 6000;

        public float LDO_VSYS_Min { get; set; } = 8.5F;
        public float LDO_VSYS_Max { get; set; } = 9.5F;
        public float LDO_VSYS_3V3_OFF_Min { get; set; } = 3.9F;
        public float LDO_VSYS_3V3_OFF_Max { get; set; } = 4.1F;
        public float LDO_VSYS_3V3_Min { get; set; } = 3.9F;
        public float LDO_VSYS_3V3_Max { get; set; } = 4.1F;
        public float LDO_MCU_3V0_Min { get; set; } = 4.0F;
        public float LDO_MCU_3V0_Max { get; set; } = 4.3F;
        public float LDO_VDD_3V0_Min { get; set; } = 2.95F;
        public float LDO_VDD_3V0_Max { get; set; } = 3.05F;
        public float LDO_LCD_3V0_Min { get; set; } = 4.9F;
        public float LDO_LCD_3V0_Max { get; set; } = 5.1F;
        public float LDO_DC_BOOST_Min { get; set; } = 5.9F;
        public float LDO_DC_BOOST_Max { get; set; } = 6.1F;
        #endregion

        #region CURRENT_SLEEP_SHIP (4)
        public bool CURRENT_SLEEP_SHIP_Enable { get; set; } = true;

        public int CURRENT_SLEEP_SHIP_Step_Delay { get; set; } = 200;
        //public int CURRENT_SLEEP_SHIP_Pba_Delay { get; set; } = 200;
        public int CURRENT_SLEEP_SHIP_Booting_01_Delay { get; set; } = 6000;
        public int CURRENT_SLEEP_SHIP_Booting_02_Delay { get; set; } = 6000;

        public int CURRENT_SLEEP_SHIP_TCP_01_Delay { get; set; } = 1000;
        public int CURRENT_SLEEP_SHIP_TCP_02_Delay { get; set; } = 1000;
        public int CURRENT_SLEEP_SHIP_TCP_03_Delay { get; set; } = 1000;

        public float CURRENT_SLEEP_SHIP_Sleep_Curr_Min { get; set; } = 2.9F;//mA
        public float CURRENT_SLEEP_SHIP_Sleep_Curr_Max { get; set; } = 3F; //mA
        public float CURRENT_SLEEP_SHIP_Ship_Curr_Min { get; set; } = 20F; //uA
        public float CURRENT_SLEEP_SHIP_Ship_Curr_Max { get; set; } = 50F; //uA
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
        public int GPAK_Step_Delay { get; set; } = 30;
        public int GPAK_Pba_Delay { get; set; } = 100;
        public short GPAK_Result { get; set; } = 1;
        
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
        public int FLASH_MEMORY_Step_Delay { get; set; } = 30;
        public int FLASH_MEMORY_Pba_Delay { get; set; } = 100;
        public int FLASH_MEMORY_MCU_FLASH_WAIT { get; set; } = 5000;
        public int FLASH_MEMORY_MCU_EXT_WAIT { get; set; } = 3000;
        public short FLASH_MEMORY_MCU_MEMORY { get; set; } = 1;
        public short FLASH_MEMORY_EXT_MEMORY { get; set; } = 1;
        #endregion

        #region MOTOR (9)
        public bool MOTOR_Enable { get; set; } = true;
        public int MOTOR_Step_Delay { get; set; } = 100;
        public int MOTOR_PBA_Delay { get; set; } = 100;
        public int MOTOR_TCP_01_Delay { get; set; } = 1000;
        public int MOTOR_TCP_02_Delay { get; set; } = 1000;
        public uint MOTOR_PWM_Min { get; set; } = 240;
        public uint MOTOR_PWM_Max { get; set; } = 255;
        #endregion

        #region FLOODS (10)
        public bool FLOODS_Enable { get; set; } = true;
        public int FLOODS_Step_Delay { get; set; } = 30;
        public int FLOODS_TCP_01_Delay { get; set; } = 1000;
        public int FLOODS_TCP_02_Delay { get; set; } = 1000;
        public short FLOODS_USB_Floods { get; set; } = 1;
        public short FLOODS_Board_Floods { get; set; } = 1;
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
        public int CARTRIDGE_Step_Delay { get; set; } = 30;
        //public int CARTRIDGE_PBA_Delay { get; set; } = 100;
        public int CARTRIDGE_TCP_01_Delay { get; set; } = 1000;
        public int CARTRIDGE_TCP_02_Delay { get; set; } = 1000;

        public uint CARTRIDGE_CARTRIDGE_PWM_Min { get; set; } = 92000;
        public uint CARTRIDGE_CARTRIDGE_PWM_Max { get; set; } = 94080;
        public float CARTRIDGE_KATO_BOOST_Min { get; set; } = 4.65F;
        public float CARTRIDGE_KATO_BOOST_Max { get; set; } = 4.95F;
        #endregion

        #region SUB HEATER (13)
        public bool SUB_HEATER_Enable { get; set; } = true;
        public int SUB_HEATER_Step_Delay { get; set; } = 30;
        public int SUB_HEATER_TCP_01_Delay { get; set; } = 3000;
        public int SUB_HEATER_TCP_02_Delay { get; set; } = 3000;
        public uint SUB_HEATER_PWM_Min { get; set; } = 92000;
        public uint SUB_HEATER_PWM_Max { get; set; } = 94080;
        public float SUB_HEATER_BOOST_Min { get; set; } = 4.650F;
        public float SUB_HEATER_BOOST_Max { get; set; } = 4.950F;
        #endregion

        #region ACCELEROMETER (14)
        public bool ACCELEROMETER_Enable { get; set; } = true;
        public int ACCELEROMETER_Step_Delay { get; set; } = 30;
        public int ACCELEROMETER_PBA_Delay { get; set; } = 100;
        public short ACCELEROMETER_Result { get; set; } = 1;
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
        public int PBA_CMD_CHECK_START_Step_Delay { get; set; } = 100;
        public int PBA_CMD_CHECK_START_TCP_01_Delay { get; set; } = 2000;
        public int PBA_CMD_CHECK_START_Booting_01_Delay { get; set; } = 6000;
        #endregion

        #region TEST END (18)
        public bool TEST_END_Enable { get; set; } = true;
        #endregion

        #region MES
        public bool MES_Enable { get; set; } = true;
        #endregion
    }
}
