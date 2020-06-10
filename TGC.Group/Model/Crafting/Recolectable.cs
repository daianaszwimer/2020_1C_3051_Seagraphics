
namespace TGC.Group.Model.Crafting
{
    // Para definir que pueden hacer todos los elementos que podran ser recolectados por el Player
    class Recolectable
    {
        private static Recolectable _instance;

        protected Recolectable()
        {
        }

        public static Recolectable Instance()
        {
            if (_instance == null)
            {
                _instance = new Recolectable();
            }

            return _instance;
        }


        public void Recolectar(ElementoRecolectable name, int amount)
        {
            Item item = new Item(name, amount);
            Inventory.Instance().Add(item);
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
