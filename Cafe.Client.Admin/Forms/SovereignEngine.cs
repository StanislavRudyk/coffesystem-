using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace Cafe.Client.Admin
{
    public static class SovereignEngine
    {
        public static readonly Color SpaceCharcoal = Color.FromArgb(18, 19, 24);
        public static readonly Color PanelBase = Color.FromArgb(24, 25, 32);
        public static readonly Color CardBase = Color.FromArgb(28, 30, 38);
        public static readonly Color AmberAccent = Color.FromArgb(255, 171, 64);
        public static readonly Color BlueAccent = Color.FromArgb(64, 196, 255);
        public static readonly Color VioletAccent = Color.FromArgb(179, 136, 255);
        public static readonly Color PearlText = Color.FromArgb(245, 245, 242);
        public static readonly Color SmokeText = Color.FromArgb(160, 164, 175);
        public static readonly Color GlassBorder = Color.FromArgb(35, 255, 255, 255);

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

        public static void DrawPremiumBackground(Graphics g, Rectangle rect)
        {
            using (var lgb = new LinearGradientBrush(rect, Color.FromArgb(42, 45, 54), Color.FromArgb(28, 30, 36), 45f))
                g.FillRectangle(lgb, rect);

            var colors = new[] { 
                new { Color = AmberAccent, X = 0.1f, Y = -0.1f, Size = 0.9f, Alpha = 55 },
                new { Color = BlueAccent, X = 0.85f, Y = 0.25f, Size = 0.8f, Alpha = 45 },
                new { Color = VioletAccent, X = 0.45f, Y = 0.8f, Size = 1.0f, Alpha = 35 }
            };

            foreach (var glow in colors)
            {
                using (var path = new GraphicsPath())
                {
                    float w = rect.Width * glow.Size;
                    float h = rect.Height * glow.Size;
                    path.AddEllipse(rect.Width * glow.X - w/2, rect.Height * glow.Y - h/2, w, h);
                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(glow.Alpha, glow.Color);
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }

            int spacing = 60;
            using (var p = new Pen(Color.FromArgb(6, 255, 255, 255), 1f))
            {
                for (int x = 0; x < rect.Width; x += spacing) g.DrawLine(p, x, 0, x, rect.Height);
                for (int y = 0; y < rect.Height; y += spacing) g.DrawLine(p, 0, y, rect.Width, y);
            }
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

        public static void DrawBentoPanel(Graphics g, RectangleF rect, float rad = 22f)
        {
            using (var path = GetRoundRect(rect, rad))
            {
                using (var br = new SolidBrush(Color.FromArgb(200, 38, 40, 48)))
                    g.FillPath(br, path);

                using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1.5f))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawPath(pen, path);
                }

                var glowRect = rect;
                glowRect.Inflate(-1, -1);
                using (var p = new Pen(Color.FromArgb(3, 255, 255, 255), 4f))
                    g.DrawPath(p, path);
            }
        }

        public static void DrawSpectralLine(Graphics g, PointF[] points, Color color)
        {
            if (points.Length < 2) return;
            using (var p = new Pen(Color.FromArgb(100, color), 6f))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawCurve(p, points);
            }
            using (var p = new Pen(color, 2.5f))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawCurve(p, points);
            }
        }

        public static void DrawHeatMapCell(Graphics g, RectangleF rect, float intensity, Color color)
        {
            int alpha = (int)(20 + 200 * intensity);
            using (var path = GetRoundRect(rect, 6))
            {
                using (var br = new SolidBrush(Color.FromArgb(alpha, color)))
                    g.FillPath(br, path);
                if (intensity > 0.7f)
                    g.DrawPath(new Pen(Color.FromArgb(100, Color.White), 1f), path);
            }
        }

        public static void DrawProgressBar(Graphics g, RectangleF rect, float percent, Color color)
        {
            using (var path = GetRoundRect(rect, rect.Height / 2))
            {
                g.FillPath(new SolidBrush(Color.FromArgb(30, Color.Black)), path);
                var progressRect = new RectangleF(rect.X, rect.Y, rect.Width * percent, rect.Height);
                if (progressRect.Width > rect.Height)
                {
                    using (var pPath = GetRoundRect(progressRect, rect.Height / 2))
                    {
                        using (var lgb = new LinearGradientBrush(progressRect, color, Color.FromArgb(180, color), 0f))
                            g.FillPath(lgb, pPath);
                        g.DrawPath(new Pen(Color.FromArgb(80, Color.White), 1f), pPath);
                    }
                }
            }
        }

        public static float Spring(float cur, float target, ref float vel, float tension, float friction)
        {
            float force = (target - cur) * tension;
            vel += force;
            vel *= friction;
            return cur + vel;
        }

        public static void DrawDonut(Graphics g, RectangleF rect, float[] values, Color[] colors)
        {
            if (values == null || values.Length == 0) return;
            float total = values.Sum();
            if (total == 0) total = 1;
            float startAngle = -90;
            float holeSize = rect.Width * 0.72f;
            var holeRect = new RectangleF(rect.X + (rect.Width - holeSize)/2, rect.Y + (rect.Height - holeSize)/2, holeSize, holeSize);

            for (int i = 0; i < values.Length; i++)
            {
                float sweep = (values[i] / total) * 360;
                if (sweep > 0)
                {
                    using (var path = new GraphicsPath())
                    {
                        path.AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweep);
                        g.FillPath(new SolidBrush(colors[i % colors.Length]), path);
                    }
                }
                startAngle += sweep;
            }

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(holeRect);
                g.FillPath(new SolidBrush(Color.FromArgb(20, 21, 26)), path);
                g.DrawPath(new Pen(Color.FromArgb(30, 255, 255, 255), 1f), path);
            }
        }

        public static void DrawBadge(Graphics g, RectangleF rect, string text, Color color)
        {
            using (var path = GetRoundRect(rect, rect.Height / 2))
            {
                g.FillPath(new SolidBrush(Color.FromArgb(25, color)), path);
                g.DrawPath(new Pen(Color.FromArgb(80, color), 1f), path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, GetFont("Montserrat Bold", 7f), new SolidBrush(color), rect, sf);
            }
        }

        public static void DrawGlowPoint(Graphics g, PointF pt, float size, Color color)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(pt.X - size, pt.Y - size, size * 2, size * 2);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(160, color);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }
            g.FillEllipse(Brushes.White, pt.X - 2.5f, pt.Y - 2.5f, 5, 5);
        }

        public static void DrawIcon(Graphics g, string name, RectangleF rect, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;
            using (var p = new Pen(color, 2.2f))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
                switch (name)
                {
                    case "stats":
                        g.DrawLine(p, x + w * 0.2f, y + h * 0.8f, x + w * 0.2f, y + h * 0.45f);
                        g.DrawLine(p, x + w * 0.5f, y + h * 0.8f, x + w * 0.5f, y + h * 0.15f);
                        g.DrawLine(p, x + w * 0.8f, y + h * 0.8f, x + w * 0.8f, y + h * 0.6f);
                        break;
                    case "menu":
                        g.DrawLine(p, x, y + h * 0.3f, x + w, y + h * 0.3f);
                        g.DrawLine(p, x, y + h * 0.5f, x + w, y + h * 0.5f);
                        g.DrawLine(p, x, y + h * 0.7f, x + w, y + h * 0.7f);
                        break;
                    case "history":
                        g.DrawEllipse(p, rect);
                        g.DrawLine(p, x + w / 2, y + h / 2, x + w / 2, y + h * 0.35f);
                        g.DrawLine(p, x + w / 2, y + h / 2, x + w * 0.7f, y + h / 2);
                        break;
                    case "person":
                        g.DrawEllipse(p, x + w * 0.25f, y + h * 0.1f, w * 0.5f, h * 0.45f);
                        g.DrawArc(p, x + w * 0.1f, y + h * 0.6f, w * 0.8f, h * 0.6f, 180, 180);
                        break;
                    case "cash":
                        g.DrawRectangle(p, x, y + h * 0.25f, w, h * 0.5f);
                        g.DrawEllipse(p, x + w * 0.35f, y + h * 0.35f, w * 0.3f, h * 0.3f);
                        break;
                    case "pulse":
                        g.DrawLines(p, new PointF[] { new PointF(x, y + h/2), new PointF(x + w*0.3f, y + h/2), new PointF(x+w*0.4f, y+h*0.2f), new PointF(x+w*0.6f, y+h*0.8f), new PointF(x+w*0.7f, y+h/2), new PointF(x+w, y+h/2)});
                        break;
                    case "edit":
                        g.DrawLine(p, x + w * 0.2f, y + h * 0.8f, x + w * 0.4f, y + h * 0.8f);
                        g.DrawLine(p, x + w * 0.2f, y + h * 0.8f, x + w * 0.2f, y + h * 0.6f);
                        g.DrawLine(p, x + w * 0.2f, y + h * 0.6f, x + w * 0.7f, y + h * 0.1f);
                        g.DrawLine(p, x + w * 0.7f, y + h * 0.1f, x + w * 0.9f, y + h * 0.3f);
                        g.DrawLine(p, x + w * 0.9f, y + h * 0.3f, x + w * 0.4f, y + h * 0.8f);
                        break;
                    case "star":
                        var points = new PointF[10];
                        float cx = x + w / 2, cy = y + h / 2;
                        float rIn = w * 0.2f, rOut = w * 0.45f;
                        for (int i = 0; i < 10; i++) {
                            float r = (i % 2 == 0) ? rOut : rIn;
                            double angle = (Math.PI * 2 * i / 10) - Math.PI / 2;
                            points[i] = new PointF(cx + (float)Math.Cos(angle) * r, cy + (float)Math.Sin(angle) * r);
                        }
                        g.DrawPolygon(p, points);
                        break;
               }
            }
        }
        public static void DrawActionButton(Graphics g, float x, float y, float w, float h, string text, Color accent, Color? textOverride = null)
        {
            var rect = new RectangleF(x, y, w, h);
            using (var path = GetRoundRect(rect, 8))
            {
                using (var br = new SolidBrush(Color.FromArgb(50, accent))) g.FillPath(br, path);
                using (var p = new Pen(Color.FromArgb(150, accent), 1.5f)) g.DrawPath(p, path);
            }

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, GetFont("Montserrat Bold", 7.5f), new SolidBrush(textOverride ?? Color.White), rect, sf);
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
