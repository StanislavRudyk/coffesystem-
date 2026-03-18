using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cafe.Shared.Models;
using Cafe.Shared.Services;
using Cafe.Client.Admin.Forms;

namespace Cafe.Client.Admin.Views
{
    public partial class MenuView : UserControl
    {
        private List<Product> _items = new List<Product>();
        private List<Product> _filteredItems = new List<Product>();
        private List<Category> _allCategories = new List<Category>();
        private int _selectedCategory = 0;
        private TextBox _searchBox;
        private float _scrollOffset = 0;
        private float _maxScroll = 0;

        public MenuView()
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
            this.MouseWheel += (s, e) => {
                _scrollOffset += e.Delta > 0 ? 40 : -40;
                if (_scrollOffset > 0) _scrollOffset = 0;
                if (_scrollOffset < -_maxScroll) _scrollOffset = -_maxScroll;
                Invalidate();
            };
            RefreshData();
            
            this.Resize += (s, e) => {
                _searchBox.Location = new Point(55, 57);
                _searchBox.Width = (int)((this.Width - 40) * 0.65f) - 60;
            };
        }

        private async void RefreshData()
        {
            _allCategories = await ApiService.Instance.GetCategoriesAsync();
            _items = await ApiService.Instance.GetProductsAsync();
            _filteredItems = _items;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = this.Width - 40;

            float topY = 20;
            DrawBentoSection(g, 20, topY, w * 0.65f, 70, "ПОШУК ТОВАРІВ");
            

            var searchRect = new RectangleF(40, topY + 32, w * 0.65f - 40, 30);
            using (var p = new Pen(Color.FromArgb(40, Color.White), 1f))
            using (var path = SovereignEngine.GetRoundRect(searchRect, 8))
            {
                using(var br = new SolidBrush(Color.FromArgb(20, 22, 25))) g.FillPath(br, path);
                g.DrawPath(p, path);
            }
            if (_searchBox.Width == 0) {
                _searchBox.Location = new Point(55, (int)topY + 37);
                _searchBox.Width = (int)(w * 0.65f) - 60;
            }

            DrawBentoSection(g, 20 + w * 0.65f + 20, topY, w * 0.35f - 20, 70, "КЕРУВАННЯ");
            SovereignEngine.DrawActionButton(g, 20 + w * 0.65f + 40, topY + 32, w * 0.35f - 60, 30, "ДОДАТИ ТОВАР", SovereignEngine.AmberAccent);

            DrawCategoryTabs(g, 20, (int)topY + 90);

            if (_allCategories.Count == 0) {
                g.DrawString("КАТЕГОРІЇ НЕ ЗАВАНТАЖЕНІ", SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.Gray, 20, topY + 160);
                return;
            }
            
            var groups = _allCategories.GroupBy(c => MapCategoryName(c.Name)).ToList();
            if (_selectedCategory >= groups.Count) _selectedCategory = 0;
            var currentGroup = groups[_selectedCategory];
            var validDbNames = currentGroup.Select(x => x.Name).ToList();
            
            string query = _searchBox.Text.ToLower();
            _filteredItems = _items.FindAll(x => validDbNames.Contains(x.Category?.Name) && (string.IsNullOrEmpty(query) || x.Name.ToLower().Contains(query)));
            
            if (_filteredItems.Count == 0) {
                g.DrawString(string.IsNullOrEmpty(query) ? "У ЦІЙ КАТЕГОРІЇ ПОКИ НЕМАЄ ТОВАРІВ" : "НІЧОГО НЕ ЗНАЙДЕНО", SovereignEngine.GetFont("Montserrat", 9f), Brushes.Gray, 30, topY + 160);
                return;
            }

            float yy = topY + 155 + _scrollOffset;
            float rowH = 65;
            float spacing = 12;

            var oldClip = g.Clip;
            var itemArea = new RectangleF(20, topY + 145, w + 20, this.Height - (topY + 145) - 20);
            g.SetClip(itemArea);

            for (int i = 0; i < _filteredItems.Count; i++)
            {
                string dispCat = currentGroup.Key;
                DrawMenuRow(g, 20, yy, w, rowH, _filteredItems[i].Name, dispCat, _filteredItems[i].Price.ToString("N2") + " ₴", _filteredItems[i].IsActive);
                yy += rowH + spacing;
            }

            g.Clip = oldClip;
            _maxScroll = Math.Max(0, (yy - _scrollOffset) - (this.Height - 40));

            if (_maxScroll > 0)
            {
                float viewH = this.Height - (topY + 145) - 40;
                float contentH = (yy - _scrollOffset) - (topY + 155);
                float thumbH = Math.Max(30, viewH * (viewH / (contentH + viewH)));
                float thumbY = topY + 145 + (-_scrollOffset / _maxScroll) * (viewH - thumbH);

                var scrollRect = new RectangleF(this.Width - 15, topY + 145, 4, viewH);
                using (var br = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
                    g.FillRectangle(br, scrollRect);

                var thumbRect = new RectangleF(this.Width - 15, thumbY, 4, thumbH);
                using (var br = new SolidBrush(Color.FromArgb(80, SovereignEngine.AmberAccent)))
                    g.FillRectangle(br, thumbRect);
            }
        }

        private string MapCategoryName(string dbName) => SovereignEngine.MapCategoryName(dbName);

        private async void View_MouseClick(object sender, MouseEventArgs e)
        {
            float w = this.Width - 40;
            float topY = 20;

            if (e.X >= 20 + w * 0.65f + 40 && e.X <= 20 + w - 20 && e.Y >= topY + 32 && e.Y <= topY + 62)
            {
                if (_allCategories.Count > 0)
                {
                    var modal = new Cafe.Client.Admin.Forms.ProductEditModal(_allCategories);
                    if (modal.ShowDialog() == DialogResult.OK)
                    {
                        await ApiService.Instance.CreateProductAsync(modal.ProductInfo);
                        RefreshData();
                    }
                }
                return;
            }

            int tabX = 20;
            var groups = _allCategories.GroupBy(c => MapCategoryName(c.Name)).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var dispName = groups[i].Key;
                var size = CreateGraphics().MeasureString(dispName, SovereignEngine.GetFont("Montserrat Bold", 8.5f));
                int tabW = (int)size.Width + 40;
                if (e.X >= tabX && e.X <= tabX + tabW && e.Y >= topY + 90 && e.Y <= topY + 122)
                {
                    _selectedCategory = i;
                    Invalidate();
                    return;
                }
                tabX += tabW + 12;
            }

            if (groups.Count == 0 || _selectedCategory < 0 || _selectedCategory >= groups.Count) return;
            var currentGroup = groups[_selectedCategory];
            var validDbNames = currentGroup.Select(x => x.Name).ToList();
            float rowY = topY + 155 + _scrollOffset; 
            float rowH = 65;
            float spacing = 12;

            for (int i = 0; i < _filteredItems.Count; i++)
            {
                float btnDelX = 20 + w - 50;
                float btnEditX = btnDelX - 35;


                float hitTop = rowY + 20;
                float hitBottom = rowY + 45;


                if (e.X >= btnEditX && e.X <= btnEditX + 24 && e.Y >= hitTop && e.Y <= hitBottom)
                {
                    var modal = new Cafe.Client.Admin.Forms.ProductEditModal(_allCategories, _filteredItems[i]);
                    if (modal.ShowDialog() == DialogResult.OK)
                    {
                        await ApiService.Instance.UpdateProductAsync(modal.ProductInfo);
                        RefreshData();
                    }
                    return;
                }

                if (e.X >= btnDelX && e.X <= btnDelX + 24 && e.Y >= hitTop && e.Y <= hitBottom)
                {
                    if (Cafe.Client.Admin.Forms.SovereignConfirm.Show($"Ви дійсно хочете видалити товар '{_filteredItems[i].Name}'?", "ПІДТВЕРДЖЕННЯ") == DialogResult.Yes)
                    {
                        await ApiService.Instance.DeleteProductAsync(_filteredItems[i].Id);
                        RefreshData();
                    }
                    return;
                }
                rowY += rowH + spacing;
            }
        }

        private void DrawBentoSection(Graphics g, float x, float y, float w, float h, string title)
        {
            SovereignEngine.DrawBentoPanel(g, new RectangleF(x, y, w, h), 16);
            g.DrawString(title, SovereignEngine.GetFont("Montserrat Bold", 6.5f), new SolidBrush(Color.FromArgb(130, Color.White)), x + 20, y + 12);
        }



        private void DrawCategoryTabs(Graphics g, int x, int y)
        {
            int curX = x;
            var groups = _allCategories.GroupBy(c => MapCategoryName(c.Name)).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                bool isSel = i == _selectedCategory;
                string dispName = groups[i].Key;
                var size = g.MeasureString(dispName, SovereignEngine.GetFont("Montserrat Bold", 8.5f));
                int w = (int)size.Width + 40;

                if (isSel)
                {
                    var rect = new RectangleF(curX, y, w, 32);
                    using (var path = SovereignEngine.GetRoundRect(rect, 10))
                    {
                        using (var br = new SolidBrush(Color.FromArgb(25, SovereignEngine.AmberAccent))) g.FillPath(br, path);
                        using (var p = new Pen(SovereignEngine.AmberAccent, 1.2f)) g.DrawPath(p, path);
                    }
                }

                g.DrawString(dispName, SovereignEngine.GetFont("Montserrat Bold", 8.5f), 
                    new SolidBrush(isSel ? SovereignEngine.AmberAccent : Color.FromArgb(100, Color.White)), curX + 20, y + 8);
                curX += w + 12;
            }
        }

        private void DrawMenuRow(Graphics g, float x, float y, float w, float h, string name, string cat, string price, bool ok)
        {
            var rect = new RectangleF(x, y, w, h);
            SovereignEngine.DrawBentoPanel(g, rect, 14);

            g.FillEllipse(ok ? Brushes.LimeGreen : Brushes.OrangeRed, x + 25, y + h / 2 - 4, 8, 8);
            g.DrawString(name, SovereignEngine.GetFont("Montserrat Bold", 10f), Brushes.White, x + 50, y + 14);
            g.DrawString(cat, SovereignEngine.GetFont("Montserrat", 8f), Brushes.Gray, x + 50, y + 34);

            g.DrawString(price, SovereignEngine.GetFont("Montserrat Bold", 11f), new SolidBrush(SovereignEngine.AmberAccent), x + w - 160, y + h / 2 - 8);


            var btnEdit = new RectangleF(x + w - 85, y + h / 2 - 12, 24, 24);
            using (var p = new Pen(Color.FromArgb(200, SovereignEngine.AmberAccent), 2.0f))
            {
                g.DrawEllipse(p, btnEdit);

                g.DrawLine(p, btnEdit.X + 7, btnEdit.Bottom - 7, btnEdit.X + 11, btnEdit.Bottom - 7);
                g.DrawLine(p, btnEdit.X + 7, btnEdit.Bottom - 7, btnEdit.X + 7, btnEdit.Bottom - 11);
                g.DrawLine(p, btnEdit.X + 7, btnEdit.Bottom - 11, btnEdit.Right - 8, btnEdit.Y + 7);
                g.DrawLine(p, btnEdit.Right - 8, btnEdit.Y + 7, btnEdit.Right - 4, btnEdit.Y + 11);
                g.DrawLine(p, btnEdit.Right - 4, btnEdit.Y + 11, btnEdit.X + 11, btnEdit.Bottom - 7);
            }

            var btnDel = new RectangleF(x + w - 50, y + h / 2 - 12, 24, 24);
            using (var p = new Pen(Color.FromArgb(220, 240, 60, 60), 2.0f))
            {
                g.DrawEllipse(p, btnDel);
                g.DrawLine(p, btnDel.X + 8, btnDel.Y + 8, btnDel.Right - 8, btnDel.Bottom - 8);
                g.DrawLine(p, btnDel.X + 8, btnDel.Bottom - 8, btnDel.Right - 8, btnDel.Y + 8);
            }
        }
    }
}
