using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cafe.Shared.Models;

namespace Cafe.Client.Admin.Forms
{
    public class OrderDetailsModal : Form
    {
        private Order _order;
        private List<OrderItem> _items;

        public OrderDetailsModal(Order order)
        {
            _order = order;
            _items = order.Items ?? new List<OrderItem>();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SovereignEngine.PanelBase;
            this.DoubleBuffered = true;

            var btnClose = new Label
            {
                Text = "×",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18f),
                Size = new Size(40, 40),
                Location = new Point(460, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;


            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 2f))
            {
                g.DrawRectangle(p, 1, 1, Width - 2, Height - 2);
            }

            g.DrawString("ДЕТАЛІ ЗАМОВЛЕННЯ", SovereignEngine.GetFont("Montserrat Bold", 14f), Brushes.White, 30, 30);

            string statusText = "НЕВІДОМО";
            Color sColor = SovereignEngine.AmberAccent;
            switch (_order.Status)
            {
                case 0: statusText = "В ПРОЦЕСІ"; break;
                case 1: statusText = "ГОТУЄТЬСЯ"; break;
                case 2: statusText = "ЗАВЕРШЕНО"; sColor = Color.SpringGreen; break;
                case 3: statusText = "СКАСОВАНО"; sColor = Color.OrangeRed; break;
            }
            
            g.DrawString($"ID: {_order.Id}", SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.Gray, 30, 60);
            g.DrawString($"СТАТУС: {statusText}", SovereignEngine.GetFont("Montserrat Bold", 9f), new SolidBrush(sColor), 120, 60);
            g.DrawString($"КАСИР: {_order.User?.FullName ?? _order.User?.Username ?? "СИСТЕМА"}", 
                         SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.Gray, 30, 85);
            g.DrawString($"ДАТА: {_order.CreatedAt:dd.MM.yyyy HH:mm}", 
                         SovereignEngine.GetFont("Montserrat", 9f), Brushes.Gray, 250, 85);

            float yy = 125;
            g.DrawString("ТОВАР", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, 30, yy);
            g.DrawString("К-СТЬ", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, 320, yy);
            g.DrawString("ЦІНА", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, 400, yy);
            
            yy += 25;
            using (var p = new Pen(Color.FromArgb(30, Color.White))) g.DrawLine(p, 30, yy, 470, yy);
            yy += 15;

            foreach (var item in _items)
            {
                string pName = item.Product?.Name ?? "Товар";
                g.DrawString(pName, SovereignEngine.GetFont("Montserrat", 10f), Brushes.White, 30, yy);
                g.DrawString(item.Quantity.ToString(), SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.White, 325, yy);
                g.DrawString($"{item.PriceAtSale * item.Quantity:N2} ₴", SovereignEngine.GetFont("Montserrat Bold", 10f), 
                             new SolidBrush(SovereignEngine.AmberAccent), 400, yy);
                yy += 35;
            }

            yy = Height - 80;
            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 1f)) g.DrawLine(p, 30, yy, 470, yy);
            
            g.DrawString("ЗАГАЛЬНА СУМА:", SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.Gray, 30, yy + 25);
            g.DrawString($"{_order.TotalAmount:N2} ₴", SovereignEngine.GetFont("Montserrat Bold", 18f), 
                         new SolidBrush(SovereignEngine.AmberAccent), 280, yy + 18);
        }
    }
}
