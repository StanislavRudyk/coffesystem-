using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{
    public class GradientGlowButton : Control
    {
        private System.Windows.Forms.Timer anim;
        private int step = 0;
        private bool hov = false;
        private bool press = false;

        private readonly Color cStart = Color.FromArgb(255, 140, 0);   
        private readonly Color cMid = Color.FromArgb(255, 40, 120);   
        private readonly Color cEnd = Color.FromArgb(255, 0, 128);   
        private readonly Color cGlow = Color.FromArgb(255, 0, 128);   
        private readonly Color cShadow = Color.FromArgb(255, 0, 128);  

        public GradientGlowButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Size = new System.Drawing.Size(260, 50);
            Font = new System.Drawing.Font("Segoe UI", 14f, FontStyle.Bold);
            Cursor = Cursors.Hand;

            anim = new System.Windows.Forms.Timer { Interval = 10 };
            anim.Tick += (s, e) =>
            {
                if (hov) { if (step < 12) step++; else anim.Stop(); }
                else { if (step > 0) step--; else anim.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hov = true; anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hov = false; anim.Start(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { press = true; Invalidate(); } }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); if (e.Button == MouseButtons.Left) { press = false; Invalidate(); } }

        private static GraphicsPath MakePill(Rectangle r)
        {
            var path = new GraphicsPath();
            int d = r.Height;
            if (d < 1) d = 1;
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 90, 180);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 180);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float t = step / 12f;
            float scale = press ? 0.96f : (1f + 0.02f * t);
            int W = (int)(Width * 0.85f * scale); 
            int H = (int)(Height * 0.85f * scale);
            var rect = new Rectangle((Width - W) / 2, (Height - H) / 2, W, H);


            float shadowIntensity = 0.40f + 0.30f * t;
            for (int i = 5; i >= 1; i--)
            {
                int alpha = (int)(shadowIntensity * 120 / i);
                var sr = new Rectangle(rect.X, rect.Y + i + 2, rect.Width, rect.Height); 
                using (var sp = MakePill(sr))
                using (var br = new SolidBrush(Color.FromArgb(alpha, cShadow)))
                    g.FillPath(br, sp);
            }


            float gs = 0.50f + 0.50f * t;
            for (int i = 24; i >= 1; i--)
            {
                int alpha = (int)(gs * 200 / (i * 0.8f + 0.5f));
                if (alpha < 1) continue;
                var gr = new Rectangle(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2);
                using (var gp = MakePill(gr))
                using (var pen = new Pen(Color.FromArgb(alpha, cGlow), 1.5f))
                    g.DrawPath(pen, gp);
            }

            using (var path = MakePill(rect))
            {
                using (var lgb = new LinearGradientBrush(rect, cStart, cEnd, 0f))
                {
                    var blend = new ColorBlend(3);
                    blend.Positions = new float[] { 0f, 0.5f, 1f };
                    blend.Colors = new Color[] { cStart, cMid, cEnd };
                    lgb.InterpolationColors = blend;
                    g.FillPath(lgb, path);
                }

                using (var innerPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1.5f))
                {
                    var innerRect = rect;
                    innerRect.Inflate(-2, -2);
                    using (var innerPath = MakePill(innerRect))
                        g.DrawPath(innerPen, innerPath);
                }

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var br = new SolidBrush(Color.White))
                    g.DrawString(Text, Font, br, rect, sf);
            }
        }
    }
}
