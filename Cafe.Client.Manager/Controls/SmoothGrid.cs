using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Controls
{
    public class SmoothGrid : Control
    {
        private List<Control> _items = new List<Control>();
        private float _scrollOffset = 0f;
        private float _targetOffset = 0f;
        private Timer _scrollTimer;
        private int _columns = 3;
        private int _padding = 15;
        private int _itemWidth = 140;
        private int _itemHeight = 240;

        public SmoothGrid()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.MouseWheel += SmoothGrid_MouseWheel;

            _scrollTimer = new Timer { Interval = 15 }; // Optimized interval
            _scrollTimer.Tick += (s, e) =>
            {
                float diff = _targetOffset - _scrollOffset;
                if (Math.Abs(diff) > 0.1f)
                {
                    _scrollOffset += diff * 0.2f; // Smoother interpolation
                    LayoutItems(false); // Quick layout update
                }
                else
                {
                    _scrollOffset = _targetOffset;
                    _scrollTimer.Stop();
                    LayoutItems(true); // Final layout update with full visibility check
                }
            };
        }

        public void AddItem(Control c)
        {
            _items.Add(c);
            this.Controls.Add(c);
            LayoutItems(true);
        }

        public void Clear()
        {
            this.Controls.Clear();
            _items.Clear();
            _targetOffset = 0;
            _scrollOffset = 0;
            LayoutItems(true);
        }

        private void SmoothGrid_MouseWheel(object sender, MouseEventArgs e)
        {
            _targetOffset += e.Delta > 0 ? 150 : -150;
            LimitScroll();
            if (!_scrollTimer.Enabled) _scrollTimer.Start();
        }

        private void LimitScroll()
        {
            int totalRows = (int)Math.Ceiling((double)_items.Count / _columns);
            int contentHeight = totalRows * (_itemHeight + _padding) + _padding;
            float maxScroll = Math.Max(0, contentHeight - Height);
            
            if (_targetOffset > 0) _targetOffset = 0;
            if (_targetOffset < -maxScroll) _targetOffset = -maxScroll;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _columns = Math.Max(1, Width / (_itemWidth + _padding));
            LayoutItems(true);
        }

        private void LayoutItems(bool fullScan)
        {
            if (_items.Count == 0 || Width < 50) return;
            
            this.SuspendLayout();
            
            // Calculate total width of one row to center it
            int rowWidth = _columns * _itemWidth + (_columns - 1) * _padding;
            int startX = (Width - rowWidth) / 2;
            if (startX < _padding) startX = _padding;

            for (int i = 0; i < _items.Count; i++)
            {
                int row = i / _columns;
                int col = i % _columns;

                int x = startX + col * (_itemWidth + _padding);
                int y = _padding + row * (_itemHeight + _padding) + (int)_scrollOffset;

                if (_items[i].Left != x || _items[i].Top != y) _items[i].Location = new Point(x, y);

                bool isVisible = (y + _itemHeight > 0 && y < Height);
                if (_items[i].Visible != isVisible) _items[i].Visible = isVisible;
            }
            this.ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Background is handled by parent or drawn here
            // We just need a clean surface
        }
    }
}
