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
using p2_40_Main_PBA_Tester.Communication;
using p2_40_Main_PBA_Tester.UI;
using p2_40_Main_PBA_Tester.LIB;

namespace p2_40_Main_PBA_Tester.Forms
{
    public partial class ManualForm : Form
    {
        #region Field
        MainForm mainform;
        TcpChannelClient board;

        #endregion


        #region Init
        public ManualForm(MainForm parentform)
        {
            this.mainform = parentform;
            InitializeComponent();
            ConnectEvent();
        }
        private void ManualForm_Load(object sender, EventArgs e)
        {
            cboxSelectChannel.SelectedIndex = 0;
            LoadInputBaseTable();
        }

        

        private void LoadInputBaseTable()
        {
            dgvInputBase.Rows.Clear();

            string[] items = {"PS1_S1 (Volt)", "PS1_S1 (Curr)", "VBUS", "VDD_3_0V",
                "LCD_3_0V","SYS","SYS_3V3", "MCU_3V3",
                "VBOOST", "HT_BOOST",
                "SUB_HT_BOOST_OUT", "KATO_BOOST_OUT", "MOTOR",
                "H_COUNT_CHECK_INT", "KA_COUNT_CHECK_INT",
                "SH_COUNT_CHECK_INT"};

            foreach (var name in items)
            {
                int row = dgvInputBase.Rows.Add();
                dgvInputBase.Rows[row].Cells["colBaseName"].Value = name;
                dgvInputBase.Rows[row].Cells["colValue"].Value = "0";
            }
        }


        #endregion

        #region Utility
        private bool CheckChannel()
        {
            int ch = cboxSelectChannel.SelectedIndex;
            if (ch < 0 || CommManager.Boards[ch] == null || !CommManager.Boards[ch].IsConnected())
            {
                MessageBox.Show("this Channel is not connected!");
                return false;
            }

            return true;
        }

        
        #endregion

        #region Event
        private void ConnectEvent()
        {
            cboxSelectChannel.SelectedIndexChanged += cboxSelectChannel_SelectedIndexChanged;
            btnTcpConnectionCheck.Click += btnTcpConnectionCheck_Click;
            btnOpenPbaTerminal.Click += btnOpenPbaTerminal_Click;
            btnOutputBaseWrite.Click += btnOutputBaseWrite_Click;
            btnSwAllRead.Click += btnSwitchReadAll_Click;
            btnClose.Click += btnClose_Click;
            dgvInputBase.CellContentClick += dgvInputBase_CellContentClick;
            btnTesterInit.Click += btnTesterInit_Click;
            btnTesterReset.Click += btnTesterReset_Click;
            btnTesterFwVerCheck.Click += btnTesterFwVerCheck_Click;

            // btnSw1~40 클릭 이벤트 연결
            for (int i = 1; i <= 40; i++)
            {
                var btn = this.Controls.Find($"btnSw{i}", true).FirstOrDefault() as Button;
                if (btn != null)
                {
                    btn.Click += BtnSw_Click;
                }
            }
        }

        

        private async void BtnSw_Click(object sender, EventArgs e)
        {
            if (!CheckChannel()) return;

            if (sender is Button btn)
            {
                // 버튼 이름에서 번호 추출 (예: "btnSw1" -> 1)
                string btnName = btn.Name;
                if (btnName.StartsWith("btnSw") && int.TryParse(btnName.Substring(5), out int swIndex))
                {
                    // 현재 상태 확인 (LightGreen이면 On)
                    bool isOn_now = btn.BackColor == Color.LightGreen;
                    string nowStatus = isOn_now ? "ON" : "OFF";

                    bool isOn_nextStatus = !isOn_now;
                    string nextStatus = isOn_nextStatus ? "ON" : "OFF";
                    // 장비에 먼저 Write
                    bool success = await WriteSingleSwitch(swIndex - 1, isOn_nextStatus);
                    if (success)
                    {
                        btn.BackColor = isOn_nextStatus ? Color.LightGreen : SystemColors.Control;
                        Console.WriteLine($"{nowStatus} -> {nextStatus} [SW {swIndex}]");
                    }
                    else
                    {
                        Console.WriteLine("스위치 토글 실패");
                    }
                }
            }
        }
        private async void btnTesterInit_Click(object sender, EventArgs e)
        {
            await InitTester();
        }
        private async void btnTesterReset_Click(object sender, EventArgs e)
        {
            await ResetTester();
        }

        private async void btnOutputBaseWrite_Click(object sender, EventArgs e)
        {
            await WriteOutputPs1();
        }

        

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboxSelectChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboxSelectChannel.SelectedIndex)
            {
                case 0:
                    {
                        board = CommManager.Boards[0];
                        break;
                    }
                case 1:
                    {
                        board = CommManager.Boards[1];
                        break;
                    }
                case 2:
                    {
                        board = CommManager.Boards[2];
                        break;
                    }
                case 3:
                    {
                        board = CommManager.Boards[3];
                        break;
                    }
            }

            lblTcpConnectionStatus.Text = "";
            lblTcpConnectionStatus.BackColor = Color.Transparent;
        }

        private void btnTcpConnectionCheck_Click(object sender, EventArgs e)
        {
            if (board != null && board.IsConnected())
            {
                lblTcpConnectionStatus.Text = "Open!";
                lblTcpConnectionStatus.BackColor = Color.LimeGreen;
            }
            else
            {
                lblTcpConnectionStatus.Text = "Not Opened!";
                lblTcpConnectionStatus.BackColor = Color.LightCoral;
            }
        }

        private async void btnTesterFwVerCheck_Click(object sender, EventArgs e)
        {
            await CheckTesterFwVer();
        }

        

        private void btnOpenPbaTerminal_Click(object sender, EventArgs e)
        {
            var form = new PBA_TerminalForm(this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            form.Show(this);
        }

        

        

        private async void btnSwitchReadAll_Click(object sender, EventArgs e)
        {
            await ReadAllSwitch();
        }

        private async void dgvInputBase_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvInputBase.Columns[e.ColumnIndex].Name != "colRead") return;

            await ReadInputBase(e.RowIndex);
        }



        #endregion

        #region Task

        private async Task WriteOutputPs1()
        {
            try
            {
                if (!CheckChannel()) return;

                byte[] DataByteArray = null;
                var text1 = tboxPs1VoltWrite.Text;
                var text2 = tboxPs1CurrWrite.Text;
                if (float.TryParse(text1, out float volt))
                {
                    DataByteArray = BitConverter.GetBytes(volt); //기본 리틀엔디안
                    //Array.Reverse(DataByteArray); //빅엔디안으로 바꾸기
                    Console.WriteLine($"변환된 PS1 Volt 바이트 배열: {BitConverter.ToString(DataByteArray)}");
                }
                else
                {
                    Console.WriteLine($"유효한 float 숫자가 아닙니다. [PS1 Volt]");
                    return;
                }

                if (float.TryParse(text2, out float curr))
                {
                    byte[] TmpByteArray = BitConverter.GetBytes(curr); //기본 리틀엔디안
                    //Array.Reverse(TmpByteArray); //빅엔디안으로 바꾸기
                    Console.WriteLine($"변환된 PS1 Curr 바이트 배열: {BitConverter.ToString(TmpByteArray)}");

                    DataByteArray = DataByteArray.Concat(TmpByteArray).ToArray();
                }
                else
                {
                    Console.WriteLine($"유효한 float 숫자가 아닙니다. [PS1 Curr]");
                    return;
                }

                byte[] tx = new TcpProtocol(0x05, 0x01, DataByteArray).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상 => rx : {rx}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ps1 Write 중 예외 발생! " + ex.Message);
            }
        }
        

        
        private async Task ReadAllSwitch()
        {
            try
            {
                if (!CheckChannel()) return;

                byte[] tx = new TcpProtocol(0x04, 0x00).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상 => rx : {rx}");
                    return;
                }

                byte[] switchBytes = rx.Skip(7).Take(5).Reverse().ToArray();

                for (int i = 0; i < 40; i++)
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;

                    // btnSw1 ~ btnSw40 매핑
                    var btn = this.Controls.Find($"btnSw{i + 1}", true).FirstOrDefault() as Button;

                    if (btn != null && byteIndex < switchBytes.Length)
                    {
                        // 이미지상 LSB가 낮은 번호 스위치이므로 (1 << bitIndex) 로직 유지
                        bool isOn = (switchBytes[byteIndex] & (1 << bitIndex)) != 0;
                        btn.BackColor = isOn ? Color.LightGreen : SystemColors.Control;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
            }

        }

        private async Task WriteAllSwitch()
        {
            try
            {
                if (!CheckChannel()) return;

                byte[] switchBytes = new byte[5];

                for (int i = 0; i < 40; i++)
                {
                    var btn = this.Controls.Find($"btnSw{i + 1}", true).FirstOrDefault() as Button;
                    if (btn != null && btn.BackColor == Color.LightGreen)
                    {
                        int byteIndex = i / 8;
                        int bitIndex = i % 8;

                        // 로컬 배열은 [Num 1, Num 2, Num 3, Num 4, Num 5] 순서로 채워짐
                        switchBytes[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }

                byte[] sendBytes = switchBytes.Reverse().ToArray();
                byte[] tx = new TcpProtocol(0x04, 0x01, sendBytes).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상 => rx : {rx}");
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
            }
        }

        private async Task<bool> WriteSingleSwitch(int switchIndex, bool isOn_nextStatus)
        {
            try
            {
                if (!CheckChannel()) return false;
                if (switchIndex < 0 || switchIndex >= 40) return false;
                // 현재 모든 스위치 상태 읽기
                byte[] switchBytes = new byte[5];
                for (int i = 0; i < 40; i++)
                {
                    var btn = this.Controls.Find($"btnSw{i + 1}", true).FirstOrDefault() as Button;
                    if (btn != null && btn.BackColor == Color.LightGreen)
                    {
                        int byteIdx = i / 8;
                        int bitIdx = i % 8;
                        switchBytes[byteIdx] |= (byte)(1 << bitIdx);
                    }
                }
                
                // 변경할 스위치만 업데이트
                int byteIndex = switchIndex / 8;
                int bitIndex = switchIndex % 8;
                if (isOn_nextStatus) //On 상태로 바꿀거면
                {
                    switchBytes[byteIndex] |= (byte)(1 << bitIndex);
                }
                else //끌거면
                    switchBytes[byteIndex] &= (byte)~(1 << bitIndex);

                byte[] sendBytes = switchBytes.Reverse().ToArray();
                byte[] tx = new TcpProtocol(0x04, 0x01, sendBytes).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);
                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상 => rx : {rx}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
                return false;
            }
        }

        private async Task ReadInputBase(int rowIndex)
        {
            try
            {
                if (!CheckChannel()) return;

                var row = dgvInputBase.Rows[rowIndex];
                string name = row.Cells["colBaseName"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Input Base Name 없음");
                    return;
                }

                byte item;
                bool isPwm;
                //string[] items = {"PS1_S1 (Volt)", "PS1_S1 (Curr)", "VBUS", "VDD_3_0V",
                //"LCD_3_0V","SYS","SYS_3V3", "MCU_3V3",
                //"VBOOST", "HT_BOOST",
                //"SUB_HT_BOOST_OUT", "KATO_BOOST_OUT", "MOTOR",
                //"H_COUNT_CHECK_INT", "KA_COUNT_CHECK_INT",
                //"SH_COUNT_CHECK_INT"};

                if (string.Equals(name, "PS1_S1 (Volt)"))
                {
                    item = 0x01;
                    isPwm = false;
                }
                else if (string.Equals(name, "PS1_S1 (Curr)"))
                {
                    item = 0x02;
                    isPwm = false;
                }
                else if (string.Equals(name, "VBUS"))
                {
                    item = 0x03;
                    isPwm = false;
                }
                else if (string.Equals(name, "VDD_3_0V"))
                {
                    item = 0x04;
                    isPwm = false;
                }
                else if (string.Equals(name, "LCD_3_0V"))
                {
                    item = 0x05;
                    isPwm = false;
                }
                else if (string.Equals(name, "SYS"))
                {
                    item = 0x06;
                    isPwm = false;
                }
                else if (string.Equals(name, "SYS_3V3"))
                {
                    item = 0x07;
                    isPwm = false;
                }
                else if (string.Equals(name, "MCU_3V3"))
                {
                    item = 0x08;
                    isPwm = false;
                }
                else if (string.Equals(name, "VBOOST"))
                {
                    item = 0x09;
                    isPwm = false;
                }
                else if (string.Equals(name, "HT_BOOST"))
                {
                    item = 0x0A;
                    isPwm = false;
                }
                else if (string.Equals(name, "SUB_HT_BOOST_OUT"))
                {
                    item = 0x0B;
                    isPwm = false;
                }
                else if (string.Equals(name, "KATO_BOOST_OUT"))
                {
                    item = 0x0C;
                    isPwm = false;
                }
                else if (string.Equals(name, "MOTOR"))
                {
                    item = 0x0D;
                    isPwm = true;
                }
                else if (string.Equals(name, "H_COUNT_CHECK_INT"))
                {
                    item = 0x0E;
                    isPwm = true;
                }
                else if (string.Equals(name, "KA_COUNT_CHECK_INT"))
                {
                    item = 0x0F;
                    isPwm = true;
                }
                else if (string.Equals(name, "SH_COUNT_CHECK_INT"))
                {
                    item = 0x10;
                    isPwm = true;
                }
                else if (string.Equals(name, "SLEEP"))
                {
                    item = 0x11;
                    isPwm = false;
                }
                else if (string.Equals(name, "SHIP"))
                {
                    item = 0x12;
                    isPwm = false;
                }
                else
                {
                    Console.WriteLine("이 기능은 구현되지 않았습니다.");
                    return;
                }

                byte[] tx = new TcpProtocol(0x06, item).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상: {name}");
                    return;
                }

                if(isPwm)
                {
                    // rx[7]부터 FLOAT 4바이트 (Big Endian)
                    //byte[] floatBytes = rx.Skip(7).Take(4).Reverse().ToArray();
                    byte[] floatBytes = rx.Skip(7).Take(4).ToArray();
                    float value = BitConverter.ToSingle(floatBytes, 0);
                    row.Cells["colValue"].Value = value.ToString("F7");
                }
                else
                {
                    uint value = BitConverter.ToUInt32(rx, 7);
                    row.Cells["colValue"].Value = value.ToString("F7");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"InputBase Read 실패: {ex.Message}");
            }
        }

        private async Task InitTester()
        {
            try
            {
                if (!CheckChannel()) return;

                byte[] tx = new TcpProtocol(0x01, 0x00).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX 이상: {rx}");
                    return;
                }

                Console.WriteLine("Tester 초기화 완료!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"InputBase Read 실패: {ex.Message}");
            }
        }

        private async Task ResetTester()
        {
            try
            {
                if (!CheckChannel()) return;

                Console.WriteLine("소프트웨어 리셋을 시작합니다. (통신 상태에 따라 ACK가 수신되지 않을 수도 있습니다.)");

                byte[] tx = new TcpProtocol(0x01, 0xFF).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX : {rx}");
                    Console.WriteLine($"RX 값이 이상하거나 ACK가 수신되지 않았습니다.");

                    return;
                }

                Console.WriteLine("Tester 리셋 완료!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"InputBase Read 실패: {ex.Message}");
            }
        }

        private async Task CheckTesterFwVer()
        {
            try
            {
                if (!CheckChannel()) return;

                byte[] tx = new TcpProtocol(0x00, 0x00).GetPacket();
                byte[] rx = await board.SendAndReceivePacketAsync(tx, Settings.Instance.Board_Read_Timeout);

                if (!UtilityFunctions.CheckTcpRxData(tx, rx))
                {
                    Console.WriteLine($"RX : {rx}");
                    Console.WriteLine($"RX 값이 이상하거나 ACK가 수신되지 않았습니다.");

                    return;
                }

                byte year = rx[7];
                byte month = rx[8];
                byte day = rx[9];
                byte Ver_high = rx[10];
                byte Ver_low = rx[11];

                string result = $"20{year}년 {month}월 {day}일 Version {Ver_high}.{Ver_low}";
                lblTesterFwVer.Text = result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Check Tester Fw Ver 실패: {ex.Message}");
            }
        }






        #endregion


    }
}
