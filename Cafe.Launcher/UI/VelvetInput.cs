using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Cafe.Launcher.UI
{
    public class VelvetInput : Control
    {
        private SmoothTextBox inner;
        private System.Windows.Forms.Timer animFocus;
        private System.Windows.Forms.Timer animHover;
        private float focusStep = 0f;
        private float hoverStep = 0f;
        private bool focused = false;
        private bool hovered = false;
        
        private string _hint = "";
        private bool _useSystemPass = false;
        private char _passChar = '•';
        private bool _showPass = false;

        private static readonly Color cAmber = Color.FromArgb(255, 171, 64);
        private static readonly Color cAmberSoft = Color.FromArgb(255, 145, 0);
        private static readonly Color cGlass = Color.FromArgb(15, 255, 255, 255);
        private static readonly Color cBackInternal = Color.FromArgb(13, 13, 13);
        private static readonly Color cText = Color.White;

        [Category("Appearance"), Browsable(true), Localizable(true)]
        [DefaultValue("")]
        public string PlaceholderText 
        { 
            get => _hint; 
            set { _hint = value; if (inner != null) inner.HintText = value; Invalidate(); } 
        }

        [Category("Appearance"), Browsable(true)]
        [DefaultValue(false)]
        public bool UseSystemPasswordChar 
        { 
            get => _useSystemPass; 
            set { 
                _useSystemPass = value; 
                _showPass = !value;
                ApplyPasswordState();
                Invalidate(); 
            } 
        }

        private void ApplyPasswordState()
        {
            if (inner == null) return;
            inner.UseSystemPasswordChar = _useSystemPass && !_showPass;
            if (!_useSystemPass) inner.PasswordChar = '\0';
            else inner.PasswordChar = _showPass ? '\0' : _passChar;
            
            inner.Font = this.Font;
            inner.BackColor = cBackInternal;
            UpdateInnerLayout();
        }

        [Category("Appearance"), Browsable(true)]
        [DefaultValue('•')]
        public char PasswordChar
        {
            get => _passChar;
            set { _passChar = value; if (inner != null) inner.PasswordChar = value; Invalidate(); }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set {
                base.BackColor = value;
                if (inner != null)
                    inner.BackColor = (value == Color.Transparent) ? cBackInternal : value;
                Invalidate();
            }
        }

        public override Color ForeColor
        {
            get => base.ForeColor;
            set {
                base.ForeColor = value;
                if (inner != null) inner.ForeColor = value;
                Invalidate();
            }
        }

        public override Font Font
        {
            get => base.Font;
            set {
                base.Font = value;
                if (inner != null) inner.Font = value;
                UpdateInnerLayout();
            }
        }

        [Browsable(true)]
        public override string Text { get => inner.Text; set => inner.Text = value; }

        public VelvetInput()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = cText;
            Size = new Size(460, 56);
            
            Font = new Font("Segoe UI Semibold", 12f);
            Cursor = Cursors.IBeam;

            inner = new SmoothTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = cBackInternal,
                ForeColor = cText,
                Font = this.Font,
                ShortcutsEnabled = true,
                Multiline = false
            };
            
            inner.Enter += (s, e) => { focused = true; animFocus.Start(); };
            inner.Leave += (s, e) => { focused = false; animFocus.Start(); };
            inner.TextChanged += (s, e) => Invalidate();
            
            Controls.Add(inner);

            animFocus = new System.Windows.Forms.Timer { Interval = 15 };
            animFocus.Tick += (s, e) =>
            {
                if (focused) { if (focusStep < 1f) focusStep += 0.12f; else animFocus.Stop(); }
                else { if (focusStep > 0f) focusStep -= 0.12f; else animFocus.Stop(); }
                Invalidate();
            };

            animHover = new System.Windows.Forms.Timer { Interval = 15 };
            animHover.Tick += (s, e) =>
            {
                if (hovered) { if (hoverStep < 1f) hoverStep += 0.15f; else animHover.Stop(); }
                else { if (hoverStep > 0f) hoverStep -= 0.15f; else animHover.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hovered = true; animHover.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hovered = false; animHover.Start(); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_useSystemPass && GetEyeRect().Contains(e.Location))
                Cursor = Cursors.Hand;
            else
                Cursor = Cursors.IBeam;
        }

        protected override void OnMouseDown(MouseEventArgs e) 
        { 
            base.OnMouseDown(e); 
            if (_useSystemPass && GetEyeRect().Contains(e.Location))
            {
                _showPass = !_showPass;
                ApplyPasswordState();
                Invalidate();
            }
            else
            {
                inner.Focus(); 
            }
        }

        private Rectangle GetEyeRect() => new Rectangle(Width - 42, 0, 42, Height);

        protected override void OnResize(EventArgs e) { base.OnResize(e); UpdateInnerLayout(); }

        private void UpdateInnerLayout()
        {
            if (inner == null) return;
            int h = TextRenderer.MeasureText("Wg", Font).Height;
            int y = (Height - h) / 2;
            int x = 20;
            inner.Location = new Point(x, y);
            inner.Width = Width - (UseSystemPasswordChar ? 65 : 40);
        }

        private static int C(float alpha) => Math.Max(0, Math.Min(255, (int)alpha));

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
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new RectangleF(1.5f, 1.5f, Width - 3.5f, Height - 3.5f);
            float rad = 14f;
            float glowI = Math.Max(focusStep, hoverStep * 0.5f);

            using (var path = RR(rect, rad))
            {
                this.Region = new Region(path);

                if (glowI > 0.01f)
                {
                    for (int i = 8; i >= 1; i--)
                    {
                        int alpha = C(glowI * 45 / (i * 0.8f + 0.2f));
                        if (alpha < 1) continue;
                        var gr = rect; gr.Inflate(i * 0.9f, i * 0.9f);
                        using (var gp = RR(gr, rad + i * 0.8f))
                        using (var p = new Pen(Color.FromArgb(alpha, cAmberSoft), 1.5f))
                            g.DrawPath(p, gp);
                    }
                }

                using (var br = new SolidBrush(Color.FromArgb(25, 25, 25, 25)))
                    g.FillPath(br, path);

                using (var p = new Pen(Color.FromArgb(40, Color.White), 1.2f))
                {
                    var rim = rect; rim.Inflate(-1f, -1f);
                    using (var rp = RR(rim, rad - 1))
                        g.DrawPath(p, rp);
                }

                Color bc = Color.FromArgb(C(60 + glowI * 195), 255, 171, 64);
                if (glowI < 0.1f) bc = Color.FromArgb(90, 255, 255, 255);
                using (var p = new Pen(bc, 1.8f))
                    g.DrawPath(p, path);

                if (hoverStep > 0.01f)
                {
                    g.SetClip(path);
                    float sx = -200 + (Width + 400) * hoverStep;
                    using (var sb = new LinearGradientBrush(new RectangleF(sx, 0, 140, Height), Color.Transparent, Color.FromArgb(C(hoverStep * 28), 255, 255, 255), 45f))
                    {
                        var cb = new ColorBlend(3);
                        cb.Positions = new float[] { 0f, 0.5f, 1f };
                        cb.Colors = new Color[] { Color.Transparent, Color.FromArgb(C(hoverStep * 35), 255, 255, 255), Color.Transparent };
                        sb.InterpolationColors = cb;
                        g.FillRectangle(sb, sx, 0, 140, Height);
                    }
                    g.ResetClip();
                }
            }

            if (_useSystemPass) DrawEye(g, Width - 32, Height / 2);
        }

        private void DrawEye(Graphics g, int cx, int cy)
        {
            int ew = 20, eh = 12;
            var er = new Rectangle(cx - ew / 2, cy - eh / 2, ew, eh);
            using (var p = new Pen(focused ? cAmber : Color.FromArgb(100, cAmber), 1.5f))
            {
                g.DrawArc(p, er.X, er.Y - 2, ew, eh + 4, 10, 160);
                g.DrawArc(p, er.X, er.Y - 2, ew, eh + 4, 190, 160);
                if (_showPass) g.FillEllipse(new SolidBrush(p.Color), cx - 3, cy - 3, 6, 6);
                else {
                    g.DrawEllipse(p, cx - 3, cy - 3, 6, 6);
                    g.DrawLine(p, cx - 8, cy + 6, cx + 8, cy - 6);
                }
            }
        }

        private class SmoothTextBox : TextBox
        {
            private const int WM_PAINT = 0x000F;
            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string HintText { get; set; } = "";

            public SmoothTextBox()
            {
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_PAINT && string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(HintText))
                {
                    using (Graphics g = Graphics.FromHwnd(this.Handle))
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                        TextRenderer.DrawText(g, HintText, Font, ClientRectangle, 
                            Color.FromArgb(130, 160, 160, 160), 
                            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
                    }
                }
            }
        }
    }
}
