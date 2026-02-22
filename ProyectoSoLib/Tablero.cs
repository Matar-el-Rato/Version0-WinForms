using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    public class Tablero
    {
        private List<Casilla> casillas = new List<Casilla>();

        public Tablero()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Linear board (1-10)
            for (int i = 0; i <= 10; i++) // 0 is base
            {
                casillas.Add(new Casilla(i, "Normal", null));
            }
        }

        public Casilla GetCasilla(int id)
        {
            if (id >= 0 && id < casillas.Count) return casillas[id];
            return null;
        }

        public int GetNextPosition(int currentPos, int move)
        {
            int nextPos = currentPos + move;
            if (nextPos > 10) return -1; // Cannot move past 10
            return nextPos;
        }
    }
}
