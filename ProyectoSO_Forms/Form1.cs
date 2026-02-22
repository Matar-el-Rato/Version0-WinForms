using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoSO_Forms
{
    public partial class Form1 : Form
    {
        private ProyectoSoLib.Juego juego;
        private Label[] squares = new Label[11];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i <= 10; i++)
            {
                squares[i] = new Label
                {
                    Text = i.ToString(),
                    BorderStyle = BorderStyle.FixedSingle,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(40, 40),
                    Location = new Point(5 + i * 45, 50),
                    BackColor = Color.White
                };
                pnlBoard.Controls.Add(squares[i]);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            List<string> nombres = new List<string> { txtPlayer1.Text, txtPlayer2.Text };
            List<string> colores = new List<string> { "Yellow", "Cyan" };
            juego = new ProyectoSoLib.Juego(nombres, colores);

            btnStart.Enabled = false;
            btnRoll.Enabled = true;
            UpdateUI();
        }

        private void btnRoll_Click(object sender, EventArgs e)
        {
            int dadoValor = ProyectoSoLib.Dado.Lanzar();
            lblDice.Text = dadoValor.ToString();

            string result = juego.MoverFicha(dadoValor);

            if (result == "INVALID")
            {
                // Just log it (already handled in Juego.cs)
                juego.SiguienteTurno();
            }
            else if (result != "OK")
            {
                MessageBox.Show(result);
            }
            else
            {
                if (juego.HaTerminado())
                {
                    UpdateUI();
                    MessageBox.Show("¡Juego Terminado! Ganador: " + juego.GetJugadorActual().GetNombre());
                    this.Close();
                    return;
                }
                juego.SiguienteTurno();
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            var actual = juego.GetJugadorActual();
            lblTurn.Text = $"Turno de: {actual.GetNombre()} ({actual.GetColor()})";

            // Clear squares
            for (int i = 0; i <= 10; i++)
            {
                squares[i].BackColor = Color.White;
                squares[i].Text = i.ToString();
            }

            // Draw players
            foreach (var j in juego.GetJugadores())
            {
                int pos = j.GetFichas()[0].GetPosicionActual().GetIdCasilla();
                squares[pos].BackColor = Color.FromName(j.GetColor());
                squares[pos].Text += "\n" + j.GetNombre().Substring(0, 1);
            }

            lstProtocol.Items.Clear();
            foreach (var log in juego.GetProtocolo())
            {
                lstProtocol.Items.Add(log);
            }
            if (lstProtocol.Items.Count > 0)
                lstProtocol.SelectedIndex = lstProtocol.Items.Count - 1;

            string scores = "";
            foreach (var jugador in juego.GetJugadores())
            {
                scores += $"{jugador.GetNombre()}: {jugador.GetPuntuacion()} pts | ";
            }
            lblScores.Text = scores;
        }

        private void lstProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
