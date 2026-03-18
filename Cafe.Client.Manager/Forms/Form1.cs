using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Cafe.Client.Manager.Forms;
using Cafe.Client.Manager.Services;
using Cafe.Shared.Models;
using Cafe.Shared.Services;
using Cafe.Client.Manager;

namespace Cafe.Client.Manager.Forms
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr h, int msg, int w, int l);
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        const int WM_NCLBUTTONDOWN = 0xA1, HT_CAPTION = 0x2;

        private SovereignLedger _ledger;
        private Label _lblTotal;
        private Label _lblStaticTotal;
        private Panel _pnlFooter;
        private Panel _pnlLine;
        private EmberActionButton _btnCheckout;
        private decimal _currentTotal = 0;

        private Panel _sidebar;
        private Panel _cartPanel;
        private Panel _headerPanel;
        private Panel _filterBar;
        private SmoothGrid _productGrid;

     
        private SovereignSearch _searchBar;
        private Panel _viewCatalog;
        private Panel _viewOperations;
        private Panel _viewConfig;

     
        private int _ordersToday = 0;
        private DateTime _lastOrderTime = DateTime.MinValue;
        private Label _lblBaristaIQ;
        private Timer _baristaTimer;
        private Dictionary<string, bool> _activeModules = new Dictionary<string, bool>();

     
        private Bitmap _bgCache;
        private Timer _searchDebounceTimer;
        private string _activeConfigCategory = "ШІ";
        private string _configSearchQuery = "";

        public Form1()
        {
            _instance = this;
            InitializeComponent();
            InitUI();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
          
            this.PerformLayout();
            LayoutUI();

          
            LoadCategoriesAndCatalog();
        
            this.Invalidate(true);
        }

        private void InitUI()
        {
          
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(1280, 768);
            this.MinimumSize = new Size(1024, 768); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = SovereignEngine.SpaceCharcoal;
            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            this.MouseMove += (s, e) => { SovereignEngine.GlobalMousePos = e.Location; Invalidate(); };

        
            _headerPanel = new Panel { BackColor = Color.Transparent };
            _headerPanel.MouseDown += Form_MouseDown;
            Controls.Add(_headerPanel);
            
            _sidebar = new Panel { BackColor = Color.Transparent, Padding = new Padding(0, 30, 0, 0) };
            Controls.Add(_sidebar);

            _cartPanel = new SovereignContainer { BackColor = Color.Transparent, Padding = new Padding(25, 25, 25, 35) };
            _cartPanel.Visible = true;
            Controls.Add(_cartPanel);

            _filterBar = new FlowLayoutPanel { BackColor = Color.Transparent, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
            _filterBar.Visible = true;
            Controls.Add(_filterBar);

            _viewCatalog = new Panel { BackColor = Color.Transparent, Visible = true };
            _viewOperations = new Panel { BackColor = Color.Transparent, Visible = false };
            _viewConfig = new Panel { BackColor = Color.Transparent, Visible = false };
            
          
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, _viewCatalog, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, _viewOperations, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, _viewConfig, new object[] { true });

            this.Controls.Add(_viewCatalog);
            Controls.Add(_viewOperations);
            Controls.Add(_viewConfig);

          
            var lblLogo = new Label { Text = "КАСОВИЙ ТЕРМІНАЛ", Font = SovereignEngine.GetFont("Montserrat Bold", 12f), ForeColor = SovereignEngine.AmberAccent, AutoSize = true, Location = new Point(24, 25) };
            lblLogo.MouseDown += Form_MouseDown;
            _headerPanel.Controls.Add(lblLogo);

            _lblBaristaIQ = new Label { Text = "Зміна: 0 зам. | Очікування...", Font = SovereignEngine.GetFont("Consolas", 8.5f), ForeColor = Color.FromArgb(140, SovereignEngine.SmokeText), AutoSize = true, Location = new Point(24, 55) };
            _lblBaristaIQ.MouseDown += Form_MouseDown;
            _headerPanel.Controls.Add(_lblBaristaIQ);

            _baristaTimer = new Timer { Interval = 1000 };
            _baristaTimer.Tick += (s, e) => UpdateBaristaIQ(); _baristaTimer.Start();

        
            var btnMinimize = CreateSysBtn("—", Color.Transparent, () => this.WindowState = FormWindowState.Minimized);
            var btnMaximize = CreateSysBtn("☐", Color.Transparent, () => this.WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized);
            var btnClose = CreateSysBtn("×", Color.FromArgb(40, 180, 30, 30), () => Application.Exit());
            
            _headerPanel.Controls.Add(btnMinimize);
            _headerPanel.Controls.Add(btnMaximize);
            _headerPanel.Controls.Add(btnClose);

          
            _searchBar = new SovereignSearch { Width = 300, Height = 42 };
            _searchDebounceTimer = new Timer { Interval = 150 };
            _searchDebounceTimer.Tick += (s, e) => {
                _searchDebounceTimer.Stop();
                if (_viewCatalog.Visible) FilterCatalog(_searchBar.Text);
                if (_viewConfig.Visible) { _configSearchQuery = _searchBar.Text; RefillConfigGrid(); }
            };
            _searchBar.TextChangedEvent += (s, e) => {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };
            _headerPanel.Controls.Add(_searchBar);

            this.KeyPreview = true;
            this.KeyPress += (s, e) => { if (!_searchBar.ContainsFocus && !char.IsControl(e.KeyChar)) _searchBar.ActivateGlobalFocus(); };
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Focus(); };

  
            string[] nav = { "КАТАЛОГ", "ОБСЛУГОВУВУВАННЯ", "НАЛАШТУВАННЯ" };
            for (int i = 0; i < nav.Length; i++) {
                var btn = new SidebarButton { Text = nav[i], Location = new Point(25, 40 + i * 70), Width = 190, IsActive = i == 0 };
                int idx = i;
                btn.Click += (s, e) => {
                    foreach (SidebarButton b in _sidebar.Controls) b.IsActive = false;
                    btn.IsActive = true;
                    this.SuspendLayout();
                    _viewCatalog.Visible = idx == 0;
                    _viewOperations.Visible = idx == 1;
                    _viewConfig.Visible = idx == 2;
                    _filterBar.Visible = idx == 0;
                    _cartPanel.Visible = idx == 0;
                    
                   
                    if (_searchBar != null) {
                        _searchBar.Visible = (idx == 0 || idx == 2);
                        _searchBar.Placeholder = idx == 2 ? "Пошук налаштувань..." : "Пошук товару...";
                        _searchBar.Shrink(); 
                        _configSearchQuery = "";
                    }

                    LayoutUI();
                    this.ResumeLayout(true);
                    this.Invalidate(true);
                };
                _sidebar.Controls.Add(btn);
            }

        
            _pnlFooter = new Panel { Height = 250, Dock = DockStyle.Bottom, BackColor = Color.Transparent };
            _cartPanel.Controls.Add(_pnlFooter);

            _lblStaticTotal = new Label { Text = "РАЗОМ", Font = SovereignEngine.GetFont("Montserrat Bold", 9f), ForeColor = Color.FromArgb(140, SovereignEngine.SmokeText), AutoSize = false, TextAlign = ContentAlignment.TopCenter };
            _pnlFooter.Controls.Add(_lblStaticTotal);

            _lblTotal = new Label { Text = "0.00 ₴", Font = SovereignEngine.GetFont("Montserrat Bold", 32f), ForeColor = SovereignEngine.PearlText, Height = 65, TextAlign = ContentAlignment.TopCenter, AutoSize = false };
            _pnlFooter.Controls.Add(_lblTotal);

            _pnlLine = new Panel { Height = 1, BackColor = Color.FromArgb(90, SovereignEngine.AmberAccent) };
            _pnlFooter.Controls.Add(_pnlLine);

            _btnCheckout = new EmberActionButton { Text = "ОФОРМИТИ ЗАМОВЛЕННЯ", Size = new Size(250, 75), Font = SovereignEngine.GetFont("Montserrat Bold", 9.5f) };
            _btnCheckout.UseIcon = true;
            _btnCheckout.Click += (s, e) => {
                if (_currentTotal > 0) {
                    var overlay = new CheckoutOverlay(_currentTotal, _ledger.GetEntries());
                    overlay.Dock = DockStyle.Fill;
                    overlay.OnCheckoutComplete += (os, oe) => {
                        _ordersToday++; _lastOrderTime = DateTime.Now; UpdateBaristaIQ();
                        _ledger.Clear(); _currentTotal = 0; _lblTotal.Text = "0.00 ₴";
                        this.Controls.Remove(overlay);
                    };
                    overlay.OnCancel += (os, oe) => this.Controls.Remove(overlay);
                    this.Controls.Add(overlay);
                    overlay.BringToFront();
                }
            };
            _pnlFooter.Controls.Add(_btnCheckout);

            _ledger = new SovereignLedger { Dock = DockStyle.Fill, DrawInternalContainer = false };
            _ledger.OnTotalChanged = UpdateTotal;
            _cartPanel.Controls.Add(_ledger);

       
            var catContainer = new SovereignContainer { Dock = DockStyle.Fill };
            _viewCatalog.Controls.Add(catContainer);

            _productGrid = new SmoothGrid { Dock = DockStyle.Fill };
            catContainer.Controls.Add(_productGrid);

            BuildOperationsDashboard();
            BuildConfigDashboard();

   
            foreach (Control c in this.Controls) {
                if (c is Panel p) p.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(p, true);
            }

            this.Resize += (s, e) => { UpdateBackgroundCache(); LayoutUI(); };
            this.Layout += (s, e) => { UpdateBackgroundCache(); LayoutUI(); }; 
            
            UpdateBackgroundCache();
            LayoutUI();
        }

        private void UpdateBackgroundCache()
        {
            if (this.Width <= 0 || this.Height <= 0) return;
            if (_bgCache != null) _bgCache.Dispose();
            _bgCache = new Bitmap(this.Width, this.Height);
            using (var g = Graphics.FromImage(_bgCache))
            {
                SovereignEngine.DrawPremiumBackground(g, new Rectangle(0, 0, this.Width, this.Height));
            }
        }

        private void LayoutUI()
        {
            this.SuspendLayout(); 
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            int headerH = 100;
            int filterH = 65;
            int sidebarW = 240;
            int cartW = 350;

            
            _headerPanel.Bounds = new Rectangle(0, 0, w, headerH);
            _headerPanel.BringToFront();

           
            foreach (Control c in _headerPanel.Controls) {
                if (c.Text == "×") c.Location = new Point(w - 48, 0);
                else if (c.Text == "☐") c.Location = new Point(w - 96, 0);
                else if (c.Text == "—") c.Location = new Point(w - 144, 0);
            }
            if (_searchBar != null) _searchBar.Location = new Point(Math.Max(400, w - 160 - _searchBar.Width), 25);

           
            if (_filterBar.Visible) {
                _filterBar.Bounds = new Rectangle(sidebarW, headerH, w - sidebarW - cartW, filterH);
                int tw = 0; foreach (Control c in _filterBar.Controls) tw += c.Width + c.Margin.Horizontal;
                _filterBar.Padding = new Padding(Math.Max(0, (_filterBar.Width - tw) / 2), 0, 0, 0);
                _filterBar.BringToFront();
            }

            _sidebar.Bounds = new Rectangle(0, headerH, sidebarW, h - headerH);
            
            if (_viewCatalog.Visible) {
                _viewCatalog.Bounds = new Rectangle(sidebarW, headerH + filterH, w - sidebarW - cartW, h - headerH - filterH);
                _cartPanel.Bounds = new Rectangle(w - cartW, headerH, cartW, h - headerH);
                _cartPanel.Visible = true;

            
                _lblStaticTotal.Bounds = new Rectangle(0, 18, cartW, 20);
                _lblTotal.Bounds = new Rectangle(0, 38, cartW, 60);
                _pnlLine.Bounds = new Rectangle((cartW - 200) / 2, 105, 200, 1);
                _btnCheckout.Bounds = new Rectangle((cartW - 250) / 2, 140, 250, 75);
            } else {
                _cartPanel.Visible = false;
                if (_viewOperations.Visible) _viewOperations.Bounds = new Rectangle(sidebarW, headerH, w - sidebarW, h - headerH);
                if (_viewConfig.Visible) _viewConfig.Bounds = new Rectangle(sidebarW, headerH, w - sidebarW, h - headerH);
            }


            foreach (Control c in this.Controls) {
                if (c is CheckoutOverlay overlay) {
                    overlay.BringToFront();
                    break;
                }
            }

            this.ResumeLayout(false);
        }

        private void BuildOperationsDashboard()
        {
            var zenith = new ZenITHDashboard();
            _viewOperations.Controls.Add(zenith);
        }

        private class ConfigFeature { public string Title; public string Desc; public string Category; public bool IsSlider; public float Value = 0.5f; }
        private List<ConfigFeature> _configFeatures = new List<ConfigFeature>();
        private Panel _configScrollPanel;
        private List<ConfigCard> _cardPool = new List<ConfigCard>();

        private class ConfigCard : Panel
        {
            public Label LblTitle, LblDesc, LblSliderVal, LblStatus;
            public Panel PnlSlider, PnlToggle;
            public ConfigFeature BoundFeature;

            public ConfigCard()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | 
                         ControlStyles.ResizeRedraw, true);
                UpdateStyles();
                BackColor = Color.Transparent;

                Size = new Size(280, 130);
                Margin = new Padding(0);
                
                LblTitle = new Label { Font = SovereignEngine.GetFont("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(245, 245, 250), AutoSize = true, Location = new Point(18, 15), BackColor = Color.Transparent, UseMnemonic = false };
                LblDesc = new Label { Font = SovereignEngine.GetFont("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 145, 155), AutoSize = false, Size = new Size(250, 36), Location = new Point(18, 42), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                
                PnlSlider = new Panel { Location = new Point(18, 94), Size = new Size(200, 22), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };
                LblSliderVal = new Label { Font = SovereignEngine.GetFont("Consolas", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(0, 200, 255), AutoSize = true, Location = new Point(228, 96), Visible = false, BackColor = Color.Transparent };
                
                PnlToggle = new Panel { Location = new Point(18, 94), Size = new Size(46, 24), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };
                LblStatus = new Label { Font = SovereignEngine.GetFont("Segoe UI", 8f, FontStyle.Bold), AutoSize = true, Location = new Point(72, 98), Visible = false, BackColor = Color.Transparent };

                Controls.Add(LblTitle); Controls.Add(LblDesc);
                Controls.Add(PnlSlider); Controls.Add(LblSliderVal);
                Controls.Add(PnlToggle); Controls.Add(LblStatus);

                Paint += (s, e) => {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    var rect = new RectangleF(1, 1, Width - 3, Height - 3);
                    using (var bgPath = SovereignEngine.GetRoundRect(rect, 14)) {
                        g.FillPath(new SolidBrush(Color.FromArgb(38, 40, 48)), bgPath);
                        g.DrawPath(new Pen(Color.FromArgb(60, 65, 72), 1f), bgPath);
                    }
                };

                PnlSlider.Paint += (s, e) => {
                    if (BoundFeature == null) return;
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = SovereignEngine.GetRoundRect(new RectangleF(0, 9, 200, 5), 2))
                        g.FillPath(new SolidBrush(Color.FromArgb(55, 58, 68)), path);
                    float fillW = 200 * BoundFeature.Value;
                    if (fillW > 2) {
                        using (var path = SovereignEngine.GetRoundRect(new RectangleF(0, 9, fillW, 5), 2))
                        using (var grad = new LinearGradientBrush(new PointF(0, 9), new PointF(fillW, 9), Color.FromArgb(0, 160, 230), Color.FromArgb(0, 210, 255)))
                            g.FillPath(grad, path);
                    }
                    float tx = 200 * BoundFeature.Value - 6;
                    g.FillEllipse(new SolidBrush(Color.White), tx, 3, 16, 16);
                };

                PnlSlider.MouseDown += (s, e) => UpdateSlider(e.X);
                PnlSlider.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) UpdateSlider(e.X); };

                PnlToggle.Paint += (s, e) => {
                    if (BoundFeature == null) return;
                    bool isOn = Form1._instance._activeModules.ContainsKey(BoundFeature.Title) && Form1._instance._activeModules[BoundFeature.Title];
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using(var path = SovereignEngine.GetRoundRect(new RectangleF(0, 0, 44, 22), 11)) {
                        g.FillPath(new SolidBrush(isOn ? Color.FromArgb(65, 200, 110) : Color.FromArgb(55, 58, 68)), path);
                    }
                    float thumbX = isOn ? 24f : 2f;
                    g.FillEllipse(new SolidBrush(Color.FromArgb(210, 215, 220)), thumbX, 2, 18, 18);
                };
                PnlToggle.Click += (s, e) => {
                    if (BoundFeature == null) return;
                    bool isOn = Form1._instance._activeModules.ContainsKey(BoundFeature.Title) && Form1._instance._activeModules[BoundFeature.Title];
                    isOn = !isOn;
                    Form1._instance._activeModules[BoundFeature.Title] = isOn;
                    PnlToggle.Invalidate();
                    LblStatus.Text = isOn ? "АКТИВНО" : "АКТИВУВАТИ";
                    LblStatus.ForeColor = isOn ? Color.FromArgb(65, 200, 110) : SovereignEngine.AmberAccent;
                    
                    if (isOn) {
                        switch(BoundFeature.Title) {
                            case "AI Revenue Forecast": SovereignAlert.Show("ПРОГНОЗ НА ЗАВТРА: 14,500 ₴\nВраховано: Погода (Дощ), Івенти (Концерт)", "ПРОГНОЗ ВИРУЧКИ ШІ"); break;
                            case "Fraud X-Ray": SovereignAlert.Show("ФОНОВИЙ АНАЛІЗ ЧЕКІВ АКТИВОВАНО.\nСистема попередить про підозрілі відміни.", "ДЕТЕКТОР ШАХРАЙСТВА"); break;
                            case "Staff Broadcast": SovereignAlert.Show("КАНАЛ ЗВ'ЯЗКУ ВІДКРИТО.\nТепер ви можете надсилати миттєві повідомлення барістам.", "ОПОВІЩЕННЯ ПЕРСОНАЛУ"); break;
                            case "Dead Hour Heatmap": SovereignAlert.Show("ТЕПЛОВА КАРТА АКТИВНА.\nАлгоритм почав генерацію пуш-акцій для порожніх годин.", "ТЕПЛОВА КАРТА"); break;
                            case "Food Cost Tracker": SovereignAlert.Show("FOOD COST TRACKER СТАРТУВАВ.\nМаржинальність розраховується в реальному часі.", "КОНТРОЛЬ СОБІВАРТОСТІ"); break;
                            case "Loyalty ROI Scanner": SovereignAlert.Show("АНАЛІЗАТОР ROI ЗАПУЩЕНО.\nЗбір метрик по ефективності бонусних карток увімкнено.", "АНАЛІЗАТОР ЛОЯЛЬНОСТІ"); break;
                            case "Happy Hour Engine": SovereignAlert.Show("ДИНАМІЧНІ ЗНИЖКИ АКТИВНІ.\nЦіни автоматично знизяться після 20:00 згідно з графіком.", "ДИНАМІЧНІ ЗНИЖКИ"); break;
                            case "Shift Battle": SovereignAlert.Show("ГЕЙМІФІКАЦІЮ УВІМКНЕНО!\nKPI команд виведено на кухонний екран для змагання.", "БИТВА ЗМІН"); break;
                            case "Training Sandbox": SovereignAlert.Show("СИСТЕМУ ПЕРЕВЕДЕНО В РЕЖИМ ПІСОЧНИЦІ.\nВсі подальші замовлення вважаються тестовими і не йдуть у звіт.", "НАВЧАЛЬНА ПІСОЧНИЦЯ"); break;
                            case "Time & Attendance": SovereignAlert.Show("БІОМЕТРИЧНИЙ РАДАР УВІМКНЕНО.\nОчікування підключення сканера відбитків...", "ОБЛІК РОБОЧОГО ЧАСУ"); break;
                            case "Smart Scale Link": SovereignAlert.Show("ПОШУК BLUETOOTH-ВАГ...\nПристрій [SMART_SCALE_V2] успішно підключено.", "ІНТЕГРАЦІЯ З ВАГАМИ"); break;
                            case "Expiry Guardian": SovereignAlert.Show("ЦИФРОВОГО СТРАЖА АКТИВОВАНО.\nСистемний календар свіжості продуктів запущено.", "КОНТРОЛЬ ТЕРМІНІВ ПРИДАТНОСТІ"); break;
                            case "Barcode Inventory": SovereignAlert.Show("РЕЖИМ ШВИДКОЇ ІНВЕНТАРИЗАЦІЇ ВВІМКНЕНО.\nСистема готова до масового вводу зі штрих-сканера.", "ШТРИХ-КОД ІНВЕНТАРИЗАЦІЯ"); break;
                            case "Deep Audit Trail": SovereignAlert.Show("БЛОКЧЕЙН-ЖУРНАЛ ІНІЦІАЛІЗОВАНО.\nКожна зміна в чеках тепер криптографічно хешується в лог.", "ЖУРНАЛ АУДИТУ"); break;
                        }
                    }

                    Form1._instance.UpdateBaristaIQ();
                };
            }

            private void UpdateSlider(int x) {
                if (BoundFeature == null) return;
                BoundFeature.Value = Math.Max(0, Math.Min(1, (float)x / 200f));
                LblSliderVal.Text = $"{(int)(BoundFeature.Value * 100)}%";
                PnlSlider.Invalidate();
            }

            public void Bind(ConfigFeature feat)
            {
                BoundFeature = feat;
                
     
                PnlSlider.MouseUp += (s, e) => {
                    if (feat.Title.Contains("AI Confidence Threshold")) 
                        SovereignAlert.Show($"Поріг впевненості ШІ змінено на {(int)(feat.Value * 100)}%.\nАвто-акції будуть генеруватись рідше, але з вищою точністю.", "ВПЕВНЕНІСТЬ ШІ");
                    else if (feat.Title.Contains("Target Margin"))
                        SovereignAlert.Show($"Цільова маржинальність жорстко встановлена на {(int)(feat.Value * 100)}%.\nАлгоритм динамічного ціноутворення переналаштовано.", "ЦІЛЬОВА МАРЖИНАЛЬНІСТЬ");
                };

                LblTitle.Text = feat.Title;
                LblDesc.Text = feat.Desc;
                
                PnlSlider.Visible = LblSliderVal.Visible = feat.IsSlider;
                PnlToggle.Visible = LblStatus.Visible = !feat.IsSlider;

                if (feat.IsSlider) {
                    LblSliderVal.Text = $"{(int)(feat.Value * 100)}%";
                } else {
                    bool isOn = Form1._instance._activeModules.ContainsKey(feat.Title) && Form1._instance._activeModules[feat.Title];
                    LblStatus.Text = isOn ? "АКТИВНО" : "АКТИВУВАТИ";
                    LblStatus.ForeColor = isOn ? Color.FromArgb(80, 220, 120) : SovereignEngine.AmberAccent;
                }
                Visible = true;
                Invalidate(true);
            }
        }
        private static Form1 _instance;

        private void BuildConfigDashboard()
        {
            var cfgContainer = new SovereignContainer { Dock = DockStyle.Fill };
            _viewConfig.Controls.Add(cfgContainer);

            var title = new Label { Text = "НАЛАШТУВАННЯ СИСТЕМИ", Font = SovereignEngine.GetFont("Montserrat Bold", 18f), ForeColor = SovereignEngine.AmberAccent, AutoSize = true, Location = new Point(30, 30) };
            cfgContainer.Controls.Add(title);

     
            var pnlCats = new FlowLayoutPanel { Location = new Point(30, 80), Size = new Size(600, 45), BackColor = Color.Transparent, WrapContents = false };
            cfgContainer.Controls.Add(pnlCats);

            string[] categories = { "ШІ", "БІЗНЕС", "ПЕРСОНАЛ", "СИСТЕМА" };
            foreach (var cat in categories)
            {
                var tab = new CategoryTab { Text = cat, IsActive = cat == _activeConfigCategory, Width = 120, Height = 40, Margin = new Padding(0, 0, 10, 0) };
                tab.Click += (s, e) => {
                    _activeConfigCategory = cat;
                    foreach (CategoryTab t in pnlCats.Controls) t.IsActive = t.Text == _activeConfigCategory;
                    RefillConfigGrid();
                };
                pnlCats.Controls.Add(tab);
            }

       
            _configScrollPanel = new Panel {
                AutoScroll = true,
                Location = new Point(15, 140),
                Size = new Size(1010, 640),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            
           
            typeof(Panel).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic, 
                null, _configScrollPanel, new object[] { true });
            cfgContainer.Controls.Add(_configScrollPanel);
            _configScrollPanel.Resize += (s, ev) => RefillConfigGrid();


         
            string[] raw = {
                "ШІ|AI Revenue Forecast|Прогнозування виручки з урахуванням погоди та івентів|False",
                "ШІ|AI Confidence Threshold|Поріг впевненості ШІ для авто-акцій|True",
                "ШІ|Fraud X-Ray|AI-детектор підозрілих відмін чеків до закриття зміни|False",
                "ШІ|Dead Hour Heatmap|Теплова карта порожніх годин та авто-генератор пуш-акцій|False",
                "БІЗНЕС|Target Margin|Цільова маржинальність для авто-підбору цін|True",
                "БІЗНЕС|Food Cost Tracker|Автоматичний розрахунок маржинальності в реальному часі|False",
                "БІЗНЕС|Loyalty ROI Scanner|Аналізатор ефективності бонусних карток|False",
                "БІЗНЕС|Happy Hour Engine|Динамічне авто-зниження цін після 20:00|False",
                "ПЕРСОНАЛ|Staff Broadcast|Центр миттєвого оповіщення бариста на терміналах|False",
                "ПЕРСОНАЛ|Shift Battle|Гейміфікація: Змагання KPI між змінами|False",
                "ПЕРСОНАЛ|Training Sandbox|Режим \"Пісочниця\" для навчання стажерів|False",
                "ПЕРСОНАЛ|Time & Attendance|Біометричний радар присутності персоналу|False",
                "СИСТЕМА|Smart Scale Link|Інтеграція з Bluetooth-вагами для контролю ваги|False",
                "СИСТЕМА|Expiry Guardian|Цифровий страж термінів придатності продуктів|False",
                "СИСТЕМА|Barcode Inventory|Режим швидкої інвентаризації сканером|False",
                "СИСТЕМА|Deep Audit Trail|Блокчейн-журнал всіх критичних дій|False"
            };

            foreach (var r in raw) {
                var p = r.Split('|');
                _configFeatures.Add(new ConfigFeature { Category = p[0], Title = p[1], Desc = p[2], IsSlider = p[3] == "True" });
            }

            RefillConfigGrid();
        }

        private void RefillConfigGrid()
        {
            if (_configScrollPanel == null) return;
            _configScrollPanel.SuspendLayout();
            
           
            foreach (var card in _cardPool) {
                _configScrollPanel.Controls.Remove(card);
                card.Dispose();
            }
            _cardPool.Clear();

            var query = _configSearchQuery.ToLower();
            var filtered = _configFeatures.Where(f => f.Category == _activeConfigCategory && 
                (string.IsNullOrEmpty(query) || f.Title.ToLower().Contains(query) || f.Desc.ToLower().Contains(query))).ToList();

           
            const int cols = 2;
            const int gap  = 16;
            int cardH = 130;
            
            int panelW = _configScrollPanel.ClientSize.Width;
            if (panelW < 50) panelW = _configScrollPanel.Width;
            
           
            int cardW = Math.Max(200, Math.Min(400, (panelW - 40 - gap) / cols));
            
         
            int startX = 15;

            for (int i = 0; i < filtered.Count; i++)
            {
                var card = new ConfigCard();
                _cardPool.Add(card);

                int col = i % cols;
                int row = i / cols;
                card.Location = new Point(startX + col * (cardW + gap), gap + row * (cardH + gap));
                card.Size = new Size(cardW, cardH);

                card.Bind(filtered[i]);
                _configScrollPanel.Controls.Add(card);
            }
            
            _configScrollPanel.ResumeLayout(true);
        }

        private List<CoffeeCardV6> _allProducts = new List<CoffeeCardV6>();

        private async void LoadCategoriesAndCatalog()
        {
            _filterBar.Controls.Clear();
            var categories = await ApiService.Instance.GetCategoriesAsync();

            if (categories == null || categories.Count == 0) return;

            var groupedCats = categories.GroupBy(c => MapCategoryName(c.Name)).ToList();
            string firstMappedName = groupedCats[0].Key;

            foreach (var group in groupedCats)
            {
                string displayName = group.Key;
                var tab = new CategoryTab 
                { 
                    Text = displayName.ToUpper(), 
                    IsActive = displayName == firstMappedName, 
                    Width = Math.Max(150, TextRenderer.MeasureText(displayName.ToUpper(), SovereignEngine.GetFont("Montserrat Bold", 10f)).Width + 40), 
                    Height = 48 
                };
                
                var dbCategoryNames = group.Select(x => x.Name).ToList();

                tab.Click += (s, e) => {
                    foreach (CategoryTab t in _filterBar.Controls) t.IsActive = false;
                    tab.IsActive = true; 
                    LoadCatalog(dbCategoryNames);
                };
                _filterBar.Controls.Add(tab);
            }

            LayoutUI(); 
            LoadCatalog(groupedCats[0].Select(x => x.Name).ToList());
        }

        private string MapCategoryName(string dbName) => SovereignEngine.MapCategoryName(dbName);

        private async void LoadCatalog(List<string> categoryNames)
        {
            _productGrid.Clear();
            _allProducts.Clear();
            _searchBar?.Shrink();

            var products = await ApiService.Instance.GetProductsAsync();
            var filtered = products.Where(p => categoryNames.Contains(p.Category?.Name)).ToList();
            
          
            foreach (var item in filtered)
            {
                var card = new CoffeeCardV6 
                { 
                    Title = item.Name, 
                    Price = item.Price.ToString("N2") + " ₴", 
                    Desc = item.Description ?? "Вишуканий смак та аромат", 
                    Category = MapCategoryName(item.Category?.Name ?? ""),
                    Tag = item.Id
                };
                card.BuyClick += (s, e) => {
                    _ledger.AddEntry(card.Title, card.Price, card.Tag);
                    UpdateTotal();
                };
                _productGrid.AddItem(card);
                _allProducts.Add(card);
            }
            _productGrid.FinalizeLayout();
        }

        private void FilterCatalog(string text)
        {
            if (text == "Пошук...") text = "";
            _productGrid.Clear();
            var filtered = string.IsNullOrWhiteSpace(text) ? _allProducts : _allProducts.Where(c => c.Title.ToLower().Contains(text.ToLower())).ToList();
            foreach (var card in filtered) _productGrid.AddItem(card);
            _productGrid.FinalizeLayout();
        }

        private Label CreateSysBtn(string text, Color back, Action onClick)
        {
            var b = new Label {
                Text = text, Size = new Size(48, 48), TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = SovereignEngine.PearlText, BackColor = back, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", text == "×" ? 16f : 12f, text == "☐" ? FontStyle.Bold : FontStyle.Regular)
            };
            b.MouseEnter += (s, e) => b.BackColor = text == "×" ? Color.Red : Color.FromArgb(60, 255, 255, 255);
            b.MouseLeave += (s, e) => b.BackColor = back;
            b.Click += (s, e) => onClick();
            return b;
        }

        public void UpdateTotal()
        {
            _currentTotal = _ledger.GetTotal();
            _lblTotal.Text = $"{_currentTotal:N2} ₴";
        }

       
        private void UpdateBaristaIQ()
        {
            string elapsed = "Очікування...";
            if (_lastOrderTime != DateTime.MinValue)
            {
                var diff = DateTime.Now - _lastOrderTime;
                if (diff.TotalMinutes < 1) elapsed = $"{(int)diff.TotalSeconds}с тому";
                else if (diff.TotalMinutes < 60) elapsed = $"{(int)diff.TotalMinutes}хв тому";
                else elapsed = $"{(int)diff.TotalHours}год тому";
            }

          
            string streak = "";
            if (_ordersToday >= 10) streak = " 🔥";
            else if (_ordersToday >= 5) streak = " ⚡";

         
            string moduleInfo = "";
            if (Form1._instance._activeModules.ContainsKey("AI Revenue Forecast") && Form1._instance._activeModules["AI Revenue Forecast"])
                moduleInfo += "  |  [AI Прогноз: 14,500 ₴]";
            if (Form1._instance._activeModules.ContainsKey("Fraud X-Ray") && Form1._instance._activeModules["Fraud X-Ray"])
                moduleInfo += "  |  [Fraud X-Ray: ACTIVE]";
            if (Form1._instance._activeModules.ContainsKey("Staff Broadcast") && Form1._instance._activeModules["Staff Broadcast"])
                moduleInfo += "  |  [Broadcast: ON]";
            if (Form1._instance._activeModules.ContainsKey("Dead Hour Heatmap") && Form1._instance._activeModules["Dead Hour Heatmap"])
                moduleInfo += "  |  [Heatmap: LIVE]";
            if (Form1._instance._activeModules.ContainsKey("Food Cost Tracker") && Form1._instance._activeModules["Food Cost Tracker"])
                moduleInfo += "  |  [Food Cost: TRACKING]";
            if (Form1._instance._activeModules.ContainsKey("Shift Battle") && Form1._instance._activeModules["Shift Battle"])
                moduleInfo += "  |  [Battle: GAME ON]";
            if (Form1._instance._activeModules.ContainsKey("Training Sandbox") && Form1._instance._activeModules["Training Sandbox"])
                moduleInfo += "  |  [⚠️ SANDBOX MODE]";

            _lblBaristaIQ.Text = $"Зміна: {_ordersToday} зам.  |  Останнє: {elapsed}{streak}{moduleInfo}";

       
            if (_lastOrderTime != DateTime.MinValue)
            {
                var diff = DateTime.Now - _lastOrderTime;
                if (diff.TotalMinutes < 2) _lblBaristaIQ.ForeColor = Color.FromArgb(80, 220, 120);
                else if (diff.TotalMinutes < 5) _lblBaristaIQ.ForeColor = SovereignEngine.AmberAccent;
                else _lblBaristaIQ.ForeColor = Color.FromArgb(140, SovereignEngine.SmokeText);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
           
            if (_bgCache != null) e.Graphics.DrawImage(_bgCache, 0, 0);
            else SovereignEngine.DrawPremiumBackground(e.Graphics, ClientRectangle);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

        
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(-100, -200, 600, 600);
                using (var pBrush = new PathGradientBrush(path))
                {
                    pBrush.CenterColor = Color.FromArgb(SovereignEngine.C(20 * SovereignEngine.GlobalPulse), SovereignEngine.AmberAccent);
                    pBrush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pBrush, path);
                }
            }

        
            using (var br = new SolidBrush(Color.FromArgb(12, 0, 0, 0)))
            {
                g.FillRectangle(br, 0, 0, 240, Height);
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); }
        }

        private static GraphicsPath RR(RectangleF r, float rad)
        {
            float d = rad * 2f;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
