using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class LedgerEntry
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public int Qty { get; set; } = 1;
        public object Tag { get; set; }
        public float VisualScale = 0f;
        public float VVel = 0f;
        public bool IsDeleting = false;
    }


    public class SovereignLedger : Control
    {
        private List<LedgerEntry> _entries = new List<LedgerEntry>();
        private Timer _ledgerTimer;
        private float _scrollPos = 0f;
        private float _targetScroll = 0f;
        private float _sVel = 0f;
        private int _itemH = 80;       
        public bool DrawInternalContainer { get; set; } = true;
        public Action OnTotalChanged;
        
        public decimal GetTotal()
        {
            decimal total = 0;
            foreach(var e in _entries) total += decimal.Parse(e.Price.Replace(" ₴", "").Replace(".", ",")) * e.Qty;
            return total;
        }

        public List<LedgerEntry> GetEntries() => _entries;

        public SovereignLedger()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            _ledgerTimer = new Timer { Interval = 16 };
            _ledgerTimer.Tick += (s, e) => {
                bool changed = false;
                
                for (int i = _entries.Count - 1; i >= 0; i--)
                {
                    var entry = _entries[i];
                    float prev = entry.VisualScale;
                    
                    if (entry.IsDeleting)
                    {
                        entry.VisualScale = SovereignEngine.Spring(entry.VisualScale, 0f, ref entry.VVel, 0.2f, 0.7f);
                        if (entry.VisualScale < 0.02f)
                        {
                            _entries.RemoveAt(i);
                            _targetScroll = Math.Max(0, (_entries.Count * _itemH) - Height + 40);
                            OnTotalChanged?.Invoke();
                            changed = true;
                            continue;
                        }
                    }
                    else
                    {
                        entry.VisualScale = SovereignEngine.Spring(entry.VisualScale, 1f, ref entry.VVel, 0.15f, 0.75f);
                    }
                    
                    if (Math.Abs(entry.VisualScale - prev) > 0.001f) changed = true;
                }

                float pScroll = _scrollPos;
                _scrollPos = SovereignEngine.Spring(_scrollPos, _targetScroll, ref _sVel, 0.1f, 0.8f);
                if (Math.Abs(_scrollPos - pScroll) > 0.1f) changed = true;

                if (changed) Invalidate();
                else _ledgerTimer.Stop();
            };
        }

        public void AddEntry(string name, string price, object tag = null)
        {
            var existing = _entries.Find(x => x.Name == name);
            if (existing != null)
            {
                existing.Qty++;
                existing.VisualScale = 1.15f;
            }
            else
            {
                _entries.Add(new LedgerEntry { Name = name, Price = price, Tag = tag });
                _targetScroll = Math.Max(0, (_entries.Count * _itemH) - Height + 40);
            }
            _ledgerTimer.Start();
            Invalidate();
        }

        public void Clear()
        {
            _entries.Clear();
            _targetScroll = 0;
            _ledgerTimer.Start();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || _entries.Count == 0) return;


            float adjY = e.Y + _scrollPos - 60;
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                float y = i * _itemH + 10;
                
            
                RectangleF btnPlus = new RectangleF(Width - 45, y + 22, 32, 32);
                RectangleF btnMinus = new RectangleF(Width - 105, y + 22, 32, 32);

                if (btnPlus.Contains(e.X, adjY))
                {
                    entry.Qty++;
                    entry.VisualScale = 1.15f;
                    _ledgerTimer.Start();
                    OnTotalChanged?.Invoke();
                    Invalidate();
                    return;
                }
                else if (btnMinus.Contains(e.X, adjY))
                {
                    entry.Qty--;
                    if (entry.Qty <= 0)
                    {
                        entry.IsDeleting = true;
                        entry.VVel = -0.5f;
                    }
                    else
                    {
                        entry.VisualScale = 1.15f;
                    }
                    _ledgerTimer.Start();
                    OnTotalChanged?.Invoke();
                    Invalidate();
                    return;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _targetScroll -= e.Delta > 0 ? 120 : -120;
            LimitScroll();
            _ledgerTimer.Start();
        }

        private void LimitScroll()
        {
            float max = Math.Max(0, (_entries.Count * _itemH) - Height + 40);
            if (_targetScroll < 0) _targetScroll = 0;
            if (_targetScroll > max) _targetScroll = max;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

     
            if (DrawInternalContainer)
            {
                var fullRect = new RectangleF(1, 1, Width - 6, Height - 6);
                using (var path = SovereignEngine.GetRoundRect(fullRect, 24))
                {
                    using (var br = new SolidBrush(Color.FromArgb(12, 255, 255, 255)))
                        g.FillPath(br, path);
                    using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1.2f))
                        g.DrawPath(pen, path);
                }
            }

     
            SovereignEngine.DrawShoppingBag(g, new RectangleF(15, 12, 24, 24), SovereignEngine.AmberAccent);
            g.DrawString("ВАШ КОШИК", SovereignEngine.GetFont("Montserrat Bold", 10.5f),
                new SolidBrush(SovereignEngine.PearlText), 50, 16);
            
            using (var p = new Pen(Color.FromArgb(40, SovereignEngine.AmberAccent), 1f))
                g.DrawLine(p, 15, 48, Width - 25, 48);

            if (_entries.Count == 0)
            {
              
                float iconSize = 120;
                var bagRect = new RectangleF((Width - iconSize)/2, Height/2 - 80, iconSize, iconSize);
                SovereignEngine.DrawShoppingBag(g, bagRect, Color.FromArgb(20, Color.White));
                
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var br = new SolidBrush(Color.FromArgb(60, SovereignEngine.SmokeText)))
                    g.DrawString("КОШИК ПОРОЖНІЙ", SovereignEngine.GetFont("Montserrat Medium", 9f), br, 
                        new RectangleF(0, Height/2 + 20, Width, 30), sf);
                return;
            }

            g.TranslateTransform(0, -_scrollPos + 60);

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                float y = i * _itemH + 10;
                float vs = entry.VisualScale;
                
            
                RectangleF rect = new RectangleF(10, y, Width - 25, _itemH - 12);

              
                using (var path = SovereignEngine.GetRoundRect(rect, 14))
                {
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(220 * vs), 28, 28, 32)))
                        g.FillPath(br, path);
                 
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(180 * vs), SovereignEngine.AmberAccent)))
                        g.FillRectangle(br, 10, y + 15, 3, 35);

                    using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(35 * vs), 255, 255, 255), 1f))
                        g.DrawPath(pen, path);
                }

                var fTitle = SovereignEngine.GetFont("Montserrat SemiBold", 9.5f);
                var fInfo = SovereignEngine.GetFont("Segoe UI", 8.2f);
                var fPrice = SovereignEngine.GetFont("Consolas", 12f, FontStyle.Bold);

              
                float nameMaxW = rect.Width - 110; 
                using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(235 * vs), SovereignEngine.PearlText)))
                    g.DrawString(entry.Name, fTitle, br, new RectangleF(25, y + 12, nameMaxW, 22), 
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });

              
                decimal itemPrice = 0;
                try { itemPrice = decimal.Parse(entry.Price.Replace(" ₴", "").Replace(".", ",")); } catch { }
                string priceInfo = $"{entry.Qty} × {entry.Price} = {itemPrice * entry.Qty:N2} ₴";

                using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(160 * vs), SovereignEngine.AmberAccent)))
                    g.DrawString(priceInfo, fInfo, br, new RectangleF(25, y + 36, rect.Width - 110, 25));

        
                RectangleF btnPlus = new RectangleF(Width - 52, y + 20, 32, 32);
                RectangleF btnMinus = new RectangleF(Width - 118, y + 20, 32, 32);
                var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                int alpha = SovereignEngine.C(255 * vs);

                using (var pM = new GraphicsPath()) using (var pP = new GraphicsPath())
                {
                    pM.AddEllipse(btnMinus); pP.AddEllipse(btnPlus);
                  
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(50 * vs), 45, 45, 52)))
                    { g.FillPath(br, pM); g.FillPath(br, pP); }
                    
                  
                    using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(85 * vs), SovereignEngine.AmberAccent), 1.25f))
                    { g.DrawPath(pen, pM); g.DrawPath(pen, pP); }
                    
                    g.DrawString("−", SovereignEngine.GetFont("Consolas", 14f, FontStyle.Bold), new SolidBrush(Color.FromArgb(alpha, 255, 120, 120)), btnMinus, sfC);
                    g.DrawString("+", SovereignEngine.GetFont("Consolas", 14f, FontStyle.Bold), new SolidBrush(Color.FromArgb(alpha, 120, 255, 120)), btnPlus, sfC);
                }

            
                var fQty = SovereignEngine.GetFont("Montserrat Bold", 11f);
                g.DrawString(entry.Qty.ToString(), fQty, new SolidBrush(Color.FromArgb(alpha, SovereignEngine.PearlText)), 
                             new RectangleF(Width - 105, y + 17, 70, 40), sfC);
            }
        }
    }
}
