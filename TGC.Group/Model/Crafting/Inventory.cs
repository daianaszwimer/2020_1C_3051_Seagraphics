using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC.Group.Model.Crafting
{
    class Inventory
    {
        private List<Item> Recolectados; // Almaceno todo los elementos que va recolectando el Player
        private List<Crafting> PosiblesCrafteos; // Almaceno los crafteos existentes
        private static Inventory _instance = null;

        protected Inventory()

        {
            // El inventario comienza vacio
            //Se iran guardan/actualizando las cantidades de los items recolectados
            this.Recolectados = new List<Item>();

            // Defino todos los crafteos que pueden crear para que el inventario sepa cuales son
            this.PosiblesCrafteos = new List<Crafting>();

            // Agrego uno de los crafteos posibles el cuchillo
            //Faltan crear mas crafteos
            Cuchillo cuchillo = new Cuchillo();
            PosiblesCrafteos.Add(cuchillo);
            Botiquin botiquin = new Botiquin();
            PosiblesCrafteos.Add(botiquin);

        }

        public static Inventory Instance()
        {
            if (_instance == null)
            {
                _instance = new Inventory();
            }

            return _instance;
        }

        // Funcion para que alguien externo agreguen un item al inventario
        public void Add(Item item)
        {

            // Averiguo si el elemento capturado por el Player existe en el inventario
            Item inventoryItem = Recolectados.Find(i => ItemMatches(i, item));

            // Incremento la cantidad si el elemento ya esta en el inventario
            if (inventoryItem != null)
            {
                inventoryItem.Add(item.Amount());
            }
            else
            {
                Recolectados.Add(item);
            }

            // Averiguo si tengo los elementos suficientes para craftear
            PosiblesCrafteos.ForEach(crafteo => HabilitarCrafteos(crafteo));
        }

        private void HabilitarCrafteos(Crafting crafteo)
        {
            // Si el crafteo ya se puede llevar a cabo porque tengo las cantidades necesarias habilito el crafteo
            /*if (crafteo.PuedeCraftear() && !crafteo.EstoyCrafteado())
            {
                crafteo.ActivarCrafteo();
            }*/

            if (!crafteo.EstoyCrafteado())
            {
                if (crafteo.PuedeCraftear())
                {
                    crafteo.ActivarCrafteo();
                }
                else
                {
                    crafteo.Deshabilitar();
                }
            }

        }

        //Permite eliminar un item del inventario o disminuir la cantidad de unidades del mismo
        public void DisminuirUnidadesItem(ElementoRecolectable tipo, int cantidad)
        {
            Item item = Recolectados.Find(ItemInventory => ItemInventory.tipoDeElemento() == tipo);
            if (item != null)
            {
                if (item.Amount() == cantidad)
                {
                    Recolectados.Remove(item);
                }
                else
                {
                    item.Take(cantidad);
                }
            }
        }

        // Permite que se use el crafteo, le da la habilidad al player y si es un crafteo reutilizable permite que se vuelve a crear
        public void UsarCrafteo(Crafting Crafteo)
        {
            if (Crafteo.SoyReutilizable())
            {
                Crafteo.DarHabilidadAPlayer();
                Crafting CrafteoDisponible = PosiblesCrafteos.Find(CrafteoPosible => CrafteoPosible.Tipo() == Crafteo.Tipo());
                CrafteoDisponible.Reutilizar();
            }
            else
            {
                Crafteo.DarHabilidadAPlayer();
                Crafteo.Deshabilitar();
            }
            PosiblesCrafteos.ForEach(crafteo => HabilitarCrafteos(crafteo));
        }

        // Devuelve una lista con los crafteos habilitados
        public List<Crafting> CrafteosDisponibles()
        {
            List<Crafting> Disponibles = PosiblesCrafteos.FindAll(Crafteo => Crafteo.EstoyHabilitado());
            return Disponibles;
        }

        // Me fijo si dos items son iguales
        private bool ItemMatches(Item inventoryItem, Item item) { return inventoryItem.IsSameItem(item); }

        // Sirve para averiguar cuantos elementos tiene un tipo especifico
        public int cuantosTenesDe(ElementoRecolectable elemento)
        {
            Item item = Recolectados.Find(x => x.tipoDeElemento() == elemento);
            if (item != null)
                return item.Amount();
            else
                return 0;
        }

        // Devuelve los items que se encuentran en el inventario
        public List<Item> GetItems() { return Recolectados; }

        // Devuelve todos los crafteos que existen aunque no esten habilitados
        public List<Crafting> GetCraftings() { return PosiblesCrafteos; }
    }
}
