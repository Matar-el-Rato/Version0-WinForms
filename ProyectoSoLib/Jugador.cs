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
        string id;
        string color;
        List<Ficha> fichas = new List<Ficha>();
        int fichasEnBase;
        int fichasEnMeta;
        Casilla casillaSalida;
        Casilla 


    }
}
