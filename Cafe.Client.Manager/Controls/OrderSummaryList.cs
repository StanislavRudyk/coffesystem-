using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Controls
{
    public class OrderSummaryItem : Control
    {
        public string CoffeeName { get; set; }
        public string Price { get; set; }
        public int Count { get; set; } = 1;

        public event EventHandler RemoveClick;

        public OrderSummaryItem()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            Height = 65;
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (PointToClient(Cursor.Position).X > Width - 40) RemoveClick?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var br = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
                g.FillRectangle(br, 5, 5, Width - 10, Height - 10);

            using (var fBold = new Font("Segoe UI Bold", 10f))
            using (var fReg = new Font("Montserrat", 9f))
            {
                g.DrawString(CoffeeName, fBold, Brushes.White, 15, 12);
                g.DrawString($"{Count}x {Price}", fReg, Brushes.Gray, 15, 34);
            }

            // Remove Button
            using (var f = new Font("Segoe UI", 12f))
            using (var br = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("×", f, br, new Rectangle(Width - 40, 0, 40, Height), sf);
            }

            using (var p = new Pen(Color.FromArgb(10, 255, 255, 255), 1f))
                g.DrawLine(p, 15, Height - 1, Width - 15, Height - 1);
        }
    }

    public class OrderSummaryList : Control
    {
        private List<OrderSummaryItem> _items = new List<OrderSummaryItem>();
        private FlowLayoutPanel _container;

        public OrderSummaryList()
        {
            _container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            Controls.Add(_container);
        }

        public void AddItem(string name, string price)
        {
            var item = new OrderSummaryItem { CoffeeName = name, Price = price, Width = _container.Width - 25 };
            item.RemoveClick += (s, e) => { _container.Controls.Remove(item); };
            _container.Controls.Add(item);
            _container.ScrollControlIntoView(item);
        }

        public void Clear() => _container.Controls.Clear();
    }
}
