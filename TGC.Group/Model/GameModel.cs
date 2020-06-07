using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Fog;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Group.Model.Entidades;
using TGC.Group.Model.Crafting;
using System.Collections.Generic;
using TGC.Group.Model.Gui;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
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

        // variable que indica si esta dentro o fuera de la nave, en base a esto se renderiza un escenario u otro
        // comienza en la nave por lo tanto se inicializa en true
        // cuando se cambia esta variable hay que ocultar todos los elemntos del escenario anterior y hacer un fade a negro
        // como todavia no tenemos ese escenario, lo dejamos en false temporalmente

        DateTime timestamp;

        bool estaEnNave = false;

        Fondo oceano;
        TgcSimpleTerrain heightmap;
        Control focusWindows;
        Point mousePosition;

        //Entidades
        Shark shark;
        List<Metal> metalesOro;
        List<Coral> corales;
        List<Fish> peces;

        FPSCamara FPSCamara;
        Player Player;
        Nave nave;
        InteriorNave interiorNave;

        //Shaders
        TgcFog fog;
        Effect e_fog;

        // data de los heightmaps
        string marBnwDir = "\\Heightmaps\\heightmap_bnw.jpg";
        string marTexDir = "\\Heightmaps\\heightmap_tex.jpg";
        float marScaleXZ = 10f;
        float marScaleY = .5f;
        float marOffsetY = -150f;

        private Entity setearMeshParaLista(Entity elemento, int i, float posicionY = 0)
        {
            elemento.Init();
            //todo: descomentar cuando recolectable se mueva a entity
            //elemento.Recolectable = recolectador;
            int seed = DateTime.Now.Millisecond + i;
            Random random = new Random(seed);
            float y = posicionY == 0 ? i/10 : posicionY;
            TGCVector3 posicion = new TGCVector3((i * (random.Next(0, 5) * (float)Math.Sin(i)) * 2), y, i * 2 * (float)Math.Cos(i) * random.Next(0, 5));
            // todo: fixear corales y oro que se superponen
            // todo: nave se superpone con cosas del piso, si la ponemos arriba en el limite con el agua no pasaria mas
            elemento.cambiarPosicion(posicion);
            return elemento;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
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

            //Esconder cursor
            focusWindows = d3dDevice.CreationParameters.FocusWindow;
            mousePosition = focusWindows.PointToScreen(new Point(focusWindows.Width / 2, focusWindows.Height / 2));
            //Cursor.Hide();

            //Settear jugador y camara
            Player = Player.Instance();
            Player.SetInput(Input);
            Player.InitMesh();

            FPSCamara = new FPSCamara(Camera, Input);

            //Inicializar camara
            var cameraPosition = new TGCVector3(0, 100, 150);
            var lookAt = TGCVector3.Empty;
            Camera.SetCamera(cameraPosition, lookAt);



            //Iniciar HUD
            Hud.Init(MediaDir);
            Hud.ChangeStatus(Hud.Status.Gameplay);

            //Cargar enviroment
            oceano = new Fondo(MediaDir, ShadersDir);
            oceano.Init();
            oceano.Camera = Camera;

            heightmap = new TgcSimpleTerrain();
            heightmap.loadHeightmap(MediaDir + marBnwDir, marScaleXZ, marScaleY, new TGCVector3(0, marOffsetY, 0));
            heightmap.loadTexture(MediaDir + marTexDir);

            //Cargar entidades
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "yellow_fish-TgcScene.xml");
            var mesh = scene.Meshes[0];

            peces = new List<Fish>();
            int i = 0;
            while (i < 20)
            {
                Fish fish;
                string meshName = i.ToString();
                fish = new Fish(mesh.clone(meshName));
                fish = (Fish)setearMeshParaLista(fish, i);
                peces.Add(fish);
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "shark-TgcScene.xml");
            mesh = scene.Meshes[0];

            shark = new Shark(mesh);
            shark.Init();
            
            scene = loader.loadSceneFromFile(MediaDir + "coral-TgcScene.xml");
            mesh = scene.Meshes[0];
            

            corales = new List<Coral>();
            i = 0;
            while (i < 25)
            {
                Coral coral;
                string meshName = i.ToString();
                coral = new Coral(mesh.clone(meshName));
                coral = (Coral)setearMeshParaLista(coral, i * 4, -20);
                corales.Add(coral);
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "Oro-TgcScene.xml");
            mesh = scene.Meshes[0];
            metalesOro = new List<Metal>();
            i = 0;
            while (i < 10)
            {
                Metal oro;
                string meshName = i.ToString();
                oro = new Metal(mesh.clone(meshName));
                oro = (Metal)setearMeshParaLista(oro, i * 8, -20);
                oro.Tipo = ElementoRecolectable.oro;
                metalesOro.Add(oro);
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "ship-TgcScene.xml");
            nave = Nave.Instance();
            nave.Init(scene);

            //Cargar shaders
            fog = new TgcFog();
            fog.Color = Color.FromArgb(30, 144, 255);
            fog.Density = 1;
            fog.EndDistance = 1000;
            fog.StartDistance = 1;
            fog.Enabled = true;

            e_fog = TGCShaders.Instance.LoadEffect(ShadersDir + "e_fog.fx");

            interiorNave = InteriorNave.Instance();
            interiorNave.Init(MediaDir);


            
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        { 
            PreUpdate();
            Hud.Update(Input);

            if (estaEnNave)
            {
                interiorNave.Update();

            } else
            {
                // update de elementos de agua
                nave.Update();
                oceano.Update();

                DateTime actualTimestamp = DateTime.Now;
                // Mostrar Tiburon cada X cantidad de tiempo
                if (actualTimestamp.Subtract(timestamp).TotalSeconds > 15)
                {
                    shark.Spawn();
                    timestamp = actualTimestamp;
                }
                shark.Update(ElapsedTime);

                foreach (var pez in peces)
                {
                    pez.Update(ElapsedTime);
                }
                foreach (var coral in corales)
                {
                    coral.Update(ElapsedTime);
                }
                foreach (var oro in metalesOro)
                {
                    oro.Update(ElapsedTime);
                }

            }

            // esto se hace siempre
            //Lockear mouse
            Cursor.Position = mousePosition;

            //Camara y jugador
            FPSCamara.Update(ElapsedTime);

            Player.Update(FPSCamara, ElapsedTime, ref estaEnNave);

            PostUpdate();

        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            ClearTextures();
            D3DDevice.Instance.Device.BeginScene();
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            Hud.Render();

            fog.updateValues();
            e_fog.SetValue("ColorFog", fog.Color.ToArgb());
            e_fog.SetValue("CameraPos", TGCVector3.TGCVector3ToFloat4Array(Camera.Position));
            e_fog.SetValue("StartFogDistance", fog.StartDistance);
            e_fog.SetValue("EndFogDistance", fog.EndDistance);
            e_fog.SetValue("Density", fog.Density);

            if (estaEnNave)
            {
                interiorNave.Render();
            } else
            {
                oceano.Effect(e_fog);
                oceano.Technique("RenderScene");
                oceano.Render();
                
                heightmap.Effect = e_fog;
                heightmap.Technique = "RenderScene";
                heightmap.Render();

                foreach (var pez in peces)
                {
                    pez.Effect(e_fog);
                    pez.Technique("RenderScene");
                    pez.Render();
                }
                foreach (var coral in corales)
                {
                    coral.Effect(e_fog);
                    coral.Technique("RenderScene");
                    coral.Render();
                }


                shark.Effect(e_fog);
                shark.Technique("RenderScene");
                shark.Render();

                nave.Effect(e_fog);
                nave.Technique("RenderScene");
                nave.Render();

                foreach (var oro in metalesOro)
                {
                    oro.Effect(e_fog);
                    oro.Technique("RenderScene");
                    oro.Render();
                }
            }

            //Dibuja un texto por pantalla
            /*
            DrawText.drawText("Con la tecla P se activa el GodMode", 5, 20, Color.DarkKhaki);
            DrawText.drawText("A,S,D,W para moverse, Ctrl y Espacio para subir o bajar.", 5, 35, Color.DarkKhaki);
            DrawText.drawText("Player Ypos: " + Player.Position().Y, 5, 50, Color.DarkRed);
            DrawText.drawText("Health: " + Player.Health(), 5, 70, Color.DarkSalmon);
            DrawText.drawText("Oxygen: " + Player.Oxygen(), 5, 80, Color.DarkSalmon);
            DrawText.drawText("Camera: \n" + FPSCamara.cam_angles, 5, 100, Color.DarkSalmon);
            DrawText.drawText("Con la tecla O entra o sale de la nave", 5, 145, Color.DarkKhaki);
            DrawText.drawText("Inventario: \n" + inventory.inventoryMostrarItemsRecolectados(), 5, 160, Color.DarkRed);
            DrawText.drawText("Crafteos disponibles: \n" + inventory.inventoryMostrarCrafteos(), 200, 160, Color.DarkRed);
            */

            Player.Render();

            PostRender();

        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            Player.Dispose();
            oceano.Dispose();
            heightmap.Dispose();

            foreach (var pez in peces)
            {
                pez.Dispose();
            }
            foreach (var coral in corales)
            {
                coral.Dispose();
            }
            foreach (var oro in metalesOro)
            {
                oro.Dispose();
            }
            shark.Dispose();
            nave.Dispose();

            interiorNave.Dispose();

            Hud.Dispose();
        }
    }
}