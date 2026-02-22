using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    /*
    Jugador amarillo: 
        Casilla salida -> 5
        Casilla inicio pasillo a meta -> 68

    Jugador azul: 
        Casilla salida -> 22
        Casilla inicio pasillo -> 17

    Jugador rojo:
        Casilla salida -> 39
        Casilla inicio pasillo -> 34

    Jugador azul: 
        Casilla salida -> 56
        Casilla inicio pasillo -> 59

    NUMERACION CASILLAS: 1-68
    CASILLAS PASILLO A META 1(entrada) + 7(camino) + 1 (meta) 
    CASILLAS SEGURAS: 5, 12, 17, 22, 29, 34, 39, 46, 51, 56, 63, 68
    */
    public class Jugador
    {
        private string id;
        private string nombre;
        private string color;
        private List<Ficha> fichas = new List<Ficha>();
        private int puntuacion;

        public Jugador(string id, string nombre, string color)
        {
            this.id = id;
            this.nombre = nombre;
            this.color = color;
            this.puntuacion = 0;
            // Solo creamos 1 ficha para simplificar el juego
            Ficha unaFicha = new Ficha(color, id + "_F1");
            fichas.Add(unaFicha);
        }

        public string GetId()
        {
            return id;
        }

        public string GetNombre()
        {
            return nombre;
        }

        public string GetColor()
        {
            return color;
        }

        public List<Ficha> GetFichas()
        {
            return fichas;
        }

        public int GetPuntuacion()
        {
            return puntuacion;
        }

        public void AddPuntuacion(int puntos)
        {
            puntuacion = puntuacion + puntos;
        }

        public int GetFichasEnMeta()
        {
            int contador = 0;
            foreach (Ficha f in fichas)
            {
                if (f.GetEstado() == "meta")
                {
                    contador = contador + 1;
                }
            }
            return contador;
        }
    }
}
