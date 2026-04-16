using Google.Cloud.Firestore;
using MiniSocialApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSocialApp
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Nhập tên đi bro");
                return;
            }

            // ✅ FIX 12: Validate số điện thoại
            if (!string.IsNullOrEmpty(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{9,11}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ (9–11 chữ số).");
                return;
            }

            btnLogin.Enabled = false;

            try
            {
                var userService = new UserService(new FirestoreContext());
                var user = await userService.LoginOrCreate(name, phone);

                // lưu current user
                CurrentUserStore.User = user;

                // mở main form
                var main = new Form1();
                // Khi Form1 đóng (không phải logout) → đóng luôn login form để thoát app
                main.FormClosed += (s2, e2) =>
                {
                    if (!main.IsRestarting)
                        this.Close(); // đóng main form → thoát app
                };
                main.Show();

                // Ẩn login form (không Close vì login là main form của Application.Run)
                this.Hide();
            }
            catch (Exception ex)
            {
                // ✅ FIX 3: Xử lý lỗi mạng/Firestore – re-enable nút login
                MessageBox.Show("Không thể đăng nhập. Kiểm tra kết nối mạng.\n\nChi tiết: " + ex.Message,
                                "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
            }
        }
    }
}
