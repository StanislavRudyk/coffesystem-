using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Cafe.Client.Admin.Forms
{
    public class SovereignAlert : Form
    {
        private string _title;
        private string _message;

        public SovereignAlert(string message, string title = "СИСТЕМНЕ ПОВІДОМЛЕННЯ")
        {
            _title = title;
            _message = message;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(420, 220);
            this.BackColor = Color.FromArgb(25, 27, 34);
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            this.Load += (s, e) => {
                var path = SovereignEngine.GetRoundRect(new Rectangle(0, 0, Width, Height), 20);
                this.Region = new Region(path);
            };

            var lblTitle = new Label
            {
                Text = _title.ToUpper(),
                Font = SovereignEngine.GetFont("Montserrat Bold", 11f),
                ForeColor = SovereignEngine.AmberAccent,
                AutoSize = true,
                Location = new Point(30, 30),
                BackColor = Color.Transparent
            };

            var lblMessage = new Label
            {
                Text = _message,
                Font = SovereignEngine.GetFont("Montserrat", 10f),
                ForeColor = Color.FromArgb(210, 215, 225),
                AutoSize = false,
                Size = new Size(360, 80),
                Location = new Point(30, 70),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            var btnOk = new Button
            {
                Text = "ЗРОЗУМІЛО",
                Bounds = new Rectangle(145, 160, 130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = SovereignEngine.AmberAccent,
                ForeColor = Color.Black,
                Font = SovereignEngine.GetFont("Montserrat Bold", 9f),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Region = new Region(SovereignEngine.GetRoundRect(new RectangleF(0, 0, 130, 40), 10));
            btnOk.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnOk);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 2f))
            {
                var rect = new Rectangle(1, 1, Width - 3, Height - 3);
                using (var path = SovereignEngine.GetRoundRect(rect, 20))
                {
                    g.DrawPath(p, path);
                }
            }
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
                Form parent = Application.OpenForms.Cast<Form>().LastOrDefault();
                alert.ShowDialog(parent);
            }
        }
    }
}
