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
                MessageBox.Show(this, $"Đã gửi link reset mật khẩu tới {email}, vui lòng kiểm tra muc spam.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private void label5_Click(object sender, EventArgs e)
        {
            register registerForm = new register();
            registerForm.Show();
            this.Hide();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Dùng port 8081 cho đồng bộ với bên Register
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
                                       "<h1 style='color:#1877f2;'>Koobecaf Login</h1>" +
                                       "<p>Xac thuc thanh cong!</p>" +
                                       "<script>setTimeout(function(){ window.close(); }, 2000);</script></body></html>";

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                listener.Stop();

                if (!string.IsNullOrEmpty(code))
                {
                    LoginWithGoogleCode(code, redirectUri);
                }
            }
            catch (Exception ex)
            {
                if (listener != null) listener.Stop();
                ShowError("Lỗi kết nối Google: " + ex.Message);
            }
        }

        private async void LoginWithGoogleCode(string code, string redirectUri)
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
                MessageBox.Show(this, "Đăng nhập Google thành công!", "Koobecaf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new Dashboard().Show();
                this.Hide();
            }
            catch (Exception ex) { ShowError("Lỗi xác thực Google: " + ex.Message); }
        }
    }
}
