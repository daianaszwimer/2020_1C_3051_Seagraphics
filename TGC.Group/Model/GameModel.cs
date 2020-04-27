using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer m�s ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TGCExample
    {

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        DateTime timestamp;

        Fondo oceano;
        TgcSimpleTerrain heightmap;
        Control focusWindows;
        Point mousePosition;
        // todo: probablemente deleguemos la generacion de peces en otra clase
        // para la primer entrega lo dejamos asi
        Pez pez;
        Pez otroPez;
        Tiburon tiburoncin;

        //Caja que se muestra en el ejemplo.
        private TGCBox Box { get; set; }
        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        private Player Player { get; set; }


        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, estructuras de optimizaci�n, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        { 
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
            
            timestamp = DateTime.Now;

            //Utilizando esta propiedad puedo activar el update/render a intervalos constantes.
            FixedTickEnable = true;
            //Se puede configurar el tiempo en estas propiedades TimeBetweenUpdates y TimeBetweenRenders, por defecto esta puedo en 1F / FPS_60 es a lo minimo que deberia correr el TP.
            //De no estar a gusto como se ejecuta el metodo Tick (el que maneja el GameLoop) el mismo es virtual con lo cual pueden sobrescribirlo.

            //Inicializar camara
            var cameraPosition = new TGCVector3(0, 0, 125);
            var lookAt = TGCVector3.Empty;
            Camera.SetCamera(cameraPosition, lookAt);

            oceano = new Fondo(MediaDir, ShadersDir);
            oceano.Init();
            oceano.Camera = Camera;

            var bnwDir = MediaDir + "\\Heightmaps\\heightmap_bnw.jpg";
            var texDir = MediaDir + "\\Heightmaps\\heightmap_tex.jpg";
            float scaleXZ = 10f;
            float scaleY = .5f;
            float offsetY = -150f;
            heightmap = new TgcSimpleTerrain();
            heightmap.loadHeightmap(bnwDir, scaleXZ, scaleY, new TGCVector3(0, offsetY, 0));
            heightmap.loadTexture(texDir);

            // Cargo un pez
            pez = new Pez(MediaDir, ShadersDir);
            pez.Init();
            pez.actualizarPosicion(new TGCVector3(10, 0, 15));

            otroPez = new Pez(MediaDir, ShadersDir);
            otroPez.Init();
            otroPez.actualizarPosicion(new TGCVector3(10, 10, 15));

            tiburoncin = new Tiburon(MediaDir, ShadersDir);
            tiburoncin.Init();

            focusWindows = d3dDevice.CreationParameters.FocusWindow;
            mousePosition = focusWindows.PointToScreen(new Point(focusWindows.Width / 2, focusWindows.Height / 2));
            Cursor.Hide();

            Player = new Player();
            Player.InitMesh();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        { 
            PreUpdate();

            oceano.Update();
            pez.Update();
            otroPez.Update();

            BoundingBox = false;
            DateTime actualTimestamp = DateTime.Now;
            // Mostrar Tiburon cada X cantidad de tiempo
            if (actualTimestamp.Subtract(timestamp).TotalSeconds > 15)
            {
                tiburoncin.aparecer(Camera);
                timestamp = actualTimestamp;
            }
            tiburoncin.Update();

            Cursor.Position = mousePosition;
            Player.Update(Input, Camera, ElapsedTime);
            PostUpdate();

        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqu� todo el c�digo referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones seg�n nuestra conveniencia.
            PreRender();

            oceano.Render();
            heightmap.Render();
            pez.Render();
            otroPez.Render();
            tiburoncin.Render();

            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla P se activa el GodMode", 5, 20, Color.DarkKhaki);
            DrawText.drawText("A,S,D,W para moverse, Ctrl y Espacio para subir o bajar.", 5, 35, Color.DarkKhaki);
            DrawText.drawText("Player Ypos: " + Player.Position().Y, 5, 50, Color.DarkRed);
            DrawText.drawText("Health: " + Player.Health(), 5, 70, Color.DarkSalmon);
            DrawText.drawText("Oxygen: " + Player.Oxygen(), 5, 80, Color.DarkSalmon);
            DrawText.drawText("Camera: \n" + Player.cam_angles, 5, 100, Color.DarkSalmon);
            
            Player.UpdateTransform();
            Player.Render();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecuci�n del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gr�ficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            Player.Dispose();
            oceano.Dispose();
            heightmap.Dispose();
            pez.Dispose();
            otroPez.Dispose();
            tiburoncin.Dispose();
        }
    }
}