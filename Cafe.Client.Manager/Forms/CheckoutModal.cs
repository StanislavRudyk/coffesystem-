using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CheckoutModal : Form
    {
        private decimal _total;
        private bool _isCard = true;
        private TextBox _txtComments;

        public CheckoutModal(decimal total)
        {
            _total = total;
            

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 600);
            this.BackColor = SovereignEngine.ObsidianDeep;
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;


            var lblTitle = new Label { 
                Text = "ОФОРМЛЕННЯ ЗАМОВЛЕННЯ", 
                Font = SovereignEngine.GetFont("Montserrat Bold", 14f), 
                ForeColor = SovereignEngine.AmberAccent,
                AutoSize = true,
                Location = new Point(40, 40)
            };
            Controls.Add(lblTitle);

            var lblTotalScale = new Label { 
                Text = "ДО СПЛАТИ:", 
                Font = SovereignEngine.GetFont("Segoe UI Semibold", 10f), 
                ForeColor = SovereignEngine.SmokeText,
                AutoSize = true,
                Location = new Point(40, 90)
            };
            Controls.Add(lblTotalScale);

            var lblTotal = new Label { 
                Text = $"{_total:N2} ₴", 
                Font = SovereignEngine.GetFont("Consolas", 36f, FontStyle.Bold), 
                ForeColor = SovereignEngine.PearlText,
                AutoSize = true,
                Location = new Point(36, 120)
            };
            Controls.Add(lblTotal);


            var btnClose = new Label {
                Text = "×",
                Font = new Font("Segoe UI Semilight", 22f),
                ForeColor = Color.FromArgb(120, SovereignEngine.PearlText),
                Size = new Size(60, 60),
                Location = new Point(this.Width - 60, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            Controls.Add(btnClose);

            var lblPay = new Label { Text = "СПОСІБ ОПЛАТИ:", Font = SovereignEngine.GetFont("Segoe UI Semibold", 10f), ForeColor = SovereignEngine.SmokeText, AutoSize = true, Location = new Point(40, 220) };
            Controls.Add(lblPay);

            var btnCard = new PaymentMethodToggle("💳 КАРТКА", true) { Location = new Point(40, 255) };
            var btnCash = new PaymentMethodToggle("💵 ГОТІВКА", false) { Location = new Point(255, 255) };
            
            btnCard.Click += (s, e) => { _isCard = true; btnCard.IsActive = true; btnCash.IsActive = false; };
            btnCash.Click += (s, e) => { _isCard = false; btnCard.IsActive = false; btnCash.IsActive = true; };
            Controls.Add(btnCard);
            Controls.Add(btnCash);

            var lblComment = new Label { Text = "КОМЕНТАР:", Font = SovereignEngine.GetFont("Segoe UI Semibold", 10f), ForeColor = SovereignEngine.SmokeText, AutoSize = true, Location = new Point(40, 360) };
            Controls.Add(lblComment);

            var commentBg = new Panel { Location = new Point(40, 395), Size = new Size(420, 60), BackColor = Color.FromArgb(15, 255, 255, 255) };
            commentBg.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using(var p = SovereignEngine.GetRoundRect(new RectangleF(0,0,commentBg.Width-1,commentBg.Height-1), 12))
                    e.Graphics.DrawPath(new Pen(Color.FromArgb(40, 255, 255, 255)), p);
            };
            Controls.Add(commentBg);

            _txtComments = new TextBox {
                Location = new Point(10, 20),
                Width = 400,
                BorderStyle = BorderStyle.None,
                BackColor = SovereignEngine.ObsidianDeep, 
                ForeColor = SovereignEngine.PearlText,
                Font = SovereignEngine.GetFont("Segoe UI", 11f),
                Text = ""
            };
            _txtComments.BackColor = Color.FromArgb(22, 22, 22); 
            commentBg.Controls.Add(_txtComments);


            var btnConfirm = new EmberActionButton { 
                Text = "ПІДТВЕРДИТИ ОПЛАТУ", 
                Location = new Point(40, 490), 
                Size = new Size(420, 60),
                Font = SovereignEngine.GetFont("Montserrat Bold", 10.5f)
            };
            btnConfirm.Click += (s, e) => {
                string method = _isCard ? "КАРТКОЮ" : "ГОТІВКОЮ";
                MessageBox.Show($"ТРАНЗАКЦІЯ {Guid.NewGuid().ToString().Substring(0,8).ToUpper()} УХВАЛЕНА\nСУМА: {_total:N2} ₴\nОПЛАТА: {method}\nКОМЕНТАР: {_txtComments.Text}", 
                        "SOVEREIGN NETWORK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            };
            Controls.Add(btnConfirm);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = SovereignEngine.GetRoundRect(new RectangleF(0, 0, Width - 1, Height - 1), 20))
            {
                using (var pgb = new LinearGradientBrush(ClientRectangle, Color.FromArgb(40, 255, 255, 255), Color.Transparent, 90f))
                    g.DrawPath(new Pen(pgb, 2f), path);
            }
        }
    }

    public class PaymentMethodToggle : Control
    {
        private bool _isActive;
        public bool IsActive { 
            get => _isActive; 
            set { _isActive = value; Invalidate(); } 
        }

        public PaymentMethodToggle(string text, bool isActive)
        {
            Text = text;
            IsActive = isActive;
            Size = new Size(205, 70);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = SovereignEngine.GetRoundRect(new RectangleF(0,0,Width-1,Height-1), 14))
            {
        
                using (var br = new SolidBrush(IsActive ? Color.FromArgb(30, SovereignEngine.AmberAccent) : Color.FromArgb(15, 255, 255, 255)))
                    g.FillPath(br, path);

   
                using (var p = new Pen(IsActive ? SovereignEngine.AmberAccent : Color.FromArgb(40, 255, 255, 255), IsActive ? 2f : 1f))
                    g.DrawPath(p, path);
            }

            using (var br = new SolidBrush(IsActive ? SovereignEngine.AmberAccent : SovereignEngine.PearlText))
            {
                var sf = new StringFormat{ Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Text, SovereignEngine.GetFont("Montserrat SemiBold", 11f), br, ClientRectangle, sf);
            }
        }
    }
}
