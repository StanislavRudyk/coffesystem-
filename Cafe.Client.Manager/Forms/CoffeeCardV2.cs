using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CoffeeCardV2 : Control
    {
        public string Title { get; set; } = "Coffee Name";
        public string Price { get; set; } = "0.00 ₴";
        
        public event EventHandler BuyClick;

        private bool _isHovered = false;
        private float _hoverScale = 1.0f;
        private float _glowAnim = 0f;
        private Timer _animTimer;
        private Point _mousePos = new Point(0,0);

        public CoffeeCardV2()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(220, 310);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;

            _animTimer = new Timer { Interval = 15 };
            _animTimer.Tick += (s, e) =>
            {
                bool changed = false;
                if (_isHovered) 
                { 
                    if (_hoverScale < 1.04f) { _hoverScale += 0.008f; changed = true; }
                    if (_glowAnim < 1.0f) { _glowAnim += 0.08f; changed = true; }
                }
                else 
                { 
                    if (_hoverScale > 1.0f) { _hoverScale -= 0.008f; changed = true; }
                    if (_glowAnim > 0f) { _glowAnim -= 0.08f; changed = true; }
                }

                if (!changed) _animTimer.Stop();
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; _animTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _animTimer.Start(); }
        protected override void OnMouseMove(MouseEventArgs e) { _mousePos = e.Location; if (_isHovered) Invalidate(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); BuyClick?.Invoke(this, e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float w = Width * _hoverScale;
            float h = Height * _hoverScale;
            float x = (Width - w) / 2;
            float y = (Height - h) / 2;


            if (_glowAnim > 0.01f)
            {
                using (var path = RR(new RectangleF(x + 5, y + 5, w - 10, h - 10), 24))
                {
                    using (var pgb = new PathGradientBrush(path))
                    {
                        int alpha = (int)(_glowAnim * 40);
                        if (alpha > 255) alpha = 255;
                        pgb.CenterColor = Color.FromArgb(alpha, 255, 171, 64);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }


            using (var path = RR(new RectangleF(x + 10, y + 10, w - 20, h - 20), 22))
            {

                using (var br = new LinearGradientBrush(new RectangleF(x, y, w, h),
                    Color.FromArgb(40, 255, 255, 255), Color.FromArgb(5, 255, 255, 255), 45f))
                {
                    g.FillPath(br, path);
                }

                if (_isHovered)
                {
                    using (var glarePath = new GraphicsPath())
                    {
                        glarePath.AddEllipse(_mousePos.X - 100, _mousePos.Y - 100, 200, 200);
                        using (var pgb = new PathGradientBrush(glarePath))
                        {
                            pgb.CenterColor = Color.FromArgb(25, 255, 255, 255);
                            pgb.SurroundColors = new Color[] { Color.Transparent };
                            g.SetClip(path);
                            g.FillPath(pgb, glarePath);
                            g.ResetClip();
                        }
                    }
                }

                using (var p = new Pen(Color.FromArgb(50, 255, 255, 255), 1.5f))
                    g.DrawPath(p, path);
            }


            int iconSize = 85;
            int iconX = Width / 2 - iconSize / 2;
            int iconY = 50;
            
            using (var p = new GraphicsPath())
            {
                p.AddEllipse(iconX, iconY, iconSize, iconSize);
                using (var br = new LinearGradientBrush(new Rectangle(iconX, iconY, iconSize, iconSize),
                    Color.FromArgb(30, 255, 255, 255), Color.FromArgb(10, 255, 255, 255), 90f))
                {
                    g.FillPath(br, p);
                }
                
                using (var pen = new Pen(Color.FromArgb(200, 255, 171, 64), 2.5f))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                   
                    g.DrawArc(pen, Width / 2 - 15, iconY + 35, 30, 25, 0, 180);
                    g.DrawLine(pen, Width / 2 - 15, iconY + 35, Width / 2 + 15, iconY + 35);
                    g.DrawBezier(pen, Width / 2 + 12, iconY + 38, Width / 2 + 25, iconY + 38, Width / 2 + 25, iconY + 50, Width / 2 + 12, iconY + 50);
                }
            }

    
            using (var fTitle = new Font("Segoe UI Semibold", 12f))
            using (var fPrice = new Font("Montserrat Bold", 15f))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(Title, fTitle, Brushes.White, new RectangleF(0, 155, Width, 30), sf);
                
                int priceAlpha = (int)(200 + _glowAnim * 55);
                if (priceAlpha > 255) priceAlpha = 255;
                g.DrawString(Price, fPrice, new SolidBrush(Color.FromArgb(priceAlpha, 255, 171, 64)), new RectangleF(0, 195, Width, 40), sf);
            }

            int btnW = 180;
            int btnH = 50;
            int btnX = (Width - btnW) / 2;
            int btnY = Height - 75;
            using (var bPath = RR(new RectangleF(btnX, btnY, btnW, btnH), 15))
            {
                int btnAlpha = (int)(210 + _glowAnim * 45);
                if (btnAlpha > 255) btnAlpha = 255;
                
                using (var br = new LinearGradientBrush(new Rectangle(btnX, btnY, btnW, btnH),
                    Color.FromArgb(btnAlpha, 255, 171, 64), Color.FromArgb(btnAlpha, 255, 100, 30), 45f))
                {
                    g.FillPath(br, bPath);
                }
                
                using (var f = new Font("Segoe UI Bold", 10f))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("ДОДАТИ В КОШИК", f, Brushes.Black, new RectangleF(btnX, btnY, btnW, btnH), sf);
                }
            }
        }

        private GraphicsPath RR(RectangleF r, float rad)
        {
            float d = rad * 2f;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
