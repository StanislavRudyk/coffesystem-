using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Admin.Forms
{
    public class SovereignConfirm : Form
    {
        public static DialogResult Show(string message, string title = "ПІДТВЕРДЖЕННЯ")
        {
            using (var form = new SovereignConfirm(message, title))
            {
                return form.ShowDialog();
            }
        }

        private SovereignConfirm(string message, string title)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SovereignEngine.PanelBase;
            this.DoubleBuffered = true;

            var lblTitle = new Label {
                Text = title,
                ForeColor = SovereignEngine.AmberAccent,
                Font = SovereignEngine.GetFont("Montserrat Bold", 10f),
                Location = new Point(30, 30),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            var lblMsg = new Label {
                Text = message,
                ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Montserrat", 11f),
                Location = new Point(30, 70),
                Size = new Size(340, 60),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(lblMsg);

            var btnYes = new Button {
                Text = "ТАК",
                Size = new Size(100, 40),
                Location = new Point(160, 140),
                FlatStyle = FlatStyle.Flat,
                BackColor = SovereignEngine.AmberAccent,
                ForeColor = Color.Black,
                Font = SovereignEngine.GetFont("Montserrat Bold", 9f)
            };
            btnYes.Click += (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); };
            this.Controls.Add(btnYes);

            var btnNo = new Button {
                Text = "НІ",
                Size = new Size(100, 40),
                Location = new Point(270, 140),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Montserrat Bold", 9f)
            };
            btnNo.Click += (s, e) => { this.DialogResult = DialogResult.No; this.Close(); };
            this.Controls.Add(btnNo);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 2f))
            {
                g.DrawRectangle(p, 1, 1, Width - 2, Height - 2);
            }
        }
    }
}
