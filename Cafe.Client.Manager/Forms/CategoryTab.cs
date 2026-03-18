using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class CategoryTab : Control
    {
        private bool _active = false;
        private float _hoverStep = 0f;
        private Timer _anim;

        public bool IsActive { get => _active; set { _active = value; Invalidate(); } }

        public CategoryTab()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(130, 45);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI Semibold", 9.5f);

            _anim = new Timer { Interval = 15 };
            _anim.Tick += (s, e) =>
            {
                if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
                {
                    if (_hoverStep < 1f) _hoverStep += 0.15f; else _anim.Stop();
                }
                else
                {
                    if (_hoverStep > 0f) _hoverStep -= 0.15f; else _anim.Stop();
                }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _anim.Start(); }
        protected override void OnMouseLeave(EventArgs e) { _anim.Start(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float glow = Math.Max(_hoverStep, _active ? 1f : 0f);
            
            if (_active)
            {
                using (var path = GetPath(new RectangleF(5, 5, Width - 10, Height - 10), 10))
                {
                    using (var br = new LinearGradientBrush(new RectangleF(0, 0, Width, Height),
                        Color.FromArgb(C(20), 255, 171, 64), Color.Transparent, 90f))
                    {
                        g.FillPath(br, path);
                    }
                    using (var p = new Pen(Color.FromArgb(C(100), 255, 171, 64), 1.2f)) 
                        g.DrawPath(p, path);
                }
            }
            else if (_hoverStep > 0.01f)
            {
                using (var br = new SolidBrush(Color.FromArgb(C(_hoverStep * 8), 255, 255, 255))) 
                    g.FillRectangle(br, 10, Height - 10, Width - 20, 2);
            }

            var textCol = _active ? Color.FromArgb(255, 171, 64) : Color.FromArgb(SovereignEngine.C(180 + glow * 75), 255, 255, 255);
            using (var br = new SolidBrush(textCol))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Text, Font, br, ClientRectangle, sf);
            }
        }

        private int C(float v) => Math.Max(0, Math.Min(255, (int)v));

        private GraphicsPath GetPath(RectangleF r, float rad)
        {
            var path = new GraphicsPath();
            float d = rad * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
