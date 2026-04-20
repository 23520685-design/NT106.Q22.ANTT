namespace New
{
    partial class IncomingMsg
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
            lblMsg = new Label();
            lblTime = new Label();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new Point(217, 148);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(40, 40);
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            // 
            // lblMsg
            // 
            lblMsg.BackColor = Color.LightGray;
            lblMsg.Location = new Point(280, 150);
            lblMsg.Name = "lblMsg";
            lblMsg.Size = new Size(88, 38);
            lblMsg.TabIndex = 1;
            lblMsg.Text = "label1";
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Location = new Point(280, 207);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(59, 25);
            lblTime.TabIndex = 2;
            lblTime.Text = "label1";
            // 
            // IncomingMsg
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblTime);
            Controls.Add(lblMsg);
            Controls.Add(picAvatar);
            Name = "IncomingMsg";
            Size = new Size(800, 450);
            Load += IncomingMsg_Load;
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Label lblMsg;
        private Label lblTime;
    }
}