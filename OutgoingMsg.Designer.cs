namespace New
{
    partial class OutgoingMsg
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
            lblMessage = new Label();
            lblTime = new Label();
            SuspendLayout();
            // 
            // lblMessage
            // 
            lblMessage.AutoSize = true;
            lblMessage.Location = new Point(82, 0);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(59, 25);
            lblMessage.TabIndex = 0;
            lblMessage.Text = "label1";
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Location = new Point(82, 45);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(59, 25);
            lblTime.TabIndex = 1;
            lblTime.Text = "label1";
            // 
            // OutgoingMsg
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            Controls.Add(lblTime);
            Controls.Add(lblMessage);
            MaximumSize = new Size(300, 0);
            Name = "OutgoingMsg";
            Size = new Size(141, 0);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblMessage;
        private Label lblTime;
    }
}