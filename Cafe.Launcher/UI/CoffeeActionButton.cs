using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{
    public class CoffeeActionButton : Control
    {
        private System.Windows.Forms.Timer anim;
        private float scale = 1.0f;
        private bool hov = false;

        private Color cStart = Color.FromArgb(230, 138, 92); 
        private Color cEnd = Color.FromArgb(139, 69, 19);  

        public CoffeeActionButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Size = new Size(460, 52);
            Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            Cursor = Cursors.Hand;

            anim = new System.Windows.Forms.Timer { Interval = 10 };
            anim.Tick += (s, e) =>
            {
                if (hov) { if (scale < 1.02f) scale += 0.002f; else anim.Stop(); }
                else { if (scale > 1.00f) scale -= 0.002f; else anim.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hov = true; anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hov = false; anim.Start(); }

        private static GraphicsPath RR(RectangleF r, float rad)
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float targetW = Width * scale;
            float targetH = Height * scale;
            var rect = new RectangleF((Width - targetW) / 2, (Height - targetH) / 2, targetW - 1, targetH - 1);
            int rad = 14;

            using (var path = RR(rect, rad))
            {

                using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0), 2f))
                {
                    var sr = rect; sr.Offset(0, 2);
                    using (var sp = RR(sr, rad)) g.FillPath(new SolidBrush(Color.FromArgb(50, 0, 0, 0)), sp);
                }


                using (var lgb = new LinearGradientBrush(rect, cStart, cEnd, 45f))
                    g.FillPath(lgb, path);


                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var br = new SolidBrush(ForeColor))
                    g.DrawString(Text, Font, br, rect, sf);
            }
        }
    }
}
