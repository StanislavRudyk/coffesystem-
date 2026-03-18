using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{

    public class EmberActionButton : Button
    {
        private float _hoverState = 0f;
        private float _hoverVel = 0f;
        private float _pressState = 0f;
        private float _pressVel = 0f;
        private Timer _animTimer;
        private bool _isHovered = false;
        private bool _isPressed = false;

        public EmberActionButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            
            Size = new Size(200, 50);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Bold", 12f);

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) =>
            {
                _hoverState = SovereignEngine.Spring(_hoverState, _isHovered ? 1f : 0f, ref _hoverVel, 0.12f, 0.8f);
                _pressState = SovereignEngine.Spring(_pressState, _isPressed ? 1f : 0f, ref _pressVel, 0.18f, 0.75f);
                
                if (Math.Abs(_hoverVel) < 0.001f && Math.Abs(_pressVel) < 0.001f && !_isHovered && !_isPressed) 
                    _animTimer.Stop();
                Invalidate();
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; 
                return cp;
            }
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _isHovered = true; _animTimer?.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _isHovered = false; _isPressed = false; _animTimer?.Start(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if (e.Button == MouseButtons.Left) { _isPressed = true; _animTimer?.Start(); } }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _isPressed = false; _animTimer?.Start(); }

        public bool UseIcon { get; set; } = true;

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

      
            if (Parent != null)
            {
                using (var pe = new PaintEventArgs(g, e.ClipRectangle))
                {
                    g.TranslateTransform(-Left, -Top);
                    try {
         
                        var mBackground = Parent.GetType().GetMethod("OnPaintBackground", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        mBackground?.Invoke(Parent, new object[] { pe });
                        
                        var mPaint = Parent.GetType().GetMethod("OnPaint", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        mPaint?.Invoke(Parent, new object[] { pe });
                    } catch { }
                    g.TranslateTransform(Left, Top);
                }
            }

            float pp = _pressState;
            float p = 1.0f;
            float offX = pp * 1f;
            float offY = pp * 2.5f;

            var rect = new RectangleF(p + offX, p + offY, Width - p * 2 - offX, Height - p * 2 - offY);
            float cornerRadius = rect.Height / 2f; 

            using (var path = GetRoundRectPath(rect, cornerRadius))
            {
        
                using (var br = new LinearGradientBrush(rect, 
                    Color.FromArgb(255, 230, 140), Color.FromArgb(255, 120, 40), 90f))
                {
                    g.FillPath(br, path);
                }

            
                using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(160 * (1-pp)), 255, 255, 255), 1.2f))
                    g.DrawPath(pen, path);

                if (pp > 0.05f)
                {
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(pp * 60), 0, 0, 0)))
                        g.FillPath(br, path);
                }

                if (_hoverState > 0.05f)
                {
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(_hoverState * 40), 255, 255, 255)))
                        g.FillPath(br, path);
                }

            
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                float textShift = UseIcon ? -12 : 0;
                
                using (var b = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
                {
                    var textRect = rect;
                    textRect.X += textShift;
                    g.DrawString(Text, Font, b, textRect, sf);
                }

                if (UseIcon)
                {
                    float iconSize = 22;
                    var iconRect = new RectangleF(rect.X + rect.Width/2 + (rect.Width/3.2f), 
                                                rect.Y + (rect.Height - iconSize)/2, iconSize, iconSize);
                    SovereignEngine.DrawCheckoutIcon(g, iconRect, Color.FromArgb(230, 0, 0, 0));
                }
            }
        }

        private GraphicsPath GetRoundRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2f;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
