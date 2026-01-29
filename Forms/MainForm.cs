using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using p2_40_Main_PBA_Tester.Forms;
using p2_40_Main_PBA_Tester.Communication;
using p2_40_Main_PBA_Tester.Data;
using p2_40_Main_PBA_Tester.UserControls;
using System.Data.OleDb;

namespace p2_40_Main_PBA_Tester
{
    public partial class MainForm : Form
    {
        #region Init
        public MainForm()
        {
            InitializeComponent();
            ConnectEvent();
        }

        

        private async void MainForm_Load(object sender, EventArgs e)
        {
            Init_ChControls();
            Init_McuLotModel_Value();
            UpdateLayoutByChannelConfig();
            await CommManager.ConnectAllComponent(Settings.Instance.Board_Connect_Timeout);
        }

        private void Init_ChControls()
        {
            chControl_ch1.Init(1);
            chControl_ch2.Init(2);
            chControl_ch3.Init(3);
            chControl_ch4.Init(4);

            Settings.Instance.OkCount_Ch1++;
            Settings.Instance.NgCount_Ch2++;
        }

        private void Init_McuLotModel_Value()
        {
            lblValueMcuNo.Text = Settings.Instance.Mcu_No;
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

        #endregion

        #region Event
        private void ConnectEvent()
        {
            Application.ApplicationExit += OnApplicationExit;
            this.btnComSettingsOpen.Click += btnComSettingsOpen_Click;
            this.btnRecipeSettingsOpen.Click += btnRecipeSettingsOpen_Click;
            this.btnCalibrationOpen.Click += btnCalibrationOpen_Click;
            this.btnManualOpen.Click += btnManualOpen_Click;
            this.lblValueMcuNo.DoubleClick += lblValueMcuNo_DoubleClick;
            this.lblValueLotNo.DoubleClick += lblValueLotNo_DoubleClick;
            this.lblValueModel.DoubleClick += lblValueModel_DoubleClick;
        }

        

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Settings.Instance.Save();
        }

        private void btnManualOpen_Click(object sender, EventArgs e)
        {
            var form = new ManualForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
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
            if (!RequireLogin(this)) return;

            var form = new RecipeSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
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
            var form = new McuLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void lblValueLotNo_DoubleClick(object sender, EventArgs e)
        {
            var form = new McuLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }

        private void lblValueMcuNo_DoubleClick(object sender, EventArgs e)
        {
            var form = new McuLotModelSettingForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.ShowDialog(this);
        }


        #endregion


    }
}
