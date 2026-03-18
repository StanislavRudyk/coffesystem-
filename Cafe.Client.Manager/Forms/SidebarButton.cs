using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
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
                float pad = 4 + (1f - glow) * 2;
                using (var path = RR(new RectangleF(pad, pad, Width - pad*2, Height - pad*2), 14))
                {
                    using (var br = new SolidBrush(Color.FromArgb(C(glow * 15), 255, 255, 255)))
                        g.FillPath(br, path);

                    if (active)
                    {
                        using (var p = new Pen(Color.FromArgb(C(80), 255, 171, 64), 1.2f)) 
                            g.DrawPath(p, path);
                        
                      
                        using (var br = new LinearGradientBrush(new RectangleF(pad, pad, Width - pad*2, Height - pad*2),
                            Color.FromArgb(C(15), 255, 171, 64), Color.Transparent, 90f)) 
                            g.FillPath(br, path);
                    }
                }


                float lineWidth = 1f + (glow * 3f); 
                float lineH = 8f + (glow * 10f); 
                using (var p = new Pen(Color.FromArgb(C((int)(50 + glow * 205)), SovereignEngine.AmberAccent), lineWidth))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawLine(p, 10, Height/2f - lineH, 10, Height/2f + lineH);
                }
            }

          
            var sf = new StringFormat{ LineAlignment = StringAlignment.Center };
            
          
            if (glow > 0.05f) {
                using (var gBr = new SolidBrush(Color.FromArgb(C((int)(glow * 80)), SovereignEngine.AmberAccent)))
                    g.DrawString(Text, Font, gBr, new PointF(42, Height / 2 + 1), sf);
                using (var gBr = new SolidBrush(Color.FromArgb(C((int)(glow * 40)), SovereignEngine.AmberAccent)))
                    g.DrawString(Text, Font, gBr, new PointF(42, Height / 2 + 2), sf);
            }

            int tR = (int)(200 + glow * (SovereignEngine.AmberAccent.R - 200));
            int tG = (int)(200 + glow * (SovereignEngine.AmberAccent.G - 200));
            int tB = (int)(200 + glow * (SovereignEngine.AmberAccent.B - 200));

            using(var br = new SolidBrush(Color.FromArgb(255, tR, tG, tB)))
                g.DrawString(Text, Font, br, new PointF(42, Height / 2), sf);
        }

        private int C(float v) => Math.Max(0, Math.Min(255, (int)v));

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
