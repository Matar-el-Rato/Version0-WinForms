using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    public class Casilla
    {
        private int id;
        private string tipoCasilla; //normal, seguro, pasillo o meta 
        private string color; //gris(null) , azul, verde, rojo, amarillo

        public Casilla() //constructor de casilla cuando se crea el obejo ficha (estará en la base)
        {
            this.id = 0;
            this.tipoCasilla = "base";
            this.color = null;
        }
        public Casilla(int id, string tipoCasilla, string color)
        {
            this.id = id;
            this.tipoCasilla = tipoCasilla;
            this.color = color; 
        }

        public int GetIdCasilla() {return this.id;}
        public string GetTipoCasilla () {return this.tipoCasilla;}
        public string GetColor () {return this.color;}

        public bool EsSegura()
        {
            return this.tipoCasilla == "Segura" || this.tipoCasilla == "Pasillo" || this.tipoCasilla == "Meta";

            //if (this.tipoCasilla == "Segura" || this.tipoCasilla == "Pasillo" || this.tipoCasilla == "Meta") { return true; }
            //return false;
        }
    }
}
