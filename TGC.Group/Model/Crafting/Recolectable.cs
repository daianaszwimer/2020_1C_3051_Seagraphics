using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.Crafting
{
    // Para definir que pueden hacer todos los elementos que podran ser recolectados por el Player
    interface IRecolectable
    {
    }

    // Defino todos los elementos que seran recolectables
    public enum ElementoRecolectable
    {
        coral,
        madera,
        hierro,
        bronce,
        pez
        // se pueden agregar mas
    }
}
