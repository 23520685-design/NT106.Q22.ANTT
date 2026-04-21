using System;
using System.Drawing;
using System.Windows.Forms;

namespace New
{
    public partial class OutgoingMsg : UserControl
    {
        public OutgoingMsg(string message, string time)
        {
            InitializeComponent();

            // Gán nội dung
            lblMessage.Text = message;
            lblTime.Text = time;

            int maxWidth = 250;
            Size size = TextRenderer.MeasureText(lblMessage.Text, lblMessage.Font,
                        new Size(maxWidth, 0), TextFormatFlags.WordBreak);

            lblMessage.Size = new Size(size.Width + 10, size.Height + 10);

            this.Width = lblMessage.Width + 20;
            this.Height = lblMessage.Height + lblTime.Height + 15;
        }  
    }
}