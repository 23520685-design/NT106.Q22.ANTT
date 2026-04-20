using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace New
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            flowLayoutPanelContacts.Controls.Clear(); // Xóa các item "rác" tạo bằng tay ở Design

            string[] names = { "Ralph Edwards", "Eleanor Pena", "Arlene McCoy" };
            foreach (var name in names)
            {
                ContactItem item = new ContactItem(name, "Online", null);

                // ĐĂNG KÝ Ở ĐÂY
                item.ItemClick += Contact_ItemClick;

                SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 0, "Search friends...");

                flowLayoutPanelContacts.Controls.Add(item);
            }
        }


        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            {
                string messageText = txtInput.Text.Trim();

                if (!string.IsNullOrEmpty(messageText))
                {
                    // 1. Tạo một tin nhắn gửi đi (Màu xanh)
                    // Lưu ý: "10:30" là ví dụ, bạn có thể dùng DateTime.Now.ToString("HH:mm")
                    OutgoingMsg msg = new OutgoingMsg(messageText, DateTime.Now.ToString("HH:mm"));

                    // 2. Chỉnh chiều rộng tin nhắn bằng với chiều rộng khung chat (trừ đi thanh cuộn)
                    msg.Width = flowLayoutPanelChat.Width - 25;

                    // 3. Thêm vào khung chat
                    flowLayoutPanelChat.Controls.Add(msg);

                    // 4. Xóa trống ô nhập và cuộn xuống cuối
                    txtInput.Clear();
                    flowLayoutPanelChat.ScrollControlIntoView(msg);
                }
            }
        }
        private void Contact_ItemClick(object sender, EventArgs e)
        {
            // 1. Tìm đúng ContactItem vừa click (dùng code cũ của bạn)
            ContactItem clickedItem = null;
            if (sender is ContactItem item) clickedItem = item;
            else if (sender is Control child) clickedItem = child.Parent as ContactItem;

            if (clickedItem == null) return;

            // 2. Cập nhật tên lên Header
            lblChatName.Text = clickedItem.GetContactName();

            // 3. QUAN TRỌNG: Xóa sạch tin nhắn cũ của người trước đó
            flowLayoutPanelChat.Controls.Clear();

            // 4. (Tùy chọn) Load tin nhắn của người mới từ Database hoặc List
            // LoadMessagesForUser(clickedItem.GetContactName());

            // 5. Đổi màu nền để đánh dấu người đang chọn
            foreach (Control c in flowLayoutPanelContacts.Controls)
            {
                c.BackColor = Color.White;
            }
            clickedItem.BackColor = Color.LightGray;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void AddMessage(string text, bool isOutgoing)
        {
            if (isOutgoing)
            {
                var msg = new OutgoingMsg(text, DateTime.Now.ToString("HH:mm"));
                msg.Anchor = AnchorStyles.Right;
                msg.Dock = DockStyle.None;
                int marginLeft = flowLayoutPanelChat.Width - msg.Width - 30;
                msg.Margin = new Padding(marginLeft, 5, 10, 5);
                flowLayoutPanelChat.Controls.Add(msg);
            }
            else
            {
                var msg = new IncomingMsg(text, DateTime.Now.ToString("HH:mm"));
                msg.Margin = new Padding(10, 5, 0, 5);
                flowLayoutPanelChat.Controls.Add(msg);
            }
            flowLayoutPanelChat.ScrollControlIntoView(flowLayoutPanelChat.Controls[flowLayoutPanelChat.Controls.Count - 1]);
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower().Trim();

            // Nếu bạn đang dùng Placeholder, hãy kiểm tra để tránh lọc nhầm
            if (keyword == "tìm kiếm...") return;

            // Duyệt qua tất cả các ContactItem trong FlowLayoutPanel
            foreach (Control control in flowLayoutPanelContacts.Controls)
            {
                if (control is ContactItem item)
                {
                    // Lấy tên của người bạn (dùng hàm GetContactName bạn đã viết)
                    string contactName = item.GetContactName().ToLower();

                    // Nếu tên chứa từ khóa thì hiện, ngược lại thì ẩn
                    if (contactName.Contains(keyword))
                    {
                        item.Visible = true;
                    }
                    else
                    {
                        item.Visible = false;
                    }
                }
            }
        }
        // Khi click vào ô tìm kiếm
        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Tìm kiếm...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        // Khi click ra ngoài mà không nhập gì
        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Tìm kiếm...";
                txtSearch.ForeColor = Color.Silver;
            }
        }
        // Khai báo hàm hệ thống để gửi tin nhắn cho TextBox
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501; // Mã lệnh tạo placeholder

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ShowSettingsInTab()
        {
            // 1. Khởi tạo Form Settings
            UCSettings fSettings = new UCSettings();

            // 2. Cấu hình để Form có thể bị nhúng
            fSettings.TopLevel = false;          // QUAN TRỌNG: Tắt quyền "cấp cao" để làm con của control khác
            fSettings.FormBorderStyle = FormBorderStyle.None; // Xóa viền, thanh tiêu đề của Form Settings
            fSettings.Dock = DockStyle.Fill;     // Để nó lấp đầy tabPage2

            // 3. Thêm vào tabPage2
            tabPage2.Controls.Clear();           // Xóa các rác cũ nếu có
            tabPage2.Controls.Add(fSettings);    // Đưa Form Settings vào

            // 4. Hiển thị
            fSettings.Show();

            // 5. Chuyển Tab sang trang Settings
            tabControl1.SelectedTab = tabPage2;
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            UCSettings f = new UCSettings();
            f.TopLevel = false;
            f.Dock = DockStyle.Fill;
            f.FormBorderStyle = FormBorderStyle.None;

            // Gán việc cho hành động QuayVeTrangChu
            f.QuayVeTrangChu = () => {
                tabControl1.SelectedIndex = 0;
            };

            tabPage2.Controls.Clear();
            tabPage2.Controls.Add(f);
            f.Show();
            tabControl1.SelectedIndex = 1;
        }
    }
}
