using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Cafe.Launcher
{
    public partial class LauncherForm : Form
    {
        [DllImport("dwmapi.dll")] static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins m);
        [DllImport("dwmapi.dll")] static extern int DwmIsCompositionEnabled(out bool e);
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr h, int msg, int w, int l);
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        private struct Margins { public int Left, Right, Top, Bottom; }
        const int WM_NCLBUTTONDOWN = 0xA1, HT_CAPTION = 0x2;

        private System.Windows.Forms.Timer _globalTimer = null!;
        private float _shimmerPos = -1f;
        private float _aromaStep = 0f;

        private List<SteamParticle> _particles = new List<SteamParticle>();
        private Random _rnd = new Random();

        private class SteamParticle
        {
            public float X, Y, Size, Opacity, Speed;
            public float Drifts;
        }

        public LauncherForm() { InitializeComponent(); Init(); }
        public LauncherForm(Form _) { InitializeComponent(); Init(); }

        private void Init()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(800, 520);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            var uiFont = new Font("Segoe UI Semibold", 12f);
            var darkBg = Color.FromArgb(13, 13, 13);
            
            txtUsername.PlaceholderText = "Введіть логін...";
            txtUsername.Font = uiFont;
            txtUsername.BackColor = darkBg;
            txtUsername.ForeColor = Color.White;
            txtUsername.Height = 60;

            txtPassword.PlaceholderText = "Введіть пароль...";
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = uiFont;
            txtPassword.BackColor = darkBg;
            txtPassword.ForeColor = Color.White;
            txtPassword.Height = 60;

            btnLogin.Text = "УВІЙТИ";
            btnLogin.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Height = 60;
            btnLogin.Region = new Region(RR(new RectangleF(0, 0, btnLogin.Width, btnLogin.Height), 14));

            for (int i = 0; i < 12; i++) SpawnParticle();

            MouseMove += (s, e) =>
            {
                foreach (var p in _particles)
                {
                    float dx = p.X - e.X;
                    float dy = p.Y - e.Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 150)
                    {
                        float force = (150 - dist) / 150f;
                        p.X += dx * force * 0.2f;
                        p.Y += dy * force * 0.2f;
                    }
                }
            };

            _globalTimer = new System.Windows.Forms.Timer { Interval = 25 };
            _globalTimer.Tick += (s, e) =>
            {
                _shimmerPos += 0.015f;
                if (_shimmerPos > 2f) _shimmerPos = -1.5f;
                _aromaStep += 0.06f;
                UpdateParticles();
                Invalidate();
            };
            _globalTimer.Start();

            this.FormClosing += (s, e) =>
            {
                if (_globalTimer != null)
                {
                    _globalTimer.Stop();
                    _globalTimer.Dispose();
                }
            };
        }

        private void SpawnParticle()
        {
            _particles.Add(new SteamParticle
            {
                X = _rnd.Next(20, 780),
                Y = _rnd.Next(100, 550),
                Size = _rnd.Next(200, 450),
                Opacity = 0,
                Speed = _rnd.Next(1, 4) / 10f,
                Drifts = (float)(_rnd.NextDouble() * Math.PI * 2)
            });
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Y -= p.Speed;
                p.X += (float)Math.Sin(p.Drifts + p.Y / 35f) * 0.45f;
                if (p.Y < 40) p.Opacity -= 0.012f; else if (p.Opacity < 0.12f) p.Opacity += 0.006f;
                if (p.Opacity <= 0 && p.Y < 40) { _particles.RemoveAt(i); SpawnParticle(); }
            }
        }

        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ClassStyle |= 0x20000; return cp; }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            bool comp;
            if (DwmIsCompositionEnabled(out comp) == 0 && comp)
            {
                var m = new Margins { Left = 1, Right = 1, Top = 1, Bottom = 1 };
                DwmExtendFrameIntoClientArea(Handle, ref m);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < 45)
            { ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); }
        }

        private static GraphicsPath RR(RectangleF r, float radius)
        {
            float d = radius * 2f;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float W = (float)Width, H = (float)Height;

            using (var path = RR(new RectangleF(0, 0, W - 1, H - 1), 16))
            {
                this.Region = new Region(path);

                using (var lgb = new LinearGradientBrush(new RectangleF(0, 0, W, H),
                    Color.FromArgb(255, 10, 6, 5), Color.FromArgb(255, 24, 14, 12), 45f))
                    g.FillPath(lgb, path);

                using (var p = new Pen(Color.FromArgb(40, 255, 255, 255), 1f))
                    g.DrawPath(p, path);

                foreach (var p in _particles)
                {
                    using (var br = new SolidBrush(Color.FromArgb((int)(p.Opacity * 255), 255, 255, 255)))
                        g.FillEllipse(br, p.X - p.Size / 2, p.Y - p.Size / 2, p.Size, p.Size);
                }

                var dashBox = new RectangleF(120, 55, W - 240, H - 140);
                using (var br = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                    g.FillRectangle(br, dashBox);
                using (var p = new Pen(Color.FromArgb(25, 255, 211, 155), 1.2f))
                    g.DrawRectangle(p, dashBox.X, dashBox.Y, dashBox.Width, dashBox.Height);
            }

            DrawGildedLogo(g, W / 2f, 115f);

            using (var hbr = new LinearGradientBrush(new RectangleF(0, 0, W, 42),
                Color.FromArgb(190, 4, 2, 1), Color.Transparent, 90f))
                g.FillRectangle(hbr, 0, 0, W, 42);

            using (var font = new Font("Montserrat Bold", 9f))
            using (var br = new SolidBrush(Color.FromArgb(210, 230, 210, 180)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("CAFE SYSTEM | ВХІД В СИСТЕМУ", font, br, new RectangleF(0, 0, W, 42), sf);

                using (var p = new Pen(Color.FromArgb(35, 255, 255, 255), 1f))
                    g.DrawLine(p, 0, 42, W, 42);
            }

            using (var font = new Font("Montserrat", 17f, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(255, 230, 190, 110)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("CAFE SYSTEM", font, br, new RectangleF(0, 168, W, 35), sf);
            }

            DrawFooter(g, W, H);

            if (!string.IsNullOrEmpty(_currentError))
            {
                var errRect = new RectangleF(50, 65, W - 100, 60);
                using (var path = RR(errRect, 8))

                {
                    using (var br = new SolidBrush(Color.FromArgb(220, 200, 40, 40)))
                        g.FillPath(br, path);
                    using (var font = new Font("Segoe UI Semibold", 9.5f))
                    using (var br = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(_currentError, font, br, errRect, sf);
                    }
                }
            }
        }

        private void DrawGildedLogo(Graphics g, float cx, float cy)
        {
            for (int i = 0; i < 3; i++)
            {
                float t = (_aromaStep + i * 1.5f) % 4.5f;
                int a = (int)((1f - t / 4.5f) * 55);
                if (a < 1) continue;
                using (var p = new Pen(Color.FromArgb(a, 212, 175, 55), 1.5f))
                {
                    float y = cy - 50 - t * 12;
                    float w = 24 + t * 18;
                    g.DrawArc(p, cx - w / 2, y, w, 12, 0, 180);
                }
            }

            var rect = new RectangleF(cx - 32, cy - 38, 64, 76);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(255, 255, 225, 130);
                    pgb.SurroundColors = new Color[] { Color.FromArgb(255, 110, 60, 20) };
                    g.FillPath(pgb, path);
                }

                g.SetClip(path);
                float shimX = rect.Left - 60 + (rect.Width + 120) * _shimmerPos;
                using (var shimBr = new LinearGradientBrush(new RectangleF(shimX, rect.Top, 45, rect.Height),
                    Color.Transparent, Color.FromArgb(170, Color.White), 70f))
                {
                    shimBr.SetSigmaBellShape(0.5f);
                    g.FillRectangle(shimBr, shimX, rect.Top, 45, rect.Height);
                }
                g.ResetClip();

                using (var p = new Pen(Color.FromArgb(130, 255, 220, 160), 2f))
                    g.DrawPath(p, path);
            }

            using (var p = new Pen(Color.FromArgb(160, 255, 255, 255), 1.6f))
            {
                g.DrawArc(p, cx - 32, cy - 38, 64, 45, 42, 96);
                g.DrawArc(p, cx - 32, cy + 0, 64, 45, 222, 96);
            }
        }

        private void DrawFooter(Graphics g, float W, float H)
        {
            float barY = H - 38;
            using (var sep = new Pen(Color.FromArgb(30, 255, 211, 155), 1f))
                g.DrawLine(sep, 130f, barY, W - 130f, barY);

            using (var br = new SolidBrush(Color.FromArgb(0, 201, 167)))
                g.FillEllipse(br, 140f, barY + 14f, 8f, 8f);

            using (var f = new Font("Segoe UI Semibold", 8.5f))
            using (var br = new SolidBrush(Color.FromArgb(180, 240, 230, 220)))
            {
                g.DrawString("СЕРВЕР: ГОТОВИЙ", f, br, new PointF(155f, barY + 10f));
                var sf = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString("СИСТЕМА КАФЕ", f, br, new PointF(W - 140f, barY + 10f), sf);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Text = "ЗАВАНТАЖЕННЯ...";
            btnLogin.Enabled = false;

            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowCustomError("Введіть логін та пароль");
                ResetLoginState();
                return;
            }

            try
            {
                var authUser = await Cafe.Shared.Services.ApiService.Instance.LoginAsync(user, pass);
                if (authUser != null)
                {
                    string targetProject = authUser.Role == "Admin" ? "Cafe.Client.Admin" : "Cafe.Client.Manager";
                    string exeName = targetProject + ".exe";
                    
                    // Поиск .exe файла 
                    string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    string targetPath = "";
                    
                    // Рекурсивный поиск вверх до корня решения
                    DirectoryInfo dir = new DirectoryInfo(currentDir);
                    while (dir != null && targetPath == "")
                    {
                        string possibleSlnFolder = Path.Combine(dir.FullName, targetProject);
                        if (Directory.Exists(possibleSlnFolder))
                        {
                            string pathNet48 = Path.Combine(possibleSlnFolder, "bin", "Debug", "net48", exeName);
                            string pathPlain = Path.Combine(possibleSlnFolder, "bin", "Debug", exeName);

                            if (File.Exists(pathNet48)) targetPath = pathNet48;
                            else if (File.Exists(pathPlain)) targetPath = pathPlain;
                        }
                        dir = dir.Parent;
                    }

                    if (File.Exists(targetPath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = targetPath,
                            WorkingDirectory = Path.GetDirectoryName(targetPath)
                        });
                        Application.Exit();
                    }
                    else
                    {
                        ShowCustomError($"Не знайдено файл клієнта:\nШукали: {exeName}");
                        ResetLoginState();
                    }
                }
                else
                {
                    ShowCustomError("Невірний логін або пароль");
                    ResetLoginState();
                }
            }
            catch (Exception)
            {
                ShowCustomError($"Помилка з'єднання з сервером.");
                ResetLoginState();
            }
        }

        private string _currentError = "";
        private void ShowCustomError(string msg)
        {
            _currentError = msg;
            Invalidate(); // Запустить OnPaint для перерисовки
        }

        private void ResetLoginState()
        {
            btnLogin.Text = "УВІЙТИ";
            btnLogin.Enabled = true;
        }

        private void btnSettings_Click(object sender, EventArgs e) { }
        private void txtUsername_Click(object sender, EventArgs e) 
        { 
            _currentError = ""; 
            Invalidate(); 
        }
    }
}