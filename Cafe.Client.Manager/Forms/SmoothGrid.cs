using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class SmoothGrid : Control
    {
        private List<Control> _items = new List<Control>();
        private float _scrollOffset = 0f;
        

        private int _columns = 3;
        private int _padding = 15;
        private int _itemWidth = 190;
        private int _itemHeight = 270;

        public SmoothGrid()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            MouseWheel += SmoothGrid_MouseWheel;
        }

        private Point[] _cachedPositions;
        private bool _needsFullLayout = true;

        public void AddItem(Control c)
        {
            _items.Add(c);
            Controls.Add(c);
            _needsFullLayout = true;
        }

        public void FinalizeLayout()
        {
            _scrollOffset = 0;
            LayoutItems(true);
        }

        public void Clear()
        {
            foreach (Control c in _items) c.Dispose();
            Controls.Clear();
            _items.Clear();
            _scrollOffset = 0;
            _needsFullLayout = true;
            LayoutItems(true);
        }

        private void SmoothGrid_MouseWheel(object sender, MouseEventArgs e)
        {
            _scrollOffset += e.Delta > 0 ? 150 : -150;
            LimitScroll();
            LayoutItems(true);
        }

        private void LimitScroll()
        {
            int totalRows = (int)Math.Ceiling((double)_items.Count / _columns);
            int contentHeight = totalRows * (_itemHeight + _padding) + _padding;
            float maxScroll = Math.Max(0, contentHeight - Height);
            
            if (_scrollOffset > 0) _scrollOffset = 0;
            if (_scrollOffset < -maxScroll) _scrollOffset = -maxScroll;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int newCols = Math.Max(1, (Width + 2 - _padding) / (_itemWidth + _padding)); 
            if (newCols != _columns)
            {
                _columns = newCols;
                _needsFullLayout = true;
            }
            LayoutItems(true);
        }

        private void LayoutItems(bool fullScan)
        {
            if (_items.Count == 0) return;

            if (_needsFullLayout || _cachedPositions == null || _cachedPositions.Length != _items.Count)
            {
                _cachedPositions = new Point[_items.Count];
                int totalGridWidth = _columns * _itemWidth + (_columns - 1) * _padding;
                int startX = Math.Max(_padding, (Width - totalGridWidth) / 2);

                for (int i = 0; i < _items.Count; i++)
                {
                    int row = i / _columns;
                    int col = i % _columns;
                    _cachedPositions[i] = new Point(startX + col * (_itemWidth + _padding), _padding + row * (_itemHeight + _padding));
                }
                _needsFullLayout = false;
            }

            SuspendLayout();
            int currentOffset = (int)_scrollOffset;
            
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var cached = _cachedPositions[i];
                int targetY = cached.Y + currentOffset;

                bool isVisible = (targetY + _itemHeight > -50 && targetY < Height + 50);
                
                if (isVisible)
                {
                    if (item.Left != cached.X || item.Top != targetY)
                        item.Location = new Point(cached.X, targetY);
                    
                    if (!item.Visible) item.Visible = true;
                }
                else
                {
                    if (item.Visible) item.Visible = false;
                }
            }
            ResumeLayout();
        }

        protected override void OnPaint(PaintEventArgs e) 
        { 
            base.OnPaint(e); 
            var g = e.Graphics;

            int totalRows = (int)Math.Ceiling((double)_items.Count / _columns);
            int contentHeight = totalRows * (_itemHeight + _padding) + _padding;
            float maxScroll = Math.Max(0, contentHeight - Height);

            if (maxScroll > 0)
            {
                float viewH = Height - 20;
                float thumbH = Math.Max(30, viewH * (viewH / contentHeight));
                float thumbY = 10 + (-_scrollOffset / maxScroll) * (viewH - thumbH);

                var scrollRect = new RectangleF(Width - 10, 10, 4, viewH);
                using (var br = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
                    g.FillRectangle(br, scrollRect);

                var thumbRect = new RectangleF(Width - 10, thumbY, 4, thumbH);
                using (var br = new SolidBrush(Color.FromArgb(80, SovereignEngine.AmberAccent)))
                    g.FillRectangle(br, thumbRect);
            }
        }
        public void HandleMouseWheelExternally(MouseEventArgs e)
        {
            _scrollOffset += e.Delta > 0 ? 150 : -150;
            LimitScroll();
            LayoutItems(true);
        }
    }
}
