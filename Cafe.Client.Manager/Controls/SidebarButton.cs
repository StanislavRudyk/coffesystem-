using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Controls
{
    public class SidebarButton : Control
    {
        private Timer anim;
        private float hoverStep = 0f;
        private bool hov = false;
        private bool active = false;

        public Image Icon { get; set; }
        public bool IsActive 
        { 
            get => active; 
            set { active = value; Invalidate(); } 
        }

        public SidebarButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            Size = new Size(220, 55);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Semibold", 10.5f);

            anim = new Timer { Interval = 15 };
            anim.Tick += (s, e) =>
            {
                if (hov) { if (hoverStep < 1f) hoverStep += 0.15f; else anim.Stop(); }
                else { if (hoverStep > 0f) hoverStep -= 0.15f; else anim.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { hov = true; anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { hov = false; anim.Start(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float glow = Math.Max(0f, Math.Min(1f, Math.Max(hoverStep, active ? 1f : 0f)));
            
            if (glow > 0.01f)
            {
                // Multi-layered glass highlighting
                using (var path = RR(new RectangleF(5, 5, Width - 10, Height - 10), 12))
                {
                    int alpha1 = (int)(glow * 25);
                    if (alpha1 > 255) alpha1 = 255;
                    using (var br = new SolidBrush(Color.FromArgb(alpha1, 255, 171, 64)))
                        g.FillPath(br, path);

                    if (active)
                    {
                        using (var p = new Pen(Color.FromArgb(120, 255, 171, 64), 1.5f))
                            g.DrawPath(p, path);
                    }
                }

                // Lead accent line
                int alpha2 = (int)(glow * 255);
                if (alpha2 > 255) alpha2 = 255;
                using (var p = new Pen(Color.FromArgb(alpha2, 255, 171, 64), 3.5f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawLine(p, 8, 15, 8, Height - 15);
                }
            }

            int alphaText = (int)(180 + glow * 75);
            if (alphaText > 255) alphaText = 255;
            Color textCol = active ? Color.FromArgb(255, 171, 64) : Color.FromArgb(alphaText, 255, 255, 255);
            using (var br = new SolidBrush(textCol))
            {
                var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(Text, Font, br, new Rectangle(45, 0, Width - 55, Height), sf);
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
