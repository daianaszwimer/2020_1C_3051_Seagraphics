using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC.Group.Model.Crafting
{
    class Botiquin : Crafting // Ejemplo de un crafteo posible
    {

        private Dictionary<ElementoRecolectable, int> Composicion;

        private bool estoyHabilitado;
        private bool estoyCrafteado;
        private string path;
        private bool reutilizable;
        public Botiquin()
        {
            this.path = "\\Items\\botiquin_bnw.png";
            this.Composicion = new Dictionary<ElementoRecolectable, int>();
            this.Composicion.Add(ElementoRecolectable.fish, 5);
            this.Composicion.Add(ElementoRecolectable.coral,4);
            
            this.reutilizable = true;
            this.estoyHabilitado = false;
            this.estoyCrafteado = false;
        }

        public ElementoCraftreable Tipo()
        {
            return ElementoCraftreable.botiquin;
        }

        public bool PuedeCraftear()
        {
            return Composicion.All(Elemento => Inventory.Instance().cuantosTenesDe(Elemento.Key) >= Elemento.Value);
        }

        public bool EstoyHabilitado()
        {
            return this.estoyHabilitado;
        }

        public void ActivarCrafteo()
        {
            this.path = "\\Items\\botiquin.png";
            this.estoyHabilitado = true;
        }

        public void Craftear()
        {
            if (!estoyCrafteado && estoyHabilitado)
            {
                Console.WriteLine("\n\nCrafteado!");
                estoyCrafteado = true;
                Inventory.Instance().DisminuirUnidadesItem(ElementoRecolectable.fish, 5);
                Inventory.Instance().DisminuirUnidadesItem(ElementoRecolectable.coral, 4);
                Inventory.Instance().UsarCrafteo(this);
                Console.WriteLine(estoyCrafteado);
            }
        }

        public bool EstoyCrafteado()
        {
            return this.estoyCrafteado;
        }

        public void DarHabilidadAPlayer()
        {
            Player.Instance().Curarme();
        }

        public bool SoyReutilizable()
        {
            return reutilizable;
        }

        public void Reutilizar()
        {
            Deshabilitar();
            this.estoyCrafteado = false;
        }

        public string ObtenerIcono()
        {
            return path;
        }
        public void Deshabilitar()
        {
            this.estoyHabilitado = false;
            this.path = "\\Items\\botiquin_bnw.png";
        }
    }
}
