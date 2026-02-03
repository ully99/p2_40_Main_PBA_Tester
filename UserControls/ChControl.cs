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
using p2_40_Main_PBA_Tester.UI;
namespace p2_40_Main_PBA_Tester.UserControls
{
    public partial class ChControl : UserControl
    {
        public int ChannelIndex { get; private set; } // 1~4
        public RichTextLogger Logger { get; private set; }
        public RichTextLogger CommLogger { get; private set; }

        private System.Diagnostics.Stopwatch _stepWatch = new System.Diagnostics.Stopwatch();
        private System.Windows.Forms.Timer _displayTimer = new System.Windows.Forms.Timer();

        public enum NowStatus { READY, RUNNING, PASS, FAIL, STOP }
        public enum TaskStatus { READY, RUNNING, PASS, FAIL, STOP }

        public ChControl()
        {
            InitializeComponent();
        }
        public void Init(int channelIndex)
        {
            ChannelIndex = channelIndex;

            Logger = new RichTextLogger(tboxLog);
            CommLogger = new RichTextLogger(tboxComm);
            Logger.Initialize();
            CommLogger.Initialize();

            InitTimer();

            BindUseStatus();
            BindCounts();

            InitGrid();
        }

        public void ClearAllData()
        {
            // 1. 그리드(레시피) 비우기
            SetRecipeList(null);

            // 2. 로그 비우기 (필요하면)
            if (tboxLog != null) tboxLog.Clear();
            if (tboxComm != null) tboxComm.Clear();

            // 3. 상태 라벨 초기화

            lblTaskStatus.Text = "-"; // 혹은 READY
            lblTaskStatus.BackColor = Color.White;

        }

        public void InitGrid()
        {
            dgvTaskList.Columns.Clear();

            // 컬럼 1개: "Test Item" (이름은 맘대로)
            dgvTaskList.Columns.Add("colItem", "검사 항목");
            dgvTaskList.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // 꽉 차게

            // 디자인 다듬기
            dgvTaskList.RowHeadersVisible = false;      // 왼쪽 화살표 헤더 숨김
            dgvTaskList.AllowUserToAddRows = false;     // 빈 행 추가 방지
            dgvTaskList.ReadOnly = true;                // 수정 불가
            dgvTaskList.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // 셀 말고 줄 전체 선택
            dgvTaskList.MultiSelect = false;            // 다중 선택 금지
            dgvTaskList.BackgroundColor = Color.White;  // 배경색 깔끔하게

            // (선택) 헤더도 보기 싫으면 아래 주석 해제
            dgvTaskList.ColumnHeadersVisible = false;

        }

        public void InitTimer()
        {
            _displayTimer.Interval = 1000; // 1초
            _displayTimer.Tick += (s, e) =>
            {
                lblTaskTime.Text = _stepWatch.Elapsed.ToString(@"mm/:ss");
            };
        }

        public void SetRecipeList(List<string> taskList) //작업 리스트 세팅
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetRecipeList(taskList)));
                return;
            }

            dgvTaskList.Rows.Clear();

            if (taskList == null) return;

            foreach (var task in taskList)
            {
                // 컬럼이 하나니까 값도 하나만 넣으면 됨
                dgvTaskList.Rows.Add(task);
            }

            dgvTaskList.ClearSelection(); // 처음에 선택된 거 없게
        }

        public void UpdateItemStatus(int stepIndex, TaskStatus status) // 작업 상태 세팅
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateItemStatus(stepIndex, status)));
                return;
            }

            // 범위 체크
            if (stepIndex < 0 || stepIndex >= dgvTaskList.Rows.Count) return;

            var row = dgvTaskList.Rows[stepIndex];

            // 상태별 색상 칠하기 (텍스트 변경 로직은 제거함)
            switch (status)
            {
                case TaskStatus.RUNNING:
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    // 현재 진행 중인 항목이 보이도록 스크롤 이동
                    dgvTaskList.FirstDisplayedScrollingRowIndex = stepIndex;
                    break;

                case TaskStatus.PASS:
                    row.DefaultCellStyle.BackColor = Color.LimeGreen;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;

                case TaskStatus.FAIL:
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;

                case TaskStatus.READY:
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;
            }
        }

        public void StartInspection()
        {
            _stepWatch.Restart();
            _displayTimer.Start();
            UpdateNowStatus(NowStatus.RUNNING);
        }

        public void StopInspection()
        {
            _stepWatch.Stop();
            _displayTimer.Stop();
            UpdateNowStatus(NowStatus.STOP);
        }

        public void EndInspection(bool Result)
        {
            _stepWatch.Stop();
            _displayTimer.Stop();
            UpdateNowStatus(Result ? NowStatus.PASS : NowStatus.FAIL);
        }
        
        

        private void BindUseStatus()
        {
            var set = Settings.Instance;

            // 기존 바인딩 초기화
            lblUseStatus.DataBindings.Clear();

            // 1. Text 속성 바인딩 ("USE" / "NOT USE")
            var textBinding = lblUseStatus.DataBindings.Add(
                "Text",
                set,
                $"Use_CH{ChannelIndex}",
                true,
                DataSourceUpdateMode.OnPropertyChanged
            );

            textBinding.Format += (s, e) =>
            {
                bool use = (e.Value is bool b) && b;
                e.Value = use ? "USE" : "NOT USE";
            };

            // 2. BackColor 속성 바인딩 (Honeydew / LightGray)
            var colorBinding = lblUseStatus.DataBindings.Add(
                "BackColor",
                set,
                $"Use_CH{ChannelIndex}",
                true,
                DataSourceUpdateMode.OnPropertyChanged
            );

            colorBinding.Format += (s, e) =>
            {
                bool use = (e.Value is bool b) && b;
                // USE(true)면 Honeydew, 아니면 LightGray
                e.Value = use ? Color.Honeydew : Color.LightGray;
            };
        }

        private void BindCounts()
        {
            lblTotalCount.DataBindings.Clear();
            lblOkCount.DataBindings.Clear();
            lblNgCount.DataBindings.Clear();

            var set = Settings.Instance;

            lblTotalCount.DataBindings.Add(
                "Text", set, $"TotalCount_Ch{ChannelIndex}", true, DataSourceUpdateMode.OnPropertyChanged);

            lblOkCount.DataBindings.Add(
                "Text", set, $"OkCount_Ch{ChannelIndex}", true, DataSourceUpdateMode.OnPropertyChanged);

            lblNgCount.DataBindings.Add(
                "Text", set, $"NgCount_Ch{ChannelIndex}", true, DataSourceUpdateMode.OnPropertyChanged);
        }

        public void UpdateNowStatus(NowStatus status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateNowStatus(status)));
                return;
            }

            lblTaskStatus.Text = status.ToString();
            switch (status)
            {
                case NowStatus.RUNNING: lblTaskStatus.BackColor = Color.Yellow; break;
                case NowStatus.PASS: lblTaskStatus.BackColor = Color.Lime; break;
                case NowStatus.FAIL: lblTaskStatus.BackColor = Color.Red; break;
                case NowStatus.READY: lblTaskStatus.BackColor = Color.AliceBlue; break;
                case NowStatus.STOP: lblTaskStatus.BackColor = Color.LightCoral; break;
                default: lblTaskStatus.BackColor = Color.LightGray; break;
            }
        }

        #region Event

        private void lblClearLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tboxLog.Clear();
        }

        private void lblClearComm_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tboxComm.Clear();
        }

        #endregion
    }
}
