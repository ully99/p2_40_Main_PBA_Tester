using System;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using p2_40_Main_PBA_Tester.Communication;

namespace p2_40_Main_PBA_Tester.Data
{
    public class MesData
    {
        #region 공통 컬럼 (ROW_ID는 DB 자동 증가)
        public string EQUIPMENT_ID { get; set; }
        public string TESTER_VER { get; set; }
        public string MODEL { get; set; }
        public string WORK_TIME { get; set; }
        public string CHANNEL { get; set; }
        public string SPEC_FILE { get; set; }
        public string TACT_TIME { get; set; }
        public string TOTAL_JUDGMENT { get; set; }
        public string NG_ITEM { get; set; }
        #endregion

        #region 검사항목 컬럼
        public string PBA_QR_CODE { get; set; } = "";      // QR READ
        public string MAIN_INFO_FW_VER { get; set; } = "";       // MCU INFO - Main Firmware Version
        public string MCU_ID { get; set; } = "";
        public string MAIN_INFO_FW_VER_LDC { get; set; } = "";         // MCU INFO - LDC Firmware Version
        public string MAIN_INFO_IMAGE_VER { get; set; } = "";       // MCU INFO - Image Firmware Version
        public string VBUS_CHG_IN { get; set; } = "";           // OVP - VBUS Voltage
        public string OVP_CHG_IN { get; set; } = "";           // OVP - OVP Voltage
        public string VSYS_VOLT { get; set; } = "";             // LDO - VSYS
        public string VSYS_3V3 { get; set; } = "";             // LDO - VSYS 3V3
        public string MCU_3V0 { get; set; } = "";              // LDO - MCU 3V0
        public string VDD_3V0 { get; set; } = "";              // LDO - VDD 3V0
        public string LCD_3V0 { get; set; } = "";              // LDO - LCD 3V0
        public string DC_BOOST { get; set; } = "";             // LDO - DC BOOST
        public string VDD_3V0_OFF { get; set; } = "";              // LDO - VDD 3V0 OFF
        public string LCD_3V0_OFF { get; set; } = "";              // LDO - LCD 3V0 OFF
        public string DC_BOOST_OFF { get; set; } = "";             // LDO - DC BOOST OFF
        public string SLEEP_CURR { get; set; } = "";           // CURRENT_SLEEP_SHIP - Sleep Current (mA)
        public string SHIP_CURR { get; set; } = "";            // CURRENT_SLEEP_SHIP - Ship Current (uA)
        public string CHARGE_HVDCP { get; set; } = "";             // CHARGE - HVDCP Charge Current
        
        public string CHARGE_PPS { get; set; } = "";               // CHARGE - PPS Charge Current (IBAT)
        public string USB_CHECK_TOP { get; set; } = "";        // USB CHECK - TOP DP/DM
        public string USB_CHECK_BOT { get; set; } = "";        // USB CHECK - BOTTOM DP/DM
        public string MOTOR_PWM { get; set; } = "";            // MOTOR - PWM 출력 확인
        public string FLOOD_BOARD { get; set; } = "";          // FLOODS - Board Flood
        public string FLOOD_USB { get; set; } = "";            // FLOODS - USB Flood
        public string HEATER_PIN_OFF { get; set; } = "";     // HEATER - Sensing Pin OFF 저항
        public string HEATER_PIN_ON { get; set; } = "";      // HEATER - Sensing Pin ON 저항
        public string HEATER_PWM { get; set; } = "";           // HEATER - PWM
        public string CARTRIDGE_BOOST { get; set; } = "";      // CARTRIDGE - KATO Boost Voltage
        public string CARTRIDGE_PWM { get; set; } = "";        // CARTRIDGE - PWM
        public string SUB_HEATER_BOOST { get; set; } = "";     // SUB HEATER - Boost Voltage
        public string SUB_HEATER_PWM { get; set; } = "";       // SUB HEATER - PWM
        public string GPAK_CHECK { get; set; } = "";           // GPAK
        public string ACCELEROMETER { get; set; } = "";        // ACCELEROMETER
        public string MCU_MEMORY { get; set; } = "";           // FLASH MEMORY - MCU Flash 무결성
        public string EXT_MEMORY { get; set; } = "";           // FLASH MEMORY - EXT Flash 무결성
        public string PBA_FLAG { get; set; } = "";         // PBA FLAG
        #endregion

        #region CSV 전용 컬럼 (DB에는 저장하지 않음)
        //작업 결과
        public string CHARGE_PPS_VBUS { get; set; } = "";
        public string CHARGE_PPS_VBAT { get; set; } = "";

        public string RESULT_QR_READ { get; set; } = "";
        public string RESULT_MCU_INFO { get; set; } = "";
        public string RESULT_OVP { get; set; } = "";
        public string RESULT_LDO { get; set; } = "";
        public string RESULT_CURRENT_SLEEP_SHIP { get; set; } = "";
        public string RESULT_CHARGE { get; set; } = "";
        public string RESULT_GPAK { get; set; } = "";
        public string RESULT_USB_CHECK { get; set; } = "";
        public string RESULT_FLASH_MEMORY { get; set; } = "";
        public string RESULT_MOTOR { get; set; } = "";
        public string RESULT_FLOODS { get; set; } = "";
        public string RESULT_HEATER { get; set; } = "";
        public string RESULT_CARTRIDGE { get; set; } = "";
        public string RESULT_SUB_HEATER { get; set; } = "";
        public string RESULT_ACCELEROMETER { get; set; } = "";
        public string RESULT_FLAG_INIT { get; set; } = "";
        public string RESULT_PBA_FLAG { get; set; } = "";
        public string RESULT_PBA_CMD_CHECK_START { get; set; } = "";
        public string RESULT_PBA_TEST_END { get; set; } = "";
        public string MES_RESULT { get; set; } = "";
        #endregion

        #region DB 연동
        public static string GetConnectionString()
        {
            return OleDbHelper.ConnStr.SqlServer(
                Settings.Instance.DB_IP,
                Settings.Instance.DB_PORT,
                Settings.Instance.DB_NAME,
                Settings.Instance.DB_USER,
                Settings.Instance.DB_PW);
        }

        public int Insert(string tableName = null, int timeoutSeconds = 30)
        {
            string table = tableName ?? Settings.Instance.DB_TABLE;
            var map = GetDbMap();

            string cols = string.Join(", ", map.Select(x => x.ColName));
            string placeholders = string.Join(", ", map.Select(x => "?"));
            string sql = $"INSERT INTO {table} ({cols}) VALUES ({placeholders})";

            var ps = map.Select(x => new OleDbParameter { Value = x.Value }).ToArray();
            return OleDbHelper.ExecuteNonQuery(GetConnectionString(), sql, timeoutSeconds, ps);
        }

        public Task<int> InsertAsync(string tableName = null, int timeoutSeconds = 30, CancellationToken ct = default(CancellationToken))
        {
            string table = tableName ?? Settings.Instance.DB_TABLE;
            var map = GetDbMap();

            string cols = string.Join(", ", map.Select(x => x.ColName));
            string placeholders = string.Join(", ", map.Select(x => "?"));
            string sql = $"INSERT INTO {table} ({cols}) VALUES ({placeholders})";

            var ps = map.Select(x => new OleDbParameter { Value = x.Value }).ToArray();
            return OleDbHelper.ExecuteNonQueryAsync(GetConnectionString(), sql, timeoutSeconds, ct, ps);
        }

        public static async Task<(bool ok, string message)> CheckInterlockAsync(string mcuId, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                string proc = Settings.Instance.INTERLOCK_PROCEDURE_1;
                string ret = await OleDbHelper.ExecuteProcedureAsync(GetConnectionString(), proc, 10, ct, new OleDbParameter("@MCU_ID", mcuId));
                string upper = (ret ?? "").Trim().ToUpper();
                return (upper == "PASS", upper);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region 데이터 매핑 구조 (유지보수 핵심 영역)
        private static object ToDbValue(string s) => string.IsNullOrEmpty(s) ? "" : s;

        // DB에 들어갈 컬럼과 값을 1:1로 매핑합니다.
        private List<(string ColName, object Value)> GetDbMap()
        {
            var list = new List<(string, object)>();
            void Add(string col, object val) => list.Add((col, ToDbValue((string)val)));

            Add("EQUIPMENT_ID", EQUIPMENT_ID); Add("TESTER_VER", TESTER_VER); Add("MODEL", MODEL);
            Add("WORK_TIME", WORK_TIME); Add("MCU_ID", MCU_ID); Add("CHANNEL", CHANNEL);
            Add("SPEC_FILE", SPEC_FILE); Add("TACT_TIME", TACT_TIME); Add("TOTAL_JUDGMENT", TOTAL_JUDGMENT);
            Add("NG_ITEM", NG_ITEM); Add("PBA_QR_CODE", PBA_QR_CODE);
            Add("MAIN_INFO_FW_VER", MAIN_INFO_FW_VER); Add("MAIN_INFO_FW_VER_LDC", MAIN_INFO_FW_VER_LDC); Add("MAIN_INFO_IMAGE_VER", MAIN_INFO_IMAGE_VER);
            Add("VBUS_CHG_IN", VBUS_CHG_IN); Add("OVP_CHG_IN", OVP_CHG_IN);
            Add("VSYS_VOLT", VSYS_VOLT); Add("VSYS_3V3", VSYS_3V3); Add("MCU_3V0", MCU_3V0); Add("VDD_3V0", VDD_3V0); Add("LCD_3V0", LCD_3V0); Add("DC_BOOST", DC_BOOST);
            Add("VDD_3V0_OFF", VDD_3V0_OFF); Add("LCD_3V0_OFF", LCD_3V0_OFF); Add("DC_BOOST_OFF", DC_BOOST_OFF);
            Add("SLEEP_CURR", SLEEP_CURR); Add("SHIP_CURR", SHIP_CURR);
            Add("CHARGE_HVDCP", CHARGE_HVDCP); Add("CHARGE_PPS", CHARGE_PPS);
            Add("USB_CHECK_TOP", USB_CHECK_TOP); Add("USB_CHECK_BOT", USB_CHECK_BOT);
            Add("MOTOR_PWM", MOTOR_PWM);
            Add("FLOOD_BOARD", FLOOD_BOARD); Add("FLOOD_USB", FLOOD_USB);
            Add("HEATER_PIN_OFF", HEATER_PIN_OFF); Add("HEATER_PIN_ON", HEATER_PIN_ON); Add("HEATER_PWM", HEATER_PWM);
            Add("CARTRIDGE_BOOST", CARTRIDGE_BOOST); Add("CARTRIDGE_PWM", CARTRIDGE_PWM);
            Add("SUB_HEATER_BOOST", SUB_HEATER_BOOST); Add("SUB_HEATER_PWM", SUB_HEATER_PWM);
            Add("GPAK_CHECK", GPAK_CHECK); Add("ACCELEROMETER", ACCELEROMETER);
            Add("MCU_MEMORY", MCU_MEMORY); Add("EXT_MEMORY", EXT_MEMORY);
            Add("PBA_FLAG", PBA_FLAG);

            return list;
        }

        // CSV에 들어갈 컬럼명, 값, 스펙을 1:1로 매핑합니다.
        private List<(string ColName, object Value, string SpecValue)> GetCsvMap(Settings s)
        {
            var list = new List<(string, object, string)>();
            // needSpec이 true일 때만 GetSpecForCsvColumn를 호출해 스펙을 가져옵니다.
            void Add(string col, object val, bool needSpec = false)
            {
                string spec = needSpec ? GetSpecForCsvColumn(col, s) : null;
                list.Add((col, ToDbValue((string)val), spec));
            }

            // 1. 공통 정보 (스펙 없음)
            Add("EQUIPMENT_ID", EQUIPMENT_ID); Add("TESTER_VER", TESTER_VER); Add("MODEL", MODEL); Add("WORK_TIME", WORK_TIME);
            Add("CHANNEL", CHANNEL); Add("SPEC_FILE", SPEC_FILE); Add("TACT_TIME", TACT_TIME); Add("TOTAL_JUDGMENT", TOTAL_JUDGMENT); Add("NG_ITEM", NG_ITEM);

            // 2. 항목별 데이터 (RESULT는 스펙 없이, 측정값은 스펙 포함)
            Add("PBA_QR_CODE", PBA_QR_CODE, true); Add("RESULT_QR_READ", RESULT_QR_READ);

            Add("MCU_ID", MCU_ID, true); Add("MAIN_INFO_FW_VER", MAIN_INFO_FW_VER, true); Add("MAIN_INFO_FW_VER_LDC", MAIN_INFO_FW_VER_LDC, true);
            Add("MAIN_INFO_IMAGE_VER", MAIN_INFO_IMAGE_VER, true); Add("RESULT_MCU_INFO", RESULT_MCU_INFO);

            Add("VBUS_CHG_IN", VBUS_CHG_IN, true); Add("OVP_CHG_IN", OVP_CHG_IN, true); Add("RESULT_OVP", RESULT_OVP);

            Add("VSYS_VOLT", VSYS_VOLT, true); Add("VSYS_3V3", VSYS_3V3, true); Add("MCU_3V0", MCU_3V0, true);
            Add("VDD_3V0", VDD_3V0, true); Add("LCD_3V0", LCD_3V0, true); Add("DC_BOOST", DC_BOOST, true);
            Add("VDD_3V0_OFF", VDD_3V0_OFF, true); Add("LCD_3V0_OFF", LCD_3V0_OFF, true); Add("DC_BOOST_OFF", DC_BOOST_OFF, true);
            Add("RESULT_LDO", RESULT_LDO);

            Add("SLEEP_CURR", SLEEP_CURR, true); Add("SHIP_CURR", SHIP_CURR, true); Add("RESULT_CURRENT_SLEEP_SHIP", RESULT_CURRENT_SLEEP_SHIP);

            Add("CHARGE_HVDCP", CHARGE_HVDCP, true); Add("CHARGE_PPS_VBUS", CHARGE_PPS_VBUS);
            Add("CHARGE_PPS_VBAT", CHARGE_PPS_VBAT);  Add("CHARGE_PPS", CHARGE_PPS, true); Add("RESULT_CHARGE", RESULT_CHARGE);

            Add("USB_CHECK_TOP", USB_CHECK_TOP, true); Add("USB_CHECK_BOT", USB_CHECK_BOT, true); Add("RESULT_USB_CHECK", RESULT_USB_CHECK);

            Add("MOTOR_PWM", MOTOR_PWM, true); Add("RESULT_MOTOR", RESULT_MOTOR);

            Add("FLOOD_BOARD", FLOOD_BOARD, true); Add("FLOOD_USB", FLOOD_USB, true); Add("RESULT_FLOODS", RESULT_FLOODS);

            Add("HEATER_PIN_OFF", HEATER_PIN_OFF, true); Add("HEATER_PIN_ON", HEATER_PIN_ON, true); Add("HEATER_PWM", HEATER_PWM, true); Add("RESULT_HEATER", RESULT_HEATER);

            Add("CARTRIDGE_BOOST", CARTRIDGE_BOOST, true); Add("CARTRIDGE_PWM", CARTRIDGE_PWM, true); Add("RESULT_CARTRIDGE", RESULT_CARTRIDGE);

            Add("SUB_HEATER_BOOST", SUB_HEATER_BOOST, true); Add("SUB_HEATER_PWM", SUB_HEATER_PWM, true); Add("RESULT_SUB_HEATER", RESULT_SUB_HEATER);

            Add("GPAK_CHECK", GPAK_CHECK, true); Add("RESULT_ACCELEROMETER", RESULT_ACCELEROMETER); Add("ACCELEROMETER", ACCELEROMETER, true); Add("RESULT_GPAK", RESULT_GPAK);

            Add("MCU_MEMORY", MCU_MEMORY, true); Add("EXT_MEMORY", EXT_MEMORY, true); Add("RESULT_FLASH_MEMORY", RESULT_FLASH_MEMORY);

            Add("RESULT_PBA_FLAG", RESULT_PBA_FLAG); Add("PBA_FLAG", PBA_FLAG, true); Add("FLAG INIT", RESULT_FLAG_INIT);

            Add("RESULT_PBA_TEST_END", RESULT_PBA_TEST_END); Add("RESULT_PBA_CMD_CHECK_START", RESULT_PBA_CMD_CHECK_START);

            // 3. 최종 결과 (스펙 없음)
            Add("MES_RESULT", MES_RESULT);

            return list;
        }
        #endregion

        #region CSV 로그 저장
        private static readonly object _csvLock = new object();

        private static string CsvQuoteCell(object v)
        {
            string cell = (v == null || v == DBNull.Value) ? "" : v.ToString();
            cell = cell.Replace("\"", "\"\"");
            return $"\"{cell}\"";
        }

        private static string SpecRangeF(float min, float max) => string.Format(CultureInfo.InvariantCulture, "{0:G}~{1:G}", min, max);
        private static string SpecRangeU(uint min, uint max) => string.Format(CultureInfo.InvariantCulture, "{0}~{1}", min, max);
        private static string SpecRangeS(short min, short max) => string.Format(CultureInfo.InvariantCulture, "{0}~{1}", min, max);

        private static string GetSpecForCsvColumn(string columnName, Settings s)
        {
            if (s == null) return "";

            switch (columnName)
            {
                case "MCU_ID": return s.MCU_INFO_Mcu_Id_Len.ToString(CultureInfo.InvariantCulture);
                case "PBA_QR_CODE": return s.QR_READ_Len.ToString(CultureInfo.InvariantCulture);
                case "MAIN_INFO_FW_VER": return s.MCU_INFO_Main_Fw_Ver ?? "";
                case "MAIN_INFO_FW_VER_LDC": return s.MCU_INFO_LDC_Fw_Ver ?? "";
                case "MAIN_INFO_IMAGE_VER": return s.MCU_INFO_Image_Fw_Ver ?? "";
                case "VBUS_CHG_IN": return SpecRangeF(s.OVP_VBUS_Min, s.OVP_VBUS_Max);
                case "OVP_CHG_IN": return SpecRangeF(s.OVP_OVP_Min, s.OVP_OVP_Max);
                case "VSYS_VOLT": return SpecRangeF(s.LDO_VSYS_Min, s.LDO_VSYS_Max);
                case "VSYS_3V3": return SpecRangeF(s.LDO_VSYS_3V3_Min, s.LDO_VSYS_3V3_Max);
                case "MCU_3V0": return SpecRangeF(s.LDO_MCU_3V0_Min, s.LDO_MCU_3V0_Max);
                case "VDD_3V0": return SpecRangeF(s.LDO_VDD_3V0_Min, s.LDO_VDD_3V0_Max);
                case "LCD_3V0": return SpecRangeF(s.LDO_LCD_3V0_Min, s.LDO_LCD_3V0_Max);
                case "DC_BOOST": return SpecRangeF(s.LDO_DC_BOOST_Min, s.LDO_DC_BOOST_Max);
                case "VDD_3V0_OFF": return SpecRangeF(s.LDO_VDD_3V0_OFF_Min, s.LDO_VDD_3V0_OFF_Max);
                case "LCD_3V0_OFF": return SpecRangeF(s.LDO_LCD_3V0_OFF_Min, s.LDO_LCD_3V0_OFF_Max);
                case "DC_BOOST_OFF": return SpecRangeF(s.LDO_DC_BOOST_OFF_Min, s.LDO_DC_BOOST_OFF_Max);
                case "SLEEP_CURR": return SpecRangeF(s.CURRENT_SLEEP_SHIP_Sleep_Curr_Min, s.CURRENT_SLEEP_SHIP_Sleep_Curr_Max);
                case "SHIP_CURR": return SpecRangeF(s.CURRENT_SLEEP_SHIP_Ship_Curr_Min, s.CURRENT_SLEEP_SHIP_Ship_Curr_Max);
                case "CHARGE_HVDCP": return SpecRangeF(s.CHARGE_HVDCP_Min, s.CHARGE_HVDCP_Max);
                case "CHARGE_PPS": return SpecRangeS(s.CHARGE_PPS_Min, s.CHARGE_PPS_Max);
                case "USB_CHECK_TOP": return s.USB_CHECK_TOP.ToString(CultureInfo.InvariantCulture);
                case "USB_CHECK_BOT": return s.USB_CHECK_BOTTOM.ToString(CultureInfo.InvariantCulture);
                case "MOTOR_PWM": return SpecRangeU(s.MOTOR_PWM_Min, s.MOTOR_PWM_Max);
                case "FLOOD_BOARD": return s.FLOODS_Board_Floods.ToString(CultureInfo.InvariantCulture);
                case "FLOOD_USB": return s.FLOODS_USB_Floods.ToString(CultureInfo.InvariantCulture);
                case "HEATER_PIN_OFF": return SpecRangeF(s.HEATER_Sensing_Pin_Off_Min, s.HEATER_Sensing_Pin_Off_Max);
                case "HEATER_PIN_ON": return SpecRangeF(s.HEATER_Sensing_Pin_On_Min, s.HEATER_Sensing_Pin_On_Max);
                case "HEATER_PWM": return SpecRangeU(s.HEATER_PWM_Min, s.HEATER_PWM_Max);
                case "CARTRIDGE_BOOST": return SpecRangeF(s.CARTRIDGE_KATO_BOOST_Min, s.CARTRIDGE_KATO_BOOST_Max);
                case "CARTRIDGE_PWM": return SpecRangeU(s.CARTRIDGE_CARTRIDGE_PWM_Min, s.CARTRIDGE_CARTRIDGE_PWM_Max);
                case "SUB_HEATER_BOOST": return SpecRangeF(s.SUB_HEATER_BOOST_Min, s.SUB_HEATER_BOOST_Max);
                case "SUB_HEATER_PWM": return SpecRangeU(s.SUB_HEATER_PWM_Min, s.SUB_HEATER_PWM_Max);
                case "GPAK_CHECK": return s.GPAK_Result.ToString(CultureInfo.InvariantCulture);
                case "ACCELEROMETER": return s.ACCELEROMETER_Result.ToString(CultureInfo.InvariantCulture);
                case "MCU_MEMORY": return s.FLASH_MEMORY_MCU_MEMORY.ToString(CultureInfo.InvariantCulture);
                case "EXT_MEMORY": return s.FLASH_MEMORY_EXT_MEMORY.ToString(CultureInfo.InvariantCulture);
                case "PBA_FLAG": return s.PBA_FLAG_FLAG.ToString(CultureInfo.InvariantCulture);
                default: return "";
            }
        }

        public bool SaveResultToCsv()
        {
            lock (_csvLock)
            {
                try
                {
                    var s = Settings.Instance;
                    var csvData = GetCsvMap(s); // 매핑된 데이터를 한 번만 가져옴

                    string lot = $"LOT{s.Lot_No?.Replace(",", " ") ?? "1"}";
                    string model = s.Model_No?.Replace(",", " ") ?? "Model";
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    string mc = s.Mc_No?.Replace(",", " ") ?? "mc";

                    string dir = Path.Combine(Application.StartupPath, "Log", lot, model, date);
                    Directory.CreateDirectory(dir);

                    string filePath = Path.Combine(dir, $"{mc}_{date}.csv");
                    bool isNewFile = !File.Exists(filePath);

                    using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                    {
                        if (isNewFile)
                        {
                            // 동적 헤더 생성 (스펙이 필요한 컬럼 뒤에는 자동으로 SPEC_컬럼이 붙음)
                            var headers = new List<string>();
                            foreach (var item in csvData)
                            {
                                if (item.SpecValue != null) headers.Add("SPEC_" + item.ColName);
                                headers.Add(item.ColName);
                            }
                            writer.WriteLine(string.Join(",", headers));
                        }

                        // 동적 데이터 생성
                        var rowData = new List<string>();
                        foreach (var item in csvData)
                        {
                            if (item.SpecValue != null) rowData.Add(CsvQuoteCell(item.SpecValue));
                            rowData.Add(CsvQuoteCell(item.Value));
                        }
                        writer.WriteLine(string.Join(",", rowData));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CSV 저장 오류: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region 유틸리티
        public void SetResultColumn(string TaskName, bool isPass)
        {
            string resultStr = isPass ? "PASS" : "FAIL";
            switch (TaskName)
            {
                case "QR READ": RESULT_QR_READ = resultStr; break;
                case "MCU INFO": RESULT_MCU_INFO = resultStr; break;
                case "OVP": RESULT_OVP = resultStr; break;
                case "LDO": RESULT_LDO = resultStr; break;
                case "CURRENT_SLEEP_SHIP": RESULT_CURRENT_SLEEP_SHIP = resultStr; break;
                case "CHARGE": RESULT_CHARGE = resultStr; break;
                case "GPAK": RESULT_GPAK = resultStr; break;
                case "USB CHECK": RESULT_USB_CHECK = resultStr; break;
                case "FLASH MEMORY": RESULT_FLASH_MEMORY = resultStr; break;
                case "MOTOR": RESULT_MOTOR = resultStr; break;
                case "FLOODS": RESULT_FLOODS = resultStr; break;
                case "HEATER": RESULT_HEATER = resultStr; break;
                case "CARTRIDGE": RESULT_CARTRIDGE = resultStr; break;
                case "SUB HEATER": RESULT_SUB_HEATER = resultStr; break;
                case "ACCELEROMETER": RESULT_ACCELEROMETER = resultStr; break;
                case "PBA FLAG": RESULT_PBA_FLAG = resultStr; break;
                case "FLAG INIT": RESULT_FLAG_INIT = resultStr; break;
                case "PBA CMD CHECK START": RESULT_PBA_CMD_CHECK_START = resultStr; break;
                case "PBA TEST END": RESULT_PBA_TEST_END = resultStr; break;
            }
        }

        public void Clear()
        {
            EQUIPMENT_ID = null; TESTER_VER = null; MODEL = null;
            WORK_TIME = null; MCU_ID = null; CHANNEL = null;
            SPEC_FILE = null; TACT_TIME = null; TOTAL_JUDGMENT = null; NG_ITEM = null;
            PBA_QR_CODE = null;
            MAIN_INFO_FW_VER = null; MAIN_INFO_FW_VER_LDC = null; MAIN_INFO_IMAGE_VER = null;
            VBUS_CHG_IN = null; OVP_CHG_IN = null;
            VSYS_VOLT = null; VSYS_3V3 = null;
            MCU_3V0 = null; VDD_3V0 = null; LCD_3V0 = null; DC_BOOST = null;
            VDD_3V0_OFF = null; LCD_3V0_OFF = null; DC_BOOST_OFF = null;
            SLEEP_CURR = null; SHIP_CURR = null;
            CHARGE_HVDCP = null; CHARGE_PPS = null;
            USB_CHECK_TOP = null; USB_CHECK_BOT = null;
            MOTOR_PWM = null;
            FLOOD_BOARD = null; FLOOD_USB = null;
            HEATER_PIN_OFF = null; HEATER_PIN_ON = null; HEATER_PWM = null;
            CARTRIDGE_BOOST = null; CARTRIDGE_PWM = null;
            SUB_HEATER_BOOST = null; SUB_HEATER_PWM = null;
            GPAK_CHECK = null; ACCELEROMETER = null;
            MCU_MEMORY = null; EXT_MEMORY = null;
            PBA_FLAG = null;

            RESULT_QR_READ = ""; RESULT_MCU_INFO = ""; RESULT_OVP = ""; RESULT_LDO = "";
            RESULT_CURRENT_SLEEP_SHIP = ""; RESULT_CHARGE = ""; RESULT_GPAK = ""; RESULT_USB_CHECK = "";
            RESULT_FLASH_MEMORY = ""; RESULT_MOTOR = ""; RESULT_FLOODS = ""; RESULT_HEATER = "";
            RESULT_CARTRIDGE = ""; RESULT_SUB_HEATER = ""; RESULT_ACCELEROMETER = ""; RESULT_FLAG_INIT = "";
            RESULT_PBA_FLAG = ""; MES_RESULT = "";
        }

        public static string GetCreateTableSql(string tableName)
        {
            return $@"
            CREATE TABLE {tableName}
            (
                ROW_ID               INT            IDENTITY(1,1) PRIMARY KEY,
                -- 공통
                EQUIPMENT_ID         NVARCHAR(20)   NOT NULL,
                TESTER_VER           NVARCHAR(20)   NOT NULL,
                MODEL                NVARCHAR(20)   NOT NULL,
                WORK_TIME            NVARCHAR(14)   NOT NULL,
                MCU_ID               NVARCHAR(50)   NOT NULL,
                CHANNEL              NVARCHAR(5)    NULL,
                SPEC_FILE            NVARCHAR(200)  NOT NULL,
                TACT_TIME            NVARCHAR(10)   NULL,
                TOTAL_JUDGMENT       NVARCHAR(10)   NOT NULL,
                NG_ITEM              NVARCHAR(300)  NULL,
                -- QR READ
                PBA_QR_CODE          NVARCHAR(50)   NULL,
                -- MCU INFO
                MAIN_INFO_FW_VER     NVARCHAR(30)   NULL,
                MAIN_INFO_FW_VER_LDC NVARCHAR(30)   NULL,
                MAIN_INFO_IMAGE_VER  NVARCHAR(30)   NULL,
                -- OVP
                VBUS_CHG_IN          NVARCHAR(10)   NULL,
                OVP_CHG_IN           NVARCHAR(10)   NULL,
                -- LDO
                VSYS_VOLT            NVARCHAR(10)   NULL,
                VSYS_3V3_OFF         NVARCHAR(10)   NULL,
                VSYS_3V3             NVARCHAR(10)   NULL,
                MCU_3V0              NVARCHAR(10)   NULL,
                VDD_3V0              NVARCHAR(10)   NULL,
                LCD_3V0              NVARCHAR(10)   NULL,
                DC_BOOST             NVARCHAR(10)   NULL,
                VDD_3V0_OFF          NVARCHAR(10)   NULL,
                LCD_3V0_OFF          NVARCHAR(10)   NULL,
                DC_BOOST_OFF         NVARCHAR(10)   NULL,
                -- CURRENT_SLEEP_SHIP
                SLEEP_CURR           NVARCHAR(10)   NULL,
                SHIP_CURR            NVARCHAR(10)   NULL,
                -- CHARGE
                CHARGE_HVDCP         NVARCHAR(10)   NULL,
                CHARGE_PPS           NVARCHAR(10)   NULL,
                -- USB CHECK
                USB_CHECK_TOP        NVARCHAR(30)   NULL,
                USB_CHECK_BOT        NVARCHAR(30)   NULL,
                -- FLAG INIT
                FLAG_INIT            NVARCHAR(10)   NULL,
                -- MOTOR
                MOTOR_PWM            NVARCHAR(20)   NULL,
                -- FLOODS
                FLOOD_BOARD          NVARCHAR(20)   NULL,
                FLOOD_USB            NVARCHAR(20)   NULL,
                -- HEATER
                HEATER_PIN_OFF       NVARCHAR(20)   NULL,
                HEATER_PIN_ON        NVARCHAR(20)   NULL,
                HEATER_PWM           NVARCHAR(20)   NULL,
                -- CARTRIDGE
                CARTRIDGE_BOOST      NVARCHAR(20)   NULL,
                CARTRIDGE_PWM        NVARCHAR(20)   NULL,
                -- SUB HEATER
                SUB_HEATER_BOOST     NVARCHAR(20)   NULL,
                SUB_HEATER_PWM       NVARCHAR(20)   NULL,
                -- GPAK
                GPAK_CHECK           NVARCHAR(20)   NULL,
                -- ACCELEROMETER
                ACCELEROMETER        NVARCHAR(20)   NULL,
                -- FLASH MEMORY
                MCU_MEMORY           NVARCHAR(20)   NULL,
                EXT_MEMORY           NVARCHAR(20)   NULL,
                -- PBA FLAG
                PBA_FLAG             NVARCHAR(10)   NULL
            );";
        }
        #endregion
    }
}