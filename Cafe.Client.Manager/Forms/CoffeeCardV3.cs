using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CoffeeCardV3 : Control
    {
        public string Title { get; set; } = "Coffee Name";
        public string Price { get; set; } = "0.00 ₴";
        public string Desc { get; set; } = "Organic beans, premium roast";

        public event EventHandler BuyClick;

        private bool _isHovered = false;
        private float _animProgress = 0f;
        private Timer _timer;
        private Point _cursorPos;

        public CoffeeCardV3()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(240, 340);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;

            _timer = new Timer { Interval = 15 };
            _timer.Tick += (s, e) =>
            {
                if (_isHovered) { if (_animProgress < 1f) _animProgress += 0.08f; else _timer.Stop(); }
                else { if (_animProgress > 0f) _animProgress -= 0.08f; else _timer.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; _timer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _timer.Start(); }
        protected override void OnMouseMove(MouseEventArgs e) { _cursorPos = e.Location; if (_isHovered) Invalidate(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); BuyClick?.Invoke(this, e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float hover = _animProgress;
            float scale = 1f + hover * 0.03f;
            float w = Width * scale;
            float h = Height * scale;
            float x = (Width - w) / 2;
            float y = (Height - h) / 2;


            if (hover > 0.01f)
            {
                using (var path = GetRoundRectPath(new RectangleF(x + 15, y + 15, w - 30, h - 30), 28))
                {
                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(C(hover * 55), 255, 171, 64);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }

    
            using (var path = GetRoundRectPath(new RectangleF(x + 10, y + 10, w - 20, h - 20), 24))
            {
         
                using (var br = new LinearGradientBrush(new RectangleF(x, y, w, h),
                    Color.FromArgb(C(35), 255, 255, 255), Color.FromArgb(C(5), 10, 10, 10), 45f))
                {
                    g.FillPath(br, path);
                }

          
                using (var p = new Pen(Color.FromArgb(C(40 + hover * 60), 255, 255, 255), 1.2f))
                {
                    g.DrawPath(p, path);
                }

          
                if (hover > 0.01f)
                {
                    using (var glarePath = new GraphicsPath())
                    {
                        
                        float angle = (float)(DateTime.Now.Ticks / 20000000.0);
                        float sx = _cursorPos.X + (float)Math.Cos(angle) * 30;
                        float sy = _cursorPos.Y + (float)Math.Sin(angle) * 30;
                        
                        glarePath.AddEllipse(sx - 120, sy - 120, 240, 240);
                        using (var pgb = new PathGradientBrush(glarePath))
                        {
                            pgb.CenterColor = Color.FromArgb(C(hover * 22), 255, 255, 255);
                            pgb.SurroundColors = new Color[] { Color.Transparent };
                            g.SetClip(path);
                            g.FillPath(pgb, glarePath);
                            g.ResetClip();
                        }

                       
                        using (var streakPath = new GraphicsPath())
                        {
                            streakPath.AddEllipse(_cursorPos.X - 60, _cursorPos.Y - 60, 120, 120);
                            using (var pgb2 = new PathGradientBrush(streakPath))
                            {
                                pgb2.CenterColor = Color.FromArgb(C(hover * 45), 255, 255, 255);
                                pgb2.SurroundColors = new Color[] { Color.Transparent };
                                g.SetClip(path);
                                g.FillPath(pgb2, streakPath);
                                g.ResetClip();
                            }
                        }
                    }
                }
            }

            int iconArea = 100;
            int cx = Width / 2;
            int cy = 70;
            using (var p = new GraphicsPath())
            {
                p.AddEllipse(cx - iconArea/2, cy - iconArea/2, iconArea, iconArea);
                using (var br = new LinearGradientBrush(new Rectangle(cx - iconArea/2, cy - iconArea/2, iconArea, iconArea),
                    Color.FromArgb(C(20), 255, 171, 64), Color.Transparent, 90f))
                    g.FillPath(br, p);

                using (var pen = new Pen(Color.FromArgb(C(220), 255, 171, 64), 2.5f))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, cx - 20, cy + 5, 40, 25, 0, 180);
                    g.DrawLine(pen, cx - 20, cy + 5, cx + 20, cy + 5);
                
                    g.DrawArc(pen, cx + 15, cy + 8, 12, 12, -90, 180);
              
                    using (var sPen = new Pen(Color.FromArgb(C(100 + hover * 155), 255, 255, 255), 1.5f))
                    {
                        g.DrawArc(sPen, cx - 10, cy - 25, 10, 20, -90, 120);
                        g.DrawArc(sPen, cx + 2, cy - 30, 8, 20, -90, 120);
                    }
                }
            }

            using (var fTitle = new Font("Montserrat Bold", 13f))
            using (var fDesc = new Font("Segoe UI", 8.5f))
            using (var fPrice = new Font("Montserrat Bold", 16f))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(Title, fTitle, Brushes.White, new RectangleF(0, 150, Width, 30), sf);
                g.DrawString(Desc, fDesc, Brushes.Gray, new RectangleF(20, 180, Width - 40, 40), sf);

                using (var br = new SolidBrush(Color.FromArgb(C(220 + hover * 35), 255, 171, 64)))
                    g.DrawString(Price, fPrice, br, new RectangleF(0, 220, Width, 40), sf);
            }

      
            int bw = (int)(180 + hover * 10);
            int bh = 54;
            int bx = (Width - bw) / 2;
            int by = Height - 75;
            using (var bPath = GetRoundRectPath(new RectangleF(bx, by, bw, bh), 16))
            {
                using (var br = new LinearGradientBrush(new Rectangle(bx, by, bw, bh),
                    Color.FromArgb(255, 171, 64), Color.FromArgb(255, 80, 20), 45f))
                {
                    g.FillPath(br, bPath);
                }
                
                using (var f = new Font("Segoe UI Bold", 10.5f))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("КУПИТИ ЗАРАЗ", f, Brushes.Black, new RectangleF(bx, by, bw, bh), sf);
                }
            }
        }

        private int C(float v) => Math.Max(0, Math.Min(255, (int)v));

        private GraphicsPath GetRoundRectPath(RectangleF rect, float radius)
        {
            float diameter = radius * 2f;
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
