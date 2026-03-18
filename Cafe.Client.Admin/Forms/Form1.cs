using System;
using Cafe.Client.Admin.Views;
using Cafe.Client.Admin.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Cafe.Client.Admin
{
    public partial class Form1 : Form
    {
        private Panel pnlSidebar;
        private Panel pnlMainContent;
        private Panel pnlViewCanvas;
        private Label lblViewTitle;
        private List<SidebarButton> _navButtons = new List<SidebarButton>();

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();
            SetupNavigation();
        }

        private void SetupCustomUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = SovereignEngine.SpaceCharcoal;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1280, 768);

            pnlSidebar = new Panel {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(16, 17, 20)
            };
            this.Controls.Add(pnlSidebar);

            pnlMainContent = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlMainContent);

            pnlMainContent.BringToFront();

            Panel pnlHeader = new Panel {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(40, 25, 0, 0)
            };
            pnlHeader.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } };
            pnlMainContent.Controls.Add(pnlHeader);

            lblViewTitle = new Label {
                Text = "ДЕРЖПАНЕЛЬ",
                ForeColor = Color.White,
                Font = SovereignEngine.GetFont("Montserrat Bold", 22f),
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblViewTitle);

            pnlViewCanvas = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(60, 20, 60, 40)
            };
            pnlMainContent.Controls.Add(pnlViewCanvas);
            pnlViewCanvas.BringToFront();

            var btnMin = new Label {
                Text = "—",
                ForeColor = Color.FromArgb(150, Color.White),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(this.Width - 240 - 80, 25),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMin.MouseEnter += (s, e) => btnMin.ForeColor = Color.White;
            btnMin.MouseLeave += (s, e) => btnMin.ForeColor = Color.FromArgb(150, Color.White);

            var btnCls = new Label {
                Text = "✕",
                ForeColor = Color.FromArgb(150, Color.White),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(this.Width - 240 - 45, 25),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            btnCls.Click += (s, e) => Application.Exit();
            btnCls.MouseEnter += (s, e) => btnCls.ForeColor = Color.Red;
            btnCls.MouseLeave += (s, e) => btnCls.ForeColor = Color.FromArgb(150, Color.White);

            pnlHeader.Controls.Add(btnMin);
            pnlHeader.Controls.Add(btnCls);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void SetupNavigation()
        {
            int yOffset = 100;
            var menuItems = new[] { 
                new { Title = "СТАТИСТИКА", Icon = "stats" },
                new { Title = "МЕНЮ ТОВАРІВ", Icon = "menu" },
                new { Title = "ІСТОРІЯ ЧЕКІВ", Icon = "history" },
                new { Title = "ПЕРСОНАЛ", Icon = "person" }
            };

            foreach (var item in menuItems)
            {
                var btn = new SidebarButton(item.Title, item.Icon) {
                    Location = new Point(10, yOffset),
                    Width = pnlSidebar.Width - 20,
                    Height = 45
                };
                btn.Click += (s, e) => ActivateButton((SidebarButton)s);
                pnlSidebar.Controls.Add(btn);
                _navButtons.Add(btn);
                yOffset += 55;
            }

            if (_navButtons.Count > 0) ActivateButton(_navButtons[0]);
        }

        private void ActivateButton(SidebarButton target)
        {
            foreach (var btn in _navButtons) btn.IsActive = (btn == target);
            lblViewTitle.Text = target.Title;

            switch (target.Title)
            {
                case "СТАТИСТИКА": ShowView(new DashboardView()); break;
                case "МЕНЮ ТОВАРІВ": ShowView(new MenuView()); break;
                case "ІСТОРІЯ ЧЕКІВ": ShowView(new HistoryView()); break;
                case "ПЕРСОНАЛ": ShowView(new PersonnelView()); break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SovereignEngine.DrawPremiumBackground(e.Graphics, this.ClientRectangle);
        }

        private void ShowView(UserControl view)
        {
            pnlViewCanvas.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlViewCanvas.Controls.Add(view);
        }
    }
}
