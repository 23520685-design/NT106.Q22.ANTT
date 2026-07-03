using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniSocialApp.LoginUI_Design
{
    public class GradientPanel : Panel
    {
        public Color gradientTop { get; set; }
        public Color gradientBottom { get; set; }

        public GradientPanel()
        {
            this.Resize += GradientPanel_Resize;
        }

        private void GradientPanel_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush linear = new LinearGradientBrush(
                this.ClientRectangle,
                this.gradientTop,
                this.gradientBottom,
                90F))
            {
                e.Graphics.FillRectangle(linear, this.ClientRectangle);
            }

            base.OnPaint(e);
        }
    }
}