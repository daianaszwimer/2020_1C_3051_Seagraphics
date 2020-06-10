using System;

namespace TGC.Group.Model.Crafting
{
    // Defino todos los elementos crafteables que existiran
    public enum ElementoCraftreable
    {
        cuchilllo,
        oxigeno,
        botiquin
    }
    interface Crafting // Defino la interface que deberan utilizar todos los crafteos creados
    {
        // Me dice cual es elemento que voy a craftear
        ElementoCraftreable Tipo();

        // Averiguo si el inventario tiene los elementos necesarios para craftear un elemento
        // y si todavia no esta habilitado
        bool PuedeCraftear();

        /* Indica el estado de un crafteo, es decir cuando el player no tiene los items necesarios
         * todavia no puede ser crafteados por lo cual sigue inhabilitado su uso, cuando el player
         * consigo los items necesarios, este crafteo se habilitara permitiendole al player utilizarlo
         */
        bool EstoyHabilitado();

        /* Consiste en que en la pantalla del inventario el crafteo este "gris" como inhabilitado
           Una vez que se pueda craftear el elemento dejara de estar gris en la pantalla y permite 
           ser utilizado por el Player cuando el lo craftee*/
        void ActivarCrafteo();

        // Elimina los items consumidos del inventario y le da una habilidad el player chequeando que
        // aun no este crafteado (sirve de control para los crafteos de uso durable).
        void Craftear();

        /* Control para no craftear un elemento dos veces
         */
        bool EstoyCrafteado();

        // Es particular de cada crafteo dandole algun tipo de mejor al player
        void DarHabilidadAPlayer();

        /* Nos indica si un crafteo se puede volver a crear o si es de uso ilimitado
          Ejemplo cuchillo = dura para siempre - Medicina = se usa una unica vez*/
        bool SoyReutilizable();

        /* En el caso de los crafteos de uso unico permite al player volver a craftear
         * dichos elementos
         */
        void Reutilizar();

        //Nos da el path del icono, el cual varia segun el estado del crafteo
        String ObtenerIcono();

        void Deshabilitar();
    }


}
