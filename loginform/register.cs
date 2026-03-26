using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace loginform
{
    public partial class register : Form
    {
        private HttpListener listener;

        public register()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        // Quay lại Login
        private void label5_Click(object sender, EventArgs e)
        {
            new login().Show();
            this.Hide();
        }

        // --- ĐĂNG KÝ BẰNG EMAIL/PASS ---
        private async void button1_Click(object sender, EventArgs e)
        {
            var authClient = FirebaseService.GetAuthClient();
            try
            {
                await authClient.CreateUserWithEmailAndPasswordAsync(logintext1.Text.Trim(), logintext2.Text.Trim());

                MessageBox.Show("Đăng ký tài khoản thành công!",
                                "Thông báo Koobecaf",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification);

                new Dashboard().Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- ĐĂNG KÝ BẰNG GOOGLE ---
        private async void button2_Click(object sender, EventArgs e)
        {
            // Dùng port 8081 để tránh xung đột nếu lỡ Form Login chưa đóng hẳn
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
                MessageBox.Show("Lỗi Server: " + ex.Message);
            }
        }

        private void RegisterWithGoogleCode(string code, string redirectUri)
        {
            try
            {
                string decodedCode = System.Net.WebUtility.UrlDecode(code);

                var access = AuthResponse.Exchange(decodedCode,
                                                  FirebaseService.GoogleClientId,
                                                  FirebaseService.GoogleClientSecret,
                                                  redirectUri);

                MessageBox.Show("Đăng ký qua Google thành công!",
                                "Thông báo Koobecaf",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification);

                new Dashboard().Show();
                this.Hide();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
    }
}