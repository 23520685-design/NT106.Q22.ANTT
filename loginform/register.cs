using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace loginform
{
    public partial class register : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);
        private HttpListener listener;

        public register()
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

        private void label5_Click(object sender, EventArgs e)
        {
            new login().Show();
            this.Hide();
        }

        // --- ĐĂNG KÝ BẰNG EMAIL/PASS ---
        private async void button1_Click(object sender, EventArgs e)
        {
            var authClient = FirebaseService.GetAuthClient();
            string email = logintext1.Text.Trim();
            string password = logintext2.Text.Trim();

            // 1. Kiểm tra trống
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng điền đầy đủ thông tin đăng ký!");
                return;
            }

            // 2. Kiểm tra độ dài mật khẩu (Luật của Firebase là tối thiểu 6 ký tự)
            if (password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự nhé!");
                return;
            }

            try
            {
                await authClient.CreateUserWithEmailAndPasswordAsync(email, password);

                MessageBox.Show(this, "Đăng ký tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                new Dashboard().Show();
                this.Hide();
            }
            catch (FirebaseAuthException ex)
            {
                string friendlyMessage = "Đăng ký thất bại, vui lòng thử lại.";
                string rawError = ex.Message.ToLower();

                // Bắt các lỗi đặc thù của Đăng ký
                if (rawError.Contains("email_exists"))
                {
                    friendlyMessage = "Email này đã được đăng ký!";
                }
                else if (rawError.Contains("invalid_email") || rawError.Contains("invalid-email"))
                {
                    friendlyMessage = "Email không đúng định dạng!";
                }
                else if (rawError.Contains("too_many_attempts"))
                {
                    friendlyMessage = "Thao tác quá nhanh, thử lại sau nhé.";
                }

                ShowError(friendlyMessage);
            }
            catch (Exception ex)
            {
                ShowError("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void ShowError(string msg)
        {
            // Không dùng ServiceNotification để tránh mất Focus
            MessageBox.Show(this, msg, "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Activate();
        }

        // --- ĐĂNG KÝ/ĐĂNG NHẬP NHANH BẰNG GOOGLE ---
        private async void button2_Click(object sender, EventArgs e)
        {
            string redirectUri = "http://localhost:8081/";
            string url = $"https://accounts.google.com/o/oauth2/auth?client_id={FirebaseService.GoogleClientId}&redirect_uri={redirectUri}&scope=email%20profile&response_type=code";

            try
            {
                if (listener == null || !listener.IsListening)
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add(redirectUri);
                    listener.Start();
                }

                Process.Start(url);

                HttpListenerContext context = await listener.GetContextAsync();
                string code = context.Request.QueryString.Get("code");

                string responseString = "<html><body style='font-family:Arial;text-align:center;padding-top:50px;'>" +
                                       "<h1 style='color:#1877f2;'>Koobecaf Registration</h1>" +
                                       "<p>Xac thuc thanh cong!</p>" +
                                       "<script>setTimeout(function(){ window.close(); }, 2000);</script></body></html>";

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                listener.Stop();

                if (!string.IsNullOrEmpty(code))
                {
                    RegisterWithGoogleCode(code, redirectUri);
                }
            }
            catch (Exception ex)
            {
                if (listener != null) listener.Stop();
                ShowError("Lỗi kết nối Google: " + ex.Message);
            }
        }

        private async void RegisterWithGoogleCode(string code, string redirectUri)
        {
            try
            {
                string decodedCode = System.Net.WebUtility.UrlDecode(code);
                var access = AuthResponse.Exchange(decodedCode,
                                                  FirebaseService.GoogleClientId,
                                                  FirebaseService.GoogleClientSecret,
                                                  redirectUri);
                var credential = GoogleProvider.GetCredential(access.IdToken, OAuthCredentialTokenType.IdToken);

                var authClient = FirebaseService.GetAuthClient();
                var userCredential = await authClient.SignInWithCredentialAsync(credential);
                MessageBox.Show(this, "Liên kết Google thành công!", "Koobecaf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new Dashboard().Show();
                this.Hide();
            }
            catch (Exception ex) { ShowError("Lỗi xác thực Google: " + ex.Message); }
        }
    }
}