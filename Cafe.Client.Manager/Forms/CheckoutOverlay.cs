using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cafe.Client.Manager;
using Cafe.Client.Manager.Services;
using Cafe.Shared.Models;
using System.Linq;
using Cafe.Shared.Services;

namespace Cafe.Client.Manager.Forms
{
    public class CheckoutOverlay : UserControl
    {
        private decimal _total;
        private List<LedgerEntry> _orderItems;
        private bool _isCard = true;
        private TextBox _txtComments;
        private TextBox _txtDiscount;
        private TextBox _txtBonusCard;
        private float _opacity = 0f;
        private Timer _animTimer;
        
        public event EventHandler OnCheckoutComplete;
        public event EventHandler OnCancel;

        private Panel _pnlCol1, _pnlCol2, _pnlCol3;

        public CheckoutOverlay(decimal total, List<LedgerEntry> items = null)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            _total = total;
            _orderItems = items ?? new List<LedgerEntry>();
            Dock = DockStyle.Fill;
            BackColor = SovereignEngine.SpaceCharcoal;
            DoubleBuffered = true;

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => {
                _opacity += 0.1f;
                if (_opacity >= 1f) { _opacity = 1f; _animTimer.Stop(); }
                Invalidate();
            };
            _animTimer.Start();

            BuildLayout();
        }

        private void BuildLayout()
        {
            var pnlTop = new Panel { Height = 90, Dock = DockStyle.Top, BackColor = Color.Transparent };
            Controls.Add(pnlTop);

            var lblBack = new Label {
                Text = "[ ← НАЗАД ]",
                Font = SovereignEngine.GetFont("Montserrat Bold", 9f),
                ForeColor = SovereignEngine.SmokeText,
                AutoSize = true,
                Location = new Point(40, 35),
                Cursor = Cursors.Hand
            };
            lblBack.Click += (s, e) => OnCancel?.Invoke(this, EventArgs.Empty);
            pnlTop.Controls.Add(lblBack);

            var lblTitle = new Label {
                Text = "ОФОРМЛЕННЯ ЗАМОВЛЕННЯ",
                Font = SovereignEngine.GetFont("Montserrat Bold", 20f),
                ForeColor = SovereignEngine.PearlText,
                AutoSize = true,
                Location = new Point(Width / 2 - 150, 28)
            };
            pnlTop.Controls.Add(lblTitle);

            var pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 10, 30, 10), BackColor = Color.Transparent };
            Controls.Add(pnlMain);

            _pnlCol1 = new Panel { Dock = DockStyle.Left, Width = Width/3, Padding = new Padding(15, 0, 15, 0) };
            _pnlCol2 = new Panel { Dock = DockStyle.Left, Width = Width/3, Padding = new Padding(15, 0, 15, 0) };
            _pnlCol3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 15, 0) };
            pnlMain.Controls.Add(_pnlCol3);
            pnlMain.Controls.Add(_pnlCol2);
            pnlMain.Controls.Add(_pnlCol1);

            this.Resize += (s, e) => {
                lblTitle.Left = Width / 2 - lblTitle.Width / 2;
                _pnlCol1.Width = pnlMain.Width / 3;
                _pnlCol2.Width = pnlMain.Width / 3;
            };


            var col1 = new SovereignContainer { Dock = DockStyle.Fill, Padding = new Padding(12) };
            _pnlCol1.Controls.Add(col1);

            var lblC1Title = new Label { Text = "ВАШЕ ЗАМОВЛЕННЯ", Font = SovereignEngine.GetFont("Montserrat Bold", 10f), ForeColor = SovereignEngine.AmberAccent, AutoSize = true, Location = new Point(30, 30) };
            col1.Controls.Add(lblC1Title);

            var receiptList = new FlowLayoutPanel { 
                Location = new Point(15, 75), 
                Size = new Size(col1.Width - 30, col1.Height - 160),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent, 
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 0, 15, 0)
            };
            col1.Controls.Add(receiptList);

            foreach(var item in _orderItems) {
                var card = new ReceiptItemCard(item) { 
                    Width = receiptList.Width - 25,
                    Margin = new Padding(0, 0, 0, 8)
                };
                receiptList.Controls.Add(card);
            }

            receiptList.Resize += (s, e) => {
                foreach(Control c in receiptList.Controls) c.Width = receiptList.ClientSize.Width - 5;
            };

            var lblTotal = new Label { 
                Text = $"ВСЬОГО:  {_total:N2} ₴", 
                Font = SovereignEngine.GetFont("Montserrat Bold", 18f), 
                ForeColor = SovereignEngine.PearlText, 
                AutoSize = false, Size = new Size(300, 40)
            };
            col1.Controls.Add(lblTotal);

            col1.Resize += (s, e) => {
                receiptList.Height = col1.Height - 180;
                lblTotal.Location = new Point(30, col1.Height - 65);
            };

            var col2 = new SovereignContainer { Dock = DockStyle.Fill, Padding = new Padding(12) };
            _pnlCol2.Controls.Add(col2);

            var lblC2Title = new Label { Text = "СПОСІБ ОПЛАТИ", Font = SovereignEngine.GetFont("Montserrat Bold", 10f), ForeColor = SovereignEngine.AmberAccent, AutoSize = true, Location = new Point(30, 30) };
            col2.Controls.Add(lblC2Title);

            var selCard = new PaymentSelectionCard("КАРТКОЮ", true) { Location = new Point(30, 85), Size = new Size(200, 105) };
            var selCash = new PaymentSelectionCard("ГОТІВКОЮ", false) { Location = new Point(30, 210), Size = new Size(200, 105) };
            
            selCard.Click += (s, e) => { if(!selCard.Selected) { _isCard = true; selCard.Selected = true; selCash.Selected = false; } };
            selCash.Click += (s, e) => { if(!selCash.Selected) { _isCard = false; selCard.Selected = false; selCash.Selected = true; } };

            col2.Resize += (s, e) => {
                selCard.Width = col2.Width - 60; selCard.Height = 105;
                selCash.Width = col2.Width - 60; selCash.Height = 105;
            };
            col2.Controls.Add(selCard);
            col2.Controls.Add(selCash);

            var col3 = new SovereignContainer { Dock = DockStyle.Fill, Padding = new Padding(12) };
            _pnlCol3.Controls.Add(col3);

            var lblC3Title = new Label { Text = "ДОДАТКОВІ ПАРАМЕТРИ", Font = SovereignEngine.GetFont("Montserrat Bold", 10f), ForeColor = SovereignEngine.AmberAccent, AutoSize = true, Location = new Point(30, 30) };
            col3.Controls.Add(lblC3Title);

            var inpComment = new GlassInput("КОМЕНТАР ДО ЗАМОВЛЕННЯ...", true) { Location = new Point(30, 85), Height = 140 };
            _txtComments = inpComment.TextBox;

            var inpDiscount = new GlassInput("ЗНИЖКА (%)", false, true) { Location = new Point(30, 240), Height = 55 };
            _txtDiscount = inpDiscount.TextBox;

            var inpBonus = new GlassInput("БОНУСНА КАРТКА", false) { Location = new Point(30, 310), Height = 55 };
            _txtBonusCard = inpBonus.TextBox;

            col3.Resize += (s, e) => {
                inpComment.Width = col3.Width - 60;
                inpDiscount.Width = col3.Width - 60;
                inpBonus.Width = col3.Width - 60;
            };
            col3.Controls.Add(inpComment);
            col3.Controls.Add(inpDiscount);
            col3.Controls.Add(inpBonus);

            var pnlBottom = new Panel { Height = 120, Dock = DockStyle.Bottom, BackColor = Color.Transparent };
            Controls.Add(pnlBottom);

            var btnCancel = new Label {
                Text = "[ СКАСУВАТИ ]",
                Font = SovereignEngine.GetFont("Montserrat Bold", 10f),
                ForeColor = Color.FromArgb(230, 80, 80),
                AutoSize = true,
                Location = new Point(50, 48),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => OnCancel?.Invoke(this, EventArgs.Empty);
            pnlBottom.Controls.Add(btnCancel);

            var btnPay = new EmberActionButton { 
                Text = "ОПЛАТИТИ", 
                Size = new Size(420, 70), 
                Location = new Point(Width - 470, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlBottom.Controls.Add(btnPay);

            _txtDiscount.TextChanged += (s, e) => {
                if (decimal.TryParse(_txtDiscount.Text.Replace("%", "").Trim(), out decimal d)) {
                    decimal final = _total * (1 - (d / 100m));
                    lblTotal.Text = $"ВСЬОГО:  {final:N2} ₴";
                } else {
                    lblTotal.Text = $"ВСЬОГО:  {_total:N2} ₴";
                }
            };

            btnPay.Click += async (s, e) => {
                decimal final = _total;
                if (decimal.TryParse(_txtDiscount.Text, out decimal d)) final *= (1 - (d/100m));
                

                int uid = ApiService.Instance.CurrentUser?.Id ?? 0;
                if (uid == 0) {
                    var users = await ApiService.Instance.GetUsersAsync();
                    uid = users.FirstOrDefault()?.Id ?? 1; 
                }

                var order = new Order
                {
                    UserId = uid,
                    TotalAmount = final,
                    Status = 2,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };

                foreach (var item in _orderItems)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = (int)item.Tag, 
                        PriceAtSale = decimal.TryParse(item.Price.Replace(" ₴", "").Replace(".", ","), out decimal p) ? p : 0,
                        Quantity = item.Qty
                    });
                }

                await ApiService.Instance.CreateOrderAsync(order);

                string msg = $"ОПЛАТА {final:N2} ₴ УСПІШНО ПРОВЕДЕНА {(_isCard ? "КАРТКОЮ" : "ГОТІВКОЮ")}.";
                SovereignAlert.Show(msg.ToUpper(), "УСПІХ");
                OnCheckoutComplete?.Invoke(this, EventArgs.Empty);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (_opacity < 0.01f) return;
            

            using (var br = new SolidBrush(Color.FromArgb((int)(255 * _opacity * 0.95f), SovereignEngine.SpaceCharcoal)))
                g.FillRectangle(br, ClientRectangle);
        }
    }

    public class ReceiptItemCard : Control {
        private LedgerEntry _item;
        public ReceiptItemCard(LedgerEntry item) {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            _item = item;
            Height = 50;
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }
        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(2, 2, Width - 5, Height - 5);
            using (var path = SovereignEngine.GetRoundRect(r, 15)) {

                using (var br = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                    g.FillPath(br, path);
                

                using (var lgb = new LinearGradientBrush(r, Color.FromArgb(15, 255, 255, 255), Color.Transparent, 90f))
                    g.FillPath(lgb, path);

                g.DrawPath(new Pen(Color.FromArgb(40, 255, 255, 255), 1.2f), path);
                

                g.FillRectangle(new SolidBrush(SovereignEngine.AmberAccent), 2, 12, 3, Height - 24);
            }
            g.DrawString(_item.Name, SovereignEngine.GetFont("Montserrat SemiBold", 10f), new SolidBrush(SovereignEngine.PearlText), 22, 14);
            
            string price = _item.Price.Replace(" ₴", "");
            decimal.TryParse(price.Replace(".", ","), out decimal p);
            string total = (p * _item.Qty).ToString("N2") + " ₴";
            var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            g.DrawString(total, SovereignEngine.GetFont("Consolas", 12f, FontStyle.Bold), new SolidBrush(SovereignEngine.AmberAccent), new Rectangle(0, 0, Width - 20, Height), sf);
        }

    }

    public class PaymentSelectionCard : Control {
        public string Title;
        private bool _selected;
        public bool Selected { get => _selected; set { _selected = value; _t.Start(); } }
        private float _anim = 0f;
        private float _vel = 0;
        private Timer _t;

        public PaymentSelectionCard(string title, bool sel) {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Title = title; _selected = sel; _anim = _selected ? 1f : 0f;
            Cursor = Cursors.Hand; DoubleBuffered = true;
            BackColor = Color.Transparent;
            _t = new Timer { Interval = 16 };
            _t.Tick += (s, e) => {
                _anim = SovereignEngine.Spring(_anim, _selected ? 1f : 0f, ref _vel, 0.12f, 0.75f);
                Invalidate(); if (Math.Abs(_anim - (_selected?1:0)) < 0.001f && Math.Abs(_vel) < 0.001f) _t.Stop();
            };
        }
        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(2, 2, Width - 5, Height - 5);
            using (var path = SovereignEngine.GetRoundRect(r, 18)) {
                int R = SovereignEngine.C(28 + (SovereignEngine.AmberAccent.R - 28) * _anim);
                int G = SovereignEngine.C(30 + (SovereignEngine.AmberAccent.G - 30) * _anim);
                int B = SovereignEngine.C(34 + (SovereignEngine.AmberAccent.B - 34) * _anim);
                using(var br = new SolidBrush(Color.FromArgb(R, G, B))) g.FillPath(br, path);
                g.DrawPath(new Pen(Color.FromArgb(SovereignEngine.C(60+195*_anim), SovereignEngine.C(60+111*_anim), SovereignEngine.C(60+4*_anim)), 1.5f), path);
            }
            var iconRect = new RectangleF(30, Height/2 - 16, 32, 32);
            Color iconColor = Color.FromArgb(SovereignEngine.C(SovereignEngine.AmberAccent.R*(1-_anim)+35*_anim), SovereignEngine.C(SovereignEngine.AmberAccent.G*(1-_anim)+35*_anim), SovereignEngine.C(SovereignEngine.AmberAccent.B*(1-_anim)+35*_anim));
            if (Title.Contains("КАРТ")) SovereignEngine.DrawCreditCardIcon(g, iconRect, iconColor);
            else SovereignEngine.DrawCashIcon(g, iconRect, iconColor);
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            g.DrawString(Title, SovereignEngine.GetFont("Montserrat Bold", 12f), new SolidBrush(iconColor), new Rectangle(85, 0, Width, Height), sf);
        }
    }

    public class GlassInput : Control {
        public TextBox TextBox;
        private string _hint;
        private bool _isPercent;
        private Label _lblHint;

        public GlassInput(string hint, bool multi, bool isPercent = false) {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            _hint = hint; _isPercent = isPercent;
            DoubleBuffered = true; Cursor = Cursors.IBeam;
            BackColor = Color.Transparent;
            
            _lblHint = new Label {
                Text = hint, ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.FromArgb(22, 22, 26), AutoSize = true,
                Font = SovereignEngine.GetFont("Segoe UI Semibold", 10f),
                Location = new Point(isPercent ? 58 : 48, multi ? 16 : 14),
                Cursor = Cursors.IBeam
            };
            
            TextBox = new TextBox {
                Text = "", Multiline = multi, BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(22, 22, 26), ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Segoe UI", 11.5f), Dock = DockStyle.Fill
            };
            
            _lblHint.Click += (s, e) => TextBox.Focus();
            
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(isPercent ? 58 : 48, multi ? 16 : 15, 12, 12), BackColor = Color.Transparent };
            pnl.Controls.Add(TextBox);
            Controls.Add(_lblHint); 
            Controls.Add(pnl);
            _lblHint.BringToFront();

            TextBox.TextChanged += (s, e) => {
                _lblHint.Visible = string.IsNullOrEmpty(TextBox.Text);
            };
            TextBox.GotFocus += (s, e) => {
                _lblHint.ForeColor = Color.FromArgb(40, 40, 40);
                Invalidate();
            };
            TextBox.LostFocus += (s, e) => {
                _lblHint.ForeColor = Color.FromArgb(80, 80, 80);
                Invalidate();
            };
            pnl.Click += (s, e) => TextBox.Focus();
        }
        protected override void OnPaint(PaintEventArgs e) {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(1, 1, Width - 3, Height - 3);
            using(var path = SovereignEngine.GetRoundRect(r, 12)) {
                g.FillPath(new SolidBrush(Color.FromArgb(22, 22, 26)), path);
                g.DrawPath(new Pen(TextBox.Focused ? SovereignEngine.AmberAccent : Color.FromArgb(55,255,255,255), 1.25f), path);
            }

            var iconRect = new RectangleF(14, TextBox.Multiline ? 14 : Height/2 - 12, 24, 24);
            Color ic = TextBox.Focused ? SovereignEngine.AmberAccent : SovereignEngine.AmberMuted;
            if (_isPercent) SovereignEngine.DrawPercentIcon(g, iconRect, ic);
            else if (TextBox.Multiline) SovereignEngine.DrawCommentIcon(g, iconRect, ic);
        }
    }
}
