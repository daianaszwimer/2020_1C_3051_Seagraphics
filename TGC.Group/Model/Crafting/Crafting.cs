using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model.Crafting
{
    // Defino todos los elementos crafteables que existiran
    public enum ElementoCraftreable
    {
        cuchilllo,
        oxigeno,
        medicina
    }
    interface Crafting // Defino la interface que deberan utilizar todos los crafteos creados
    {
        // Averiguo si el inventario tiene los elementos necesarios para craftear un elemento
        bool PuedeCraftear(Inventory inventory);

        // Me dice cual es elemento que voy a craftear
        ElementoCraftreable Tipo();

        // ---
        void Craftear();

        String obtenerImagen();

        void darHabilidadAPlayer(Player jugador);

        /* Consiste en que en la pantalla del inventario el crafteo este "gris" como inhabilitado
           Una vez que se pueda craftear el elemento dejara de estar gris en la pantalla y podra 
           Ser utilizado por el Player */
        void activarCrafteo();
        void AgregarMateriales(ElementoRecolectable elemento, int cantidad);

        bool EstoyCrafteado();
    }

    
}
