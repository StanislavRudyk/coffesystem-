using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cafe.Client.Manager.Forms
{
    public class SovereignContainer : Panel
    {
        public SovereignContainer()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Padding = new Padding(25, 25, 25, 25);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var r = ClientRectangle;
            if (r.Width <= 1 || r.Height <= 1) return;
            
            r.Width -= 1;
            r.Height -= 1;

        
            using (var path = SovereignEngine.GetRoundRect(r, 24))
            {
                using (var br = new SolidBrush(Color.FromArgb(12, 255, 255, 255)))
                {
                    g.FillPath(br, path);
                }
                
               
                using (var p = new Pen(Color.FromArgb(20, 255, 255, 255), 1.5f))
                {
                    g.DrawPath(p, path);
                }
            }

          
            using (var hcPath = SovereignEngine.GetRoundRect(new Rectangle(r.X, r.Y, r.Width, 25), 20))
            {
                using (var hcBr = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, 25), 
                    Color.FromArgb(8, SovereignEngine.AmberAccent), Color.Transparent, 90f))
                {
                    g.SetClip(hcPath);
                    g.FillPath(hcBr, hcPath);
                    g.ResetClip();
                }
            }
        }
    }
}
