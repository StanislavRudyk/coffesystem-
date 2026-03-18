using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cafe.Shared.Models;

namespace Cafe.Client.Admin.Forms
{
    public class UserEditModal : Form
    {
        public User UserInfo { get; private set; }
        private TextBox _txtUser, _txtPass, _txtRole, _txtName;
        private bool _isEdit;

        public UserEditModal(User existing = null)
        {
            _isEdit = existing != null;
            UserInfo = existing ?? new User { Username = "", Role = "Manager", FullName = "" };

            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(420, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(25, 27, 30);
            this.DoubleBuffered = true;

            InitFields();
            ApplyRounding();
        }

        private void ApplyRounding()
        {
            var path = SovereignEngine.GetRoundRect(new Rectangle(0, 0, Width, Height), 24);
            this.Region = new Region(path);
        }

        private void InitFields()
        {
            int curY = 110;

            _txtUser = CreateStyledInput("ЛОГІН", UserInfo.Username, ref curY);
            _txtName = CreateStyledInput("ПОВНЕ ІМ'Я", UserInfo.FullName, ref curY);
            _txtRole = CreateStyledInput("РОЛЬ (Admin/Manager)", UserInfo.Role, ref curY);
            _txtPass = CreateStyledInput("ПАРОЛЬ", "", ref curY, true);


            var btnCancel = CreateInvisibleButton(40, 460, 160, 45);
            btnCancel.Click += (s, e) => this.Close();

            var btnSave = CreateInvisibleButton(220, 460, 160, 45);
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(_txtUser.Text)) { SovereignAlert.Show("Введіть логін!", "ПОМИЛКА"); return; }
                UserInfo.Username = _txtUser.Text;
                UserInfo.FullName = _txtName.Text;
                UserInfo.Role = _txtRole.Text;
                if (!string.IsNullOrEmpty(_txtPass.Text)) UserInfo.PasswordHash = _txtPass.Text;
                this.DialogResult = DialogResult.OK;
            };
        }

        private Button CreateInvisibleButton(int x, int y, int w, int h)
        {
            var btn = new Button { Bounds = new Rectangle(x, y, w, h), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, Color.White);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, Color.White);
            this.Controls.Add(btn); btn.BringToFront();
            return btn;
        }

        private TextBox CreateStyledInput(string label, string val, ref int y, bool isPass = false)
        {
            var pnl = new Panel { Location = new Point(40, y), Size = new Size(340, 42), BackColor = Color.Transparent, Tag = label };
            var txt = new TextBox {
                Text = val, Location = new Point(12, 10), Width = 316,
                BackColor = Color.FromArgb(35, 37, 40), ForeColor = Color.White,
                BorderStyle = BorderStyle.None, Font = SovereignEngine.GetFont("Segoe UI", 11f),
                PasswordChar = isPass ? '*' : '\0'
            };
            pnl.Controls.Add(txt);
            txt.GotFocus += (s, e) => Invalidate();
            txt.LostFocus += (s, e) => Invalidate();
            this.Controls.Add(pnl);
            y += 75;
            return txt;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            

            g.DrawString(_isEdit ? "РЕДАГУВАТИ ПРОФІЛЬ" : "НОВИЙ СПІВРОБІТНИК", 
                         SovereignEngine.GetFont("Montserrat Bold", 15f), new SolidBrush(SovereignEngine.AmberAccent), 40, 40);


            foreach(Control c in this.Controls) {
                if (c is Panel p && p.Tag is string label) {
                    g.DrawString(label, SovereignEngine.GetFont("Montserrat Bold", 7.5f), Brushes.Gray, p.Left, p.Top - 22);
                    var rect = new RectangleF(p.Left, p.Top, p.Width, p.Height);
                    bool focused = p.Controls[0].Focused;
                    using (var path = SovereignEngine.GetRoundRect(rect, 10)) {
                        using (var br = new SolidBrush(Color.FromArgb(35, 37, 40))) g.FillPath(br, path);
                        using (var pPen = new Pen(focused ? SovereignEngine.AmberAccent : Color.FromArgb(50, Color.White), focused ? 1.8f : 1f)) g.DrawPath(pPen, path);
                    }
                }
            }

            DrawBtn(g, "СКАСУВАТИ", 40, 460, 160, 45, Color.FromArgb(45, 47, 52), Color.White);
            DrawBtn(g, _isEdit ? "ЗБЕРЕГТИ" : "ДОДАТИ", 220, 460, 160, 45, SovereignEngine.AmberAccent, Color.Black);


            using (var p = new Pen(Color.FromArgb(60, SovereignEngine.AmberAccent), 2))
                g.DrawPath(p, SovereignEngine.GetRoundRect(new Rectangle(1, 1, Width - 3, Height - 3), 24));
        }

        private void DrawBtn(Graphics g, string t, int x, int y, int w, int h, Color bg, Color fore) {
            var rect = new RectangleF(x, y, w, h);
            using(var path = SovereignEngine.GetRoundRect(rect, 10)) g.FillPath(new SolidBrush(bg), path);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(t, SovereignEngine.GetFont("Montserrat Bold", 9f), new SolidBrush(fore), rect, sf);
        }
    }
}
