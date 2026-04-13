using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace p2_40_Main_PBA_Tester.UI
{
    public class RichTextLogger
    {
        private readonly RichTextBox _box;
        private readonly int _maxLines;

        // Win32 API: 텍스트 추가 시 화면 깜빡임 방지용
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        public RichTextLogger(RichTextBox box, int maxLines = 10000)
        {
            _box = box;
            _maxLines = maxLines;
        }

        public void Initialize()
        {
            if (_box == null || _box.IsDisposed) return;

            _box.ReadOnly = true;
            _box.BackColor = Color.White;
            _box.Font = new Font("맑은 고딕", 10f);
            _box.HideSelection = false; // 포커스가 없어도 선택 영역 유지
        }



        private void UI(Action action)
        {
            if (_box == null || _box.IsDisposed) return;
            if (_box.InvokeRequired) _box.Invoke(action);
            else action();
        }

        private void Append(string text, Color color, bool isBold = false, Color? backColor = null)
        {
            _box.SelectionStart = _box.TextLength;
            _box.SelectionLength = 0;


            _box.SelectionColor = color;
            if (backColor.HasValue)
                _box.SelectionBackColor = backColor.Value;
            else
                _box.SelectionBackColor = _box.BackColor;

            if (isBold) _box.SelectionFont = new Font(_box.Font, FontStyle.Bold);
            else _box.SelectionFont = new Font(_box.Font, FontStyle.Regular);

            _box.AppendText(text);

            TrimLines();

            SendMessage(_box.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
        }

        private void TrimLines()
        {
            if (_maxLines > 0 && _box.Lines.Length > _maxLines)
            {
                // 윗부분 삭제 로직
                int firstLineEndPos = _box.GetFirstCharIndexFromLine(_box.Lines.Length - _maxLines);
                if (firstLineEndPos > 0)
                {
                    _box.ReadOnly = false;
                    _box.Select(0, firstLineEndPos);
                    _box.SelectedText = "";
                    _box.ReadOnly = true;
                }
            }
        }

        private string TimeStamp => $"[{DateTime.Now:HH:mm:ss:fff}]  ";

        public void Info(string msg)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append(msg + Environment.NewLine, Color.Black);
            });
        }

        public void Pass(string msg)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append("PASS ", Color.Green, true);
                Append(msg + Environment.NewLine, Color.Black);
            });
        }

        public void Fail(string msg)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append("FAIL ", Color.Red, true);
                Append(msg + Environment.NewLine, Color.Black);
            });
        }

        public void Comm(string msg, bool isTx)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append(isTx ? "[Tx] " : "[Rx] ", isTx ? Color.Blue : Color.DarkRed, true);
                string cleanMsg = msg.Replace("\r", "\\r").Replace("\n", "\\n");
                Append(cleanMsg + Environment.NewLine, Color.Black);
            });
        }

        public void Tx(string msg)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append("[Tx] ",Color.Blue, true);
                string cleanMsg = msg.Replace("\r", "\\r").Replace("\n", "\\n");
                Append(cleanMsg + Environment.NewLine, Color.Black);
            });
        }

        public void Rx(string msg)
        {
            UI(() => {
                Append(TimeStamp, Color.Gray);
                Append("[Rx] ", Color.DarkRed, true);
                string cleanMsg = msg.Replace("\r", "\\r").Replace("\n", "\\n");
                Append(cleanMsg + Environment.NewLine, Color.Black);
            });
        }

        public void Section(string taskName)
        {
            UI(() => {
                Append("\n", Color.Black); // 이전 로그와 간격 벌리기
                Append($"▶ {taskName} ".PadRight(40, '-'), Color.DarkBlue, true);
                //Append(TimeStamp + Environment.NewLine, Color.Gray);
                Append(Environment.NewLine, Color.Black);
            });
        }

        public void ResultSection(string taskName, bool isPass)
        {
            UI(() => {
                Color resColor = isPass ? Color.Green : Color.Red;
                string status = isPass ? "PASS" : "FAIL";

                // 제목과 같은 스타일로 출력 (Result : PASS -------------------)
                Append($"Result : {status} ", resColor, true);
                //Append(TimeStamp + Environment.NewLine, Color.Gray);
                Append(Environment.NewLine, Color.Black); // 공정 끝났으니 한 줄 더 띄우기
            });
        }

        public void TotalResult(bool isPass, string detail = "")
        {
            UI(() => {
                Color bg = isPass ? Color.Green : Color.Red;
                string status = isPass ? ">> TOTAL RESULT : PASS" : ">> TOTAL RESULT : FAIL";

                Append("\n", Color.Black);
                Append($" {status} ", Color.White, true, bg);
                if (!string.IsNullOrEmpty(detail))
                    Append($"  {detail}", Color.Black);
                //Append(TimeStamp + Environment.NewLine, Color.Gray);
                Append("\n", Color.Black);
            });
        }

        // 1. 단순 줄바꿈만 추가 (기본)
        public void NewLine()
        {
            UI(() =>
            {
                _box.AppendText(Environment.NewLine);
                TrimLines(); // 줄 수 제한 체크
                SendMessage(_box.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            });
        }

        // 2. 여러 줄을 한 번에 띄우고 싶을 때 (확장형)
        public void NewLines(int count)
        {
            if (count <= 0) return;
            UI(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    _box.AppendText(Environment.NewLine);
                }
                TrimLines();
                SendMessage(_box.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            });
        }

        public void Clear() => UI(() => _box.Clear());

    }
}
