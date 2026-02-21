using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    public class Ficha
    {
        private string color; //se indicará el color de la ficha
        private string id; //para saber que ficha es
        private string estado; //indica si esta en base, meta o tablero
        private Casilla posicionActual;

        public Ficha(string color, string id)
        {
            this.color = color;
            this.id = id;
            this.estado = "base";
            this.posicionActual = new Casilla();
        }

        public string GetColor() { return this.color; }
        public string GetId() { return this.id; } 
        public string GetEstado() { return this.estado; }
        public void SetEstado(string estado) { this.estado = estado; }
        public Casilla GetPosicionActual() {  return this.posicionActual; }
        public void SetPosicionActual(Casilla casilla) { this.posicionActual = casilla; }
        


    }
}
