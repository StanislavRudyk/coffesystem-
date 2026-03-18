using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class GlassSearchBar : TextBox
    {
        private bool _isHovered = false;
        private bool _isFocused = false;
        private Timer _anim;
        private float _glow = 0f;

        public GlassSearchBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            BorderStyle = BorderStyle.None;
            BackColor = SovereignEngine.ObsidianDeep;
            ForeColor = SovereignEngine.SmokeText;
            Font = SovereignEngine.GetFont("Segoe UI", 11.5f);
            Text = "Пошук...";
            Size = new Size(250, 30);

            _anim = new Timer { Interval = 16 };
            _anim.Tick += (s, e) => {
                bool active = _isHovered || _isFocused;
                if (active) { if (_glow < 1f) _glow += 0.15f; else _anim.Stop(); }
                else { if (_glow > 0f) _glow -= 0.15f; else _anim.Stop(); }
                Invalidate();
            };

            MouseEnter += (s, e) => { _isHovered = true; _anim.Start(); };
            MouseLeave += (s, e) => { _isHovered = false; _anim.Start(); };
            GotFocus += (s, e) => { _isFocused = true; if (Text == "Пошук...") { Text = ""; ForeColor = SovereignEngine.PearlText; } _anim.Start(); };
            LostFocus += (s, e) => { _isFocused = false; if (string.IsNullOrWhiteSpace(Text)) { Text = "Пошук..."; ForeColor = SovereignEngine.SmokeText; } _anim.Start(); };
        }

        private const int WM_PAINT = 0xF;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    // Glass Underline
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(40 + _glow * 60), 255, 255, 255)))
                        g.FillRectangle(br, 0, Height - 2, Width, 2);

                    // Active Amber Line
                    if (_glow > 0.01f)
                    {
                        using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(_glow * 255), SovereignEngine.AmberAccent)))
                            g.FillRectangle(br, Width / 2 - (Width / 2 * _glow), Height - 2, Width * _glow, 2);
                    }
                }
            }
        }
    }
}
