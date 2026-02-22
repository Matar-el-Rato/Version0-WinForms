namespace ProyectoSO_Forms
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPlayer1 = new System.Windows.Forms.TextBox();
            this.txtPlayer2 = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblTurn = new System.Windows.Forms.Label();
            this.btnRoll = new System.Windows.Forms.Button();
            this.lstProtocol = new System.Windows.Forms.ListBox();
            this.lblScores = new System.Windows.Forms.Label();
            this.lblDice = new System.Windows.Forms.Label();
            this.pnlBoard = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // txtPlayer1
            // 
            this.txtPlayer1.Location = new System.Drawing.Point(12, 12);
            this.txtPlayer1.Name = "txtPlayer1";
            this.txtPlayer1.Size = new System.Drawing.Size(100, 20);
            this.txtPlayer1.TabIndex = 0;
            this.txtPlayer1.Text = "Jugador 1";
            // 
            // txtPlayer2
            // 
            this.txtPlayer2.Location = new System.Drawing.Point(12, 38);
            this.txtPlayer2.Name = "txtPlayer2";
            this.txtPlayer2.Size = new System.Drawing.Size(100, 20);
            this.txtPlayer2.TabIndex = 1;
            this.txtPlayer2.Text = "Jugador 2";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(118, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 46);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Empezar";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblTurn
            // 
            this.lblTurn.AutoSize = true;
            this.lblTurn.Location = new System.Drawing.Point(12, 79);
            this.lblTurn.Name = "lblTurn";
            this.lblTurn.Size = new System.Drawing.Size(44, 13);
            this.lblTurn.TabIndex = 3;
            this.lblTurn.Text = "Turno: -";
            // 
            // btnRoll
            // 
            this.btnRoll.Enabled = false;
            this.btnRoll.Location = new System.Drawing.Point(12, 105);
            this.btnRoll.Name = "btnRoll";
            this.btnRoll.Size = new System.Drawing.Size(75, 23);
            this.btnRoll.TabIndex = 4;
            this.btnRoll.Text = "Lanzar Dado";
            this.btnRoll.UseVisualStyleBackColor = true;
            this.btnRoll.Click += new System.EventHandler(this.btnRoll_Click);
            // 
            // lstProtocol
            // 
            this.lstProtocol.FormattingEnabled = true;
            this.lstProtocol.Location = new System.Drawing.Point(550, 12);
            this.lstProtocol.Name = "lstProtocol";
            this.lstProtocol.Size = new System.Drawing.Size(520, 420);
            this.lstProtocol.TabIndex = 7;
            this.lstProtocol.SelectedIndexChanged += new System.EventHandler(this.lstProtocol_SelectedIndexChanged);
            // 
            // lblScores
            // 
            this.lblScores.AutoSize = true;
            this.lblScores.Location = new System.Drawing.Point(12, 400);
            this.lblScores.Name = "lblScores";
            this.lblScores.Size = new System.Drawing.Size(72, 13);
            this.lblScores.TabIndex = 8;
            this.lblScores.Text = "Puntuaciones";
            // 
            // lblDice
            // 
            this.lblDice.AutoSize = true;
            this.lblDice.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDice.Location = new System.Drawing.Point(100, 105);
            this.lblDice.Name = "lblDice";
            this.lblDice.Size = new System.Drawing.Size(19, 20);
            this.lblDice.TabIndex = 9;
            this.lblDice.Text = "0";
            // 
            // pnlBoard
            // 
            this.pnlBoard.Location = new System.Drawing.Point(20, 180);
            this.pnlBoard.Name = "pnlBoard";
            this.pnlBoard.Size = new System.Drawing.Size(520, 150);
            this.pnlBoard.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 450);
            this.Controls.Add(this.pnlBoard);
            this.Controls.Add(this.lblDice);
            this.Controls.Add(this.lblScores);
            this.Controls.Add(this.lstProtocol);
            this.Controls.Add(this.btnRoll);
            this.Controls.Add(this.lblTurn);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtPlayer2);
            this.Controls.Add(this.txtPlayer1);
            this.Name = "Form1";
            this.Text = "Parchís SO - Práctica 0";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtPlayer1;
        private System.Windows.Forms.TextBox txtPlayer2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblTurn;
        private System.Windows.Forms.Button btnRoll;
        private System.Windows.Forms.ListBox lstProtocol;
        private System.Windows.Forms.Label lblScores;
        private System.Windows.Forms.Label lblDice;
        private System.Windows.Forms.Panel pnlBoard;

        #endregion
    }
}

