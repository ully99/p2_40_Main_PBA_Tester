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
            await CommManager.ConnectAllChannel(Settings.Instance.Board_Connect_Timeout);
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
