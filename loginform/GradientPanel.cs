using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace loginform
{
    internal class GradientPanel
    {
    }
        namespace LoginUI_Design
    {
        public class GradientPanel : Panel
        {
            //Create a properties to define the colors for the gradient's top and bottom
            public Color gradientTop { get; set; }
            public Color gradientBottom { get; set; }

            //Create Constructor for the Gradient Panel Class
            public GradientPanel()
            {
                //Subscribe to the resize event to handle when the control's size changes
                this.Resize += GradientPanel_Resize;
            }

            private void GradientPanel_Resize(object sender, EventArgs e)
            {
                this.Invalidate(); //this marks the control as needing to be redrawn
            }

            //override the onPaint method to draw a gradient background
            protected override void OnPaint(PaintEventArgs e)
            {
                //create a lineargradientbrush with the specified top and bottom gradient colors
                LinearGradientBrush linear = new LinearGradientBrush(
                    this.ClientRectangle, // thie area to fill with the gradient
                    this.gradientTop, // the starting color (top of the gradient)
                    this.gradientBottom, //the ending color(bottom of the gradient)
                    90F //lastly the angle of the gradient(90 degress = vertical)

                );

                //get the graphics context for drawing
                Graphics g = e.Graphics;

                //Fill the entire control area with the gradient
                g.FillRectangle(linear, this.ClientRectangle);

                //lastly call the base class onpaint to ensure any additional paintin
                base.OnPaint(e);
            }
        }
    }
}
