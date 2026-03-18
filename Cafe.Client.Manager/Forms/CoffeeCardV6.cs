using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{

    public class CoffeeCardV6 : Control
    {
        public string Title { get; set; } = "Ethereal Espresso";
        public string Price { get; set; } = "150.00 ₴";
        public string Desc { get; set; } = "A soft, luxurious blend with hints of velvet.";
        public string Category { get; set; } = "ГАРЯЧА КАВА";
        
        public event EventHandler BuyClick;

        private float _hoverPulse = 0f;
        private float _hVel = 0f;
        private bool _isHovered = false;
        private bool _isPressed = false; 
        
      
        private float _tiltX = 0f;
        private float _tiltY = 0f;
        private float _txVel = 0f;
        private float _tyVel = 0f;

      
        private float _pressAnim = 0f;
        private float _pressVel = 0f;

        private Timer _engineTimer;
        private Point _localMouse = new Point(0,0);

        public CoffeeCardV6()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            
            BackColor = Color.Transparent;
            Size = new Size(190, 270); 
            Cursor = Cursors.Hand;

            _engineTimer = new Timer { Interval = 16 };
            _engineTimer.Tick += (s, e) => {
                _hoverPulse = SovereignEngine.Spring(_hoverPulse, _isHovered ? 1f : 0f, ref _hVel, 0.08f, 0.85f);
                _pressAnim = SovereignEngine.Spring(_pressAnim, _isPressed ? 1f : 0f, ref _pressVel, 0.2f, 0.7f);
                
                float targetTx = 0f, targetTy = 0f;
                if (_isHovered) {
                    targetTx = (_localMouse.X - Width / 2f) / (Width / 2f);
                    targetTy = (_localMouse.Y - Height / 2f) / (Height / 2f);
                }
                
                _tiltX = SovereignEngine.Spring(_tiltX, targetTx, ref _txVel, 0.1f, 0.8f);
                _tiltY = SovereignEngine.Spring(_tiltY, targetTy, ref _tyVel, 0.1f, 0.8f);

                if (Math.Abs(_hVel) < 0.001f && !_isHovered && Math.Abs(_txVel) < 0.001f && Math.Abs(_tyVel) < 0.001f && Math.Abs(_pressVel) < 0.001f) 
                    _engineTimer.Stop();

                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; _engineTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; _engineTimer.Start(); }
        protected override void OnMouseMove(MouseEventArgs e) { _localMouse = e.Location; if (_isHovered) Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; _engineTimer.Start(); }
        protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; _engineTimer.Start(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); BuyClick?.Invoke(this, e); }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Control p = Parent;
            while (p != null && !(p is SmoothGrid)) p = p.Parent;
            if (p is SmoothGrid grid) grid.HandleMouseWheelExternally(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float hp = _hoverPulse;
            float pp = _pressAnim; 
        
            float px = _tiltX * 8f;
            float py = _tiltY * 8f;
            
           
            float pressX = pp * 1f;
            float pressY = pp * 2f;
            
            RectangleF cardRect = new RectangleF(10 + pressX, 10 + pressY, Width - 20, Height - 20);

            
            if (pp < 0.5f) 
            {
                float shadowAlpha = (1f - pp * 2f); 
                using (var path = SovereignEngine.GetRoundRect(new RectangleF(cardRect.X - px*0.5f, cardRect.Y - py*0.5f + 10, cardRect.Width, cardRect.Height), 24))
                {
                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(SovereignEngine.C((40 + hp * 60) * shadowAlpha), 0, 0, 0);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }

            
            using (var path = SovereignEngine.GetRoundRect(cardRect, 22))
            {
              
                using (var lgb = new LinearGradientBrush(cardRect, SovereignEngine.CardBase, Color.FromArgb(18, 18, 22), 60f))
                    g.FillPath(lgb, path);

           
                float highlightAlpha = (1f - pp * 0.6f);
                using (var lgb = new LinearGradientBrush(cardRect, Color.FromArgb(SovereignEngine.C((30 + hp * 20) * highlightAlpha), 255, 255, 255), Color.Transparent, 90f))
                    g.FillPath(lgb, path);

                if (pp > 0.1f)
                {
                    using (var lgb = new LinearGradientBrush(cardRect, Color.FromArgb(SovereignEngine.C(pp * 30), 0, 0, 0), Color.Transparent, 90f))
                        g.FillPath(lgb, path);
                }

          
                int aR = SovereignEngine.AmberAccent.R;
                int aG = SovereignEngine.AmberAccent.G;
                int aB = SovereignEngine.AmberAccent.B;

                Color borderC = Color.FromArgb(
                    SovereignEngine.C(20 + hp * 60), 
                    SovereignEngine.C(255 - hp * (255 - aR)),
                    SovereignEngine.C(255 - hp * (255 - aG)),
                    SovereignEngine.C(255 - hp * (255 - aB))
                );

                using (var p = new Pen(borderC, 1.2f))
                    g.DrawPath(p, path);
            }

          
            g.TranslateTransform(px + pressX, py + pressY);
            
            int cx = Width / 2;
            int cy = 60;

            using (var p = new Pen(Color.FromArgb(SovereignEngine.C(100 + hp * 155), SovereignEngine.AmberAccent), 1.8f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;

                if (Category == "ХОЛОДНА КАВА")
                {
                    
                    g.DrawLine(p, cx - 12, cy - 25, cx - 15, cy + 20); 
                    g.DrawLine(p, cx + 12, cy - 25, cx + 15, cy + 20); 
                    g.DrawLine(p, cx - 15, cy + 20, cx + 15, cy + 20);              
                    
                    using(var pFrost = new Pen(Color.FromArgb(SovereignEngine.C(60 * hp), Color.White), 1f)) {
                        g.DrawLine(pFrost, cx - 8, cy - 5, cx - 10, cy + 5);
                        g.DrawLine(pFrost, cx + 8, cy + 2, cx + 10, cy + 10);
                    }
                  
                    g.DrawPolygon(p, new Point[] { new Point(cx-4, cy-5), new Point(cx, cy-9), new Point(cx+4, cy-5), new Point(cx, cy-1) });
                    
                    
                    g.DrawLine(p, cx + 8, cy - 30, cx + 4, cy - 22);
                    g.DrawLine(p, cx + 4, cy - 22, cx + 2, cy + 8);
                }
                else if (Category == "ДЕСЕРТИ")
                {
                    
                    PointF[] cake = { new PointF(cx-20, cy+15), new PointF(cx+20, cy+15), new PointF(cx+20, cy-5), new PointF(cx, cy-20), new PointF(cx-20, cy-5) };
                    g.DrawPolygon(p, cake);
                    g.DrawLine(p, cx-20, cy-5, cx+20, cy-5); 
                    
                   
                    g.DrawBezier(p, cx-15, cy-5, cx-10, cy+5, cx-5, cy, cx, cy+8);
                    
                  
                    g.FillEllipse(new SolidBrush(SovereignEngine.AmberAccent), cx - 3, cy - 25, 7, 7);
                }
                else 
                {
                    
                    g.DrawArc(p, cx - 18, cy - 5, 36, 25, 0, 180); 
                    g.DrawLine(p, cx - 18, cy - 5, cx + 18, cy - 5);   
                    
                    
                    float s = (float)Math.Sin(SovereignEngine.GlobalPulse * 5) * 2;
                    g.DrawBezier(p, cx - 8, cy - 10, cx - 12 + s, cy - 20, cx - 4 - s, cy - 25, cx - 8, cy - 35);
                    g.DrawBezier(p, cx + 8, cy - 10, cx + 4 + s, cy - 20, cx + 12 - s, cy - 25, cx + 8, cy - 35);
                    
                   
                    g.DrawArc(p, cx + 18, cy - 2, 12, 15, 270, 180);
                }
            }

            
            using (var path = new GraphicsPath()) {
                path.AddEllipse(cx - 35, cy - 35, 70, 70);
                using (var pgb = new PathGradientBrush(path)) {
                    pgb.CenterColor = Color.FromArgb(SovereignEngine.C(15 + hp * 25), SovereignEngine.AmberAccent);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

          
            var fTitle = SovereignEngine.GetFont("Montserrat Bold", 11f);     
            var fDesc = SovereignEngine.GetFont("Segoe UI", 9f);              
            var fPrice = SovereignEngine.GetFont("Consolas", 16f, FontStyle.Bold); 

            var sf = new StringFormat { Alignment = StringAlignment.Center };

           
            using (var bTitle = new SolidBrush(Color.FromArgb(SovereignEngine.C(200 + hp * 55), SovereignEngine.PearlText)))
                g.DrawString(Title, fTitle, bTitle, new RectangleF(10, 120, Width - 20, 45), sf);

           
            using (var bDesc = new SolidBrush(Color.FromArgb(SovereignEngine.C(176 + hp * 50), 176, 176, 176)))
                g.DrawString(Desc, fDesc, bDesc, new RectangleF(15, 165, Width - 30, 45), sf);

        
            using (var pBr = new SolidBrush(Color.FromArgb(SovereignEngine.C(180 + hp * 75), SovereignEngine.AmberAccent)))
                g.DrawString(Price, fPrice, pBr, new RectangleF(0, 220, Width, 35), sf);

 
            if (hp > 0.05f) {
                using (var bAdd = new SolidBrush(Color.FromArgb(SovereignEngine.C(hp * 200), SovereignEngine.AmberAccent))) {
                    var fAdd = SovereignEngine.GetFont("Montserrat SemiBold", 8f);
                    g.DrawString("ДОДАТИ В КОШИК ➔", fAdd, bAdd, new RectangleF(0, 245, Width, 20), sf);
                }
            }

            g.TranslateTransform(-px - pressX, -py - pressY);
        }
    }
}
