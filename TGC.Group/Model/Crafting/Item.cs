using System;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model
{
    class Item
    {
        // Indico que tipo de elemento recolectable es
        private ElementoRecolectable name;

        // Indico la cantidad que recolecte de ese tipo
        private int amount;

        public string obtenerImagen() { return "\\Items\\" + (int)name + ".png"; }

        // Creo un nuevo item cada vez que recolecte un elemento (lo hace el player)
        public Item(ElementoRecolectable name, int amount) { this.name = name; this.amount = amount; }

        // Disminuyo las unidades del tipo de elemento que tengo
        public void Take(int amount)
        {
            if (amount > this.amount)
                this.amount = 0;
            this.amount -= amount;
        }

        // Agrego unidades
        public void Add(int amount) { this.amount += amount; }

        // Me fijo si un item es igual a mi
        public bool IsSameItem(Item item) { return this.name == item.name; }

        // Averiguo si todavia tengo unidades
        public bool NoAmountLeft() { return amount == 0; }

        // Indico las unidades que tengo
        public int Amount() { return amount; }

        // Indico que tipo de item soy
        public ElementoRecolectable tipoDeElemento() { return this.name; }

        //Esto se borrara cuando el inventario pueda verse
        public String mostrarItem()
        {
            return "item: " + name + ", amount: " + amount.ToString() + "\n";
        }
    }
}
