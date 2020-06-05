﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model.Crafting
{
    class Cuchillo : Crafting // Ejemplo de un crafteo posible
    {
        // Determino que elementos y la cantidad que se necesita para crear un cuchillo
        private Dictionary<ElementoRecolectable, int> Composicion;

        // Me indica que es lo que voy a craftear
        public ElementoCraftreable Tipo() { return ElementoCraftreable.cuchilllo; }

        // Activa el uso del crafteo en la pantalla del inventario
        public void activarCrafteo()
        { 
            /*habilita el cuchillo en la pantalla para que el player pueda usarlo*/
        }

        // Defino lo necesario para crear un cuchillo
        public Cuchillo()
        {
            this.Composicion = new Dictionary<ElementoRecolectable, int>();
            this.Composicion.Add(ElementoRecolectable.coral,300);
            this.Composicion.Add(ElementoRecolectable.oro,150);
        }

        // Se fija que el inventario tenga las cantidades y tipos de elementos suficientes para crear un cuchillo
        public bool PuedeCraftear(Inventory inventory)
        {
            return Composicion.All(Elemento => inventory.cuantosTenesDe(Elemento.Key) >= Elemento.Value);
        }

        public void Craftear()
        {
            /*sacarle al inventario los items consumidos
             desactivar el crafteo para que no pueda craftear un cuchillo nuevamente
             */
        }

        // Sirve para agregarle elementos y cantidades al crafteo
        public void AgregarMateriales(ElementoRecolectable elemento, int cantidad)
        {
            Composicion.Add(elemento, cantidad);
        }
    }
}