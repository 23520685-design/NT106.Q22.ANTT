namespace New
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel3 = new Panel();
            btnSend = new Button();
            txtInput = new TextBox();
            pnlChatHeader = new Panel();
            lblChatName = new Label();
            flowLayoutPanelChat = new FlowLayoutPanel();
            panel1 = new Panel();
            txtSearch = new TextBox();
            flowLayoutPanelContacts = new FlowLayoutPanel();
            contactItem2 = new ContactItem();
            contactItem1 = new ContactItem();
            contactItem4 = new ContactItem();
            contactItem3 = new ContactItem();
            panel2 = new Panel();
            btnSettings = new Button();
            pnlSidebarContainer = new Panel();
            pnlContactsContainer = new Panel();
            pnlChatContainer = new Panel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            panel3.SuspendLayout();
            pnlChatHeader.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanelContacts.SuspendLayout();
            panel2.SuspendLayout();
            pnlSidebarContainer.SuspendLayout();
            pnlContactsContainer.SuspendLayout();
            pnlChatContainer.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // panel3
            // 
            panel3.Controls.Add(btnSend);
            panel3.Controls.Add(txtInput);
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 510);
            panel3.Name = "panel3";
            panel3.Size = new Size(746, 73);
            panel3.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(472, 27);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(112, 34);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtInput
            // 
            txtInput.Location = new Point(25, 29);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(397, 31);
            txtInput.TabIndex = 1;
            txtInput.TextChanged += textBox1_TextChanged;
            // 
            // pnlChatHeader
            // 
            pnlChatHeader.Controls.Add(lblChatName);
            pnlChatHeader.Dock = DockStyle.Top;
            pnlChatHeader.Location = new Point(0, 0);
            pnlChatHeader.Name = "pnlChatHeader";
            pnlChatHeader.Size = new Size(746, 68);
            pnlChatHeader.TabIndex = 2;
            // 
            // lblChatName
            // 
            lblChatName.AutoSize = true;
            lblChatName.Font = new Font("Segoe UI", 15F);
            lblChatName.ForeColor = SystemColors.Highlight;
            lblChatName.Location = new Point(101, 9);
            lblChatName.Name = "lblChatName";
            lblChatName.Size = new Size(0, 41);
            lblChatName.TabIndex = 0;
            // 
            // flowLayoutPanelChat
            // 
            flowLayoutPanelChat.AutoScroll = true;
            flowLayoutPanelChat.Dock = DockStyle.Fill;
            flowLayoutPanelChat.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelChat.Location = new Point(0, 68);
            flowLayoutPanelChat.Name = "flowLayoutPanelChat";
            flowLayoutPanelChat.Size = new Size(746, 442);
            flowLayoutPanelChat.TabIndex = 0;
            flowLayoutPanelChat.WrapContents = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(flowLayoutPanelContacts);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(250, 583);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(86, 3);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(140, 31);
            txtSearch.TabIndex = 3;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // flowLayoutPanelContacts
            // 
            flowLayoutPanelContacts.Controls.Add(contactItem2);
            flowLayoutPanelContacts.Controls.Add(contactItem1);
            flowLayoutPanelContacts.Controls.Add(contactItem4);
            flowLayoutPanelContacts.Controls.Add(contactItem3);
            flowLayoutPanelContacts.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelContacts.Location = new Point(3, 46);
            flowLayoutPanelContacts.Name = "flowLayoutPanelContacts";
            flowLayoutPanelContacts.Size = new Size(247, 549);
            flowLayoutPanelContacts.TabIndex = 0;
            flowLayoutPanelContacts.Paint += flowLayoutPanel1_Paint;
            // 
            // contactItem2
            // 
            contactItem2.Location = new Point(3, 3);
            contactItem2.Name = "contactItem2";
            contactItem2.Size = new Size(241, 90);
            contactItem2.TabIndex = 2;
            // 
            // contactItem1
            // 
            contactItem1.Location = new Point(3, 99);
            contactItem1.Name = "contactItem1";
            contactItem1.Size = new Size(241, 91);
            contactItem1.TabIndex = 1;
            // 
            // contactItem4
            // 
            contactItem4.Location = new Point(3, 196);
            contactItem4.Name = "contactItem4";
            contactItem4.Size = new Size(241, 82);
            contactItem4.TabIndex = 4;
            // 
            // contactItem3
            // 
            contactItem3.Location = new Point(3, 284);
            contactItem3.Name = "contactItem3";
            contactItem3.Size = new Size(244, 90);
            contactItem3.TabIndex = 3;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveBorder;
            panel2.Controls.Add(btnSettings);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(60, 583);
            panel2.TabIndex = 4;
            panel2.Paint += panel2_Paint;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = SystemColors.ButtonFace;
            btnSettings.BackgroundImageLayout = ImageLayout.Zoom;
            btnSettings.Dock = DockStyle.Bottom;
            btnSettings.ForeColor = SystemColors.ButtonHighlight;
            btnSettings.Image = (Image)resources.GetObject("btnSettings.Image");
            btnSettings.Location = new Point(0, 510);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(60, 73);
            btnSettings.TabIndex = 0;
            btnSettings.TextImageRelation = TextImageRelation.ImageAboveText;
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // pnlSidebarContainer
            // 
            pnlSidebarContainer.Controls.Add(panel2);
            pnlSidebarContainer.Dock = DockStyle.Left;
            pnlSidebarContainer.Location = new Point(3, 3);
            pnlSidebarContainer.Name = "pnlSidebarContainer";
            pnlSidebarContainer.Size = new Size(60, 583);
            pnlSidebarContainer.TabIndex = 3;
            // 
            // pnlContactsContainer
            // 
            pnlContactsContainer.Controls.Add(panel1);
            pnlContactsContainer.Dock = DockStyle.Left;
            pnlContactsContainer.Location = new Point(63, 3);
            pnlContactsContainer.Name = "pnlContactsContainer";
            pnlContactsContainer.Size = new Size(250, 583);
            pnlContactsContainer.TabIndex = 0;
            // 
            // pnlChatContainer
            // 
            pnlChatContainer.Controls.Add(flowLayoutPanelChat);
            pnlChatContainer.Controls.Add(panel3);
            pnlChatContainer.Controls.Add(pnlChatHeader);
            pnlChatContainer.Dock = DockStyle.Fill;
            pnlChatContainer.Location = new Point(313, 3);
            pnlChatContainer.Name = "pnlChatContainer";
            pnlChatContainer.Size = new Size(746, 583);
            pnlChatContainer.TabIndex = 0;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1070, 627);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(pnlChatContainer);
            tabPage1.Controls.Add(pnlContactsContainer);
            tabPage1.Controls.Add(pnlSidebarContainer);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1062, 589);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Main";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1062, 589);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Settings";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1070, 627);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            pnlChatHeader.ResumeLayout(false);
            pnlChatHeader.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            flowLayoutPanelContacts.ResumeLayout(false);
            panel2.ResumeLayout(false);
            pnlSidebarContainer.ResumeLayout(false);
            pnlContactsContainer.ResumeLayout(false);
            pnlChatContainer.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel3;
        private Button btnSend;
        private TextBox txtInput;
        private Panel pnlChatHeader;
        private Label lblChatName;
        private FlowLayoutPanel flowLayoutPanelChat;
        private Panel panel1;
        private TextBox txtSearch;
        private FlowLayoutPanel flowLayoutPanelContacts;
        private ContactItem contactItem2;
        private ContactItem contactItem1;
        private ContactItem contactItem4;
        private ContactItem contactItem3;
        private Panel panel2;
        private Panel pnlSidebarContainer;
        private Panel pnlContactsContainer;
        private Panel pnlChatContainer;
        private Button btnSettings;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
    }
}