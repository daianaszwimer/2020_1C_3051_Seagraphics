using System;
using System.Collections.Generic;
using System.Linq;

namespace TGC.Group.Model.Crafting
{
    class Cuchillo : Crafting // Ejemplo de un crafteo posible
    {

        private Dictionary<ElementoRecolectable, int> Composicion;

        private bool estoyHabilitado;
        private bool estoyCrafteado;
        private string path;
        private bool reutilizable;
        private int danio;

        public Cuchillo()
        {
            this.path = "\\Items\\cuchillo_bnw.png";
            this.Composicion = new Dictionary<ElementoRecolectable, int>();
            this.Composicion.Add(ElementoRecolectable.coral, 3);
            this.Composicion.Add(ElementoRecolectable.oro, 2);
            this.danio = 10;
            this.reutilizable = false;
            this.estoyHabilitado = false;
            this.estoyCrafteado = false;
        }

        public ElementoCraftreable Tipo()
        {
            return ElementoCraftreable.cuchilllo;
        }

        public bool PuedeCraftear()
        {
            return !estoyHabilitado && Composicion.All(Elemento => Inventory.Instance().cuantosTenesDe(Elemento.Key) >= Elemento.Value);
        }

        public bool EstoyHabilitado()
        {
            return this.estoyHabilitado;
        }

        public void ActivarCrafteo()
        {
            this.path = "\\Items\\cuchillo.png";
            this.estoyHabilitado = true;
        }

        public void Craftear()
        {
            if (!estoyCrafteado && estoyHabilitado)
            {
                Console.WriteLine("\n\nCrafteado!");
                estoyCrafteado = true;
                Inventory.Instance().DisminuirUnidadesItem(ElementoRecolectable.coral, 3);
                Inventory.Instance().DisminuirUnidadesItem(ElementoRecolectable.oro, 2);
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
            Player.Instance().enfrentarTiburon();
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

        public int cuantoDanioHago()
        {
            return danio;
        }

        public void Deshabilitar()
        {
            this.estoyHabilitado = false;
            this.path = "\\Items\\cuchillo_bnw.png";
        }
    }
}
