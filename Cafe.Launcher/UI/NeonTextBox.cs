using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{
    public class NeonTextBox : Control
    {
        private TextBox inner;
        private System.Windows.Forms.Timer anim;
        private int step = 0, maxStep = 10;
        private bool focused = false;

        private static readonly Color cBorder = Color.FromArgb(0, 210, 255);
        private static readonly Color cFocus = Color.FromArgb(60, 228, 255);
        private static readonly Color cBack = Color.FromArgb(20, 36, 58);
        private const int GLOW = 12, PAD = 16;

        private string _hint = "";

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PlaceholderText { get => _hint; set { _hint = value; Invalidate(); } }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseSystemPasswordChar { get => inner.UseSystemPasswordChar; set => inner.UseSystemPasswordChar = value; }

        public override string Text { get => inner.Text; set => inner.Text = value; }

        public NeonTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(460, 48); 
            Cursor = Cursors.IBeam;

            inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = cBack,
                ForeColor = Color.FromArgb(230, 242, 255),
                Font = new Font("Segoe UI", 11f)
            };
            inner.Enter += (s, e) => { focused = true; step = 0; anim.Start(); };
            inner.Leave += (s, e) => { focused = false; step = maxStep; anim.Start(); };
            inner.TextChanged += (s, e) => Invalidate();
            Controls.Add(inner);

            anim = new System.Windows.Forms.Timer { Interval = 10 };
            anim.Tick += (s, e) =>
            {
                if (focused) { if (step < maxStep) step++; else anim.Stop(); }
                else { if (step > 0) step--; else anim.Stop(); }
                Invalidate();
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (inner == null) return;
            inner.Width = Width - PAD * 2 - 20;
            inner.Location = new Point(PAD + 15, (Height - inner.Height) / 2);
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
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(PAD, PAD, Width - PAD * 2 - 1, Height - PAD * 2 - 1);
            int rad = 8;
            float t = step / (float)maxStep;
            var col = Lerp(cBorder, cFocus, t);

            float intensity = 0.25f + 0.75f * t;
            for (int i = GLOW; i >= 1; i--)
            {
     
                float power = (float)Math.Pow((GLOW - i + 1) / (float)GLOW, 1.5);
                int alpha = (int)(intensity * 180 * power);
                if (alpha < 1) continue;

                var gr = new Rectangle(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2);
                using (var gp = RR(gr, rad + i))
                using (var pen = new Pen(Color.FromArgb(alpha, col), 1.0f))
                    g.DrawPath(pen, gp);
            }

            using (var path = RR(rect, rad))
            {
                using (var fill = new SolidBrush(cBack))
                    g.FillPath(fill, path);

                using (var pen = new Pen(col, 1.5f))
                    g.DrawPath(pen, path);
            }

            if (string.IsNullOrEmpty(inner.Text) && _hint.Length > 0)
            {
                float phY = rect.Top + (rect.Height - inner.Height) / 2f;
                float phX = rect.Left + 15f;
                using (var br = new SolidBrush(Color.FromArgb(115, 138, 168)))
                    g.DrawString(_hint, inner.Font, br, new PointF(phX, phY));
            }
        }

        private static Color Lerp(Color a, Color b, float t) =>
            Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
    }
}
