namespace ProyectoSO_Forms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Codigo generado por el Disenador de Windows Forms

        private void InitializeComponent()
        {
            this.lblWelcome = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.lblSkin = new System.Windows.Forms.Label();
            this.numericSkin = new System.Windows.Forms.NumericUpDown();
            this.btnUpdateSkin = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPlayersCount = new System.Windows.Forms.Label();
            this.listPlayers = new System.Windows.Forms.ListBox();
            this.groupBoxRooms = new System.Windows.Forms.GroupBox();
            this.lblChat = new System.Windows.Forms.Label();
            this.txtChatHistory = new System.Windows.Forms.RichTextBox();
            this.txtChatInput = new System.Windows.Forms.TextBox();
            this.btnSendChat = new System.Windows.Forms.Button();
            this.panelDivider = new System.Windows.Forms.Panel();
            this.groupBoxParchis = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericSkin)).BeginInit();
            this.SuspendLayout();
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.Location = new System.Drawing.Point(10, 12);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(102, 15);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Logged in as...";
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(40)))));
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(278, 8);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(100, 24);
            this.btnLogout.TabIndex = 1;
            this.btnLogout.Text = "Log Out";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // lblSkin
            // 
            this.lblSkin.AutoSize = true;
            this.lblSkin.Location = new System.Drawing.Point(10, 40);
            this.lblSkin.Name = "lblSkin";
            this.lblSkin.Size = new System.Drawing.Size(45, 13);
            this.lblSkin.TabIndex = 2;
            this.lblSkin.Text = "Skin ID:";
            // 
            // numericSkin
            // 
            this.numericSkin.Location = new System.Drawing.Point(65, 38);
            this.numericSkin.Maximum = new decimal(new int[] {
            106,
            0,
            0,
            0});
            this.numericSkin.Minimum = new decimal(new int[] {
            101,
            0,
            0,
            0});
            this.numericSkin.Name = "numericSkin";
            this.numericSkin.Size = new System.Drawing.Size(70, 20);
            this.numericSkin.TabIndex = 3;
            this.numericSkin.Value = new decimal(new int[] {
            101,
            0,
            0,
            0});
            // 
            // btnUpdateSkin
            // 
            this.btnUpdateSkin.Location = new System.Drawing.Point(145, 36);
            this.btnUpdateSkin.Name = "btnUpdateSkin";
            this.btnUpdateSkin.Size = new System.Drawing.Size(100, 24);
            this.btnUpdateSkin.TabIndex = 4;
            this.btnUpdateSkin.Text = "Update Skin";
            this.btnUpdateSkin.UseVisualStyleBackColor = true;
            this.btnUpdateSkin.Click += new System.EventHandler(this.btnUpdateSkin_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(10, 66);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // lblPlayersCount
            // 
            this.lblPlayersCount.AutoSize = true;
            this.lblPlayersCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPlayersCount.Location = new System.Drawing.Point(10, 88);
            this.lblPlayersCount.Name = "lblPlayersCount";
            this.lblPlayersCount.Size = new System.Drawing.Size(170, 15);
            this.lblPlayersCount.TabIndex = 6;
            this.lblPlayersCount.Text = "Connected Players: 0 / 64";
            // 
            // listPlayers
            // 
            this.listPlayers.FormattingEnabled = true;
            this.listPlayers.Location = new System.Drawing.Point(10, 106);
            this.listPlayers.Name = "listPlayers";
            this.listPlayers.Size = new System.Drawing.Size(368, 69);
            this.listPlayers.TabIndex = 7;
            // 
            // groupBoxRooms
            // 
            this.groupBoxRooms.Location = new System.Drawing.Point(5, 182);
            this.groupBoxRooms.Name = "groupBoxRooms";
            this.groupBoxRooms.Size = new System.Drawing.Size(383, 280);
            this.groupBoxRooms.TabIndex = 8;
            this.groupBoxRooms.TabStop = false;
            this.groupBoxRooms.Text = "Rooms";
            // 
            // lblChat
            // 
            this.lblChat.AutoSize = true;
            this.lblChat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblChat.Location = new System.Drawing.Point(10, 468);
            this.lblChat.Name = "lblChat";
            this.lblChat.Size = new System.Drawing.Size(40, 15);
            this.lblChat.TabIndex = 9;
            this.lblChat.Text = "Chat:";
            // 
            // txtChatHistory
            // 
            this.txtChatHistory.BackColor = System.Drawing.SystemColors.Window;
            this.txtChatHistory.Location = new System.Drawing.Point(10, 486);
            this.txtChatHistory.Name = "txtChatHistory";
            this.txtChatHistory.ReadOnly = true;
            this.txtChatHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtChatHistory.Size = new System.Drawing.Size(368, 185);
            this.txtChatHistory.TabIndex = 10;
            this.txtChatHistory.Text = "";
            // 
            // txtChatInput
            // 
            this.txtChatInput.Location = new System.Drawing.Point(10, 678);
            this.txtChatInput.MaxLength = 100;
            this.txtChatInput.Name = "txtChatInput";
            this.txtChatInput.Size = new System.Drawing.Size(278, 20);
            this.txtChatInput.TabIndex = 11;
            this.txtChatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtChatInput_KeyDown);
            // 
            // btnSendChat
            // 
            this.btnSendChat.Location = new System.Drawing.Point(295, 676);
            this.btnSendChat.Name = "btnSendChat";
            this.btnSendChat.Size = new System.Drawing.Size(83, 24);
            this.btnSendChat.TabIndex = 12;
            this.btnSendChat.Text = "Send";
            this.btnSendChat.UseVisualStyleBackColor = true;
            this.btnSendChat.Click += new System.EventHandler(this.btnSendChat_Click);
            // 
            // panelDivider
            // 
            this.panelDivider.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panelDivider.Location = new System.Drawing.Point(392, 5);
            this.panelDivider.Name = "panelDivider";
            this.panelDivider.Size = new System.Drawing.Size(1, 702);
            this.panelDivider.TabIndex = 13;
            // 
            // groupBoxParchis
            // 
            this.groupBoxParchis.Location = new System.Drawing.Point(397, 5);
            this.groupBoxParchis.Name = "groupBoxParchis";
            this.groupBoxParchis.Size = new System.Drawing.Size(748, 702);
            this.groupBoxParchis.TabIndex = 14;
            this.groupBoxParchis.TabStop = false;
            this.groupBoxParchis.Text = "Parchis";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 715);
            this.Controls.Add(this.groupBoxParchis);
            this.Controls.Add(this.panelDivider);
            this.Controls.Add(this.btnSendChat);
            this.Controls.Add(this.txtChatInput);
            this.Controls.Add(this.txtChatHistory);
            this.Controls.Add(this.lblChat);
            this.Controls.Add(this.groupBoxRooms);
            this.Controls.Add(this.listPlayers);
            this.Controls.Add(this.lblPlayersCount);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUpdateSkin);
            this.Controls.Add(this.numericSkin);
            this.Controls.Add(this.lblSkin);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.lblWelcome);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Matar el Rato - Main";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericSkin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label         lblWelcome;
        private System.Windows.Forms.Button        btnLogout;
        private System.Windows.Forms.Label         lblSkin;
        private System.Windows.Forms.NumericUpDown numericSkin;
        private System.Windows.Forms.Button        btnUpdateSkin;
        private System.Windows.Forms.Label         lblStatus;
        private System.Windows.Forms.Label         lblPlayersCount;
        private System.Windows.Forms.ListBox       listPlayers;
        private System.Windows.Forms.GroupBox      groupBoxRooms;
        private System.Windows.Forms.Label         lblChat;
        private System.Windows.Forms.RichTextBox   txtChatHistory;
        private System.Windows.Forms.TextBox       txtChatInput;
        private System.Windows.Forms.Button        btnSendChat;
        private System.Windows.Forms.Panel         panelDivider;
        private System.Windows.Forms.GroupBox      groupBoxParchis;

        #endregion
    }
}
