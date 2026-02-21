using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSoLib
{
    public class Dado
    {
        private static Random random = new Random();

        public static (int, int) LanzarDados()
        {
            return (random.Next(1, 7), random.Next(1, 7));
        }
    }
}
