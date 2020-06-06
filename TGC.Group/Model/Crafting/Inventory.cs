using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.Crafting
{
    class Inventory
    {
        private List<Item> inventory; // Almaceno todo los elementos que va recolectando el Player
        private List<Crafting> Crafteos; // Almaceno los crafteos existentes

        public Inventory()
        {
            // El inventario comienza vacio
            this.inventory = new List<Item>();

            // Defino todos los crafteos existentes para que el inventario sepa cuales son
            this.Crafteos = new List<Crafting>();


            // Agrego uno de los crafteos posibles el cuchillo
            Cuchillo cuchillo = new Cuchillo();
            Crafteos.Add(cuchillo);

            //Faltan crear mas crafteos
        }

        public void Add(Item item)
        {
            // Eliminar item de la escena

            // Averiguo si el elemento capturado por el Player existe en el inventario
            Item inventoryItem = inventory.Find(i => ItemMatches(i, item));

            // Incremento la cantidad si el elemento ya esta en el inventario
            if (inventoryItem != null)
                inventoryItem.Add(item.Amount());
            else
                inventory.Add(item);

            // Averiguo si tengo los elementos suficientes para craftear
            Crafteos.ForEach(crafteo => habilitarCrafteos(crafteo));
        }

        // Me fijo si dos items son iguales
        private bool ItemMatches(Item inventoryItem, Item item) { return inventoryItem.IsSameItem(item); }

        // Sirve para averiguar cuantos elementos tiene un tipo especifico
        public int cuantosTenesDe(ElementoRecolectable elemento)
        {
            Item item = inventory.Find(x => x.tipoDeElemento() == elemento);
            if (item != null)
                return item.Amount();
            else
                return 0;
        }

        private void habilitarCrafteos(Crafting crafteo)
        {
            // Si el crafteo ya se puede llevar a cabo porque tengo las cantidades necesarias habilito el crafteo
            if (crafteo.PuedeCraftear(this))
            {
                crafteo.activarCrafteo();
                cuchilloActivado = true;
            }
            // Borrar crafteo de la lista para que no se pueda volver a craftear
        }

        public List<Item> GetItems() { return inventory; }

        public List<Crafting> GetCraftings() { return Crafteos; }

        // Este metodo no se usara cuando este el inventario visualmente
        public String inventoryMostrarItemsRecolectados()
        {
            if(inventory.Count() == 0)
            {
                return "No posee items aun";
            }
            else
            {
                String itemsRecolectados = "Items recolectados:\n";
                inventory.ForEach(item => itemsRecolectados += item.mostrarItem());
                return itemsRecolectados;
            }
        }

        //para probar creafteos despues borrarlo
        private bool cuchilloActivado = false;

        public String inventoryMostrarCrafteos()
        {
            if (!cuchilloActivado)
            {
                return "Cuchillo: no se puede utilizar";
            }
            else
            {
                return "Cuchillo: ya puede utilizarse";
            }
        }
    }
}
