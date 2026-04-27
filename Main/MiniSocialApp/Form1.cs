using MiniSocialApp.Controllers;
using MiniSocialApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MiniSocialApp
{
    public partial class Form1 : Form
    {
        private MessageHandler _messageHandler;
        public bool IsRestarting { get; private set; } = false;
        private readonly SeedDataService seedDataService = new SeedDataService(new FirestoreContext());
        public Form1()
        {
            InitializeComponent();

        }

        private string ChooseImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Chọn ảnh";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return null;
        }

        // Timer tự động refresh feed
        private System.Windows.Forms.Timer _feedTimer;

        private async void Form1_Load(object sender, EventArgs e)
        {
            //await seedDataService.SeedStarterPostsForUser(CurrentUserStore.User as Dictionary<string, object>);

            await webView21.EnsureCoreWebView2Async();
            webView21.ZoomFactor = 1.0;
            // ✅ Dùng chung 1 FirestoreContext cho toàn app
            var firestoreContext = new FirestoreContext();
            var postService = new PostService(firestoreContext);
            var postController = new PostController(postService);

            // ✅ FIX 7: Inject LikeService vào LikeController – dùng chung firestoreContext
            var likeService = new LikeService(firestoreContext);
            var likeController = new LikeController(likeService);

            var userService = new UserService(firestoreContext);
            var userController = new UserController(userService);

            _messageHandler = new MessageHandler(postController, likeController, userController);


            var path = Path.Combine(Application.StartupPath, "UI", "Home", "home.html");
            webView21.Source = new Uri(path);

            // Sau khi trang HTML load xong → gửi thông tin user xuống JS
            webView21.CoreWebView2.NavigationCompleted += (s, args) =>
            {
                var userDict = CurrentUserStore.User as Dictionary<string, object>;
                if (userDict == null) return;

                string userName = userDict.ContainsKey("userName") ? userDict["userName"]?.ToString() : "User";
                string avatar = userDict.ContainsKey("avatar") ? userDict["avatar"]?.ToString() : "";

                var msg = new
                {
                    type = "USER_UPDATED",
                    data = new { userName, avatar }
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                webView21.CoreWebView2.PostWebMessageAsJson(json);

                // ✅ Bắt đầu tự động refresh feed mỗi 15 giây
                StartFeedPolling();
            };


        // nhận message từ JS
            webView21.CoreWebView2.WebMessageReceived += async (s, x) =>
            {
                try
                {
                    string message = x.WebMessageAsJson;
                    dynamic msg = Newtonsoft.Json.JsonConvert.DeserializeObject(message);

                    Console.WriteLine("Received from JS: " + message);

                    // 🔥 Xử lý chọn ảnh – cần UI thread
                    if (msg.type == "CHOOSE_IMAGE")
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            var path1 = ChooseImage();

                            var response = new
                            {
                                type = "IMAGE_SELECTED",
                                data = new { path = path1 }
                            };

                            webView21.CoreWebView2.PostWebMessageAsJson(
                                Newtonsoft.Json.JsonConvert.SerializeObject(response)
                            );
                        }));

                        return;
                    }

                    // ✅ FIX 4: Dùng Application.Restart() thay vì tạo login form mới
                    // → tránh memory leak (form cũ bị Ẩn nhưng không Dispose)
                    if (msg.type == "LOGOUT")
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            CurrentUserStore.User = null;
                            IsRestarting = true;
                            Application.Restart();
                        }));

                        return;
                    }

                    string result = await _messageHandler.Handle(message);

                    if (result != null)
                    {
                        webView21.CoreWebView2.PostWebMessageAsJson(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            };

        }

        // ============================================================
        // AUTO REFRESH FEED – polling mỗi 15 giây
        // ============================================================
        private void StartFeedPolling()
        {
            if (_feedTimer != null) return; // tránh tạo nhiều timer

            _feedTimer = new System.Windows.Forms.Timer();
            _feedTimer.Interval = 20000; // 20 giây
            _feedTimer.Tick += async (s, ev) =>
            {
                try
                {
                    string result = await _messageHandler.Handle("{\"type\":\"GET_FEED\"}");
                    if (result != null)
                        webView21.CoreWebView2.PostWebMessageAsJson(result);
                }
                catch { /* bỏ qua lỗi poll */ }
            };
            _feedTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _feedTimer?.Stop();
            _feedTimer?.Dispose();
            base.OnFormClosed(e);
        }

    }
}