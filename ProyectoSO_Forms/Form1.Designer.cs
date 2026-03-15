namespace ProyectoSO_Forms
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
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

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblSkin = new System.Windows.Forms.Label();
            this.numericSkin = new System.Windows.Forms.NumericUpDown();
            this.btnUpdateSkin = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
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
            this.lblSkin.Location = new System.Drawing.Point(20, 60);
            this.lblSkin.Name = "lblSkin";
            this.lblSkin.Size = new System.Drawing.Size(47, 13);
            this.lblSkin.TabIndex = 1;
            this.lblSkin.Text = "Skin ID:";
            // 
            // numericSkin
            // 
            this.numericSkin.Location = new System.Drawing.Point(75, 58);
            this.numericSkin.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericSkin.Name = "numericSkin";
            this.numericSkin.Size = new System.Drawing.Size(80, 20);
            this.numericSkin.TabIndex = 2;
            // 
            // btnUpdateSkin
            // 
            this.btnUpdateSkin.Location = new System.Drawing.Point(170, 55);
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
            this.lblStatus.Location = new System.Drawing.Point(20, 95);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 140);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericSkin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSkin;
        private System.Windows.Forms.NumericUpDown numericSkin;
        private System.Windows.Forms.Button btnUpdateSkin;
        private System.Windows.Forms.Label lblStatus;

        #endregion
    }
}

