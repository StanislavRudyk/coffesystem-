using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{
    public class EmberActionButton : Control
    {
        private System.Windows.Forms.Timer anim;
        private float hoverStep = 0f;
        private bool hov = false;
        private bool press = false;

        private static readonly Color cStart = Color.FromArgb(255, 171, 64);
        private static readonly Color cEnd = Color.FromArgb(230, 81, 0);
        private static readonly Color cGlow = Color.FromArgb(255, 145, 0);

        public EmberActionButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Size = new Size(460, 56);
            Font = new Font("Montserrat", 12f, FontStyle.Bold);
            Cursor = Cursors.Hand;

            anim = new System.Windows.Forms.Timer { Interval = 16 };
            anim.Tick += (s, e) =>
            {
                if (hov) { if (hoverStep < 1f) hoverStep += 0.08f; else anim.Stop(); }
                else { if (hoverStep > 0f) hoverStep -= 0.08f; else anim.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hov = true; anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hov = false; anim.Start(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); press = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); press = false; Invalidate(); }

        private static GraphicsPath RR(RectangleF r, float rad)
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float pulse = press ? 0.98f : 1.0f;
            float targetW = Width * pulse;
            float targetH = Height * pulse;
            var rect = new RectangleF((Width - targetW) / 2, (Height - targetH) / 2, targetW - 1, targetH - 1);
            int rad = 16;

            using (var path = RR(rect, rad))
            {
                float intensity = 0.2f + 0.8f * hoverStep;

                for (int i = 12; i >= 1; i--)
                {
                    int alpha = (int)(intensity * 35 / (i * 0.4f + 0.6f));
                    if (alpha < 1) continue;
                    var gr = rect; gr.Inflate(i, i);
                    using (var gp = RR(gr, rad + i))
                    using (var pen = new Pen(Color.FromArgb(alpha, cGlow), 1.5f))
                        g.DrawPath(pen, gp);
                }

                if (hoverStep > 0.5f)
                {
                    int ca = (int)((hoverStep - 0.5f) * 2 * 100);
                    using (var p = new Pen(Color.FromArgb(ca, Color.White), 1f))
                        g.DrawPath(p, path);
                }

                float shift = 0.3f + 0.4f * hoverStep;
                using (var lgb = new LinearGradientBrush(rect, cStart, cEnd, 0f))
                {
                    var cb = new ColorBlend(3);
                    cb.Positions = new float[] { 0f, shift, 1f };
                    cb.Colors = new Color[] { cStart, Color.FromArgb(255, 230, 150, 50), cEnd };
                    lgb.InterpolationColors = cb;
                    g.FillPath(lgb, path);
                }

                if (hoverStep > 0.05f)
                {
                    g.SetClip(path);
                    float fillHeight = rect.Height * hoverStep;
                    float waveY = rect.Bottom - fillHeight;

                    using (var wavePath = new GraphicsPath())
                    {
                        wavePath.AddLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom);
                        wavePath.AddLine(rect.Right, rect.Bottom, rect.Right, waveY);

                        float lastX = rect.Right;
                        float lastY = waveY;
                        for (float x = rect.Right - 5; x >= rect.Left; x -= 5)
                        {
                            float y = waveY + (float)Math.Sin(x / 20f + hoverStep * 10f) * 3f;
                            wavePath.AddLine(lastX, lastY, x, y);
                            lastX = x; lastY = y;
                        }
                        wavePath.CloseFigure();

                        using (var liquidBr = new LinearGradientBrush(rect, Color.FromArgb(100, 255, 255, 255), Color.Transparent, -90f))
                            g.FillPath(liquidBr, wavePath);
                    }
                    g.ResetClip();
                }

                using (var p = new Pen(Color.FromArgb(80, 255, 255, 255), 1.2f))
                {
                    var rimRect = rect; rimRect.Inflate(-1.2f, -1.2f);
                    using (var rimPath = RR(rimRect, rad - 1))
                        g.DrawPath(p, rimPath);
                }

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var br = new SolidBrush(ForeColor))
                    g.DrawString(Text, Font, br, (RectangleF)rect, sf);
            }
        }
    }
}
