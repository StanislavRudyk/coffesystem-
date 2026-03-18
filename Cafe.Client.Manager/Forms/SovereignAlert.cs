using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cafe.Client.Manager.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class SovereignAlert : Form
    {
        private string _title;
        private string _message;

        public SovereignAlert(string message, string title = "СИСТЕМНЕ ПОВІДОМЛЕННЯ")
        {
            _title = title;
            _message = message;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 220);
            BackColor = Color.FromArgb(20, 22, 28);
            ShowInTaskbar = false;
            DoubleBuffered = true;

            var lblTitle = new Label
            {
                Text = _title.ToUpper(),
                Font = SovereignEngine.GetFont("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = SovereignEngine.AmberAccent,
                AutoSize = true,
                Location = new Point(25, 25),
                BackColor = Color.Transparent
            };

            var lblMessage = new Label
            {
                Text = _message,
                Font = SovereignEngine.GetFont("Segoe UI", 10f),
                ForeColor = Color.FromArgb(210, 215, 225),
                AutoSize = false,
                Size = new Size(370, 90),
                Location = new Point(25, 60),
                BackColor = Color.Transparent
            };

            var btnOk = new Label
            {
                Text = "ЗРОЗУМІЛО",
                Font = SovereignEngine.GetFont("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = SovereignEngine.AmberAccent,
                AutoSize = false,
                Size = new Size(130, 36),
                Location = new Point(145, 160),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnOk.Region = new Region(SovereignEngine.GetRoundRect(new RectangleF(0, 0, btnOk.Width, btnOk.Height), 8));

            btnOk.MouseEnter += (s, e) => { btnOk.BackColor = Color.FromArgb(255, 180, 50); };
            btnOk.MouseLeave += (s, e) => { btnOk.BackColor = SovereignEngine.AmberAccent; };
            btnOk.Click += (s, e) => this.Close();

            Controls.Add(lblTitle);
            Controls.Add(lblMessage);
            Controls.Add(btnOk);

            Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
              
                var rect = new RectangleF(0, 0, Width - 1, Height - 1);
                using (var path = SovereignEngine.GetRoundRect(rect, 15))
                {
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, 27, 34)), path);
                    e.Graphics.DrawPath(new Pen(Color.FromArgb(60, 65, 75), 1.5f), path);
                }

                
                using (var path = SovereignEngine.GetRoundRect(new RectangleF(0, 0, Width - 1, Height - 1), 15))
                {
                    e.Graphics.SetClip(path);
                    e.Graphics.FillRectangle(new SolidBrush(SovereignEngine.AmberAccent), 0, 0, Width, 3);
                    e.Graphics.ResetClip();
                }
            };
        }

       
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000;
                return cp;
            }
        }

        public static void Show(string message, string title = "СИСТЕМНЕ ПОВІДОМЛЕННЯ")
        {
            using (var alert = new SovereignAlert(message, title))
            {
                if (Application.OpenForms.Count > 0)
                {
                    alert.ShowDialog(Application.OpenForms[0]);
                }
                else
                {
                    alert.ShowDialog();
                }
            }
        }
    }
}
