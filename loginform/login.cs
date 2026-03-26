using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;

namespace loginform
{
    public partial class login : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);
        private HttpListener listener;

        public login()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var authClient = FirebaseService.GetAuthClient();
            string email = logintext1.Text.Trim();
            string password = logintext2.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập đầy đủ Email và Mật khẩu!");
                return;
            }

            try
            {
                await authClient.SignInWithEmailAndPasswordAsync(email, password);
                MessageBox.Show(this, "Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new Dashboard().Show();
                this.Hide();
            }
            catch (FirebaseAuthException ex)
            {
                string friendlyMessage = "Đã xảy ra lỗi, vui lòng thử lại sau.";
                string rawError = ex.Message.ToLower();

                if (rawError.Contains("invalid_email") || rawError.Contains("invalid-email"))
                {
                    friendlyMessage = "Email không đúng định dạng!";
                }
                else if (rawError.Contains("invalid_login_credentials") ||
                         rawError.Contains("invalid_password") ||
                         rawError.Contains("user_not_found"))
                {
                    friendlyMessage = "Email hoặc mật khẩu không chính xác.";
                }
                else if (rawError.Contains("too_many_attempts"))
                {
                    friendlyMessage = "Bạn đã thử quá nhiều lần. Vui lòng thử lại sau!";
                }

                ShowError(friendlyMessage);
            }
            catch (Exception ex) { ShowError("Lỗi hệ thống: " + ex.Message); }
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(this, msg, "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Activate();
        }

        private async void label1_Click(object sender, EventArgs e)
        {
            if (!label1.Enabled) return;
            var authClient = FirebaseService.GetAuthClient();
            string email = logintext1.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show(this, "Hãy nhập Email!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await authClient.ResetEmailPasswordAsync(email);
                MessageBox.Show(this, $"Đã gửi link tới {email}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                label1.Enabled = false;
                string originalText = label1.Text;
                for (int i = 10; i > 0; i--)
                {
                    label1.Text = $"Thử lại sau {i}s";
                    await Task.Delay(1000);
                }
                label1.Text = originalText;
                label1.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowError("Lỗi: " + ex.Message);
                label1.Enabled = true;
            }
        }
    }
}