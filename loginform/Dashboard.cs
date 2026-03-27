using System;
using System.Windows.Forms;

namespace loginform
{
    public partial class Dashboard : Form
    {
        // 1. Tạo một cờ để phân biệt bấm X hay bấm Đăng xuất
        private bool isLoggingOut = false;

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

                // 2. Bật cờ này lên trước khi đóng Form
                isLoggingOut = true;

                login loginForm = new login();
                loginForm.Show();

                this.Close();
            }
        }

        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isLoggingOut)
            {
                Application.Exit();
            }
        }
    }
}