using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cafe.Client.Admin;

namespace Cafe.Client.Admin.Forms
{
    public class SidebarButton : Control
    {
        private string _title;
        private string _iconName;
        private bool _isActive = false;
        private bool _isHovered = false;

        public string Title 
        { 
            get => _title; 
            set { _title = value; Invalidate(); } 
        }

        public string IconName
        {
            get => _iconName;
            set { _iconName = value; Invalidate(); }
        }

        public bool IsActive 
        { 
            get => _isActive; 
            set { _isActive = value; Invalidate(); } 
        }

        public SidebarButton(string title, string iconName = "")
        {
            _title = title;
            _iconName = iconName;
            Size = new Size(240, 50);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (IsActive)
            {
                using (var p = new Pen(SovereignEngine.AmberAccent, 3f))
                {
                    p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;
                    g.DrawLine(p, 2, 12, 2, Height - 12);
                }
            }

            if (!string.IsNullOrEmpty(_iconName))
            {
                var iconRect = new RectangleF(15, Height / 2 - 10, 20, 20);
                Color iconColor = IsActive ? SovereignEngine.AmberAccent : (_isHovered ? Color.White : Color.FromArgb(100, 255, 255, 255));
                SovereignEngine.DrawIcon(g, _iconName, iconRect, iconColor);
            }

            Color textColor = IsActive ? SovereignEngine.AmberAccent : (_isHovered ? Color.White : Color.FromArgb(140, 255, 255, 255));
            g.DrawString(_title, SovereignEngine.GetFont("Montserrat Bold", 9.5f), 
                new SolidBrush(textColor), 50, Height / 2 - 9);
        }
    }
}
