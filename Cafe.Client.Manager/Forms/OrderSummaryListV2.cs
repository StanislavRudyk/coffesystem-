using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class OrderItemData
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; } = 1;
        public float EntryAnim = 0f;
    }

    public class OrderSummaryListV2 : Control
    {
        private List<OrderItemData> _items = new List<OrderItemData>();
        private Timer _animTimer;
        private int _hoverIndex = -1;
        private int _itemHeight = 70;
        private int _padding = 10;

        public OrderSummaryListV2()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => {
                bool changed = false;
                foreach (var item in _items)
                {
                    if (item.EntryAnim < 1f)
                    {
                        item.EntryAnim += 0.1f;
                        if (item.EntryAnim > 1f) item.EntryAnim = 1f;
                        changed = true;
                    }
                }
                if (changed) Invalidate();
                else _animTimer.Stop();
            };
        }

        public void AddItem(string name, string price)
        {
            var existing = _items.Find(x => x.Name == name);
            if (existing != null)
            {
                existing.Quantity++;
                existing.EntryAnim = 0.8f; 
            }
            else
            {
                _items.Add(new OrderItemData { Name = name, Price = price });
            }
            _animTimer.Start();
            Invalidate();
        }

        public void Clear()
        {
            _items.Clear();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int newHover = e.Y / (_itemHeight + _padding);
            if (newHover >= 0 && newHover < _items.Count)
            {
                if (_hoverIndex != newHover) { _hoverIndex = newHover; Invalidate(); }
            }
            else if (_hoverIndex != -1) { _hoverIndex = -1; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_items.Count == 0)
            {
                using (var f = new Font("Montserrat", 8f))
                using (var br = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("КОШИК ПОРОЖНІЙ", f, br, ClientRectangle, sf);
                }
                return;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float y = i * (_itemHeight + _padding);
                float anim = item.EntryAnim;
                float offsetX = (1f - anim) * 20f;

                var rect = new RectangleF(10 + offsetX, y, Width - 30, _itemHeight);

               
                using (var path = GetPath(rect, 12))
                {
                    int baseAlpha = (i == _hoverIndex) ? 35 : 15;
                    using (var br = new SolidBrush(Color.FromArgb(C(baseAlpha * anim), 255, 255, 255)))
                        g.FillPath(br, path);

                    if (i == _hoverIndex)
                    {
                        using (var p = new Pen(Color.FromArgb(C(100 * anim), 255, 171, 64), 1.2f))
                            g.DrawPath(p, path);
                    }
                }

                
                using (var p = new Pen(Color.FromArgb(C(20 * anim), 255, 255, 255), 1f))
                    g.DrawLine(p, 25 + offsetX, y + _itemHeight + 5, Width - 45 + offsetX, y + _itemHeight + 5);

              
                using (var fBold = new Font("Montserrat Bold", 10f))
                using (var fMono = new Font("Consolas", 11f))
                using (var fSmall = new Font("Segoe UI", 8f))
                {
                    g.DrawString(item.Name, fBold, Brushes.White, 25 + offsetX, y + 15);
                    
                    string info = $"{item.Quantity} шт × {item.Price}";
                    g.DrawString(info, fSmall, Brushes.Gray, 25 + offsetX, y + 38);

                    var sf = new StringFormat { Alignment = StringAlignment.Far };
                    g.DrawString(item.Price, fMono, new SolidBrush(Color.FromArgb(C(255 * anim), 255, 171, 64)), 
                                 new RectangleF(rect.X, y + 25, rect.Width - 15, 30), sf);
                }
            }
        }

        private int C(float v) => Math.Max(0, Math.Min(255, (int)v));

        private GraphicsPath GetPath(RectangleF r, float rad)
        {
            float d = rad * 2f;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
