using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{
    public class GlassTextBox : Control
    {
        private TextBox inner;
        private bool focused = false;
        private string _hint = "";
        private IconType _icon = IconType.User;

        public enum IconType { User, Lock }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PlaceholderText { get => _hint; set { _hint = value; Invalidate(); } }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public IconType Icon { get => _icon; set { _icon = value; Invalidate(); } }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseSystemPasswordChar { get => inner.UseSystemPasswordChar; set => inner.UseSystemPasswordChar = value; }

        public override string Text { get => inner.Text; set => inner.Text = value; }

        public GlassTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(460, 52);
            Cursor = Cursors.IBeam;

            inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(40, 40, 45), 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f),
                TextAlign = HorizontalAlignment.Left
            };
            inner.Enter += (s, e) => { focused = true; Invalidate(); };
            inner.Leave += (s, e) => { focused = false; Invalidate(); };
            inner.TextChanged += (s, e) => Invalidate();
            Controls.Add(inner);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (inner == null) return;
            inner.Width = Width - 50 - 20;
            inner.Location = new Point(50, (Height - inner.Height) / 2);
        }

        private static GraphicsPath RR(Rectangle r, int rad)
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

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int rad = 12;

            using (var path = RR(rect, rad))
            {
                using (var br = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    g.FillPath(br, path);

                using (var pen = new Pen(focused ? Color.FromArgb(180, 230, 138, 92) : Color.FromArgb(60, 255, 255, 255), 1.5f))
                    g.DrawPath(pen, path);
            }

            string iconChar = _icon == IconType.User ? "👤" : "🔒";
            using (var f = new Font("Segoe UI", 12f))
            using (var br = new SolidBrush(focused ? Color.FromArgb(230, 138, 92) : Color.FromArgb(160, 255, 255, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(iconChar, f, br, new Rectangle(10, 0, 35, Height), sf);
            }

            if (string.IsNullOrEmpty(inner.Text) && !focused)
            {
                using (var br = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
                    g.DrawString(_hint, inner.Font, br, new PointF(50, (Height - inner.Height) / 2));
            }
        }
    }
}
