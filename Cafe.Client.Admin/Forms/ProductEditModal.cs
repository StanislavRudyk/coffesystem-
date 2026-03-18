using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Cafe.Shared.Models;
using Cafe.Client.Admin.Forms;

namespace Cafe.Client.Admin.Forms
{
    public class ProductEditModal : Form
    {
        private TextBox _txtName;
        private TextBox _txtPrice;
        private TextBox _txtDesc;
        private Button _btnSave;
        private Button _btnCancel;
        private int _selectedCatIdx = 0;
        private bool _isActive = true;
        
        public Product ProductInfo { get; private set; }
        private List<Category> _categories;
        private bool _isEdit;

        public ProductEditModal(List<Category> categories, Product existingProduct = null)
        {
            _categories = categories;
            _isEdit = existingProduct != null;
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 560);
            this.BackColor = Color.FromArgb(25, 27, 30);
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            ProductInfo = _isEdit ? new Product 
            { 
                Id = existingProduct.Id, 
                Name = existingProduct.Name, 
                Price = existingProduct.Price, 
                Description = existingProduct.Description,
                CategoryId = existingProduct.CategoryId, 
                IsActive = existingProduct.IsActive 
            } : new Product { IsActive = true, Name = "", Price = 0, Description = "" };

            _isActive = ProductInfo.IsActive;
            if (_isEdit) {
                var cat = _categories.FirstOrDefault(c => c.Id == ProductInfo.CategoryId);
                if (cat != null) {
                    var mapped = SovereignEngine.MapCategoryName(cat.Name);
                    var groups = _categories.GroupBy(c => SovereignEngine.MapCategoryName(c.Name)).ToList();
                    _selectedCatIdx = groups.FindIndex(g => g.Key == mapped);
                    if (_selectedCatIdx == -1) _selectedCatIdx = 0;
                }
            }

            SetupInputs();
            SetupButtons();
            ApplyRounding();
            
            this.Shown += (s, e) => _txtName.Focus();
        }

        private void SetupInputs()
        {
            _txtName = CreateStyledInput(40, 110, 370, 45, false);
            _txtName.Text = ProductInfo.Name;
            
            _txtPrice = CreateStyledInput(40, 185, 370, 45, false);
            _txtPrice.Text = _isEdit ? ProductInfo.Price.ToString("0.##") : "";
            
            _txtDesc = CreateStyledInput(40, 260, 370, 80, true);
            _txtDesc.Text = ProductInfo.Description ?? "";

            this.MouseDown += (s, e) => {
                var groups = _categories.GroupBy(c => SovereignEngine.MapCategoryName(c.Name)).ToList();
                int curX = 40;
                for (int i = 0; i < groups.Count; i++) {
                    var size = CreateGraphics().MeasureString(groups[i].Key, SovereignEngine.GetFont("Montserrat Bold", 8f));
                    var rect = new Rectangle(curX, 375, (int)size.Width + 30, 32);
                    if (rect.Contains(e.Location)) { 
                        _selectedCatIdx = i; 

                        ProductInfo.CategoryId = groups[i].First().Id;
                        Invalidate(); 
                        break; 
                    }
                    curX += (int)size.Width + 42;
                }

                if (new Rectangle(40, 425, 150, 30).Contains(e.Location)) { _isActive = !_isActive; Invalidate(); }
            };
        }

        private void SetupButtons()
        {
            _btnCancel = CreateInvisibleButton(40, 490, 180, 50);
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            _btnSave = CreateInvisibleButton(230, 490, 180, 50);
            _btnSave.Click += (s, e) => Save();
        }

        private Button CreateInvisibleButton(int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Bounds = new Rectangle(x, y, w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, Color.White);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, Color.White);
            this.Controls.Add(btn);
            btn.BringToFront();
            return btn;
        }

        private TextBox CreateStyledInput(int x, int y, int w, int h, bool multi)
        {
            var tb = new TextBox
            {
                Location = new Point(x + 12, y + 10),
                Width = w - 24,
                Height = h - 20,
                Multiline = multi,
                BackColor = Color.FromArgb(35, 37, 40),
                ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Segoe UI", 11.5f),
                BorderStyle = BorderStyle.None
            };
            if (multi) tb.Height = h - 20;
            tb.GotFocus += (s, e) => Invalidate();
            tb.LostFocus += (s, e) => Invalidate();
            this.Controls.Add(tb);
            tb.BringToFront();
            return tb;
        }

        private void ApplyRounding()
        {
            var path = SovereignEngine.GetRoundRect(new Rectangle(0, 0, Width, Height), 24);
            this.Region = new Region(path);
        }

        private void Save()
        {
            string name = _txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) {
                SovereignAlert.Show("Будь ласка, введіть назву товару.", "ПОМИЛКА ВАЛІДАЦІЇ");
                _txtName.Focus();
                return;
            }
            
            string priceStr = _txtPrice.Text.Replace(".", ",");
            if (!decimal.TryParse(priceStr, out decimal p)) {
                SovereignAlert.Show("Будь ласка, введіть коректну ціну.", "ПОМИЛКА ВАЛІДАЦІЇ");
                _txtPrice.Focus();
                return;
            }
            
            ProductInfo.Name = name;
            ProductInfo.Price = p;
            ProductInfo.Description = _txtDesc.Text;
            ProductInfo.IsActive = _isActive;
            
            var groups = _categories.GroupBy(c => SovereignEngine.MapCategoryName(c.Name)).ToList();
            if (groups.Count > 0 && _selectedCatIdx >= 0 && _selectedCatIdx < groups.Count) {
                ProductInfo.CategoryId = groups[_selectedCatIdx].First().Id;
            }
            
            this.DialogResult = DialogResult.OK;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;


            g.DrawString(_isEdit ? "РЕДАГУВАТИ ТОВАР" : "НОВИЙ ТОВАР", SovereignEngine.GetFont("Montserrat Bold", 16f), new SolidBrush(SovereignEngine.AmberAccent), 40, 40);


            DrawFieldBg(g, "НАЗВА ТОВАРУ", 40, 110, 370, 45, _txtName.Focused);
            DrawFieldBg(g, "ЦІНА (₴)", 40, 185, 370, 45, _txtPrice.Focused);
            DrawFieldBg(g, "ОПИС ТОВАРУ", 40, 260, 370, 80, _txtDesc.Focused);


            g.DrawString("КАТЕГОРІЯ", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, 40, 355);
            var groups = _categories.GroupBy(c => SovereignEngine.MapCategoryName(c.Name)).ToList();
            int curX = 40;
            for (int i = 0; i < groups.Count; i++) {
                bool sel = i == _selectedCatIdx;
                string name = groups[i].Key;
                var size = g.MeasureString(name, SovereignEngine.GetFont("Montserrat Bold", 8f));
                var rect = new RectangleF(curX, 375, size.Width + 30, 32);
                
                using (var path = SovereignEngine.GetRoundRect(rect, 8)) {
                    if (sel) {
                        using (var br = new SolidBrush(Color.FromArgb(50, SovereignEngine.AmberAccent))) g.FillPath(br, path);
                        using (var p = new Pen(SovereignEngine.AmberAccent, 1.5f)) g.DrawPath(p, path);
                    } else {
                        using (var p = new Pen(Color.FromArgb(40, Color.White), 1f)) g.DrawPath(p, path);
                    }
                }
                g.DrawString(name, SovereignEngine.GetFont("Montserrat Bold", 8f), sel ? new SolidBrush(SovereignEngine.AmberAccent) : Brushes.Gray, curX + 15, 385);
                curX += (int)size.Width + 42;
            }


            g.DrawString("СТАТУС", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, 40, 425);
            var chkRect = new Rectangle(40, 445, 20, 20);
            using (var p = new Pen(_isActive ? SovereignEngine.AmberAccent : Color.Gray, 2f)) g.DrawRectangle(p, chkRect);
            if (_isActive) g.FillRectangle(new SolidBrush(SovereignEngine.AmberAccent), chkRect.X + 4, chkRect.Y + 4, 12, 12);
            g.DrawString("АКТИВНИЙ ТОВАР", SovereignEngine.GetFont("Montserrat Bold", 9f), _isActive ? Brushes.White : Brushes.Gray, 70, 448);


            DrawBtn(g, "СКАСУВАТИ", 40, 490, 180, 50, Color.FromArgb(40, 40, 45), Color.White);
            DrawBtn(g, "ЗБЕРЕГТИ", 230, 490, 180, 50, SovereignEngine.AmberAccent, Color.Black);


            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 2)) {
                var borderPath = SovereignEngine.GetRoundRect(new Rectangle(1, 1, Width - 3, Height - 3), 24);
                g.DrawPath(p, borderPath);
            }
        }

        private void DrawFieldBg(Graphics g, string label, int x, int y, int w, int h, bool focus) {
            g.DrawString(label, SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Gray, x, y - 25);
            var rect = new RectangleF(x, y, w, h);
            using (var path = SovereignEngine.GetRoundRect(rect, 10)) {
                using (var br = new SolidBrush(Color.FromArgb(35, 37, 40))) g.FillPath(br, path);
                using (var p = new Pen(focus ? SovereignEngine.AmberAccent : Color.FromArgb(50, Color.White), focus ? 2f : 1f)) g.DrawPath(p, path);
            }
        }

        private void DrawBtn(Graphics g, string txt, int x, int y, int w, int h, Color bg, Color fore) {
            var rect = new RectangleF(x, y, w, h);
            using (var path = SovereignEngine.GetRoundRect(rect, 12)) {
                g.FillPath(new SolidBrush(bg), path);
            }
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(txt, SovereignEngine.GetFont("Montserrat Bold", 10f), new SolidBrush(fore), rect, sf);
        }
    }
}
