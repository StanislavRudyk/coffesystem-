using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CoffeeCardV4 : Control
    {
        public string Title { get; set; } = "Sovereign Coffee";
        public string Price { get; set; } = "150.00 ₴";
        public string Desc { get; set; } = "Crafted with obsidian precision and silk-roasted beans.";
        
        public event EventHandler BuyClick;

        
        private float _hoverProp = 0f;
        private float _hVel = 0f;
        private float _pressProp = 0f;
        private float _pVel = 0f;
        private bool _isHovered = false;
        private bool _isPressed = false;
        
        private Timer _engineTimer;
        private Point _localMouse = new Point(0,0);

        public CoffeeCardV4()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(240, 340);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;

            _engineTimer = new Timer { Interval = 16 };
            _engineTimer.Tick += (s, e) => {
                _hoverProp = SovereignEngine.Spring(_hoverProp, _isHovered ? 1f : 0f, ref _hVel, 0.12f, 0.8f);
                _pressProp = SovereignEngine.Spring(_pressProp, _isPressed ? 1f : 0f, ref _pVel, 0.3f, 0.7f);
                
                if (Math.Abs(_hVel) < 0.001f && Math.Abs(_pVel) < 0.001f && !_isHovered) _engineTimer.Stop();
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; _engineTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _engineTimer.Start(); }
        protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; _engineTimer.Start(); }
        protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; _engineTimer.Start(); }
        protected override void OnMouseMove(MouseEventArgs e) { _localMouse = e.Location; if (_isHovered) Invalidate(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); BuyClick?.Invoke(this, e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float hp = _hoverProp;
            float pp = _pressProp;
            
     
            float scale = 1.0f; 
            float w = Width * scale;
            float h = Height * scale;
            float x = (Width - w) / 2;
            float y = (Height - h) / 2;

            RectangleF cardRect = new RectangleF(x + 10, y + 10, w - 20, h - 20);

          
            float shadowAlpha = 20 + hp * 30;
            using (var path = SovereignEngine.GetRoundRect(new RectangleF(cardRect.X + 5, cardRect.Y + 12, cardRect.Width - 10, cardRect.Height), 24))
            {
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(SovereignEngine.C(shadowAlpha), 0, 0, 0);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

        
            using (var path = SovereignEngine.GetRoundRect(cardRect, 20))
            {
               
                using (var br = new LinearGradientBrush(cardRect, 
                    Color.FromArgb(SovereignEngine.C(40 + hp * 20), 255, 255, 255), 
                    Color.FromArgb(SovereignEngine.C(10 + hp * 10), 10, 10, 12), 45f))
                {
                    g.FillPath(br, path);
                }

         
                if (hp > 0.01f)
                {
                    using (var glintPath = new GraphicsPath())
                    {
                        glintPath.AddEllipse(_localMouse.X - 120, _localMouse.Y - 120, 240, 240);
                        using (var pgb = new PathGradientBrush(glintPath))
                        {
                            pgb.CenterColor = Color.FromArgb(SovereignEngine.C(hp * 25), 255, 255, 255);
                            pgb.SurroundColors = new Color[] { Color.Transparent };
                            g.SetClip(path);
                            g.FillPath(pgb, glintPath);
                            g.ResetClip();
                        }
                    }
                }

           
                using (var p = new Pen(Color.FromArgb(SovereignEngine.C(30 + hp * 120), SovereignEngine.AmberAccent), 1.5f))
                    g.DrawPath(p, path);
            }

       
            int cy = 80;
            using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(15 + hp * 25), SovereignEngine.AmberAccent)))
                g.FillEllipse(br, Width / 2 - 45, cy - 45, 90, 90);

    
            using (var p = new Pen(SovereignEngine.AmberAccent, 2.5f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;
                g.DrawArc(p, Width / 2 - 18, cy + 5, 36, 20, 0, 180);
                g.DrawLine(p, Width / 2 - 18, cy + 5, Width / 2 + 18, cy + 5);
                g.DrawArc(p, Width / 2 + 14, cy + 8, 12, 10, -90, 180);
                
           
                float pulse = (float)(Math.Sin(DateTime.Now.Ticks / 2000000.0) * 0.5 + 0.5);
                using (var sp = new Pen(Color.FromArgb(SovereignEngine.C(80 + pulse * 100), 255, 255, 240), 1.5f))
                {
                    g.DrawArc(sp, Width / 2 - 8, cy - 25, 8, 15, -90, 120);
                    g.DrawArc(sp, Width / 2 + 4, cy - 30, 6, 20, -90, 120);
                }
            }

           
            var fT = SovereignEngine.GetFont("Montserrat Bold", 13f);
            var fD = SovereignEngine.GetFont("Segoe UI", 8.5f);
            var fP = SovereignEngine.GetFont("Montserrat Bold", 17f);
            
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(Title, fT, new SolidBrush(SovereignEngine.PearlText), new RectangleF(0, 160, Width, 30), sf);
            g.DrawString(Desc, fD, new SolidBrush(SovereignEngine.SmokeText), new RectangleF(30, 195, Width - 60, 40), sf);

            using (var pBr = new SolidBrush(Color.FromArgb(SovereignEngine.C(220 + hp * 35), SovereignEngine.AmberAccent)))
                g.DrawString(Price, fP, pBr, new RectangleF(0, 240, Width, 40), sf);
        }
    }
}
