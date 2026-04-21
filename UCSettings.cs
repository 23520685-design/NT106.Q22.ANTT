using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace New
{
    public partial class UCSettings : Form
    {
        public UCSettings()
        {
            InitializeComponent();
        }


        public Action QuayVeTrangChu; // Khai báo một hành động

        private void btnBack_Click(object sender, EventArgs e)
        {
            QuayVeTrangChu?.Invoke(); // Gọi hành động này khi bấm nút
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Thêm dòng này để kiểm tra xem nút đã chạy vào đây chưa
            // MessageBox.Show("Nut da duoc bam!"); 

            DialogResult result = MessageBox.Show("Bạn có muốn đăng xuất không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Thoát ứng dụng
                Application.Exit();
            }
        }
    }
}
