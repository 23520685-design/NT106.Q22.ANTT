using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace loginform
{
    public partial class logintext : UserControl
    {
        public logintext()
        {
            InitializeComponent();
        }

        private string _titleText = "default value";
        private bool _isPassword = false;

        [Category("Custom Properties")]
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                _titleText = value;
                label1.Text = value;
            }
        }

        [Category("Custom Properties")]
        public bool IsPassword
        {
            get { return _isPassword; }
            set
            {
                _isPassword = value;
                textBox1.UseSystemPasswordChar = value;
            }
        }
        public override string Text
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
