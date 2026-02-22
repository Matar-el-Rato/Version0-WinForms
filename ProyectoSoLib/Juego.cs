using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    public class Juego
    {
        private List<Jugador> jugadores = new List<Jugador>();
        private Tablero tablero = new Tablero();
        private int turnoActual = 0;
        private List<string> protocolo = new List<string>();

        public Juego(List<string> nombres, List<string> colores)
        {
            for (int i = 0; i < nombres.Count; i++)
            {
                Jugador j = new Jugador((i + 1).ToString(), nombres[i], colores[i]);
                jugadores.Add(j);
            }
        }

        public Jugador GetJugadorActual()
        {
            return jugadores[turnoActual];
        }

        public List<Jugador> GetJugadores()
        {
            return jugadores;
        }

        public List<string> GetProtocolo()
        {
            return protocolo;
        }

        public string MoverFicha(int move)
        {
            Jugador j = GetJugadorActual();
            List<Ficha> misFichas = j.GetFichas();
            Ficha f = misFichas[0]; // Solo una ficha por jugador

            int posActual = f.GetPosicionActual().GetIdCasilla();
            int nextPosId = tablero.GetNextPosition(posActual, move);

            if (nextPosId == -1)
            {
                string msgError = j.GetNombre() + " (" + j.GetColor() + "): MOVIMIENTO INVÁLIDO (se pasa de 10)";
                protocolo.Add(msgError);
                return "INVALID";
            }

            Casilla nextCasilla = tablero.GetCasilla(nextPosId);
            
            // Log del protocolo
            string log = j.GetNombre() + " (" + j.GetColor() + "): movido de " + posActual + " a " + nextPosId;
            protocolo.Add(log);

            // Comprobar capturas (comer ficha)
            foreach (Jugador otroJugador in jugadores)
            {
                if (otroJugador != j)
                {
                    Ficha otraFicha = otroJugador.GetFichas()[0];
                    if (otraFicha.GetPosicionActual().GetIdCasilla() == nextPosId)
                    {
                        if (nextPosId > 0 && nextPosId < 10)
                        {
                            otraFicha.SetPosicionActual(tablero.GetCasilla(0));
                            otraFicha.SetEstado("base");
                            j.AddPuntuacion(50);
                            protocolo.Add("CAPTURADA: " + otroJugador.GetNombre() + " vuelve a inicio");
                        }
                    }
                }
            }

            f.SetPosicionActual(nextCasilla);
            if (nextPosId == 10)
            {
                f.SetEstado("meta");
                j.AddPuntuacion(100);
            }
            else
            {
                f.SetEstado("tablero");
            }

            return "OK";
        }

        public void SiguienteTurno()
        {
            turnoActual = (turnoActual + 1) % jugadores.Count;
        }

        public bool HaTerminado()
        {
            foreach (Jugador j in jugadores)
            {
                if (j.GetFichasEnMeta() > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
