using System.Drawing.Drawing2D;

namespace Cafe.Launcher.UI
{

    public class BackgroundForm : Form
    {
        public BackgroundForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(14, 18, 28); 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

     
   
            g.FillPolygon(new SolidBrush(Color.FromArgb(38, 28, 38, 68)),
                new Point[] { new Point(0, 0), new Point(Width / 2, 0), new Point(Width / 5, Height / 2) });

        
            g.FillPolygon(new SolidBrush(Color.FromArgb(50, 0, 60, 80)),
                new Point[] { new Point(Width, 0), new Point(Width - Width / 3, 0), new Point(Width - Width / 5, Height / 3) });

         
            g.FillPolygon(new SolidBrush(Color.FromArgb(42, 0, 70, 90)),
                new Point[] { new Point(Width, Height / 2), new Point(Width - 380, Height), new Point(Width, Height) });

         
            using (var lgb = new LinearGradientBrush(
                new Point(0, Height), new Point(Width / 3, Height / 2),
                Color.FromArgb(45, 0, 80, 140), Color.Transparent))
            {
                g.FillPolygon(lgb, new Point[] {
                    new Point(0, Height), new Point(Width / 3, Height), new Point(Width / 6, Height / 2)
                });
            }


            int lw = 740, lh = 480;
            Rectangle shadowCenter = new Rectangle((Width - lw) / 2, (Height - lh) / 2 + 18, lw, lh);
            for (int i = 40; i >= 1; i--)
            {
                float a = 0.55f * (1f - i / 40f);
                using (var p = new Pen(Color.FromArgb((int)(a * 100), Color.Black), i * 2f))
                    g.DrawRectangle(p, shadowCenter);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            using (var launcher = new LauncherForm(this))
                launcher.ShowDialog(this);
            this.Close();
        }
    }
}
