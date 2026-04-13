using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using p2_40_Main_PBA_Tester.Forms;
using p2_40_Main_PBA_Tester.Communication;
using p2_40_Main_PBA_Tester.Data;
using p2_40_Main_PBA_Tester.UserControls;
using p2_40_Main_PBA_Tester.UI;
using p2_40_Main_PBA_Tester.LIB;
using System.Data.OleDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;
using System.IO.Ports;


namespace p2_40_Main_PBA_Tester
{
    public partial class MainForm : Form
    {
        #region Field
        private bool _isTestRunning = false;
        private CancellationTokenSource _cts; // 중간에 STOP 누르면 취소하기 위해 사용

        public RichTextLogger JigLogger;
        public RichTextLogger JigCommLogger;

        private const int JIG_READ_DELAY = 3000; //ms

        private const byte CMD_JIG_SIGNAL = 0x01;
        private const byte ITEM_JIG_TEST_START = 0x01;
        private const byte ITEM_JIG_TEST_STOP = 0x02;
        private const byte ITEM_JIG_START = 0x11;
        private const byte ITEM_JIG_STOP = 0x12;

        private const byte CMD_JIG_CONTROL = 0x11;
        private const byte ITEM_JIG_PRODUCT_DETECT = 0x01;
        private const int WAIT_PRODUCT_DETECT = 5000; //ms
        #endregion


        #region Init
        public MainForm()
        {
            InitializeComponent();
            ConnectEvent();
            JigLogger = new RichTextLogger(tboxJigLog);
            JigCommLogger = new RichTextLogger(tboxJigComm);
        }



        private void StartLoadingAnimation()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
        }

        private void StopLoadingAnimation()
        {
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = 100;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            Init_ChControls();
            Init_McuLotModel_Value();
            UpdateLayoutByChannelConfig();

            if (CommManager.Jig != null)
                CommManager.Jig.OnPacketReceived += Jig_OnPacketReceived;

            var loadingForm = new LoadingForm("Connecting...") { Owner = this, StartPosition = FormStartPosition.CenterScreen };
            loadingForm.Show();
            Application.DoEvents();
            try
            {
                await CommManager.ConnectAllComponent(Settings.Instance.Board_Connect_Timeout);
                SettingMesImage();
                SettingFtpImage();

            }
            finally
            {
                loadingForm.Close();
                loadingForm.Dispose();
            }
        }

        private void Jig_OnPacketReceived(byte[] frame)
        {
            try
            {
                // 1. 기본 검증 (ITM 헤더: 0x49, 0x54, 0x4D)
                if (frame == null || frame.Length < 8) return;
                if (frame[0] != 0x49 || frame[1] != 0x54 || frame[2] != 0x4D) return;

                byte cmd = frame[5];
                byte item = frame[6];
                // byte swNo = frame[7]; // 필요 시 사용

                // 2. START 신호 처리
                if (cmd == CMD_JIG_SIGNAL && item == ITEM_JIG_START)
                {
                    if (_isTestRunning) return;

                    // UI 스레드 여부에 상관없이 비동기로 루틴 시작
                    if (InvokeRequired)
                    {
                        // Action을 async로 감싸서 넘김 (비동기 흐름 유지)
                        BeginInvoke(new Action(async () => await StartJigRoutine()));
                    }
                    else
                    {
                        // 이미 UI 스레드라면 Task.Run 등을 쓰지 않고 바로 비동기 호출 시작
                        // (async void의 효과를 내기 위해 _ 변수에 담아 경고 억제)
                        _ = StartJigRoutine();
                    }
                }

                // 3. STOP 신호 처리
                if (cmd == CMD_JIG_SIGNAL && item == ITEM_JIG_STOP)
                {
                    if (!_isTestRunning) return;

                    // 취소 토큰 실행
                    _cts?.Cancel();

                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(async () => await StopJigRoutine()));
                    }
                    else
                    {
                        _ = StopJigRoutine();
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Jig_OnPacketReceived Error: {ex.Message}";
                Console.WriteLine(errorMsg);
                JigLogger.Info(errorMsg);
            }
        }

        private async Task StartJigRoutine()
        {
            if (_cts == null) _cts = new CancellationTokenSource();
            var jig = CommManager.Jig;

            try
            {
                // 1. 제품 안착 감지
                byte[] tx1 = new JigProtocol(CMD_JIG_CONTROL, ITEM_JIG_PRODUCT_DETECT).GetPacket();
                byte[] rx1 = await jig.SendAndReceivePacketAsync(tx1, WAIT_PRODUCT_DETECT, _cts.Token);
                if (!UtilityFunctions.CheckJigRxData(tx1, rx1))
                {
                    JigLogger.Fail("JIG product detect fail");
                    await StopJigRoutine();
                    return;
                }

                // 채널별 안착 상태 (0: 없음, 1: 있음 이라고 가정)
                byte[] detect_ch = new byte[] { rx1[7], rx1[8], rx1[9], rx1[10] };
                Console.WriteLine($"CH1 : {rx1[7]} CH2 : {rx1[8]} CH3 : {rx1[9]} CH4 : {rx1[10]}");
            }
            catch (Exception ex)
            {
                JigLogger.Fail("START JIG ROUTINE FAIL" + ex.Message);
            }
            
        }



        private async Task StopJigRoutine()
        {
            var jig = CommManager.Jig;

            try
            {
                byte[] tx = new JigProtocol(CMD_JIG_SIGNAL, ITEM_JIG_STOP).GetPacket();
                byte[] rx = await jig.SendAndReceivePacketAsync(tx, JIG_READ_DELAY);

                _cts.Cancel();
            }
            catch (Exception ex)
            {
                JigLogger.Fail("JIG STOP COMMAND FAIL" + ex.Message);
            }
        }

        private void Init_ChControls()
        {
            chControl_ch1.Init(1);
            chControl_ch2.Init(2);
            chControl_ch3.Init(3);
            chControl_ch4.Init(4);
        }

        private void Init_McuLotModel_Value()
        {
            lblValueMcuNo.Text = Settings.Instance.Mc_No;
            lblValueLotNo.Text = Settings.Instance.Lot_No;
            lblValueModel.Text = Settings.Instance.Model_No;
        }

        public void UpdateLayoutByChannelConfig()
        {
            bool is1ChOnly = Settings.Instance.Use_1CH_Only;

            

            tableLayoutPanel_Channels.SuspendLayout(); // 깜빡임 방지
            if (is1ChOnly)
            {
                Console.WriteLine("1CH 레이아웃 설정.");
                // 1채널만 크게 보여주고 나머지는 숨기는 로직
                chControl_ch2.Visible = false;
                chControl_ch3.Visible = false;
                chControl_ch4.Visible = false;

                int totalColums = tableLayoutPanel_Channels.ColumnCount;
                tableLayoutPanel_Channels.SetColumnSpan(chControl_ch1, totalColums);

                tableLayoutPanel_Channels.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                
            }
            else
            {
                Console.WriteLine("다채널 레이아웃 설정.");

                // 다시 4채널 모드로 복구 (기존 배치가 TableLayoutPanel이라면 컨트롤만 다시 보이게)

                tableLayoutPanel_Channels.SetColumnSpan(chControl_ch1, 1);
                chControl_ch2.Visible = true;
                chControl_ch3.Visible = true;
                chControl_ch4.Visible = true;

                tableLayoutPanel_Channels.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            }

            tableLayoutPanel_Channels.ResumeLayout();
        }

        private void ToggleLogComm(TcpChannelClient board, bool enable, RichTextLogger logger)
        {
            if (board == null) return;

            if (enable)
            {
                
                board.LogCommToUI = (tbox, msg, isTx) =>
                {
                    if (isTx) logger.Tx(msg);
                    else logger.Rx(msg);
                };
            }
            else
            {
                board.LogCommToUI = null;
                board.LogTarget = null;
            }
        }

        public void ToggleJigComm(JigPort jig, bool enable, RichTextLogger logger)
        {
            if (jig == null || !jig.IsOpen) // 연결안되면 자동 끊기
            {
                jig.LogCommToUI = null;
                jig.LogTarget = null;
            }
            

            if (enable)
            {
                jig.LogCommToUI = (tbox, msg, isTx) =>
                {
                    if (isTx) logger.Tx(msg);
                    else logger.Rx(msg);
                };

                logger.Initialize();
            }
            else // 일부로 끊기
            {
                jig.LogCommToUI = null;
                jig.LogTarget = null;
            }
        }

        private void ToggleLogComm(SerialChannelPort device, bool enable, RichTextLogger logger)
        {
            if (device == null) return;

            if (enable)
            {
                // device.LogTarget = ... (SerialPort도 필요하다면 설정)
                device.LogCommToUI = (tbox, msg, isTx) =>
                {
                    if (isTx) logger.Tx(msg);
                    else logger.Rx(msg);
                };
            }
            else
            {
                device.LogCommToUI = null;
            }
        }

        private void ToggleLogComm(QrChannelPort Qr, bool enable, RichTextLogger logger)
        {
            if (Qr == null) return;

            if (enable)
            {
                Qr.LogCommToUI = (tbox, msg, isTx) =>
                {
                    if (isTx) logger.Tx(msg);
                    else logger.Rx(msg);
                };
            }
            else
            {
                Qr.LogCommToUI = null;
            }
        }


        #endregion

        #region Event
        private void ConnectEvent()
        {
            //Application.ApplicationExit += OnApplicationExit; //이거 안되는 듯?
            CommManager.RecipeQr.OnRecipePathReceived += RecipeQr_OnRecipePathReceived;
            this.FormClosing += MainForm_FormClosing;
            this.btnComSettingsOpen.Click += btnComSettingsOpen_Click;
            this.btnRecipeSettingsOpen.Click += btnRecipeSettingsOpen_Click;
            this.btnCalibrationOpen.Click += btnCalibrationOpen_Click;
            this.btnManualOpen.Click += btnManualOpen_Click;
            this.lblValueMcuNo.DoubleClick += lblValueMcuNo_DoubleClick;
            this.lblValueLotNo.DoubleClick += lblValueLotNo_DoubleClick;
            this.lblValueModel.DoubleClick += lblValueModel_DoubleClick;
            this.btnRecipeOpen.Click += btnRecipeOpen_Click;
            this.btnStartStop.Click += btnStartStop_Click;
            this.lblTextMcuNo.Click += lblTextMcuNo_Click;
        }

        

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CommManager.RecipeQr.OnRecipePathReceived -= RecipeQr_OnRecipePathReceived;
            Settings.Instance.Save();
        }

        //private void OnApplicationExit(object sender, EventArgs e)
        //{
        //    CommManager.RecipeQr.OnRecipePathReceived -= RecipeQr_OnRecipePathReceived;
        //    Settings.Instance.Save();
        //}

        private void btnManualOpen_Click(object sender, EventArgs e)
        {
            var form = new ManualForm()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog();
        }

        private void btnCalibrationOpen_Click(object sender, EventArgs e)
        {
            var form = new CalibrationForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void btnRecipeSettingsOpen_Click(object sender, EventArgs e)
        {
            if (Settings.Instance.USE_FTP)
            {
                MessageBox.Show("FTP 사용 모드에서는 레시피 세팅을 변경할 수 없습니다.\nFTP 설정을 해제해주세요.",
                                "열기 제한", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!RequireLogin(this)) return;

            var form = new RecipeSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };

            if (tboxRecipeFilePath != null && !string.IsNullOrWhiteSpace(tboxRecipeFilePath.Text))
            {
                string path = tboxRecipeFilePath.Text;

                // 경로가 실제 존재하는 파일일 경우에만 전달 (FTP 경로 등은 필터링됨)
                if (File.Exists(path))
                {
                    form.InitialRecipePath = path;
                }
            }

            form.ShowDialog(this);
        }

        private void btnComSettingsOpen_Click(object sender, EventArgs e)
        {
            var form = new ComSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private bool RequireLogin(IWin32Window owner)
        {
            using (var dlg = new LoginForm())
            {
                dlg.StartPosition = FormStartPosition.CenterScreen;
                return dlg.ShowDialog(owner) == DialogResult.OK;
            }
        }

        private void lblValueModel_DoubleClick(object sender, EventArgs e)
        {
            var form = new McLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void lblValueLotNo_DoubleClick(object sender, EventArgs e)
        {
            var form = new McLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void lblValueMcuNo_DoubleClick(object sender, EventArgs e)
        {
            var form = new McLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void lblTextMcuNo_Click(object sender, EventArgs e)
        {
            Console.Clear();
        }



        #endregion

        #region MES

        private enum MesConnState { Off, Ok, Fail }
        private void SettingMesImage()
        {
            // 상태바 기본 설정
            statusStripMes.ImageScalingSize = new Size(16, 16); // ← System.Drawing 필요
            statusStripMes.ShowItemToolTips = true;

            CheckMesStatus();
        }

        public async void CheckMesStatus()
        {
            if (!Settings.Instance.USE_MES)
            {
                SetMesStatus(MesConnState.Off, "MES 비활성화");
                return;
            }

            SetMesStatus(MesConnState.Fail);

            bool ok = await Task.Run(() =>
            {
                try
                {
                    // 세팅값을 리소스 이용해 연결문자열 구성
                    string cs =
                        $@"Provider=SQLOLEDB;Data Source={Settings.Instance.DB_IP},{Settings.Instance.DB_PORT};" +
                        $@"Initial Catalog={Settings.Instance.DB_NAME};User ID={Settings.Instance.DB_USER};" +
                        $@"Password={Settings.Instance.DB_PW};Connect Timeout=5;";

                    using (var conn = new OleDbConnection(cs))
                    {
                        conn.Open(); // 붙기만 확인
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            });

            //SetMesStatus(ok ? MesConnState.Ok : MesConnState.Fail);
            if (ok)
            {
                SetMesStatus(MesConnState.Ok, $"연결 성공: {Settings.Instance.DB_IP}");
            }
            else
            {
                SetMesStatus(MesConnState.Fail, "MES 서버에 연결할 수 없습니다. 설정을 확인하세요.");
            }

        }

        private void SetMesStatus(MesConnState state, string tip = null)
        {
            switch (state)
            {
                case MesConnState.Off:
                    lblMesStatus.Image = Properties.Resources.LED_OFF_SM;
                    lblMesStatus.Text = "MES OFF";
                    break;

                case MesConnState.Ok:
                    lblMesStatus.Image = Properties.Resources.LED_GREEN_SM;
                    lblMesStatus.Text = "MES ON";
                    break;

                case MesConnState.Fail:
                    lblMesStatus.Image = Properties.Resources.LED_RED_SM;
                    lblMesStatus.Text = "MES Connect Fail";
                    break;
            }
            if (!string.IsNullOrEmpty(tip))
                lblMesStatus.ToolTipText = tip;
        }
        #endregion

        #region FTP
        private enum FtpConnState { Off, Ok, Fail }
        private void SettingFtpImage()
        {
            statusStripMes.ImageScalingSize = new Size(16, 16);
            statusStripMes.ShowItemToolTips = true;

            CheckFtpStatus();
        }

        private void SetFtpStatus(FtpConnState state, string tip = null)
        {
            switch (state)
            {
                case FtpConnState.Off:
                    lblFtpStatus.Image = Properties.Resources.LED_OFF_SM;
                    lblFtpStatus.Text = "FTP OFF";
                    break;

                case FtpConnState.Ok:
                    lblFtpStatus.Image = Properties.Resources.LED_GREEN_SM;
                    lblFtpStatus.Text = "FTP ON";
                    break;

                case FtpConnState.Fail:
                    lblFtpStatus.Image = Properties.Resources.LED_RED_SM;
                    lblFtpStatus.Text = "FTP Connect Fail";
                    break;
            }
            if (!string.IsNullOrEmpty(tip))
                lblFtpStatus.ToolTipText = tip;
        }

        public async void CheckFtpStatus()
        {
            // 1. FTP 사용 여부 확인
            if (!Settings.Instance.USE_FTP)
            {
                SetFtpStatus(FtpConnState.Off, "FTP 비활성화 상태");
                return;
            }

            // 포트 번호가 문자열이라 숫자로 변환이 필요 (기본값 21)
            if (!int.TryParse(Settings.Instance.FTP_PORT, out int port))
            {
                port = 21; //변환 실패하면 그냥 기본값 21사용
            }

            // 2. 연결 시도 중 상태 표시 (선택 사항, 필요하면 Fail 등으로 초기화)
            SetFtpStatus(FtpConnState.Fail, "연결 확인 중...");

            // 3. FtpProfiles.TestAsync를 사용하여 연결 테스트 진행
            // SSL은  false로 고정.
            bool isOk = await FtpProfiles.TestAsync(
                Settings.Instance.FTP_HOST,
                port,
                false, // FTP_USE_SSL 대신 false 고정
                Settings.Instance.FTP_BASE_DIR,
                Settings.Instance.FTP_USER,
                Settings.Instance.FTP_PW
            );

            
            // 4. 결과에 따른 UI 업데이트
            if (isOk)
            {
                SetFtpStatus(FtpConnState.Ok, $"연결 성공: {Settings.Instance.FTP_HOST}");
            }
            else
            {
                SetFtpStatus(FtpConnState.Fail, "FTP 서버에 연결할 수 없습니다. 설정을 확인하세요.");
            }


        }


        #endregion

        #region Recipe
        private void btnRecipeOpen_Click(object sender, EventArgs e)
        {
            // [추가] FTP 사용 중이면 로컬 레시피 열기 차단
            if (Settings.Instance.USE_FTP)
            {
                MessageBox.Show("FTP 사용 모드에서는 로컬 레시피 파일을 열 수 없습니다.\nFTP 설정을 해제해주세요.",
                                "열기 제한", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            openFileDialog.Title = "레시피 파일 열기";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);

                SettingNowTask(filePath, fileName);
            }
        }

        /// <summary> RecipeQrPort RX 수신 시 호출. FTP 사용 중이면 해당 경로의 파일을 FTP에서 다운로드 후 SettingNowTask 수행 </summary>
        private async void RecipeQr_OnRecipePathReceived(string pathLine)
        {
            if (string.IsNullOrWhiteSpace(pathLine)) return;
            string trimmed = pathLine.Trim();
            if (!Settings.Instance.USE_FTP) return;

            try
            {
                if (!int.TryParse(Settings.Instance.FTP_PORT, out int port)) port = 21;

                string json = await FtpProfiles.DownloadJsonAsync(
                    Settings.Instance.FTP_HOST,
                    port,
                    false,
                    Settings.Instance.FTP_BASE_DIR ?? "",
                    Settings.Instance.FTP_USER ?? "",
                    Settings.Instance.FTP_PW ?? "",
                    trimmed
                );

                if (string.IsNullOrEmpty(json))
                {
                    BeginInvoke(new Action(() =>
                        MessageBox.Show("FTP에서 파일을 읽을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    return;
                }

                string fileName = Path.GetFileName(trimmed);
                string displayPath = $"FTP: {trimmed}";
                BeginInvoke(new Action(() => SettingNowTaskFromContent(json, fileName, displayPath)));
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                    MessageBox.Show($"FTP 레시피 로드 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
        }

        //JSON에 저장된 키 이름과 Settings의 프로퍼티이름이 같아야함!
        private void SettingNowTask(string filePath, string fileName)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                SettingNowTaskFromContent(json, fileName, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"레시피 적용 중 오류 발생: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SettingNowTaskFromContent(string json, string fileName, string displayPath)
        {
            try
            {
                JObject recipe = JObject.Parse(json);

                // 1. 파일 형식 검사
                if (recipe["Type"]?.ToString() != "Recipe")
                {
                    MessageBox.Show("유효하지 않은 레시피 파일입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                JObject settingsJson = recipe["Settings"] as JObject;
                JArray taskOrderJson = recipe["TaskOrder"] as JArray;

                if (settingsJson == null || taskOrderJson == null)
                {
                    MessageBox.Show("설정값 또는 순서 정보가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. Settings.Instance에 레시피 값 반영
                // 누락된 항목은 레시피 기본값을 사용하고, 알 수 없는 예전 필드는 무시한다.
                var instance = Settings.Instance;
                Type settingsType = typeof(Settings);
                Type recipeType = typeof(RecipeLocalBuffer);
                var defaultRecipe = new RecipeLocalBuffer();

                foreach (PropertyInfo recipeProp in recipeType.GetProperties())
                {
                    PropertyInfo settingsProp = settingsType.GetProperty(recipeProp.Name);
                    if (settingsProp == null || !settingsProp.CanWrite)
                        continue;

                    object value = recipeProp.GetValue(defaultRecipe);
                    JToken token = settingsJson[recipeProp.Name];

                    if (token != null)
                    {
                        try
                        {
                            value = token.ToObject(settingsProp.PropertyType);
                        }
                        catch
                        {
                            Console.WriteLine($"값 적용 실패: {recipeProp.Name}");
                        }
                    }

                    settingsProp.SetValue(instance, value);
                }

                // 3. 실행할 Task 목록 생성 (Enable 체크 된 것만 순서대로 담기)
                List<string> newTaskList = new List<string>();

                foreach (var taskToken in taskOrderJson)
                {
                    string taskName = taskToken.ToString(); // 예: "QR READ"

                    // Task 이름으로 Enable 속성 찾기 (예: "QR READ" -> "QR_READ_Enable")
                    // 공백을 언더바(_)로 바꾸고 뒤에 _Enable을 붙이는 규칙 사용
                    string enablePropName = taskName.Replace(" ", "_") + "_Enable";

                    PropertyInfo piEnable = settingsType.GetProperty(enablePropName);

                    if (piEnable != null)
                    {
                        bool isEnabled = (bool)piEnable.GetValue(instance);
                        if (isEnabled)
                        {
                            newTaskList.Add(taskName);
                        }
                    }
                }

                // 4. 결과 저장 및 UI 반영

                instance.RunTaskList = newTaskList;
                instance.CurrentRecipeFile = fileName;
                instance.CurrentRecipeFilePath = displayPath;
                instance.Save(); // config.json에도 저장

                if (tboxRecipeFile != null) tboxRecipeFile.Text = instance.CurrentRecipeFile;
                if (tboxRecipeFilePath != null) tboxRecipeFilePath.Text = displayPath;

                string msg = $"레시피 적용 완료!\n파일명: {instance.CurrentRecipeFile}\n총 공정 수: {instance.RunTaskList.Count}개";
                MessageBox.Show(msg, "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (instance.RunTaskList != null && instance.RunTaskList.Count > 0)
                {
                    var chControls = new ChControl[] { chControl_ch1, chControl_ch2, chControl_ch3, chControl_ch4 };
                    bool[] Ch_Enables = new bool[] { instance.Use_CH1, instance.Use_CH2, instance.Use_CH3, instance.Use_CH4 };
                    for (int i = 0; i < chControls.Length; i++)
                    {
                        if (Ch_Enables[i])
                        {
                            chControls[i].SetRecipeList(instance.RunTaskList);
                            chControls[i].UpdateNowStatus(ChControl.NowStatus.READY);
                        }
                        else chControls[i].SetRecipeList(null); //사용상태가 아닌 채널은 리스트를 비워줌.
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"레시피 적용 중 오류 발생: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ResetRecipeInfo()
        {
            // 1. UI 텍스트박스 비우기
            if (tboxRecipeFile != null) tboxRecipeFile.Text = string.Empty;
            if (tboxRecipeFilePath != null) tboxRecipeFilePath.Text = string.Empty;

            // 2. 내부 설정값(Settings) 초기화
            Settings.Instance.CurrentRecipeFile = "";
            Settings.Instance.RunTaskList = null; // 리스트 날리기
            Settings.Instance.Save();

            // 3. 각 채널 그리드(리스트) 비우기
            var chControls = new ChControl[] { chControl_ch1, chControl_ch2, chControl_ch3, chControl_ch4 };
            foreach (var ch in chControls)
            {
                ch.ClearAllData();
            }
        }

        #endregion

        #region Main Task
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (_isTestRunning)
            {
                // 실행 중이면 -> 정지 요청
                StopTestRoutine();
            }
            else
            {
                // 정지 상태면 -> 시작 요청
                StartTestRoutine();
            }
        }

        private async void StartTestRoutine()
        {
            if (string.IsNullOrEmpty(tboxRecipeFile.Text))
            {
                MessageBox.Show("Recipe를 선택해주세요");
                return;
            }
            // 이미 돌고 있으면 무시
            if (_isTestRunning) return;

            // 1) 상태 플래그 설정
            _isTestRunning = true;
            if (_cts == null) _cts = new CancellationTokenSource(); // 취소 토큰 생성

            // 2) UI 변경 (버튼을 STOP으로, 로딩바 마퀴 시작)
            btnStartStop.Text = "STOP";
            btnStartStop.BackColor = Color.IndianRed;
            btnStartStop.ForeColor = Color.Black;
            StartLoadingAnimation();

            // 3) 설정된 채널들만 테스트 시작
            var chControls = new ChControl[] { chControl_ch1, chControl_ch2, chControl_ch3, chControl_ch4 };
            bool[] chEnables = new bool[] { Settings.Instance.Use_CH1, Settings.Instance.Use_CH2, Settings.Instance.Use_CH3, Settings.Instance.Use_CH4 };

            List<Task> runningTasks = new List<Task>();

            for (int i = 0; i < chControls.Length; i++)
            {
                if (chEnables[i])
                {
                    chControls[i].ClearAllTbox();
                    // 각 채널별로 비동기 검사 시작
                    runningTasks.Add(RunSequenceAsync(chControls[i], i, _cts.Token));
                }
            }

            // 4) 모든 채널이 끝날 때까지 대기
            try
            {
                await Task.WhenAll(runningTasks);
            }
            catch (OperationCanceledException)
            {
                // Stop 버튼 눌러서 취소된 경우
                Console.WriteLine("Test Stopped by User/JIG");
            }
            finally
            {
                // 5) 모든 작업이 끝나면(혹은 취소되면) 마무리
                StopTestRoutine(isFinished: true);
            }
        }

        private void StopTestRoutine(bool isFinished = false)
        {
            // 사용자 강제 중단인 경우 토큰 취소 요청
            if (!isFinished && _cts != null)
            {
                _cts.Cancel();
            }

            _isTestRunning = false;

            // 로딩바 정지
            StopLoadingAnimation();

            // UI 원상복구 (버튼을 START로)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => StopTestRoutine(true)));
                return;
            }

            btnStartStop.Text = "START";
            btnStartStop.BackColor = Color.DarkSeaGreen; // 원래 색상 (디자이너 설정값 확인 필요)
            btnStartStop.ForeColor = Color.Black;
        }

        // ★★★ 4. 핵심 시퀀스 로직 (각 채널별로 돌아가는 함수) ★★★
        private async Task RunSequenceAsync(ChControl ch, int chIdx, CancellationToken token)
        {
            var board = CommManager.Boards[chIdx];   // TCP
            var pba = CommManager.Pbas[chIdx];       // Serial
            var qr = CommManager.QrPorts[chIdx];     // QR

            // ★ MesData 인스턴스 생성 + 공통값 세팅
            var mesData = new MesData();
            mesData.EQUIPMENT_ID = Settings.Instance.Mc_No;           // 장비 고유 ID
            mesData.TESTER_VER = Settings.Instance.Tester_Ver;             // 테스터 S/W 버전
            mesData.MODEL = Settings.Instance.Model_No;
            mesData.WORK_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            mesData.CHANNEL = $"{chIdx + 1}";
            mesData.SPEC_FILE = Settings.Instance.CurrentRecipeFilePath;

            List<string> ngItems = new List<string>();    // NG 항목 수집용
            try
            {
                //로그 연결
                ToggleLogComm(board, true, ch.CommLogger);
                ToggleLogComm(pba, true, ch.CommLogger);
                ToggleLogComm(qr, true, ch.CommLogger);

                // 1) 시작 전 초기화
                ch.StartInspection();

                List<string> taskList = Settings.Instance.RunTaskList;
                

                if (taskList == null || taskList.Count == 0)
                {
                    ch.StopInspection();
                    return;
                }


                bool totalResult = true; // 전체 결과 (하나라도 실패하면 false)

                // 2) 공정 리스트 순회
                for (int step = 0; step < taskList.Count; step++)
                {
                    await Task.Delay(100, token); //공정 사이 딜레이

                    // 스탑 버튼 눌렸는지 체크
                    if (token.IsCancellationRequested)
                    {
                        ch.StopInspection();
                        ch.Logger.Fail($"[CH{chIdx + 1}] Test is stopped.");
                        return;
                    }

                    string taskName = taskList[step];

                    // 2-1) 현재 단계 노란색(TESTING) 표시
                    ch.UpdateItemStatus(step, ChControl.TaskStatus.TESTING);

                    // 2-2) 실제 검사 함수 호출 (Data/TaskFunctions.cs)
                    bool isPass = await TaskFunctions.RunTestItem(taskName, chIdx, ch, token, totalResult, mesData);


                    // 2-3) 결과 처리
                    if (isPass)
                    {
                        ch.UpdateItemStatus(step, ChControl.TaskStatus.PASS); // 초록색
                    }
                    else
                    {
                        ch.UpdateItemStatus(step, ChControl.TaskStatus.FAIL);
                        totalResult = false;
                        ngItems.Add(taskName);

                        //await Init_Board(chIdx, ch, token);

                        if (!Settings.Instance.Use_Debug_Mode)
                        {
                            for (int remainStep = step + 1; remainStep < taskList.Count; remainStep++)
                            {
                                ch.UpdateItemStatus(remainStep, ChControl.TaskStatus.FAIL);
                            }

                            break;
                        }
                    }

                }

                


                // ★★★ 검사 끝 → DB 전송 ★★★
                if (Settings.Instance.USE_MES)
                {
                    ch.Logger.Section("MES");
                    mesData.TOTAL_JUDGMENT = totalResult ? "PASS" : "FAIL";
                    mesData.NG_ITEM = string.Join(",", ngItems);
                    mesData.TACT_TIME = ch.GetElapsedTime();

                    bool mesResult = true;
                    try
                    {
                        await mesData.InsertAsync(ct: CancellationToken.None);
                        ch.Logger.Pass($"Complete data transfer to database");
                    }
                    catch (Exception ex)
                    {
                        ch.Logger.Fail($"Data transfer to database failed : {ex.Message}");
                        mesResult = false;
                        totalResult = false;
                    }
                    ch.Logger.ResultSection("MES", mesResult);
                    mesData.MES_RESULT = mesResult ? "PASS" : "FAIL";
                }
                else
                {
                    mesData.TOTAL_JUDGMENT = totalResult ? "PASS" : "FAIL";
                    mesData.NG_ITEM = string.Join(",", ngItems);
                    mesData.TACT_TIME = ch.GetElapsedTime();
                    mesData.MES_RESULT = "SKIP";
                }


                bool csvOk = mesData.SaveResultToCsv();
                if (csvOk)
                {
                    ch.Logger.NewLine();
                    ch.Logger.Info($"Log file save complete");
                }
                else
                {
                    ch.Logger.NewLine();
                    ch.Logger.Info($"Log file save fail");
                }




                //string ngDetail = ngItems.Count > 0 ? $"NG: {string.Join(", ", ngItems)}" : "";
                ch.Logger.TotalResult(totalResult);

                ch.EndInspection(totalResult);
            }
            catch (OperationCanceledException)
            {
                ch.StopInspection();
                ch.Logger.Info($"[CH{chIdx + 1}] Test stopped.");
            }
            finally
            {
                try
                {
                    await Init_Board(chIdx, ch, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TESTER INIT 실패 : {ex.Message}[{chIdx + 1}]");
                }
                ToggleLogComm(board, false, null);
                ToggleLogComm(pba, false, null);
                ToggleLogComm(qr, false, null);
            }
        }

        private static async Task Init_Board(int ch, ChControl control, CancellationToken token)
        {
            var Board = CommManager.Boards[ch];

            if (!Board.IsConnected())
            {
                Console.WriteLine($"TCP가 연결되어있지 않습니다. [CH{ch + 1}]");
                return;
            }

            try
            {
                await Task.Delay(10);

                byte[] tx = new TcpProtocol(0x01, 0x00).GetPacket();
                int delay = Settings.Instance.Board_Read_Timeout;
                Console.WriteLine($"BOARD INIT START CMD RX 수신 대기 [Delay : {delay}ms] [CH{ch + 1}]");

                byte[] rx = await Board.SendAndReceivePacketAsync(tx, delay, token);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"TESTER INITIALIZE CMD RX 에러");
                    return;
                }
                Console.WriteLine($"TESTER INIT 완료 [CH {ch + 1}]");


                return;

            }
            catch (OperationCanceledException)
            {
                control.UpdateNowStatus(ChControl.NowStatus.STOP);
                throw;
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{ch + 1}CH] BOARD INIT 예외: {ex.Message}";
                Console.WriteLine(errorMsg);
                return;
            }
        }








        #endregion
    }
}
