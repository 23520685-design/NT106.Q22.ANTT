namespace New
{
    partial class ContactItem
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            picAvatar = new PictureBox();
            lblName = new Label();
            lblAddress = new Label();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new Point(3, 30);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(60, 60);
            picAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblName.Location = new Point(80, 30);
            lblName.Name = "lblName";
            lblName.Size = new Size(55, 20);
            lblName.TabIndex = 1;
            lblName.Text = "label1";
            // 
            // lblAddress
            // 
            lblAddress.AutoSize = true;
            lblAddress.ForeColor = SystemColors.GrayText;
            lblAddress.Location = new Point(80, 65);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(59, 25);
            lblAddress.TabIndex = 2;
            lblAddress.Text = "label1";
            // 
            // ContactItem
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblAddress);
            Controls.Add(lblName);
            Controls.Add(picAvatar);
            Name = "ContactItem";
            Size = new Size(302, 109);
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Label lblName;
        private Label lblAddress;
    }
}
