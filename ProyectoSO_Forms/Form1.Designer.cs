namespace ProyectoSO_Forms
{
    partial class Form1
    {
        /// <summary>
        /// Variable del disenador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codigo generado por el Disenador de Windows Forms

        private void InitializeComponent()
        {
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblSkin = new System.Windows.Forms.Label();
            this.numericSkin = new System.Windows.Forms.NumericUpDown();
            this.btnUpdateSkin = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPlayersCount = new System.Windows.Forms.Label();
            this.listPlayers = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericSkin)).BeginInit();
            this.SuspendLayout();
            //
            // lblWelcome
            //
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWelcome.Location = new System.Drawing.Point(20, 20);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(115, 17);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Logged in as...";
            //
            // lblSkin
            //
            this.lblSkin.AutoSize = true;
            this.lblSkin.Location = new System.Drawing.Point(20, 55);
            this.lblSkin.Name = "lblSkin";
            this.lblSkin.Size = new System.Drawing.Size(47, 13);
            this.lblSkin.TabIndex = 1;
            this.lblSkin.Text = "Skin ID:";
            //
            // numericSkin
            //
            this.numericSkin.Location = new System.Drawing.Point(75, 53);
            this.numericSkin.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericSkin.Name = "numericSkin";
            this.numericSkin.Size = new System.Drawing.Size(80, 20);
            this.numericSkin.TabIndex = 2;
            //
            // btnUpdateSkin
            //
            this.btnUpdateSkin.Location = new System.Drawing.Point(170, 50);
            this.btnUpdateSkin.Name = "btnUpdateSkin";
            this.btnUpdateSkin.Size = new System.Drawing.Size(120, 25);
            this.btnUpdateSkin.TabIndex = 3;
            this.btnUpdateSkin.Text = "Update Skin";
            this.btnUpdateSkin.UseVisualStyleBackColor = true;
            this.btnUpdateSkin.Click += new System.EventHandler(this.btnUpdateSkin_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 85);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status";
            //
            // lblPlayersCount
            //
            this.lblPlayersCount.AutoSize = true;
            this.lblPlayersCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayersCount.Location = new System.Drawing.Point(20, 115);
            this.lblPlayersCount.Name = "lblPlayersCount";
            this.lblPlayersCount.Size = new System.Drawing.Size(165, 15);
            this.lblPlayersCount.TabIndex = 5;
            this.lblPlayersCount.Text = "Connected Players: 0 / 64";
            //
            // listPlayers
            //
            this.listPlayers.FormattingEnabled = true;
            this.listPlayers.Location = new System.Drawing.Point(23, 138);
            this.listPlayers.Name = "listPlayers";
            this.listPlayers.Size = new System.Drawing.Size(374, 186);
            this.listPlayers.TabIndex = 6;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 345);
            this.Controls.Add(this.listPlayers);
            this.Controls.Add(this.lblPlayersCount);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUpdateSkin);
            this.Controls.Add(this.numericSkin);
            this.Controls.Add(this.lblSkin);
            this.Controls.Add(this.lblWelcome);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Matar el Rato - Main";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericSkin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSkin;
        private System.Windows.Forms.NumericUpDown numericSkin;
        private System.Windows.Forms.Button btnUpdateSkin;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPlayersCount;
        private System.Windows.Forms.ListBox listPlayers;

        #endregion
    }
}
