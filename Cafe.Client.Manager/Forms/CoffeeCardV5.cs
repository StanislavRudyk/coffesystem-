using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CoffeeCardV5 : Control
    {
        public string Title { get; set; } = "Ethereal Espresso";
        public string Price { get; set; } = "150.00 ₴";
        public string Desc { get; set; } = "A soft, luxurious blend with hints of velvet.";
        public string Category { get; set; } = "ГАРЯЧА КАВА";
        
        public event EventHandler BuyClick;

        private float _hoverPulse = 0f;
        private float _hVel = 0f;
        private bool _isHovered = false;
        private Timer _engineTimer;
        private Point _localMouse = new Point(0,0);

        public CoffeeCardV5()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(200, 280);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;

            _engineTimer = new Timer { Interval = 16 };
            _engineTimer.Tick += (s, e) => {
                _hoverPulse = SovereignEngine.Spring(_hoverPulse, _isHovered ? 1f : 0f, ref _hVel, 0.08f, 0.85f);
                if (Math.Abs(_hVel) < 0.001f && !_isHovered) _engineTimer.Stop();
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; _engineTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _engineTimer.Start(); }
        protected override void OnMouseMove(MouseEventArgs e) { _localMouse = e.Location; if (_isHovered) Invalidate(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); BuyClick?.Invoke(this, e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float hp = _hoverPulse;
            RectangleF cardRect = new RectangleF(10, 10, Width - 20, Height - 20);


            using (var path = SovereignEngine.GetRoundRect(new RectangleF(cardRect.X - 5, cardRect.Y - 5, cardRect.Width + 10, cardRect.Height + 10), 24))
            {
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(SovereignEngine.C(15 + hp * 20), SovereignEngine.AmberAccent);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

           
            using (var path = SovereignEngine.GetRoundRect(cardRect, 18))
            {
                using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(12 + hp * 8), 255, 255, 255)))
                    g.FillPath(br, path);

          
                if (hp > 0.01f)
                {
                    using (var glintPath = new GraphicsPath())
                    {
                        glintPath.AddEllipse(_localMouse.X - 150, _localMouse.Y - 150, 300, 300);
                        using (var pgb = new PathGradientBrush(glintPath))
                        {
                            pgb.CenterColor = Color.FromArgb(SovereignEngine.C(hp * 20), 255, 255, 255);
                            pgb.SurroundColors = new Color[] { Color.Transparent };
                            g.SetClip(path);
                            g.FillPath(pgb, glintPath);
                            g.ResetClip();
                        }
                    }
                }

            
                using (var p = new Pen(Color.FromArgb(SovereignEngine.C(25 + hp * 40), 255, 255, 255), 1f))
                    g.DrawPath(p, path);
            }

            int cy = 70;
            using (var p = new Pen(Color.FromArgb(SovereignEngine.C(100 + hp * 100), SovereignEngine.AmberAccent), 1.5f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;

                if (Category == "ХОЛОДНА КАВА")
                {
              
                    g.DrawLine(p, Width / 2 - 10, cy - 10, Width / 2 + 10, cy + 10);
                    g.DrawLine(p, Width / 2 + 10, cy - 10, Width / 2 - 10, cy + 10);
                    g.DrawLine(p, Width / 2, cy - 15, Width / 2, cy + 15);
                    g.DrawLine(p, Width / 2 - 15, cy, Width / 2 + 15, cy);
                }
                else if (Category == "ДЕСЕРТИ" || Category == "СПЕШЛ")
                {
                  
                    g.DrawEllipse(p, Width / 2 - 12, cy - 12, 24, 24);
                    g.DrawLine(p, Width / 2 - 18, cy + 15, Width / 2 + 18, cy + 15);
                }
                else
                {
                  
                    g.DrawBezier(p, Width / 2 - 25, cy, Width / 2 - 10, cy - 20, Width / 2 + 10, cy + 20, Width / 2 + 25, cy);
                    g.DrawBezier(p, Width / 2 - 25, cy + 15, Width / 2 - 10, cy - 5, Width / 2 + 10, cy + 35, Width / 2 + 25, cy + 15);
                }
            }

            using (var gl = new SolidBrush(Color.FromArgb(SovereignEngine.C(5 + hp * 10), SovereignEngine.AmberAccent)))
                g.FillEllipse(gl, Width / 2 - 40, cy - 25, 80, 80);

           
            var fTitle = SovereignEngine.GetFont("Montserrat SemiBold", 11f);
            var fDesc = SovereignEngine.GetFont("Segoe UI Light", 8.5f);
            var fPrice = SovereignEngine.GetFont("Consolas", 14f);

            var sf = new StringFormat { Alignment = StringAlignment.Center };

            using (var bTitle = new SolidBrush(Color.FromArgb(SovereignEngine.C(200 + hp * 55), SovereignEngine.PearlText)))
                g.DrawString(Title, fTitle, bTitle, new RectangleF(0, 135, Width, 25), sf);

            using (var bDesc = new SolidBrush(Color.FromArgb(200, 255, 255, 255))) 
                g.DrawString(Desc, fDesc, bDesc, new RectangleF(20, 160, Width - 40, 40), sf);

            using (var pBr = new SolidBrush(Color.FromArgb(SovereignEngine.C(180 + hp * 75), SovereignEngine.AmberAccent)))
                g.DrawString(Price, fPrice, pBr, new RectangleF(0, 220, Width, 30), sf);
        }
    }
}
