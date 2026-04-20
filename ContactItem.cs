namespace New
{
    // Thuộc tính quan trọng: Size(250, 80), Margin(0, 0, 0, 5)
    public partial class ContactItem : UserControl
    {
        public ContactItem()
        {
            InitializeComponent();

            // Tạo danh sách các thành phần cần hứng sự kiện click
            Control[] controls = { this, lblName, lblAddress, picAvatar };

            foreach (var control in controls)
            {
                if (control != null)
                {
                    control.Click += (s, e) => {
                        // Khi click vào bất cứ đâu, gọi sự kiện ItemClick của mình
                        ItemClick?.Invoke(this, e);
                    };
                }
            }
        }
        public ContactItem(string name, string address, Image avatar) : this()
        {
            lblName.Text = name;
            lblAddress.Text = address;
            picAvatar.Image = avatar;

            // Bo tròn ảnh (Gắn vào sự kiện Paint của PictureBox)
            picAvatar.Paint += (s, e) =>
            {
                using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    gp.AddEllipse(0, 0, picAvatar.Width - 1, picAvatar.Height - 1);
                    picAvatar.Region = new Region(gp);
                }
            };
        }

        // Hiệu ứng hover để biết đang chọn
        protected override void OnMouseEnter(EventArgs e)
        {
            this.BackColor = Color.FromArgb(230, 230, 230);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            this.BackColor = Color.White;
        }

        public event EventHandler ItemClick;

        // Hàm để lấy tên người dùng từ ContactItem
        public string GetContactName()
        {
            return lblName.Text;
        }
    }
}