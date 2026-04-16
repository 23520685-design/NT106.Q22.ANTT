using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Runtime.InteropServices;

namespace loginform
{
    public partial class login : Form
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private HttpListener listener;

        public login()
        {
            InitializeComponent();
            this.MouseDown += Form_MouseDown;
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
                MessageBox.Show(this, $"Đã gửi link reset mật khẩu tới {email}, vui lòng kiểm tra mục spam.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                string responseString = $@"
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background-color: #f0f2f5; 
            display: flex; 
            justify-content: center; 
            align-items: center; 
            height: 100vh; 
            margin: 0; 
        }}
        .card {{ 
            background: white; 
            padding: 40px; 
            border-radius: 12px; 
            box-shadow: 0 10px 25px rgba(0,0,0,0.1); 
            text-align: center; 
            max-width: 400px; 
            animation: fadeIn 0.6s ease-out;
        }}
        .icon-circle {{
            width: 80px;
            height: 80px;
            background-color: #4BB543;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 20px;
            color: white;
            font-size: 40px;
            box-shadow: 0 4px 10px rgba(75, 181, 67, 0.3);
        }}
        h1 {{ color: #1877f2; margin: 0 0 10px 0; font-size: 24px; }}
        p {{ color: #65676b; font-size: 16px; margin: 0; }}
        .loader {{
            margin-top: 25px;
            height: 4px;
            width: 100%;
            background: #e4e6eb;
            border-radius: 2px;
            overflow: hidden;
            position: relative;
        }}
        .loader::after {{
            content: '';
            position: absolute;
            left: 0; height: 100%; width: 0;
            background: #1877f2;
            animation: progress 2s linear forwards;
        }}
        @keyframes fadeIn {{ from {{ opacity: 0; transform: translateY(20px); }} to {{ opacity: 1; transform: translateY(0); }} }}
        @keyframes progress {{ from {{ width: 0; }} to {{ width: 100%; }} }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon-circle'>✓</div>
        <h1>Koobecaf Verified</h1>
        <p>Xác thực thành công! Hệ thống đang chuyển hướng...</p>
        <div class='loader'></div>
        <p style='font-size: 12px; margin-top: 15px; color: #bcc0c4;'>Cửa sổ này sẽ tự động đóng sau giây lát.</p>
    </div>
    <script>
        setTimeout(function(){{ window.close(); }}, 2000);
    </script>
</body>
</html>";

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

                this.Invoke((MethodInvoker)delegate {
                    this.WindowState = FormWindowState.Normal; // Nếu đang thu nhỏ thì bung lên
                    SetForegroundWindow(this.Handle);          // Ép Windows đẩy app lên trước mặt
                    this.Activate();                           // Kích hoạt Form
                });

                MessageBox.Show(this, "Đăng nhập Google thành công!", "Koobecaf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var dash = new Dashboard();
                dash.Show();
                SetForegroundWindow(dash.Handle); // Đảm bảo Dashboard cũng nằm trên cùng
                this.Hide();
            }
            catch (Exception ex) { ShowError("Lỗi xác thực Google: " + ex.Message); }
        }
    }
}
