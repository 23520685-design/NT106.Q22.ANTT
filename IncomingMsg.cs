using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace New
{
    public partial class IncomingMsg : UserControl
    {
        public IncomingMsg(string text, string time)
        {
            InitializeComponent();
            lblMsg.Text = text;
            lblTime.Text = time;

            // Config: 
            // - lblMsg: BackColor = Gray, ForeColor = White, Padding = 10
            // - Panel bao quanh lblMsg nên có AutoSize = True
            AdjustHeight();
        }

        private void AdjustHeight()
        {
            // Tự động giãn chiều cao theo nội dung chữ
            Size size = TextRenderer.MeasureText(lblMsg.Text, lblMsg.Font,
                        new Size(lblMsg.Width, 0), TextFormatFlags.WordBreak);
            lblMsg.Height = size.Height + 20;
            this.Height = lblMsg.Bottom + 10;
        }
        
            // Tự động cuộn xuống cuối

        

        private void IncomingMsg_Load(object sender, EventArgs e)
        {

        }
    }
}