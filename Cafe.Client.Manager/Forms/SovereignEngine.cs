using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public static class SovereignEngine
    {
        
        public static readonly Color SpaceCharcoal = Color.FromArgb(18, 18, 20);   // #121214 — глобальний фон
        public static readonly Color PanelBase     = Color.FromArgb(22, 22, 26);    // #16161A — панелі
        public static readonly Color CardBase      = Color.FromArgb(26, 26, 30);    // #1A1A1E — картки
        public static readonly Color InputField    = Color.FromArgb(20, 20, 24);    // #141418 — поля вводу
        public static readonly Color AmberAccent   = Color.FromArgb(255, 171, 64);
        public static readonly Color AmberMuted    = Color.FromArgb(180, 255, 171, 64);
        public static readonly Color PearlText     = Color.FromArgb(245, 245, 242); // #F5F5F2 — заголовки
        public static readonly Color SmokeText     = Color.FromArgb(176, 176, 176); // #B0B0B0 — описи (AAA)
        public static readonly Color GlassBorder   = Color.FromArgb(40, 255, 255, 255); // тонка рамка

      
        public static readonly Color ObsidianDeep  = SpaceCharcoal;
        public static readonly Color ObsidianGloss = PanelBase;

        
        private static System.Collections.Generic.Dictionary<string, Font> _fontCache = new System.Collections.Generic.Dictionary<string, Font>();
        
        public static Font GetFont(string name, float size, FontStyle style = FontStyle.Regular)
        {
            string key = $"{name}_{size}_{style}";
            if (_fontCache.TryGetValue(key, out var f)) return f;
            
            try { f = new Font(name, size, style); }
            catch { f = new Font("Segoe UI", size, style); }
            
            _fontCache[key] = f;
            return f;
        }

     
        public static int C(double v) => Math.Max(0, Math.Min(255, (int)v));
        public static int C(float v) => Math.Max(0, Math.Min(255, (int)v));
        public static Point GlobalMousePos = new Point(0, 0);
        public static float GlobalPulse = 0f;

        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

   
        public static float Spring(float current, float target, ref float velocity, float stiffness = 0.15f, float damping = 0.82f)
        {
            float displacement = target - current;
            float acceleration = stiffness * displacement;
            velocity = (velocity + acceleration) * damping;
            return current + velocity;
        }

        public static GraphicsPath GetRoundRect(RectangleF r, float rad)
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

       
        public static void DrawGlassPanel(Graphics g, RectangleF rect, float cornerRadius = 12f, float alpha = 1f)
        {
            using (var path = GetRoundRect(rect, cornerRadius))
            {
              
                using (var br = new SolidBrush(Color.FromArgb(C(200 * alpha), PanelBase.R, PanelBase.G, PanelBase.B)))
                    g.FillPath(br, path);

                
                var highlightRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height / 2f);
                using (var lgb = new LinearGradientBrush(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                    Color.FromArgb(C(18 * alpha), 255, 255, 255), Color.Transparent, 90f))
                    g.FillPath(lgb, path);

                
                using (var pen = new Pen(Color.FromArgb(C(35 * alpha), 255, 255, 255), 1f))
                    g.DrawPath(pen, path);
            }
        }

       
        public static void DrawPremiumBackground(Graphics g, Rectangle rect)
        {
           
            using (var pgb = new LinearGradientBrush(rect, Color.FromArgb(22, 22, 25), Color.FromArgb(16, 16, 18), 90f))
                g.FillRectangle(pgb, rect);

           
            DrawTechnicalHexGrid(g, rect);

         
            DrawBackgroundPolygons(g, rect);

        
            DrawWatermark(g, rect);

    
            DrawVignette(g, rect);
            DrawNoiseTexture(g, rect);
        }

        public static void DrawShoppingBag(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            float cx = rect.X + w/2, cy = rect.Y + h/2;
            
            using (var p = new Pen(color, 2.5f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;
           
                g.DrawPolygon(p, new PointF[] { 
                    new PointF(cx - w/2.2f, cy + h/2.5f), 
                    new PointF(cx + w/2.2f, cy + h/2.5f),
                    new PointF(cx + w/2.5f, cy - h/4.5f),
                    new PointF(cx - w/2.5f, cy - h/4.5f)
                });
            
                g.DrawArc(p, cx - w/5, cy - h/2, w/2.5f, h/2.5f, 180, 180);
            }
        }

        public static void DrawCheckoutIcon(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            float cx = rect.X + w/2, cy = rect.Y + h/2;

            using (var p = new Pen(color, 2.5f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
           
                g.DrawLine(p, cx - w/3, cy, cx + w/3, cy);
                g.DrawLine(p, cx + w/3, cy, cx + w/8, cy - h/4);
                g.DrawLine(p, cx + w/3, cy, cx + w/8, cy + h/4);
            }
        }

        public static void DrawCreditCardIcon(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            using (var p = new Pen(color, 2f))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawRectangle(p, rect.X, rect.Y + h*0.2f, w, h*0.6f);
                g.DrawLine(p, rect.X, rect.Y + h*0.45f, rect.Right, rect.Y + h*0.45f);
                g.DrawRectangle(p, rect.X + w*0.15f, rect.Y + h*0.6f, w*0.2f, h*0.1f);
            }
        }

        public static void DrawCashIcon(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            using (var p = new Pen(color, 2f))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawRectangle(p, rect.X, rect.Y + h*0.25f, w, h*0.5f);
                g.DrawEllipse(p, rect.X + w*0.35f, rect.Y + h*0.35f, w*0.3f, h*0.3f);
                g.DrawLine(p, rect.X + w*0.15f, rect.Y + h*0.35f, rect.X + w*0.15f, rect.Y + h*0.65f);
                g.DrawLine(p, rect.Right - w*0.15f, rect.Y + h*0.35f, rect.Right - w*0.15f, rect.Y + h*0.65f);
            }
        }

        public static void DrawCommentIcon(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            using (var p = new Pen(color, 1.8f))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawRectangle(p, rect.X, rect.Y, w, h*0.7f);
                g.DrawLine(p, rect.X + w*0.2f, rect.Y + h*0.7f, rect.X + w*0.2f, rect.Bottom);
                g.DrawLine(p, rect.X + w*0.2f, rect.Bottom, rect.X + w*0.5f, rect.Y + h*0.7f);
                g.DrawLine(p, rect.X + w*0.25f, rect.Y + h*0.25f, rect.Right - w*0.25f, rect.Y + h*0.25f);
                g.DrawLine(p, rect.X + w*0.25f, rect.Y + h*0.45f, rect.Right - w*0.25f, rect.Y + h*0.45f);
            }
        }

        public static void DrawPercentIcon(Graphics g, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float w = rect.Width, h = rect.Height;
            using (var p = new Pen(color, 2f))
            {
                g.DrawLine(p, rect.Right, rect.Y, rect.X, rect.Bottom);
                g.DrawEllipse(p, rect.X, rect.Y, w*0.35f, h*0.35f);
                g.DrawEllipse(p, rect.Right - w*0.35f, rect.Bottom - h*0.35f, w*0.35f, h*0.35f);
            }
        }

        private static void DrawTechnicalHexGrid(Graphics g, Rectangle rect)
        {
            int spacing = 60;
            using (var pen = new Pen(Color.FromArgb(12, AmberAccent), 1f)) 
            {
                for (int x = -spacing; x < rect.Width + spacing; x += spacing)
                {
                    for (int y = -spacing; y < rect.Height + spacing; y += (int)(spacing * 1.5))
                    {
                        int offset = ((y / (int)(spacing * 1.5)) % 2 == 0) ? 0 : spacing / 2;
                        g.DrawEllipse(pen, x + offset, y, 2, 2);
                    }
                }
            }
        }

        private static void DrawWatermark(Graphics g, Rectangle rect)
        {
            int cx = rect.Width / 2;
            int cy = rect.Height / 2 + 50;
            int size = 500; 

            using (var path = new GraphicsPath())
            {
                path.AddArc(cx - size/2, cy - size/4, size, size/2, 0, 180);
                path.AddLine(cx - size/2, cy - size/4, cx + size/2, cy - size/4);
                
                using (var pen = new Pen(Color.FromArgb(14, AmberAccent), 2.5f)) 
                    g.DrawPath(pen, path);
                
                using (var steamPen = new Pen(Color.FromArgb(10, Color.White), 3f)) 
                {
                    steamPen.DashStyle = DashStyle.Dash;
                    g.DrawBezier(steamPen, cx - 40, cy - size/4 - 20, cx - 80, cy - 200, cx + 20, cy - 250, cx - 20, cy - 350);
                    g.DrawBezier(steamPen, cx + 60, cy - size/4 - 10, cx + 100, cy - 180, cx, cy - 220, cx + 40, cy - 320);
                }
            }
        }

        private static void DrawBackgroundPolygons(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(12, PearlText), 1.2f))
            {
                Point[] pts = { new Point(0, rect.Height / 2), new Point(rect.Width / 3, rect.Height), new Point(rect.Width, rect.Height / 3), new Point(rect.Width / 2, 0) };
                g.DrawPolygon(pen, pts);
                Point[] pts2 = { new Point(rect.Width, 0), new Point(rect.Width / 2, rect.Height), new Point(0, rect.Height / 4) };
                g.DrawPolygon(pen, pts2);
            }
        }

        public static void DrawNoiseTexture(Graphics g, Rectangle rect)
        {
            var rng = new Random(42);
            using (var br = new SolidBrush(Color.FromArgb(5, 255, 255, 255)))
            {
                for (int i = 0; i < 3000; i++)
                    g.FillRectangle(br, rng.Next(rect.Width), rng.Next(rect.Height), 1, 1);
            }
        }

       
        public static void DrawVignette(Graphics g, Rectangle bounds)
        {
            int vig = 300; 
        
            using (var lgb = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, vig), Color.FromArgb(60, 0, 0, 0), Color.Transparent, 90f))
                g.FillRectangle(lgb, 0, 0, bounds.Width, vig);
          
            using (var lgb = new LinearGradientBrush(new Rectangle(0, bounds.Height - vig, bounds.Width, vig), Color.Transparent, Color.FromArgb(80, 0, 0, 0), 90f))
                g.FillRectangle(lgb, 0, bounds.Height - vig, bounds.Width, vig);
            
            using (var lgb = new LinearGradientBrush(new Rectangle(0, 0, vig, bounds.Height), Color.FromArgb(40, 0, 0, 0), Color.Transparent, 0f))
                g.FillRectangle(lgb, 0, 0, vig, bounds.Height);
            
            using (var lgb = new LinearGradientBrush(new Rectangle(bounds.Width - vig, 0, vig, bounds.Height), Color.Transparent, Color.FromArgb(40, 0, 0, 0), 0f))
                g.FillRectangle(lgb, bounds.Width - vig, 0, vig, bounds.Height);
        }

      
        public static void DrawGlassMaterial(Graphics g, RectangleF rect, float alphaMult = 1f)
        {
            DrawGlassPanel(g, rect, 15f, alphaMult);
        }
     
        public static string MapCategoryName(string dbName)
        {
            if (string.IsNullOrEmpty(dbName)) return "";
            string n = dbName.Trim();
            if (n.Equals("Кофе", StringComparison.OrdinalIgnoreCase) || n.Equals("Кава", StringComparison.OrdinalIgnoreCase)) return "ГАРЯЧА КАВА";
            if (n.Equals("Десерты", StringComparison.OrdinalIgnoreCase) || n.Equals("Десерти", StringComparison.OrdinalIgnoreCase)) return "ДЕСЕРТИ";
            if (n.Equals("Холодные напитки", StringComparison.OrdinalIgnoreCase) || n.Equals("Холодні напої", StringComparison.OrdinalIgnoreCase)) return "ХОЛОДНА КАВА";
            return n.ToUpper();
        }
    }
}
