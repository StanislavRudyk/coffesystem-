using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Cafe.Shared.Models;
using Cafe.Shared.Services;
using Cafe.Client.Admin.Forms;

namespace Cafe.Client.Admin.Views
{
    public partial class PersonnelView : UserControl
    {
        private List<User> _staff = new List<User>();
        private List<Order> _allOrders = new List<Order>();
        private TextBox _searchBox;
        private string _selectedRole = "ALL";

        public PersonnelView()
        {
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Transparent;

            _searchBox = new TextBox
            {
                BackColor = Color.FromArgb(20, 22, 25),
                ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Segoe UI", 11f),
                BorderStyle = BorderStyle.None,
            };
            _searchBox.TextChanged += (s, e) => { Invalidate(); };
            this.Controls.Add(_searchBox);

            this.MouseClick += View_MouseClick;
            
            this.Resize += (s, e) => {
                Invalidate();
            };

            RefreshData();
        }

        private async void RefreshData()
        {
            try 
            {
                _staff = await ApiService.Instance.GetUsersAsync();
                _allOrders = await ApiService.Instance.GetOrdersAsync();
                Invalidate();
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = this.Width - 40;
            DrawBentoSection(g, 20, 20, w * 0.7f, 70, "КЕРУВАННЯ ПЕРСОНАЛОМ");
            SovereignEngine.DrawActionButton(g, 40, 52, 180, 30, "+ ДОДАТИ СПІВРОБІТНИКА", Color.SpringGreen);


            float fx = 230;
            string[] labels = { "УСІ", "АДМІН", "МЕНЕДЖЕР" };
            string[] roles = { "ALL", "ADMIN", "MANAGER" };
            for(int i = 0; i < roles.Length; i++) {
                bool sel = _selectedRole == roles[i];
                SovereignEngine.DrawActionButton(g, fx, 52, 80, 30, labels[i], sel ? SovereignEngine.AmberAccent : Color.FromArgb(40, Color.White), sel ? Color.Black : Color.White);
                fx += 90;
            }

            var searchRect = new RectangleF(20 + w * 0.7f + 20, 20, w * 0.3f - 20, 70);
            SovereignEngine.DrawBentoPanel(g, searchRect, 16);
            g.DrawString("ПОШУК", SovereignEngine.GetFont("Montserrat Bold", 6.5f), new SolidBrush(Color.FromArgb(130, Color.White)), searchRect.X + 20, searchRect.Y + 12);
            
            var boxRect = new RectangleF(searchRect.X + 20, searchRect.Y + 30, searchRect.Width - 40, 28);
            using (var p = new Pen(Color.FromArgb(40, Color.White), 1f))
            using (var path = SovereignEngine.GetRoundRect(boxRect, 8))
            {
                using(var br = new SolidBrush(Color.FromArgb(20, 22, 25))) g.FillPath(br, path);
                g.DrawPath(p, path);
            }

            _searchBox.Location = new Point((int)boxRect.X + 10, (int)boxRect.Y + 4);
            _searchBox.Width = (int)boxRect.Width - 20;
            _searchBox.Height = (int)boxRect.Height - 8;

            string query = _searchBox.Text.ToLower();
            var filteredStaff = _staff.Where(s => 
                (string.IsNullOrEmpty(query) || s.Username.ToLower().Contains(query) || (s.FullName != null && s.FullName.ToLower().Contains(query))) &&
                (_selectedRole == "ALL" || (s.Role != null && s.Role.ToUpper() == _selectedRole))
            ).ToList();

            if (filteredStaff.Count == 0)
            {
                g.DrawString("СПІВРОБІТНИКИ НЕ ЗНАЙДЕНІ", SovereignEngine.GetFont("Montserrat Bold", 10f), 
                             new SolidBrush(Color.FromArgb(60, Color.White)), 40, 150);
                return;
            }

            float margin = 20;
            float cardW = 280;
            float cardH = 340;
            float startX = 20;
            float startY = 110;

            var topUser = _staff.OrderByDescending(s => _allOrders.Where(o => o.UserId == s.Id).Sum(o => o.TotalAmount)).FirstOrDefault();

            Color[] accents = { SovereignEngine.AmberAccent, SovereignEngine.BlueAccent, SovereignEngine.VioletAccent, Color.SpringGreen };

            for (int i = 0; i < filteredStaff.Count; i++)
            {
                var staff = filteredStaff[i];
                float cx = startX + (i % 3) * (cardW + margin);
                float cy = startY + (i / 3) * (cardH + margin);

                int orders = _allOrders.Count(o => o.UserId == staff.Id);
                decimal revenue = _allOrders.Where(o => o.UserId == staff.Id).Sum(o => o.TotalAmount);
                var lastOrder = _allOrders.Where(o => o.UserId == staff.Id).OrderByDescending(o => o.CreatedAt).FirstOrDefault();

                DrawStaffCard(g, cx, cy, cardW, cardH, staff, accents[i % accents.Length], orders, revenue, staff == topUser && revenue > 0, lastOrder?.CreatedAt);
            }
        }

        private void DrawStaffCard(Graphics g, float x, float y, float w, float h, User staff, Color accent, int orderCount, decimal totalRevenue, bool isTop, DateTime? lastSeen)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect);

            if (isTop)
            {
                SovereignEngine.DrawBadge(g, new RectangleF(x + w - 105, y + 15, 90, 20), "ЛІДЕР ПРОДАЖІВ", SovereignEngine.AmberAccent);
            }

            var avatarRect = new RectangleF(x + 20, y + 25, 60, 60);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(avatarRect);
                g.FillPath(new SolidBrush(Color.FromArgb(30, accent)), path);
                g.DrawPath(new Pen(Color.FromArgb(100, accent), 2f), path);
            }
            SovereignEngine.DrawIcon(g, "person", new RectangleF(avatarRect.X + 15, avatarRect.Y + 15, 30, 30), accent);

            g.DrawString(staff.FullName ?? staff.Username.ToUpper(), SovereignEngine.GetFont("Montserrat Bold", 11f), Brushes.White, x + 95, y + 32);
            g.DrawString(staff.Role ?? "Manager", SovereignEngine.GetFont("Montserrat", 8.5f), new SolidBrush(Color.Gray), x + 95, y + 55);


            float sy = y + 105;
            g.DrawString("ПОКАЗНИКИ ЕФЕКТИВНОСТІ", SovereignEngine.GetFont("Montserrat Bold", 7f), new SolidBrush(Color.FromArgb(80, Color.White)), x + 25, sy);
            
            sy += 25;
            DrawStatLine(g, x + 25, sy, w - 50, "ЗАМОВЛЕННЯ", orderCount.ToString(), SovereignEngine.AmberAccent);
            sy += 45;
            DrawStatLine(g, x + 25, sy, w - 50, "ВИРУЧКА", totalRevenue.ToString("N0") + " ₴", Color.SpringGreen);
            sy += 45;
            string lastText = lastSeen.HasValue ? lastSeen.Value.ToLocalTime().ToString("HH:mm") : "--:--";
            DrawStatLine(g, x + 25, sy, w - 50, "ОСТАННІЙ ЧЕК", lastText, Color.Gray);

            var btnEdit = new RectangleF(x + w - 85, y + h - 45, 30, 25);
            var btnDel = new RectangleF(x + w - 45, y + h - 45, 30, 25);

            using (var path = SovereignEngine.GetRoundRect(btnEdit, 6)) g.FillPath(new SolidBrush(Color.FromArgb(30, Color.White)), path);
            SovereignEngine.DrawIcon(g, "edit", new RectangleF(btnEdit.X + 7, btnEdit.Y + 5, 16, 16), Color.White);

            using (var path = SovereignEngine.GetRoundRect(btnDel, 6)) g.FillPath(new SolidBrush(Color.FromArgb(40, Color.OrangeRed)), path);
            g.DrawString("✕", SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.White, btnDel, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawStatLine(Graphics g, float x, float y, float w, string label, string val, Color c)
        {
            g.DrawString(label, SovereignEngine.GetFont("Montserrat", 8f), Brushes.Gray, x, y);
            var sf = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(val, SovereignEngine.GetFont("Montserrat Bold", 10f), new SolidBrush(c), new RectangleF(x + w - 150, y - 2, 150, 20), sf);
            using (var p = new Pen(Color.FromArgb(15, Color.White))) g.DrawLine(p, x, y + 25, x + w, y + 25);
        }

        private void DrawBentoSection(Graphics g, float x, float y, float w, float h, string title)
        {
            SovereignEngine.DrawBentoPanel(g, new RectangleF(x, y, w, h), 16);
            g.DrawString(title, SovereignEngine.GetFont("Montserrat Bold", 7.5f), new SolidBrush(Color.FromArgb(130, Color.White)), x + 25, y + 15);
        }

        private async void View_MouseClick(object sender, MouseEventArgs e)
        {
            if (new RectangleF(40, 52, 180, 30).Contains(e.Location))
            {
                var modal = new Cafe.Client.Admin.Forms.UserEditModal();
                if (modal.ShowDialog() == DialogResult.OK)
                {
                    await ApiService.Instance.CreateUserAsync(modal.UserInfo);
                    RefreshData();
                }
                return;
            }

            float fx = 230;
            string[] roles = { "ALL", "ADMIN", "MANAGER" };
            foreach(var r in roles) {
                if (new RectangleF(fx, 52, 80, 30).Contains(e.Location)) {
                    _selectedRole = r;
                    Invalidate();
                    return;
                }
                fx += 90;
            }

            float margin = 20;
            float cardW = 280;
            float cardH = 340;
            float startX = 20;
            float startY = 110;

            string query = _searchBox.Text.ToLower();
            var filteredStaff = _staff.Where(s => 
                (string.IsNullOrEmpty(query) || s.Username.ToLower().Contains(query) || (s.FullName != null && s.FullName.ToLower().Contains(query))) &&
                (_selectedRole == "ALL" || (s.Role != null && s.Role.ToUpper() == _selectedRole))
            ).ToList();

            for (int i = 0; i < filteredStaff.Count; i++)
            {
                float cx = startX + (i % 3) * (cardW + margin);
                float cy = startY + (i / 3) * (cardH + margin);

                var btnEdit = new RectangleF(cx + cardW - 85, cy + cardH - 45, 30, 25);
                var btnDel = new RectangleF(cx + cardW - 45, cy + cardH - 45, 30, 25);

                if (btnEdit.Contains(e.Location))
                {
                    var modal = new Cafe.Client.Admin.Forms.UserEditModal(filteredStaff[i]);
                    if (modal.ShowDialog() == DialogResult.OK)
                    {
                        await ApiService.Instance.UpdateUserAsync(modal.UserInfo);
                        RefreshData();
                    }
                    return;
                }

                if (btnDel.Contains(e.Location))
                {
                    if (SovereignConfirm.Show($"Видалити співробітника '{filteredStaff[i].Username}'?", "ПЕРСОНАЛ") == DialogResult.Yes)
                    {
                        await ApiService.Instance.DeleteUserAsync(filteredStaff[i].Id);
                        RefreshData();
                    }
                    return;
                }
            }
        }

        private void DrawShiftTimeline(Graphics g, float x, float y, float w, float h)
        {
            using (var p = new Pen(Color.FromArgb(20, Color.White), 2f))
                g.DrawLine(p, x, y + h/2, x + w, y + h/2);

            float[] morning = { 0.1f, 0.4f };
            float[] evening = { 0.5f, 0.9f };

            DrawTimelineBlock(g, x + w * morning[0], y + 15, w * (morning[1]-morning[0]), 30, SovereignEngine.AmberAccent);
            DrawTimelineBlock(g, x + w * evening[0], y + 15, w * (evening[1]-evening[0]), 30, SovereignEngine.BlueAccent);

            for(int i = 8; i <= 22; i+=2)
            {
                float px = x + w * ((float)(i-8) / 14);
                g.DrawString(i.ToString() + ":00", SovereignEngine.GetFont("Montserrat", 7f), Brushes.Gray, px - 15, y + 50);
            }
        }

        private void DrawTimelineBlock(Graphics g, float x, float y, float w, float h, Color c)
        {
            var rect = new RectangleF(x, y, w, h);
            using(var path = SovereignEngine.GetRoundRect(rect, 8))
            {
                g.FillPath(new SolidBrush(Color.FromArgb(60, c)), path);
                g.DrawPath(new Pen(Color.FromArgb(150, c), 1.5f), path);
            }
        }
    }
}
