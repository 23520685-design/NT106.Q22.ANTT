using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace loginform
{
    public partial class register : Form
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

        public register()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.MouseDown += Form_MouseDown;
        }

        // --- ĐĂNG KÝ BẰNG EMAIL/PASS ---
        private async void button1_Click(object sender, EventArgs e)
        {
            var authClient = FirebaseService.GetAuthClient();

            string email = logintext1.Text.Trim();
            string password = logintext2.Text.Trim();
            string confirmPassword = logintext3.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Hãy điền đầy đủ Email, Password và Confirm Password!");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Mật khẩu ít nhất 6 ký tự!");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Mật khẩu xác nhận không khớp!");
                logintext3.Focus();
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
                string friendlyMessage = "Đăng ký thất bại!";
                string rawError = ex.Message.ToLower();

                if (rawError.Contains("email_exists")) friendlyMessage = "Email này có đã được đăng ký!";
                else if (rawError.Contains("invalid_email")) friendlyMessage = "Email không đúng định dạng!";
                else if (rawError.Contains("too_many_attempts")) friendlyMessage = "Thao tác quá nhanh!";

                ShowError(friendlyMessage);
            }
            catch (Exception ex)
            {
                ShowError("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(this, msg, "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Activate();
        }

        // --- ĐĂNG KÝ/ĐĂNG NHẬP BẰNG GOOGLE ---
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
            border-radius: 16px; 
            box-shadow: 0 10px 30px rgba(0,0,0,0.1); 
            text-align: center; 
            max-width: 400px; 
            animation: fadeIn 0.8s ease-out;
        }}
        .icon-circle {{
            width: 80px;
            height: 80px;
            background-color: #00c853;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0 auto 20px;
            color: white;
            font-size: 45px;
            box-shadow: 0 4px 15px rgba(0, 200, 83, 0.3);
        }}
        h1 {{ color: #1877f2; margin: 0 0 10px 0; font-size: 26px; font-weight: 700; }}
        p {{ color: #4b4f56; font-size: 16px; margin: 0; line-height: 1.5; }}
        .loader {{
            margin-top: 30px;
            height: 6px;
            width: 100%;
            background: #e4e6eb;
            border-radius: 3px;
            overflow: hidden;
            position: relative;
        }}
        .loader::after {{
            content: '';
            position: absolute;
            left: 0; height: 100%; width: 0;
            background: linear-gradient(90deg, #1877f2, #00c853);
            animation: progress 2.5s linear forwards;
        }}
        @keyframes fadeIn {{ from {{ opacity: 0; transform: scale(0.9); }} to {{ opacity: 1; transform: scale(1); }} }}
        @keyframes progress {{ from {{ width: 0; }} to {{ width: 100%; }} }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon-circle'>✓</div>
        <h1>Welcome to Koobecaf</h1>
        <p>Tài khoản Google đã được liên kết thành công.</p>
        <div class='loader'></div>
        <p style='font-size: 13px; margin-top: 20px; color: #8d949e;'>Vui lòng quay lại ứng dụng để tiếp tục.</p>
    </div>
    <script>
        setTimeout(function(){{ window.close(); }}, 2500);
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

                this.Invoke((MethodInvoker)delegate {
                    this.WindowState = FormWindowState.Normal; // Nếu đang thu nhỏ thì bung lên
                    SetForegroundWindow(this.Handle);          // Ép Windows đẩy app lên trước mặt
                    this.Activate();                           // Kích hoạt Form
                });
                MessageBox.Show(this, "Liên kết Google thành công!", "Koobecaf", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var dash = new Dashboard();

                dash.Show();
                SetForegroundWindow(dash.Handle); // Đảm bảo Dashboard cũng nằm trên cùng
                this.Hide();
            }

            catch (Exception ex) { ShowError("Lỗi xác thực Google: " + ex.Message); }
        }

        private void label5_Click_1(object sender, EventArgs e)
        {
            login loginForm = new login();
            loginForm.Show();
            this.Hide();
        }
        private void logintext3_Load(object sender, EventArgs e) { }
        private void logintext2_Load(object sender, EventArgs e) { }
        private void logintext1_Load(object sender, EventArgs e) { }

        private void register_Load(object sender, EventArgs e)
        {

        }
    }
}
