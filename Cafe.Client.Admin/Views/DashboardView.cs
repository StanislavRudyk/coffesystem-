using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Text.Json;
using Cafe.Shared.Models;
using Cafe.Shared.Services;

namespace Cafe.Client.Admin.Views
{
    public partial class DashboardView : UserControl
    {
        private List<Order> _orders = new List<Order>();
        private Timer _refreshTimer;

        public DashboardView()
        {
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;
            RefreshData();

            _refreshTimer = new Timer { Interval = 10000 };
            _refreshTimer.Tick += (s, e) => { RefreshData(); };
            _refreshTimer.Start();
        }

        private async void RefreshData()
        {
            _orders = await ApiService.Instance.GetOrdersAsync();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int margin = 20;
            float startY = 10; 
            float paddingBot = 20;

            float w = this.Width;
            float h = this.Height;
            float contentW = w - 40; 
            float contentH = h - startY - paddingBot;
            float startX = 20;


            float topRowH = contentH * 0.45f;
            float botRowH = contentH - topRowH - margin;

            float revW = contentW * 0.65f;
            DrawRevenueWide(g, startX, startY, revW, topRowH);
            

            float sideAreaW = contentW - revW - margin;
            float squareW = (sideAreaW - margin) / 2f;
            float squareH = (topRowH - margin) * 0.5f;
            
            DrawStatusSquare(g, startX + revW + margin, startY, squareW, squareH, "ЗАМОВЛЕННЯ", _orders.Count.ToString(), SovereignEngine.BlueAccent, "stats");
            DrawStatusSquare(g, startX + revW + margin + squareW + margin, startY, squareW, squareH, "CЕРЕДНІЙ ЧЕК", GetAvgCheck(), Color.White, "cash");
            
            DrawPeakHours(g, startX + revW + margin, startY + squareH + margin, sideAreaW, topRowH - squareH - margin);


            float bottomCol1W = contentW * 0.45f;
            DrawTopProductsTall(g, startX, startY + topRowH + margin, bottomCol1W, botRowH);
            DrawRecentActivity(g, startX + bottomCol1W + margin, startY + topRowH + margin, contentW - bottomCol1W - margin, botRowH);
        }

        private string GetAvgCheck()
        {
            if (_orders.Count == 0) return "0 ₴";
            return (_orders.Average(v => v.TotalAmount)).ToString("N0") + " ₴";
        }

        private void DrawStatusSquare(Graphics g, float x, float y, float w, float h, string title, string val, Color accent, string icon)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);
            g.DrawString(title, SovereignEngine.GetFont("Montserrat Bold", 7f), new SolidBrush(Color.FromArgb(140, Color.White)), x + 25, y + 25);
            g.DrawString(val, SovereignEngine.GetFont("Montserrat Bold", 20f), Brushes.White, x + 25, y + 50);
            SovereignEngine.DrawIcon(g, icon, new RectangleF(x + w - 50, y + 20, 30, 30), Color.FromArgb(40, accent));
        }

        private void DrawRevenueWide(Graphics g, float x, float y, float w, float h)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);
            g.DrawString("ПОТІК ВИРУЧКИ", SovereignEngine.GetFont("Montserrat Bold", 9.5f), new SolidBrush(Color.FromArgb(140, Color.White)), x + 30, y + 30);

            var todayData = _orders.Where(o => o.CreatedAt.Date == DateTime.Today).ToList();
            if (todayData.Count < 2) {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("НЕДОСТАТНЬО ДАНИХ ДЛЯ ГРАФІКУ", SovereignEngine.GetFont("Montserrat Bold", 8f), 
                             new SolidBrush(Color.FromArgb(60, Color.White)), rect, sf);
                return;
            }

            float stepX = (w - 100) / Math.Max(1, todayData.Count - 1);
            float max = (float)todayData.Max(o => o.TotalAmount) * 1.5f;
            var points = new PointF[todayData.Count];
            for (int i = 0; i < todayData.Count; i++)
                points[i] = new PointF(x + 50 + i * stepX, y + h - 60 - ((float)todayData[i].TotalAmount / (float)max * (h - 120)));

            SovereignEngine.DrawSpectralLine(g, points, SovereignEngine.AmberAccent);
            foreach(var p in points) SovereignEngine.DrawGlowPoint(g, p, 8, SovereignEngine.AmberAccent);
        }

        private void DrawPeakHours(Graphics g, float x, float y, float w, float h)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);
            g.DrawString("ПІКОВІ ГОДИНИ (СЬОГОДНІ)", SovereignEngine.GetFont("Montserrat Bold", 7.5f), new SolidBrush(Color.FromArgb(120, Color.White)), x + 25, y + 22);

            var hourly = new int[12]; 
            foreach(var o in _orders.Where(d => d.CreatedAt.Date == DateTime.Today))
            {
                int hour = o.CreatedAt.Hour;
                if (hour >= 8 && hour < 20) hourly[hour - 8]++;
            }

            float cellW = (w - 50) / 12;
            for(int i = 0; i < 12; i++)
            {
                float intensity = hourly.Max() > 0 ? (float)hourly[i] / hourly.Max() : 0;
                SovereignEngine.DrawHeatMapCell(g, new RectangleF(x + 25 + i * cellW, y + 60, cellW - 4, 40), intensity, SovereignEngine.BlueAccent);
                if (i % 3 == 0)
                    g.DrawString((i + 8).ToString() + "h", SovereignEngine.GetFont("Montserrat", 6.5f), Brushes.Gray, x + 25 + i * cellW, y + 110);
            }
        }

        private void DrawTopProductsTall(Graphics g, float x, float y, float w, float h)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);
            g.DrawString("ТОП ТОВАРІВ", SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.White, x + 25, y + 25);

            var top = _orders.SelectMany(o => o.Items)
                           .GroupBy(item => item.Product?.Name ?? "Unknown")
                           .Select(group => new { Name = group.Key, Count = group.Sum(v => v.Quantity) })
                           .OrderByDescending(i => i.Count)
                           .Take(6).ToList();

            float yy = y + 70;
            int maxCount = top.Count > 0 ? top[0].Count : 1;
            foreach (var item in top)
            {
                g.DrawString(item.Name, SovereignEngine.GetFont("Montserrat", 8.5f), Brushes.White, x + 25, yy);
                SovereignEngine.DrawProgressBar(g, new RectangleF(x + 25, yy + 22, w - 80, 6), (float)item.Count / maxCount, SovereignEngine.AmberAccent);
                g.DrawString(item.Count.ToString(), SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, x + w - 45, yy + 18);
                yy += 50;
            }
        }

        private void DrawRecentActivity(Graphics g, float x, float y, float w, float h)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);
            g.DrawString("ОСТАННЯ АКТИВНІСТЬ", SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.White, x + 25, y + 25);

            var recent = _orders.OrderByDescending(o => o.CreatedAt).Take(7).ToList();
            float yy = y + 70;
            foreach (var o in recent)
            {
                g.DrawString(o.CreatedAt.ToString("HH:mm"), SovereignEngine.GetFont("Montserrat", 8f), Brushes.Gray, x + 25, yy + 4);
                string details = $"ЗАМОВЛЕННЯ #{o.Id:D4}";
                g.DrawString(details, SovereignEngine.GetFont("Montserrat", 9f), Brushes.White, x + 85, yy);
                g.DrawString(o.TotalAmount.ToString("N0") + " ₴", SovereignEngine.GetFont("Montserrat Bold", 9f), new SolidBrush(SovereignEngine.AmberAccent), x + w - 80, yy);
                
                using(var p = new Pen(Color.FromArgb(10, 255, 255, 255), 1f))
                    g.DrawLine(p, x + 25, yy + 35, x + w - 25, yy + 35);
                yy += 45;
            }
        }
    }
}
