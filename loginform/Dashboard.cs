using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace loginform
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc muốn đăng xuất không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var authClient = FirebaseService.GetAuthClient();
                authClient.SignOut();

                login loginForm = new login();
                loginForm.Show();

                this.Close();
            }
        }

        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms.Count == 0 || (Application.OpenForms.Count == 1 && Application.OpenForms[0] is Dashboard))
            {
                Application.Exit();
            }
        }
    }
}
