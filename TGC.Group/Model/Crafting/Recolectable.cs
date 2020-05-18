using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.Crafting
{
    // Para definir que pueden hacer todos los elementos que podran ser recolectados por el Player
    class Recolectable
    {
        Inventory inventory;

        public Recolectable(Inventory inventory)
        {
            this.inventory = inventory;
        }

        public void Recolectar(ElementoRecolectable name, int amount)
        {
            Item item = new Item(name,amount);
            inventory.Add(item);
        }

    }

    // Defino todos los elementos que seran recolectables
    public enum ElementoRecolectable
    {
        coral,
        madera,
        hierro,
        bronce,
        oro,
        fish
        // se pueden agregar mas
    }
}
