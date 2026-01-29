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
        }

        public void InitTimer()
        {
            _displayTimer.Interval = 1000; // 1초
            _displayTimer.Tick += (s, e) =>
            {
                lblTaskTime.Text = _stepWatch.Elapsed.ToString(@"mm/:ss");
            };
        }

        public void StartInspection()
        {
            _stepWatch.Restart();
            _displayTimer.Start();
        }

        public void StopInspection()
        {
            _stepWatch.Stop();
            _displayTimer.Stop();
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

        public void UpdateTaskStatus(TaskStatus status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateTaskStatus(status)));
                return;
            }

            lblTaskStatus.Text = status.ToString();
            switch (status)
            {
                case TaskStatus.RUNNING: lblTaskStatus.BackColor = Color.Yellow; break;
                case TaskStatus.PASS: lblTaskStatus.BackColor = Color.Lime; break;
                case TaskStatus.FAIL: lblTaskStatus.BackColor = Color.Red; break;
                case TaskStatus.READY: lblTaskStatus.BackColor = Color.AliceBlue; break;
                default: lblTaskStatus.BackColor = Color.LightGray; break;
            }
        }

    }
}
