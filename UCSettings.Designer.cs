namespace New
{
    partial class UCSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            picAvatar = new PictureBox();
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button2 = new Button();
            btnLogout = new Button();
            btnBack = new Button();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new Point(50, 45);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(112, 88);
            picAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picAvatar.TabIndex = 1;
            picAvatar.TabStop = false;
            // 
            // button1
            // 
            button1.Location = new Point(50, 166);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 2;
            button1.Text = "Edit avatar";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(199, 45);
            label1.Name = "label1";
            label1.Size = new Size(38, 25);
            label1.TabIndex = 3;
            label1.Text = "Ten";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(199, 108);
            label2.Name = "label2";
            label2.Size = new Size(90, 25);
            label2.TabIndex = 4;
            label2.Text = "Nickname";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(395, 39);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(150, 31);
            textBox1.TabIndex = 5;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(395, 108);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(150, 31);
            textBox2.TabIndex = 6;
            // 
            // button2
            // 
            button2.Location = new Point(324, 267);
            button2.Name = "button2";
            button2.Size = new Size(149, 34);
            button2.TabIndex = 7;
            button2.Text = "Doi mat khau";
            button2.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(324, 353);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(149, 34);
            btnLogout.TabIndex = 8;
            btnLogout.Text = "Dang xuat";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(50, 385);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(112, 34);
            btnBack.TabIndex = 9;
            btnBack.Text = "Back";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // UCSettings
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnBack);
            Controls.Add(btnLogout);
            Controls.Add(button2);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(picAvatar);
            Name = "UCSettings";
            Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Button button1;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button2;
        private Button btnLogout;
        private Button btnBack;
    }
}