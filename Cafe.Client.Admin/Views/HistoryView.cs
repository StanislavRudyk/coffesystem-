using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Cafe.Shared.Models;
using Cafe.Shared.Services;
using Cafe.Client.Admin;

namespace Cafe.Client.Admin.Views
{
    public partial class HistoryView : UserControl
    {
        private List<Order> _orders = new List<Order>();
        private float _scrollOffset = 0;
        private float _maxScroll = 0;

        public HistoryView()
        {
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;
            
            this.MouseWheel += (s, e) => {
                _scrollOffset += e.Delta > 0 ? 45 : -45;
                if (_scrollOffset > 0) _scrollOffset = 0;
                if (_scrollOffset < -_maxScroll) _scrollOffset = -_maxScroll;
                Invalidate();
            };
            this.MouseClick += View_MouseClick;

            RefreshData();
        }

        private async void RefreshData()
        {
            _orders = await ApiService.Instance.GetOrdersAsync();
            _orders = _orders.OrderByDescending(o => o.CreatedAt).ToList();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = this.Width - 40;
            DrawHeader(g, 20, 20, w, 60);

            float yy = 95 + _scrollOffset;
            

            var oldClip = g.Clip;
            g.SetClip(new RectangleF(20, 90, w + 20, Height - 110));

            if (_orders.Count == 0)
            {
                g.DrawString("ІСТОРІЯ ПОРОЖНЯ", SovereignEngine.GetFont("Montserrat Bold", 10f), 
                             new SolidBrush(Color.FromArgb(60, Color.White)), 40, 150);
            }
            else
            {
                foreach (var o in _orders)
                {
                    DrawHistoryRow(g, 20, yy, w, 55, o);
                    yy += 65;
                }
            }

            g.Clip = oldClip;
            _maxScroll = Math.Max(0, (yy - _scrollOffset) - (Height - 40));


            if (_maxScroll > 0)
            {
                float viewH = Height - 130;
                float thumbH = Math.Max(30, viewH * (viewH / (yy - _scrollOffset - 95 + viewH)));
                float thumbY = 100 + (-_scrollOffset / _maxScroll) * (viewH - thumbH);
                using (var br = new SolidBrush(Color.FromArgb(40, SovereignEngine.AmberAccent)))
                    g.FillRectangle(br, Width - 15, thumbY, 4, thumbH);
            }
        }

        private void DrawHeader(Graphics g, float x, float y, float w, float h)
        {
            SovereignEngine.DrawBentoPanel(g, new RectangleF(x, y, w, h), 14);
            var f = SovereignEngine.GetFont("Montserrat Bold", 8f);
            var b = new SolidBrush(Color.FromArgb(100, Color.White));
            g.DrawString("ДАТА ТА ЧАС", f, b, x + 25, y + 22);
            g.DrawString("ДЕТАЛІ ЗАМОВЛЕННЯ", f, b, x + 180, y + 22);
            g.DrawString("МЕТОД", f, b, x + w - 240, y + 22);
            g.DrawString("СУМА", f, b, x + w - 100, y + 22);
        }

        private void DrawHistoryRow(Graphics g, float x, float y, float w, float h, Order o)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect, 12);

            g.DrawString(o.CreatedAt.ToLocalTime().ToString("dd.MM HH:mm"), SovereignEngine.GetFont("Montserrat", 8.5f), Brushes.Gray, x + 25, y + 18);
            
            string details = $"ЗАМОВЛЕННЯ #{o.Id:D4}";
            g.DrawString(details, SovereignEngine.GetFont("Montserrat", 9f), Brushes.White, x + 180, y + 17);

            SovereignEngine.DrawBadge(g, new RectangleF(x + w - 300, y + 16, 70, 22), "PAID", SovereignEngine.AmberAccent);

            g.DrawString(o.TotalAmount.ToString("N0") + " ₴", SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.White, x + w - 210, y + 16);

            var btnDet = new RectangleF(x + w - 105, y + h / 2 - 12, 45, 24);
            var btnDel = new RectangleF(x + w - 50, y + h / 2 - 12, 35, 24);

            using (var path = SovereignEngine.GetRoundRect(btnDet, 6))
            {
                g.FillPath(new SolidBrush(Color.FromArgb(30, Color.White)), path);
                g.DrawString("INFO", SovereignEngine.GetFont("Montserrat Bold", 7f), Brushes.White, btnDet, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            using (var path = SovereignEngine.GetRoundRect(btnDel, 6))
            {
                g.FillPath(new SolidBrush(Color.FromArgb(40, Color.OrangeRed)), path);
                g.DrawString("✕", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.White, btnDel, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private async void View_MouseClick(object sender, MouseEventArgs e)
        {
            float w = this.Width - 40;
            float yy = 95 + _scrollOffset;

            foreach (var o in _orders)
            {
                var btnDet = new RectangleF(20 + w - 105, yy + 55 / 2 - 12, 45, 24);
                var btnDel = new RectangleF(20 + w - 50, yy + 55 / 2 - 12, 35, 24);

                if (btnDet.Contains(e.Location))
                {
                    var modal = new Cafe.Client.Admin.Forms.OrderDetailsModal(o);
                    modal.ShowDialog();
                    return;
                }

                if (btnDel.Contains(e.Location))
                {
                    if (Cafe.Client.Admin.Forms.SovereignConfirm.Show($"Видалити чек #{o.Id:D4}?", "ІСТОРІЯ") == DialogResult.Yes)
                    {
                        await ApiService.Instance.DeleteOrderAsync(o.Id);
                        RefreshData();
                    }
                    return;
                }

                yy += 65;
            }
        }
    }
}
