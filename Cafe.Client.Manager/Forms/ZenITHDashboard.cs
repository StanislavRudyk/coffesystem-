using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{

    public class ZenITHDashboard : UserControl
    {
        private Timer _engine;
        private float _time = 0f;
        private Random _rng = new Random();
        
 
        private float[] _radarData = new float[5];
        private float[] _radarTarget = new float[5];
        private float[] _radarVel = new float[5];
        private float[] _goldenProfile = { 0.55f, 0.75f, 0.7f, 0.2f, 0.8f };
        private string[] _axisLabels = { "Кислотність", "Щільність", "Солодкість", "Гіркота", "Арома" };

    
        private float _boilerTemp = 93.5f;
        private float _pumpPressure = 9.1f;
        private float _groupTemp = 91.2f;
        private float _flowRate = 8.2f;
        private float _waterLevel = 78f;
        private float _hopperA = 45f;
        private float _hopperB = 12f;
        private float _mainsVoltage = 228f;
        private int _pumpCycles = 124500;
        private int _filterDays = 45;

    
        private float _tempPulse = 0f;
        private float _tempPulseVel = 0f;

     
        private bool _abortPressed = false;
        private float _abortProgress = 0f;
        private DateTime _abortStartTime;

        
        private bool _flushing = false;
        private float _flushAnim = 0f, _flushVel = 0f;

    
        private float _diagAnim = 0f, _diagVel = 0f;
        private float _calibAnim = 0f, _calibVel = 0f;
        private float _reportAnim = 0f, _reportVel = 0f;
        private bool _calibrating = false;
        private bool _showingReport = false;
        private bool _isScanning = false;
        private float _scanProgress = 0f;
        private List<string> _diagTerminal = new List<string>();
        private Dictionary<string, float> _diagHealth = new Dictionary<string, float>();
        private bool _showDiagReport = false;
        private Random _rand = new Random();
        private float _tempOffset = 0f;
        private float _pressOffset = 0f;
        private float _flowOffset = 0f;
        private float[] _radarPulse = new float[5];
     
        private int _flushPhase = 0;
        private float _flushTimer = 0f;
        private float _cleaningEff = 0f;

        public ZenITHDashboard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            DoubleBuffered = true;

            _radarTarget = new float[] { 0.5f, 0.7f, 0.65f, 0.25f, 0.75f };
            for (int i = 0; i < 5; i++) _radarData[i] = 0.1f;

            _engine = new Timer { Interval = 16 };
            _engine.Tick += (s, e) => {
               
                if (_abortProgress >= 1f) { Invalidate(); return; }

                _time += 0.04f;

                for (int i = 0; i < 5; i++)
                {
                    
                    float baseT = i == 0 ? 0.5f : (i == 1 ? 0.7f : (i == 2 ? 0.65f : (i == 3 ? 0.25f : 0.75f)));
                    
                    if (i == 0) baseT += _tempOffset * 0.15f; 
                    if (i == 1) baseT += _pressOffset * 0.25f; 
                    if (i == 2) baseT += _flowOffset * 0.35f;

                    _radarTarget[i] = Math.Max(0.05f, Math.Min(0.95f, baseT));
                    
                    _radarData[i] = SovereignEngine.Spring(_radarData[i], _radarTarget[i], ref _radarVel[i], 0.06f, 0.82f);
                    _radarData[i] = Math.Max(0.05f, Math.Min(0.95f, _radarData[i]));

                   
                    if (!_calibrating && _rng.NextDouble() < 0.005) 
                        _radarTarget[i] = Math.Max(0.05f, Math.Min(0.95f, _radarTarget[i] + (float)(_rng.NextDouble() - 0.5) * 0.05f));
                }

                _tempPulse = SovereignEngine.Spring(_tempPulse, (float)(Math.Sin(_time) * 0.5 + 0.5), ref _tempPulseVel, 0.05f, 0.7f);
                
           
                if (_flushing)
                {
                    _flushTimer += 0.016f; 
                    _cleaningEff = Math.Min(100f, _cleaningEff + 0.2f);
                    
                    if (_flushTimer < 3.0f) { 
                        _flushPhase = 1; 
                        _flowRate = 12f + (float)Math.Sin(_time * 10) * 1.5f;
                    } 
                    else if (_flushTimer < 7.5f) { 
                        _flushPhase = 2; 
                        _flowRate = 18f + (float)Math.Sin(_time * 25) * 3f;
                        _pumpPressure = 10.5f + (float)Math.Cos(_time * 15) * 1.2f;
                        if (_rng.NextDouble() < 0.1) _diagTerminal.Add("> SYNC_SURGE: RELIEF_VALVE_OPEN");
                    }
                    else if (_flushTimer < 10.0f) { 
                        _flushPhase = 3; 
                        _flowRate = 4f + (float)Math.Sin(_time * 40) * 2f;
                        _boilerTemp = Math.Min(105f, _boilerTemp + 1.2f);
                    }
                    else { 
                        _flushing = false; _flushPhase = 0; _flushTimer = 0f; _cleaningEff = 0f;
                        _diagTerminal.Add("[OK] ПРОМИВКА ЗАВЕРШЕНА");
                    }
                    
                    if (_diagTerminal.Count > 10) _diagTerminal.RemoveAt(0);
                }
                else
                {
                    float fTarget = 7.5f;
                    float flowSpring = SovereignEngine.Spring(_flowRate, fTarget + (float)Math.Sin(_time * 4) * 0.5f, ref _tempPulseVel, 0.12f, 0.8f);
                    _flowRate = flowSpring + _flowOffset;
                    
                    float jitterMult = _calibrating ? 0.05f : 1.0f; 
                    _boilerTemp = (93.0f + (_tempPulse * 1.2f * jitterMult)) - (_flushAnim * 4f) + _tempOffset;
                    _pumpPressure = (8.8f + (float)(Math.Cos(_time * 0.7) * 0.3 * jitterMult + 0.3 * jitterMult)) + (_flushAnim * 1.5f) + _pressOffset;
                    _groupTemp = 90.8f + (_tempPulse * 0.5f * jitterMult) + (_tempOffset * 0.6f);
                }

                _flushAnim = SovereignEngine.Spring(_flushAnim, _flushing ? 1f : 0f, ref _flushVel, 0.08f, 0.8f);
                _diagAnim = SovereignEngine.Spring(_diagAnim, 0f, ref _diagVel, 0.12f, 0.8f);
                _calibAnim = SovereignEngine.Spring(_calibAnim, 0f, ref _calibVel, 0.12f, 0.8f);
                _reportAnim = SovereignEngine.Spring(_reportAnim, _showingReport ? 1f : 0f, ref _reportVel, 0.1f, 0.85f);
                
                for(int i = 0; i < 5; i++) _radarPulse[i] = Math.Max(0f, _radarPulse[i] - 0.05f);

                if (_isScanning)
                {
                    _scanProgress += 0.005f; 
                    if (_scanProgress < 1.0f) {
                       
                        if ((int)(_scanProgress * 1000) % 15 == 0) {
                            string[] cmds = { "СКАН", "ЗЧИТ", "КАЛІБ", "ТЕСТ", "АНАЛІЗ", "ПЕРЕВ" };
                            string[] parts = { "ТЕРМОБЛОК_T1", "ПОМПА_P1", "ГІДРО_СИСТ", "МОРНИ_A", "ЖИВЛЕННЯ", "ДАТЧИК_P" };
                            string hex = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                            _diagTerminal.Add($"> [{hex}] {cmds[_rand.Next(cmds.Length)]}::{parts[_rand.Next(parts.Length)]} ... ОК");
                            if (_diagTerminal.Count > 15) _diagTerminal.RemoveAt(0);
                        }
                    }
                    else if (_scanProgress >= 1.0f && _scanProgress < 1.02f) {
                         _showDiagReport = true;
                         _diagHealth["Термічна Стабільність"] = 0.98f;
                         _diagHealth["Ефективність Тиску"] = 0.85f;
                         _diagHealth["Гострота Жорен"] = 0.72f;
                         _diagHealth["Гідравлічний Потік"] = 0.91f;
                    }
                    if (_scanProgress >= 1.2f) { _isScanning = false; _scanProgress = 0f; _diagTerminal.Clear(); }
                }

                if (_abortPressed)
                {
                    _abortProgress = Math.Min(1f, (float)(DateTime.Now - _abortStartTime).TotalSeconds / 1.5f);
                    if (_abortProgress >= 1f) { _abortPressed = false; }
                } else if (_abortProgress < 1f) _abortProgress = Math.Max(0f, _abortProgress - 0.05f);

                Invalidate();
            };
            _engine.Start();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int pad = 24;
            int baseBtnY = CalculateButtonStartY();
            
            if (_showDiagReport) { _showDiagReport = false; return; }

            
            if (GetDiagnosticsRect(baseBtnY + 30).Contains(e.Location)) { _isScanning = true; _scanProgress = 0f; _diagAnim = 1f; _showDiagReport = false; }
            if (GetCalibrationRect(baseBtnY + 30).Contains(e.Location)) { _calibrating = !_calibrating; _calibAnim = 1f; }
            
            if (GetFlushRect(baseBtnY + 85).Contains(e.Location)) { _flushing = true; _flushTimer = 0f; _flushPhase = 1; _cleaningEff = 0f; }
            if (GetAbortRect(baseBtnY + 85).Contains(e.Location)) { _abortPressed = true; _abortStartTime = DateTime.Now; }
            
            if (_showingReport && e.X > Width - 100) _showingReport = false;
            
     
            var reportR = new Rectangle(pad + 240, pad - 2, 110, 36);
            if (reportR.Contains(e.Location)) _showingReport = !_showingReport;

      
            if (_calibrating)
            {
                int leftW = Math.Min((int)(Width * 0.55f), 600);
                int cW = (leftW - pad * 2 - 16) / 2;
                int cY = pad + 40;
                string[] sensorLabels = { 
                    "🔥 Т1 Бойлер", "💧 Т2 Група", 
                    "⚡ Р1 Помпа", "🌊 Потік води",
                    "🚰 Бак води", "🫘 Зерно (A)", "🫘 Зерно (B)", "🛡️ Фільтр",
                    "🔌 Мережа AC", "🔄 Цикли"
                };
                int[] groupSizes = { 2, 2, 4, 2 };

                int sIdx = 0;
                foreach (int size in groupSizes)
                {
                    cY += 20;
                    for (int i = 0; i < size; i++)
                    {
                        int col = i % 2;
                        int row = i / 2;
                        int cx = pad + col * (cW + 16);
                        int cy = cY + row * (70 + 6);
                        
                        var plusR = new RectangleF(cx + cW - 32, cy + 8, 24, 24);
                        var minusR = new RectangleF(cx + cW - 32, cy + 38, 24, 24);
                        
                        if (plusR.Contains(e.Location) || minusR.Contains(e.Location))
                        {
                            string lbl = sensorLabels[sIdx];
                            bool isPlus = plusR.Contains(e.Location);
                            if (lbl.Contains("Бойлер")) { _tempOffset += isPlus ? 0.2f : -0.2f; _radarPulse[0] = 1f; }
                            else if (lbl.Contains("Помпа")) { _pressOffset += isPlus ? 0.1f : -0.1f; _radarPulse[1] = 1f; }
                            else if (lbl.Contains("Потік")) { _flowOffset += isPlus ? 0.05f : -0.05f; _radarPulse[2] = 1f; }
                            
                           
                            _diagTerminal.Add($"> [CALIB] Налаштування зміщено: {lbl}");
                            if (_diagTerminal.Count > 10) _diagTerminal.RemoveAt(0);

                            Invalidate();
                            return; 
                        }
                        sIdx++;
                    }
                    cY += ((size + 1) / 2) * (70 + 6) + 4;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _abortPressed = false; _flushing = false; }

        private Rectangle GetDiagnosticsRect(int y)
        {
            int leftW = Math.Min((int)(Width * 0.55f), 600);
            return new Rectangle(24, y, (leftW / 2) - 36, 45);
        }

        private Rectangle GetCalibrationRect(int y)
        {
            int leftW = Math.Min((int)(Width * 0.55f), 600);
            return new Rectangle(24 + (leftW / 2) - 24, y, (leftW / 2) - 36, 45);
        }

        private Rectangle GetFlushRect(int y)
        {
            int leftW = Math.Min((int)(Width * 0.55f), 600);
            return new Rectangle(24, y, (leftW / 2) - 36, 50);
        }

        private Rectangle GetAbortRect(int y)
        {
            int leftW = Math.Min((int)(Width * 0.55f), 600);
            return new Rectangle(24 + (leftW / 2) - 24, y, (leftW / 2) - 36, 50);
        }

        private int CalculateButtonStartY()
        {
            int pad = 24; 
            int curY = pad + 40; 
            var rows = new[] { 1, 1, 2, 1 }; 
            int cardH = 62; 
            int cardGap = 4; 
            foreach(int r in rows) curY += 18 + (r * (cardH + cardGap)) + 2; 
            return curY + 2; 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int pad = 24; 
            int leftW = Math.Min((int)(Width * 0.55f), 600);
            int rightX = leftW + pad;
            int rightW = Width - rightX - pad;

         
            g.DrawString("ОБСЛУГОВУВАННЯ", SovereignEngine.GetFont("Montserrat Bold", 18f), new SolidBrush(SovereignEngine.PearlText), pad, pad);
            
          
            var reportR = new Rectangle(pad + 240, pad - 2, 110, 36);
            DrawTactileButton(g, reportR, "🗒️ ЗВІТИ", Color.FromArgb(160, SovereignEngine.SmokeText), _reportAnim * 0.2f);

         
            int cardW = (leftW - pad * 2 - 16) / 2;
            int cardH = 62; 
            int cardGap = 4; 
            int curY = pad + 40;

   
            var groups = new[] {
                new { Title = "🌡️ ТЕМПЕРАТУРА СИСТЕМИ", Sensors = new[] {
                    new { Label = "🔥 Т1 Бойлер",  Value = $"{_boilerTemp:F1}",  Unit = "°C",  Color = GetTempColor(_boilerTemp) },
                    new { Label = "💧 Т2 Група",   Value = $"{_groupTemp:F1}",   Unit = "°C",  Color = Color.FromArgb(80, 220, 120) },
                }},
                new { Title = "📉 ТИСК ТА ГІДРАВЛІКА", Sensors = new[] {
                    new { Label = "⚡ Р1 Помпа",   Value = $"{_pumpPressure:F1}", Unit = "Бар",  Color = GetPressureColor(_pumpPressure) },
                    new { Label = "🌊 Потік води", Value = $"{_flowRate:F1}",    Unit = "мл/с", Color = Color.FromArgb(100, 200, 255) },
                }},
                new { Title = "☕ СТАН РОЗХІДНИКІВ", Sensors = new[] {
                    new { Label = "🚰 Бак води",   Value = $"{_waterLevel:F0}",  Unit = "%",    Color = Color.FromArgb(100, 200, 255) },
                    new { Label = "🫘 Зерно (A)",  Value = $"{_hopperA:F0}%",    Unit = $"~{(int)(_hopperA * 0.66f)}",  Color = _hopperA < 15 ? SovereignEngine.AmberAccent : Color.FromArgb(80, 220, 120) },
                    new { Label = "🫘 Зерно (B)",  Value = $"{_hopperB:F0}%",    Unit = $"~{(int)(_hopperB * 0.66f)}",  Color = _hopperB < 15 ? SovereignEngine.AmberAccent : Color.FromArgb(80, 220, 120) },
                    new { Label = "🛡️ Фільтр",     Value = $"{_filterDays}",      Unit = "дн.",  Color = Color.FromArgb(160, 160, 160) },
                }},
                new { Title = "💻 СТАТУС МОДУЛІВ", Sensors = new[] {
                    new { Label = "🔌 Мережа AC",  Value = $"{_mainsVoltage:F0}", Unit = "В",    Color = Color.FromArgb(180, 180, 180) },
                    new { Label = "🔄 Цикли",      Value = $"{_pumpCycles / 1000}K", Unit = "/150K", Color = Color.FromArgb(160, 160, 160) },
                }},
            };

            foreach (var group in groups)
            {
         
                g.DrawString(group.Title, SovereignEngine.GetFont("Segoe UI Emoji", 8f, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(130, SovereignEngine.SmokeText)), pad, curY);
                curY += 18;

                for (int i = 0; i < group.Sensors.Length; i++)
                {
                    int col = i % 2;
                    int row = i / 2;
                    int cx = pad + col * (cardW + 16);
                    int cy = curY + row * (cardH + cardGap);

                    var cardRect = new RectangleF(cx, cy, cardW, cardH);
                    
                 
                    using (var shadowPath = SovereignEngine.GetRoundRect(new RectangleF(cx + 1, cy + 2, cardW, cardH), 10))
                    using (var shadowBr = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                        g.FillPath(shadowBr, shadowPath);

                    SovereignEngine.DrawGlassPanel(g, cardRect, 10f);

            
                    if (_flushing && _flushPhase > 0)
                    {
                        float wavePos = (_flushTimer / 10.0f) * (leftW + 200) - 100;
                        var waveR = new RectangleF(wavePos, cy, 150, cardH);
                        using (var lgb = new LinearGradientBrush(waveR, Color.Transparent, Color.FromArgb(40, 0, 150, 255), 0f))
                        {
                            lgb.SetSigmaBellShape(0.5f);
                            g.FillRectangle(lgb, cardRect);
                        }
                    }

               
                    float innerX = cx + 16;
                    float innerY = cy + 12;
                    using (var bLab = new SolidBrush(Color.FromArgb(180, Color.White)))
                        g.DrawString(group.Sensors[i].Label, SovereignEngine.GetFont("Segoe UI Emoji", 8.5f),
                            bLab, innerX, innerY);
                    
                    string valueStr = group.Sensors[i].Value + (group.Sensors[i].Unit.Length > 0 ? " " + group.Sensors[i].Unit : "");
                    g.DrawString(valueStr, SovereignEngine.GetFont("Consolas", 13f, FontStyle.Bold),
                        new SolidBrush(group.Sensors[i].Color), innerX, innerY + 22);

              
                    if (_calibrating)
                    {
                        string lbl = group.Sensors[i].Label;
                        if (lbl.Contains("Бойлер") || lbl.Contains("Помпа") || lbl.Contains("Потік"))
                        {
                            var bBr = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
                            var plusR = new RectangleF(cardRect.Right - 32, cardRect.Y + 8, 24, 24);
                            var minusR = new RectangleF(cardRect.Right - 32, cardRect.Y + 38, 24, 24);
                            
                            SovereignEngine.DrawGlassPanel(g, plusR, 4);
                            SovereignEngine.DrawGlassPanel(g, minusR, 4);
                            
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString("+", SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.White, plusR, sf);
                            g.DrawString("-", SovereignEngine.GetFont("Montserrat Bold", 9f), Brushes.White, minusR, sf);
                            
                            g.DrawString("ЦІЛЬ: [ЕТАЛОН]", SovereignEngine.GetFont("Consolas", 6.5f, FontStyle.Bold), 
                                new SolidBrush(Color.FromArgb(120, Color.Cyan)), innerX, cardRect.Bottom - 14);

                      
                            g.DrawString("СТАБІЛЬНО", SovereignEngine.GetFont("Montserrat Bold", 5.5f), 
                                new SolidBrush(Color.FromArgb(180, Color.Lime)), cardRect.Right - 38, cardRect.Bottom - 11);
                        }
                    }
                }
                curY += ((group.Sensors.Length + 1) / 2) * (cardH + cardGap) + 4;
            }

 
            g.DrawString($"[OK] {DateTime.Now:HH:mm} — Систему стабілізовано",
                SovereignEngine.GetFont("Consolas", 8.5f), new SolidBrush(Color.FromArgb(140, 80, 220, 120)), pad, curY + 5);

            curY += 30; 

    

        
            var diagR = GetDiagnosticsRect(curY + 30);
            DrawTactileButton(g, diagR, "🔬 ДІАГНОСТИКА", Color.FromArgb(180, 180, 180), _diagAnim);
            
            var calibR = GetCalibrationRect(curY + 30);
            string calibText = _calibrating ? "❌ СКАСУВАТИ" : "⚖️ КАЛІБРУВАННЯ";
            Color calibCol = _calibrating ? SovereignEngine.AmberAccent : Color.FromArgb(180, 180, 180);
            DrawTactileButton(g, calibR, calibText, calibCol, _calibAnim);

         
            var flushR = GetFlushRect(curY + 85);
            DrawTactileButton(g, flushR, "🌊 ПРОМИВКА", Color.FromArgb(0, 229, 255), _flushAnim);

         
            var abortR = GetAbortRect(curY + 85);
            DrawAbortButton(g, abortR, _abortProgress);

          

            // Glass container for radar
            var radarPanel = new RectangleF(rightX, pad, rightW, Height - pad * 2);
            SovereignEngine.DrawGlassPanel(g, radarPanel, 14f);

            // Title
            g.DrawString("ПРОФІЛЬ ЕКСТРАКЦІЇ", SovereignEngine.GetFont("Montserrat Bold", 12f),
                new SolidBrush(SovereignEngine.PearlText), rightX + pad, pad + 16);

            // V12.0: Clock inside Radar Panel (Higher position)
            g.DrawString(DateTime.Now.ToString("HH:mm:ss"), SovereignEngine.GetFont("Consolas", 11f), 
                new SolidBrush(Color.FromArgb(140, SovereignEngine.AmberAccent)), rightX + rightW - pad - 75, pad + 18);

            // Radar center and radius
            int rcx = rightX + rightW / 2;
            int rcy = pad + 20 + (int)((Height - pad * 2 - 40) * 0.48f);
            int rRadius = Math.Min(rightW / 2 - 60, (Height - 180) / 2 - 30);
            if (rRadius < 60) rRadius = 60;

            // Grid rings
            using (var penGrid = new Pen(Color.FromArgb(20, 255, 255, 255), 1f))
            {
                for (float f = 0.2f; f <= 1.01f; f += 0.2f)
                {
                    float r = rRadius * f;
                    g.DrawEllipse(penGrid, rcx - r, rcy - r, r * 2, r * 2);
                }
                for (int i = 0; i < 5; i++)
                {
                    double angle = -Math.PI / 2 + i * (2 * Math.PI / 5);
                    g.DrawLine(penGrid, rcx, rcy, rcx + (float)Math.Cos(angle) * rRadius, rcy + (float)Math.Sin(angle) * rRadius);
                }
            }

            // Axis labels (УКРАЇНСЬКОЮ) — V11.0: Більший шрифт, зчитування з 1м
            for (int i = 0; i < 5; i++)
            {
                double angle = -Math.PI / 2 + i * (2 * Math.PI / 5);
                float lx = rcx + (float)Math.Cos(angle) * (rRadius + 32);
                float ly = rcy + (float)Math.Sin(angle) * (rRadius + 24);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                // V15.0: Tuning Feedback on Radar
                if (_calibrating && (i == 0 || i == 1 || i == 2))
                {
                    int alpha = (int)(100 + 155 * _radarPulse[i]);
                    using (var br = new SolidBrush(Color.FromArgb(alpha, 0, 255, 255)))
                        g.DrawString("⚡ НАЛАШТУВАННЯ", SovereignEngine.GetFont("Montserrat Bold", 6.5f), br, lx, ly - 12, sf);
                }

                g.DrawString(_axisLabels[i], SovereignEngine.GetFont("Montserrat Bold", 10f),
                    new SolidBrush(Color.FromArgb(220, 255, 255, 255)), lx, ly, sf);
                // V11.0: Values in WHITE for contrast, not amber
                g.DrawString(_radarData[i].ToString("F2"), SovereignEngine.GetFont("Consolas", 9f, FontStyle.Bold),
                    new SolidBrush(SovereignEngine.PearlText), lx, ly + 17, sf);
            }

            // Golden Ratio (еталон — зелений пунктир)
            PointF[] goldenPoly = new PointF[5];
            for (int i = 0; i < 5; i++)
            {
                double a = -Math.PI / 2 + i * (2 * Math.PI / 5);
                goldenPoly[i] = new PointF(rcx + (float)Math.Cos(a) * rRadius * _goldenProfile[i], rcy + (float)Math.Sin(a) * rRadius * _goldenProfile[i]);
            }
            using (var pen = new Pen(Color.FromArgb(50, 80, 220, 120), 1.5f) { DashStyle = DashStyle.Dash })
                g.DrawPolygon(pen, goldenPoly);
            using (var br = new SolidBrush(Color.FromArgb(10, 80, 220, 120)))
                g.FillPolygon(br, goldenPoly);

            // Current shot (бурштиновий)
            PointF[] shotPoly = new PointF[5];
            for (int i = 0; i < 5; i++)
            {
                double a = -Math.PI / 2 + i * (2 * Math.PI / 5);
                shotPoly[i] = new PointF(rcx + (float)Math.Cos(a) * rRadius * _radarData[i], rcy + (float)Math.Sin(a) * rRadius * _radarData[i]);
            }

            if (shotPoly.Length >= 3)
            {
                using (var pgb = new PathGradientBrush(shotPoly))
                {
                    pgb.CenterColor = Color.FromArgb(55, SovereignEngine.AmberAccent);
                    pgb.SurroundColors = new Color[] { Color.FromArgb(10, SovereignEngine.AmberAccent) };
                    g.FillPolygon(pgb, shotPoly);
                }
            }
            using (var pen = new Pen(Color.FromArgb(200, SovereignEngine.AmberAccent), 2.5f) { LineJoin = LineJoin.Round })
                g.DrawPolygon(pen, shotPoly);

            for (int i = 0; i < 5; i++)
                g.FillEllipse(Brushes.White, shotPoly[i].X - 4, shotPoly[i].Y - 4, 8, 8);

     
            bool deviated = false;
            for (int i = 0; i < 5; i++)
                if (Math.Abs(_radarData[i] - _goldenProfile[i]) > 0.2f) { deviated = true; break; }
            if (deviated)
            {
                int blinkA = SovereignEngine.C(100 + 155 * (float)(Math.Sin(_time * 6) * 0.5 + 0.5));
                g.DrawString("⚠ ВІДХИЛЕННЯ ВІД ЕТАЛОНУ", SovereignEngine.GetFont("Montserrat Bold", 10f),
                    new SolidBrush(Color.FromArgb(blinkA, SovereignEngine.AmberAccent)), rightX + pad, (int)radarPanel.Bottom - 65);
            }

            int legY = (int)radarPanel.Bottom - 42;
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 80, 220, 120)), rightX + pad, legY, 14, 12);
            g.DrawString("Еталон", SovereignEngine.GetFont("Segoe UI Semibold", 9f), new SolidBrush(SovereignEngine.PearlText), rightX + pad + 20, legY - 2);
            g.FillRectangle(new SolidBrush(Color.FromArgb(120, SovereignEngine.AmberAccent)), rightX + pad + 110, legY, 14, 12);
            g.DrawString("Поточний шот", SovereignEngine.GetFont("Segoe UI Semibold", 9f), new SolidBrush(SovereignEngine.PearlText), rightX + pad + 130 , legY - 2);


            if (_abortProgress >= 1f)
            {
                using (var br = new SolidBrush(Color.FromArgb(200, 20, 0, 0)))
                    g.FillRectangle(br, 0, 0, Width, Height);
                
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                
              
                g.DrawString("⚠️", SovereignEngine.GetFont("Segoe UI Emoji", 40f), 
                    Brushes.Red, new RectangleF(0, -65, Width, Height), sf);
                
                g.DrawString("СИСТЕМУ ЗУПИНЕНО", SovereignEngine.GetFont("Montserrat Bold", 42f), 
                    Brushes.Red, new RectangleF(0, 0, Width, Height), sf);
                
                g.DrawString("ПЕРЕЗАВАНТАЖЕННЯ ПОТРІБНЕ", SovereignEngine.GetFont("Segoe UI Semibold", 14f), 
                    Brushes.White, new RectangleF(0, 85, Width, Height), sf);
            }

        
            if (_isScanning && _scanProgress <= 1.0f)
            {
        
                float sy = pad + 80 + _scanProgress * (Height - 200);
                using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(220 * (float)Math.Sin(_time * 18)), 0, 255, 200), 2.5f))
                    g.DrawLine(pen, pad, sy, leftW - pad, sy);
                
             
                var termR = new RectangleF(pad + 15, pad + 100, 240, 220);
                SovereignEngine.DrawGlassPanel(g, termR, 8);
                using (var pen = new Pen(Color.FromArgb(100, 0, 255, 180), 1f))
                    g.DrawPath(pen, SovereignEngine.GetRoundRect(termR, 8));

                g.DrawString("АНАЛІТИЧНА_КОНСОЛЬ_V4.1", SovereignEngine.GetFont("Consolas", 8f, FontStyle.Bold), 
                    new SolidBrush(Color.FromArgb(200, 0, 255, 255)), termR.X + 8, termR.Y + 8);
                
                for(int i = 0; i < _diagTerminal.Count; i++)
                    g.DrawString(_diagTerminal[i], SovereignEngine.GetFont("Consolas", 7.5f), 
                        new SolidBrush(Color.FromArgb(160, 0, 255, 150)), termR.X + 8, termR.Y + 30 + i * 14);


                int activeRow = (int)(_scanProgress * 4);
                if (_rand.Next(10) > 6) {
                    var highlightR = new RectangleF(pad, pad + 74 + activeRow * (cardH + 6), leftW - pad * 2, cardH);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(15, 0, 255, 150)), highlightR);
                }
            }

  
            if (_flushing)
            {
                var gaugeR = new RectangleF(pad + 60, pad + 120, 160, 160);
                SovereignEngine.DrawGlassPanel(g, gaugeR, 80);
                using (var pen = new Pen(Color.FromArgb(40, 0, 180, 255), 8f))
                    g.DrawEllipse(pen, gaugeR.X + 10, gaugeR.Y + 10, 140, 140);
                
                using (var pen = new Pen(Color.Cyan, 8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(pen, gaugeR.X + 10, gaugeR.Y + 10, 140, 140, -90, (_cleaningEff / 100f) * 360f);

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("ОЧИЩЕННЯ", SovereignEngine.GetFont("Montserrat Bold", 8f), Brushes.Cyan, gaugeR.X + 80, gaugeR.Y + 65, sf);
                g.DrawString($"{(int)_cleaningEff}%", SovereignEngine.GetFont("Consolas", 18f, FontStyle.Bold), Brushes.White, gaugeR.X + 80, gaugeR.Y + 95, sf);
                
                string phaseText = _flushPhase == 1 ? "ЗМОЧУВАННЯ" : (_flushPhase == 2 ? "СИЛОВИЙ ПРОМИВ" : "ПАРОВА ПРОДУВКА");
                g.DrawString(phaseText, SovereignEngine.GetFont("Consolas", 7f, FontStyle.Bold), 
                    new SolidBrush(Color.FromArgb(200, Color.White)), gaugeR.X + 80, gaugeR.Y + 130, sf);
            }


            if (_showDiagReport)
            {
                var repBox = new RectangleF(pad + 20, pad + 80, leftW - pad * 4, 340);
                SovereignEngine.DrawGlassPanel(g, repBox, 16);
                using (var pen = new Pen(Color.FromArgb(180, 0, 255, 255), 2f)) 
                    g.DrawPath(pen, SovereignEngine.GetRoundRect(repBox, 16));

                float rx = repBox.X + 35;
                float ry = repBox.Y + 35;
                g.DrawString("ЗВІТ ІНТЕЛЕКТУАЛЬНОГО АНАЛІЗУ", SovereignEngine.GetFont("Montserrat Bold", 13f), 
                    new SolidBrush(Color.FromArgb(240, SovereignEngine.PearlText)), rx, ry);
                ry += 55;

                foreach (var kvp in _diagHealth)
                {
                    g.DrawString(kvp.Key, SovereignEngine.GetFont("Segoe UI Semibold", 10f), Brushes.White, rx, ry);
                    var barR = new RectangleF(rx + 180, ry + 4, 140, 10);
                    
                 
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), barR);
                    
               
                    Color sCol = kvp.Value > 0.9f ? Color.FromArgb(80, 220, 120) : (kvp.Value > 0.75f ? SovereignEngine.AmberAccent : Color.FromArgb(255, 80, 80));
                    g.FillRectangle(new SolidBrush(sCol), rx + 180, ry + 4, 140 * kvp.Value, 10);
                    
                    g.DrawString($"{(int)(kvp.Value * 100)}%", SovereignEngine.GetFont("Consolas", 9.5f, FontStyle.Bold), 
                        new SolidBrush(sCol), rx + 325, ry - 1);
                    ry += 32;
                }

                if (_diagHealth.ContainsKey("Гострота Жорен") && _diagHealth["Гострота Жорен"] < 0.75f)
                {
                    ry += 15;
                    g.DrawString("⚠ РЕКОМЕНДАЦІЯ: ВИМАГАЄТЬСЯ ОБСЛУГОВУВАННЯ ЖОРЕН", SovereignEngine.GetFont("Montserrat Bold", 8.5f), 
                        new SolidBrush(SovereignEngine.AmberAccent), rx, ry);
                }

                g.DrawString("НАТИСНІТЬ БУДЬ-ДЕ, ЩОБ ЗАКРИТИ", SovereignEngine.GetFont("Segoe UI", 8.5f, FontStyle.Italic), 
                    new SolidBrush(Color.FromArgb(160, 255, 255, 255)), repBox.Left + (repBox.Width/2) - 100, repBox.Bottom - 35);
            }

  
            if (_calibrating)
            {
                int glowA = SovereignEngine.C(40 + 30 * (float)(Math.Sin(_time * 4) * 0.5 + 0.5));
                using (var pen = new Pen(Color.FromArgb(glowA, Color.Cyan), 3f))
                    g.DrawRectangle(pen, rightX + 2, pad + 2, rightW - 4, Height - pad * 2 - 4);
                
                g.DrawString("⚡ РЕЖИМ КАЛІБРУВАННЯ АКТИВНИЙ", SovereignEngine.GetFont("Montserrat Bold", 9f),
                    Brushes.Cyan, rightX + pad, (int)radarPanel.Bottom - 85);
            }

       
            if (_reportAnim > 0.01f)
            {
                float rw = 400 * _reportAnim;
                var reportBox = new RectangleF(Width - rw - pad, pad + 40, rw, Height - pad * 2 - 40);
                
                using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(_reportAnim * 240), 10, 10, 10)))
                    g.FillPath(br, SovereignEngine.GetRoundRect(reportBox, 15));
                
                using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(_reportAnim * 80), 255, 255, 255), 1.5f))
                    g.DrawPath(pen, SovereignEngine.GetRoundRect(reportBox, 15));

                if (_reportAnim > 0.8f)
                {
                    float rx = reportBox.X + 25;
                    float ry = reportBox.Y + 25;
                    g.DrawString("ЗВІТ ЕФЕКТИВНОСТІ", SovereignEngine.GetFont("Montserrat Bold", 14f), Brushes.White, rx, ry);
                    ry += 40;
                    
                    string[] stats = { 
                        "• Виконано циклів: 124,500", 
                        "• Середній тиск: 9.1 Бар", 
                        "• Стабільність темп.: 98.4%",
                        "• Витрата зерна: 42.1 кг",
                        "• Помилок системи: 0"
                    };
                    
                    foreach(var s in stats) {
                        g.DrawString(s, SovereignEngine.GetFont("Segoe UI", 11f), new SolidBrush(Color.FromArgb(200, 255, 255, 255)), rx, ry);
                        ry += 30;
                    }
                    
                    g.DrawString("ЗАКРИТИ [X]", SovereignEngine.GetFont("Montserrat Bold", 9f), 
                        new SolidBrush(Color.FromArgb(150, 255, 255, 255)), reportBox.Right - 90, reportBox.Bottom - 30);
                }
            }
        }

        private Color GetTempColor(float t) => t > 95f ? Color.FromArgb(255, 80, 80) : (t < 88f ? Color.FromArgb(80, 160, 255) : Color.FromArgb(80, 220, 120));
        private Color GetPressureColor(float p) => (p >= 8.5f && p <= 9.5f) ? Color.FromArgb(80, 220, 120) : SovereignEngine.AmberAccent;

        private void DrawTactileButton(Graphics g, Rectangle r, string text, Color color, float pressAnim)
        {
            var rect = new RectangleF(r.X + pressAnim, r.Y + pressAnim * 2, r.Width, r.Height);
            if (pressAnim < 0.3f)
            {
                using (var sp = SovereignEngine.GetRoundRect(new RectangleF(r.X + 1, r.Y + 2, r.Width, r.Height), 8))
                using (var sb = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                    g.FillPath(sb, sp);
            }
            using (var path = SovereignEngine.GetRoundRect(rect, 10))
            {
                using (var pen = new Pen(Color.FromArgb(SovereignEngine.C(130 + pressAnim * 125), color), 1.5f))
                    g.DrawPath(pen, path);
                if (pressAnim > 0.01f)
                    using (var br = new SolidBrush(Color.FromArgb(SovereignEngine.C(pressAnim * 30), color)))
                        g.FillPath(br, path);
                
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, SovereignEngine.GetFont("Segoe UI Emoji", 9f, FontStyle.Bold), new SolidBrush(color), rect, sf);
            }
        }

        private void DrawAbortButton(Graphics g, Rectangle r, float progress)
        {
            var rect = new RectangleF(r.X + progress, r.Y + progress * 2, r.Width, r.Height);
            using (var path = SovereignEngine.GetRoundRect(rect, 10))
            {
                float dim = 1f - progress * 0.3f;
                using (var lgb = new LinearGradientBrush(r, Color.FromArgb(SovereignEngine.C(200 * dim), SovereignEngine.C(50 * dim), SovereignEngine.C(50 * dim)),
                    Color.FromArgb(SovereignEngine.C(130 * dim), 0, 0), 90f))
                    g.FillPath(lgb, path);
                
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("⚠️ АВАРІЙНА ЗУПИНКА", SovereignEngine.GetFont("Segoe UI Emoji", 9f, FontStyle.Bold), Brushes.White, rect, sf);
                
                if (progress > 0.01f)
                {
                    using (var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawArc(pen, rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6, -90, progress * 360f);
                }
            }
        }
    }
}
