using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{

    public class SovereignSearch : Control
    {
        private TextBox _input;
        private bool _isFocused = false;
        private float _focusAnim = 0f;
        private float _focusVel = 0f;
        private Timer _animTimer;


        private bool _clearHovered = false;

        public event EventHandler TextChangedEvent;
        public override string Text => _input.Text;
        public string Placeholder { get; set; } = "Пошук товару...";

        public SovereignSearch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(300, 40); 
            Cursor = Cursors.IBeam;

            _input = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = SovereignEngine.InputField,
                ForeColor = SovereignEngine.PearlText,
                Font = SovereignEngine.GetFont("Segoe UI", 10f),
                Location = new Point(42, 11), 
                Width = 220,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            _input.TextChanged += (s, e) => { TextChangedEvent?.Invoke(this, e); Invalidate(); };
            _input.GotFocus += (s, e) => { _isFocused = true; _animTimer.Start(); };
            _input.LostFocus += (s, e) => { _isFocused = false; _animTimer.Start(); };

            Controls.Add(_input);

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) => {
                float prev = _focusAnim;
                _focusAnim = SovereignEngine.Spring(_focusAnim, _isFocused ? 1f : 0f, ref _focusVel, 0.15f, 0.78f);
                if (Math.Abs(_focusAnim - prev) < 0.001f) _animTimer.Stop();
                Invalidate();
            };
        }

     
        public void ActivateGlobalFocus()
        {
            _input.Focus();
        }

   
        public void Shrink()
        {
            _input.Text = "";
        }

        public void Expand()
        {
            _input.Focus();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _input.Focus();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool inClear = _input.Text.Length > 0 && e.X > Width - 34 && e.X < Width - 6 && e.Y > 8 && e.Y < 32;
            if (inClear != _clearHovered) { _clearHovered = inClear; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
        
            if (_input.Text.Length > 0 && e.X > Width - 34 && e.X < Width - 6)
            {
                _input.Text = "";
                _input.Focus();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new RectangleF(1, 1, Width - 2, Height - 2);

            
            using (var path = SovereignEngine.GetRoundRect(rect, 12))
            {
                using (var br = new SolidBrush(SovereignEngine.InputField))
                    g.FillPath(br, path);

            
                Color borderColor = Color.FromArgb(
                    SovereignEngine.C(30 + _focusAnim * 195),
                    SovereignEngine.C(60 + _focusAnim * (SovereignEngine.AmberAccent.R - 60)),
                    SovereignEngine.C(60 + _focusAnim * (SovereignEngine.AmberAccent.G - 60)),
                    SovereignEngine.C(60 + _focusAnim * (SovereignEngine.AmberAccent.B - 60))
                );
                float borderWidth = 1f + _focusAnim * 1f;
                using (var pen = new Pen(borderColor, borderWidth))
                    g.DrawPath(pen, path);
            }

            
            if (string.IsNullOrEmpty(_input.Text) && !_isFocused)
            {
                using (var br = new SolidBrush(Color.FromArgb(160, 160, 160)))
                {
                    var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                    g.DrawString(Placeholder, SovereignEngine.GetFont("Segoe UI", 10f), br,
                        new RectangleF(42, 0, Width - 80, Height), sf);
                }
            }

       
            int mx = 20, my = Height / 2;
            using (var p = new Pen(Color.FromArgb(SovereignEngine.C(100 + _focusAnim * 155), _isFocused ? SovereignEngine.AmberAccent : Color.White), 1.8f))
            {
                g.DrawEllipse(p, mx - 7, my - 7, 11, 11);
                g.DrawLine(p, mx + 1, my + 1, mx + 7, my + 7);
            }

      
            if (_input.Text.Length > 0)
            {
                int cx = Width - 22;
                int cy = Height / 2;
                Color xColor = _clearHovered ? SovereignEngine.AmberAccent : Color.FromArgb(140, 255, 255, 255);
                using (var pen = new Pen(xColor, 2f))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, cx - 5, cy - 5, cx + 5, cy + 5);
                    g.DrawLine(pen, cx + 5, cy - 5, cx - 5, cy + 5);
                }
            }
        }
    }
}
