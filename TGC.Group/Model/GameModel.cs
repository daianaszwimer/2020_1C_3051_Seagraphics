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
using TGC.Core.Sound;
using TGC.Core.Collision;
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

        // variable que indica si esta dentro o fuera de la nave, en base a esto se renderiza un escenario u otro
        // comienza en la nave por lo tanto se inicializa en true
        // cuando se cambia esta variable hay que ocultar todos los elemntos del escenario anterior y hacer un fade a negro
        // como todavia no tenemos ese escenario, lo dejamos en false temporalmente

        DateTime timestamp;

        float time;

        bool estaEnNave = false;
        bool estaEnAlgunMenu = true;
        Hud.Status estadoAnterior = Hud.Status.MainMenu;

        float nivelDelAgua = 80f;

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
        Arma arma;

        // Interior de la nave
        MesaNave mesaNave;
        LamparaNave lamparaNave;
        SogaInterior sogaInterior;
        SillaInterior sillaInterior;
        TimonInterior timonInterior;

        //Shaders
        TgcFog fog;
        Microsoft.DirectX.Direct3D.Effect effect;
        Microsoft.DirectX.Direct3D.Effect efectoDesaparecer;
        Microsoft.DirectX.Direct3D.Effect efectoInterior;

        private TgcStaticSound sonidoUnderwater;
        Tgc3dSound sharkSound;
        // data de los heightmaps
        string marBnwDir = "\\Heightmaps\\heightmap_bnw2.jpg";
        string marTexDir = "\\Heightmaps\\heightmap_tex.jpg";
        float marScaleXZ = 10f;
        float marScaleY = .35f;
        float marOffsetY = -100f;

        private Surface depthStencil;
        private Texture renderTarget, coralesBrillantes, FBHorizontalBool, FBVerticalBloom;
        private VertexBuffer fullScreenQuad;

        private TgcTexture maskTexture;
        private TgcTexture perlinTexture;
        //Optimizacion

        private Entity setearMeshParaLista(Entity elemento, int i, float posicionY = 0)
        {
            elemento.Init();
            //todo: descomentar cuando recolectable se mueva a entity
            //elemento.Recolectable = recolectador;
            int seed = DateTime.Now.Millisecond + i;
            Random random = new Random(seed);
            float y = posicionY == 0 ? i / 10 : posicionY;
            TGCVector3 posicion = new TGCVector3((i * (random.Next(0, 5) * (float)Math.Sin(i)) * 2), y, i * 2 * (float)Math.Cos(i) * random.Next(0, 5));
            elemento.cambiarPosicion(posicion, corales, metalesOro);
            return elemento;
        }

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

            //Esconder cursor
            focusWindows = d3dDevice.CreationParameters.FocusWindow;
            mousePosition = focusWindows.PointToScreen(new Point(focusWindows.Width / 2, focusWindows.Height / 2));
            //Cursor.Hide();

            Sounds.SoundsManager.Instance().sound = DirectSound.DsDevice;
            Sounds.SoundsManager.Instance().mediaDir = MediaDir;

            //Burbujas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();
            Particulas.Init(MediaDir, 20);

            //Oceano
            Oceano.Init(TGCVector3.Up * nivelDelAgua * 0.8f, 100, 100, ShadersDir);

            //Settear jugador y camara
            FPSCamara = new FPSCamara(Camera, Input);

            Player = Player.Instance();
            Player.SetInput(Input);
            Player.Init(FPSCamara);

            //Inicializar camara
            var cameraPosition = new TGCVector3(0, 100, 150);
            var lookAt = TGCVector3.Empty;
            Camera.SetCamera(cameraPosition, lookAt);

            sonidoUnderwater = new TgcStaticSound();
            sonidoUnderwater.loadSound(MediaDir + "Sounds\\mar.wav", DirectSound.DsDevice);
            
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "e_fog.fx");
            efectoInterior = TGCShaders.Instance.LoadEffect(ShadersDir + "interior.fx");


            //Iniciar HUD
            Hud.Init(MediaDir);
            Hud.ChangeStatus(Hud.Status.MainMenu);

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
            corales = new List<Coral>();
            metalesOro = new List<Metal>();

            int i = 0;
            while (i < 20)
            {
                Fish fish;
                string meshName = i.ToString();
                fish = new Fish(mesh.clone(meshName));
                fish = (Fish)setearMeshParaLista(fish, i);
                peces.Add(fish);

                fish.Effect(effect);
                fish.Technique("RenderScene");
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "shark-TgcScene.xml");
            mesh = scene.Meshes[0];

            shark = new Shark(mesh);
            shark.Init();
            sharkSound = new Tgc3dSound(MediaDir + "Sounds\\shark.wav", shark.GetMesh().Position, DirectSound.DsDevice);
            shark.setearSonido(sharkSound);
            shark.setearAlturaMaxima(nivelDelAgua-19f);
            
            efectoDesaparecer = TGCShaders.Instance.LoadEffect(ShadersDir + "perlin.fx");

            shark.setearEfectoPerlin(efectoDesaparecer);
            perlinTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures\\perlin.png");
            shark.setearPerlin(perlinTexture);

            scene = loader.loadSceneFromFile(MediaDir + "coral-TgcScene.xml");
            mesh = scene.Meshes[0];


            i = 0;
            while (i < 25)
            {
                Coral coral;
                string meshName = i.ToString();
                coral = new Coral(mesh.createMeshInstance(meshName));
                coral = (Coral)setearMeshParaLista(coral, i * 4, -17);
                corales.Add(coral);

                coral.Effect(effect);
                coral.Technique("RenderScene");
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "Oro-TgcScene.xml");
            mesh = scene.Meshes[0];
            i = 0;
            while (i < 10)
            {
                Metal oro;
                string meshName = i.ToString();
                oro = new Metal(mesh.createMeshInstance(meshName));
                oro = (Metal)setearMeshParaLista(oro, i * 8, -17);
                oro.Tipo = ElementoRecolectable.oro;
                metalesOro.Add(oro);

                oro.Effect(effect);
                oro.Technique("RenderSceneLight");
                i++;
            }

            scene = loader.loadSceneFromFile(MediaDir + "ship-TgcScene.xml");
            nave = Nave.Instance();
            nave.Init(scene, nivelDelAgua);

            scene = loader.loadSceneFromFile(MediaDir + "EspadaDoble-TgcScene.xml");
            mesh = scene.Meshes[0];
            arma = new Arma(mesh);

            scene = loader.loadSceneFromFile(MediaDir + "Mesa-TgcScene.xml");
            mesaNave = MesaNave.Instance();
            mesaNave.Init(scene);
            mesaNave.Effect(efectoInterior);
            mesaNave.Technique("RenderScene");

            scene = loader.loadSceneFromFile(MediaDir + "SogaEnrollada-TgcScene.xml");
            mesh = scene.Meshes[0];
            sogaInterior = SogaInterior.Instance();
            sogaInterior.Init(mesh);
            sogaInterior.Effect(efectoInterior);
            sogaInterior.Technique("RenderScene");

            scene = loader.loadSceneFromFile(MediaDir + "silla-TgcScene.xml");
            sillaInterior = SillaInterior.Instance();
            sillaInterior.Init(scene);
            sillaInterior.Effect(efectoInterior);
            sillaInterior.Technique("RenderScene");

            scene = loader.loadSceneFromFile(MediaDir + "Timon-TgcScene.xml");
            timonInterior = TimonInterior.Instance();
            timonInterior.Init(scene);
            timonInterior.Effect(efectoInterior);
            timonInterior.Technique("RenderScene");

            scene = loader.loadSceneFromFile(MediaDir + "LamparaTecho-TgcScene.xml");
            lamparaNave = new LamparaNave(scene.Meshes[0]);
            lamparaNave.Effect(efectoInterior);
            lamparaNave.Technique("RenderScene");


            //Cargar shaders
            fog = new TgcFog();
            fog.Color = Color.FromArgb(30, 144, 255);
            fog.Density = 1;
            fog.EndDistance = 1000;
            fog.StartDistance = 1;
            fog.Enabled = true;

            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            //Fog + Lights
            effect.SetValue("nivelAgua", nivelDelAgua);

            interiorNave = InteriorNave.Instance();
            interiorNave.Init(MediaDir);

            DirectSound.ListenerTracking = Player.Instance().mesh;

            //Mascara post process
            maskTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures\\mascara.png");
            effect.SetValue("textura_mascara", maskTexture.D3dTexture);

            // seteamos los efectos aca porque son fijos
            oceano.Effect(effect);
            oceano.Technique("RenderScene");

            heightmap.Effect = effect;
            heightmap.Technique = "RenderScene";

            shark.Effect(effect);
            shark.Technique("RenderScene");

            nave.Effect(effect);
            nave.Technique("RenderSceneLight");

            effect.SetValue("ambientColor", Color.FromArgb(255, 255, 255).ToArgb());
            effect.SetValue("diffuseColor", Color.FromArgb(255, 255, 255).ToArgb());
            effect.SetValue("specularColor", Color.FromArgb(255, 255, 255).ToArgb());

            efectoInterior.SetValue("ambientColor", Color.FromArgb(255, 255, 255).ToArgb());
            efectoInterior.SetValue("diffuseColor", Color.FromArgb(255, 255, 255).ToArgb());
            efectoInterior.SetValue("specularColor", Color.FromArgb(255, 255, 255).ToArgb());

            // dibujo el full screen quad
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };

            // Vertex buffer de los triangulos
            fullScreenQuad = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            fullScreenQuad.SetData(vertices, 0, LockFlags.None);

            // dibujo render target

            depthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            renderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            // inicializo los FB que uso para el Bloom
            coralesBrillantes = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            FBHorizontalBool = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            FBVerticalBloom = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            time += ElapsedTime;

            Hud.Update(Input);
            estaEnAlgunMenu = Hud.GetCurrentStatus() == Hud.Status.MainMenu || Hud.GetCurrentStatus() == Hud.Status.Instructions;

            //Que no se pueda hacer nada si estas en game over salvo dar enter
            if (Hud.GetCurrentStatus() == Hud.Status.GameOver || Hud.GetCurrentStatus() == Hud.Status.Win)
            {
                // si gano hay que hacer update del tiburon para verlo desaparecer
                if (Hud.GetCurrentStatus() == Hud.Status.Win)
                {
                    shark.Update(ElapsedTime);
                }
                PostUpdate();
                return;
            }
            if ((estadoAnterior == Hud.Status.MainMenu || estadoAnterior == Hud.Status.Instructions) && !estaEnAlgunMenu) {
                // me meto en la nave cuando paso del menu
                estaEnNave = true;
                Player.Update(ElapsedTime, ref estaEnNave, true);
            }
            estadoAnterior = Hud.GetCurrentStatus();
            if (estaEnAlgunMenu)
            {
                sonidoUnderwater.play(true);
                // update de elementos de agua
                nave.Update();
                Oceano.Update(time);
                oceano.Update();
                Particulas.Update(time);
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

                fog.updateValues();
                effect.SetValue("ColorFog", fog.Color.ToArgb());
                effect.SetValue("CameraPos", TGCVector3.TGCVector3ToFloat4Array(Camera.Position));
                effect.SetValue("StartFogDistance", fog.StartDistance);
                effect.SetValue("EndFogDistance", fog.EndDistance);
                effect.SetValue("Density", fog.Density);
                effect.SetValue("eyePos", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));
            }
            if (!estaEnAlgunMenu)
            {
                arma.Update();
                if (estaEnNave)
                {
                    interiorNave.Update();
                    sharkSound.stop();
                    sonidoUnderwater.stop();
                    efectoInterior.SetValue("eyePos", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));
                }
                else
                {
                    sonidoUnderwater.play(true);
                    // update de elementos de agua
                    nave.Update();
                    Oceano.Update(time);
                    oceano.Update();
                    Particulas.Update(time);

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

                    fog.updateValues();
                    effect.SetValue("ColorFog", fog.Color.ToArgb());
                    effect.SetValue("CameraPos", TGCVector3.TGCVector3ToFloat4Array(Camera.Position));
                    effect.SetValue("StartFogDistance", fog.StartDistance);
                    effect.SetValue("EndFogDistance", fog.EndDistance);
                    effect.SetValue("Density", fog.Density);
                    effect.SetValue("eyePos", TGCVector3.TGCVector3ToFloat3Array(Camera.Position));
                }


                //Camara y jugador
                FPSCamara.Update(ElapsedTime);

                Player.Update(ElapsedTime, ref estaEnNave);

            }

            // esto se hace siempre
            //Lockear mouse
            Cursor.Position = mousePosition;
            PostUpdate();

        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqu� todo el c�digo referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //ClearTextures();
            var device = D3DDevice.Instance.Device;

            // Capturamos las texturas de pantalla
            Surface screenRenderTarget = device.GetRenderTarget(0);
            Surface screenDepthSurface = device.DepthStencilSurface;

            // Especificamos que vamos a dibujar en una textura
            device.SetRenderTarget(0, renderTarget.GetSurfaceLevel(0));
            device.DepthStencilSurface = depthStencil;

            // Captura de escena en render target
            device.BeginScene();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            bool flagFuera = true;
            if (estaEnAlgunMenu)
            {
                oceano.Render();
                heightmap.Render();

                foreach (var pez in peces)
                {
                    if (IsInFrustum(pez.GetMesh()))
                    {
                        pez.Technique("RenderScene");
                        pez.Render();
                    }
                }

                effect.SetValue("shininess", 0.5f);
                effect.SetValue("KSpecular", 0.5f);
                effect.SetValue("KAmbient", 5.0f);
                effect.SetValue("KDiffuse", 2.0f);

                foreach (var coral in corales)
                {
                    if (IsInFrustum(coral.GetMesh()))
                    {
                        coral.Technique("RenderSceneLight");
                        coral.Render();
                    }
                }

                if (IsInFrustum(shark.GetMesh()))
                {
                    shark.Technique("RenderScene");
                    shark.Render();
                }
                Particulas.Render(ElapsedTime);

                //Efecto metalico
                effect.SetValue("shininess", 30f);
                effect.SetValue("KSpecular", 1.0f);
                effect.SetValue("KAmbient", 1.0f);
                effect.SetValue("KDiffuse", 0.5f);
                if (IsInFrustum(nave.obtenerMeshes()))
                {
                    nave.Technique("RenderSceneLight");
                    nave.Render();
                }

                effect.SetValue("shininess", 10f);
                effect.SetValue("KSpecular", 1.1f);
                effect.SetValue("KAmbient", 1.2f);
                effect.SetValue("KDiffuse", 0.25f);
                foreach (var oro in metalesOro)
                {
                    if (IsInFrustum(oro.GetMesh()))
                    {
                        oro.Technique("RenderSceneLight");
                        oro.Render();
                    }
                }
                Oceano.Render();
                Player.Render();
            }
            if (!estaEnAlgunMenu)
            {
                if (estaEnNave)
                {
                    efectoInterior.SetValue("shininess", 1f);
                    efectoInterior.SetValue("KSpecular", 0.1f);
                    efectoInterior.SetValue("KAmbient", 3.0f);
                    efectoInterior.SetValue("KDiffuse", 0f);
                    interiorNave.Render();

                    // constantes de iluminacion de la mesa
                    efectoInterior.SetValue("shininess", 1f);
                    efectoInterior.SetValue("KSpecular", 0.2f);
                    efectoInterior.SetValue("KAmbient", 3.0f);
                    efectoInterior.SetValue("KDiffuse", 0.1f);
                    mesaNave.Render();
                    // es el mismo material (madera) mantengo las mismas constantes
                    sillaInterior.Render();
                    timonInterior.Render();

                    // constantes de iluminacion de la soga
                    efectoInterior.SetValue("shininess", 1f);
                    efectoInterior.SetValue("KSpecular", 0.15f);
                    efectoInterior.SetValue("KAmbient", 2.0f);
                    efectoInterior.SetValue("KDiffuse", 0f);
                    sogaInterior.Render();

                    efectoInterior.SetValue("shininess", 5.0f);
                    efectoInterior.SetValue("KSpecular", 2.5f);
                    efectoInterior.SetValue("KAmbient", 5.0f);
                    efectoInterior.SetValue("KDiffuse", 0f);
                    lamparaNave.Render();
                }
                else
                {
                    flagFuera = false;
                    oceano.Render();
                    heightmap.Render();

                    foreach (var pez in peces)
                    {
                        if (IsInFrustum(pez.GetMesh()))
                        {
                            pez.Technique("RenderScene");
                            pez.Render();
                        }
                    }

                    effect.SetValue("shininess", 0.5f);
                    effect.SetValue("KSpecular", 0.5f);
                    effect.SetValue("KAmbient", 5.0f);
                    effect.SetValue("KDiffuse", 2.0f);

                    foreach (var coral in corales)
                    {
                        if (IsInFrustum(coral.GetMesh()))
                        {
                            coral.Technique("RenderSceneLight");
                            coral.Render();
                        }
                    }

                    if (IsInFrustum(shark.GetMesh()))
                    {
                        shark.Technique("RenderScene");
                        shark.Render();
                    }
                    Particulas.Render(ElapsedTime);

                    //Efecto metalico
                    effect.SetValue("shininess", 30f);
                    effect.SetValue("KSpecular", 1.0f);
                    effect.SetValue("KAmbient", 1.0f);
                    effect.SetValue("KDiffuse", 0.5f);
                    if (IsInFrustum(nave.obtenerMeshes()))
                    {
                        nave.Technique("RenderSceneLight");
                        nave.Render();
                    }

                    effect.SetValue("shininess", 10f);
                    effect.SetValue("KSpecular", 1.1f);
                    effect.SetValue("KAmbient", 1.2f);
                    effect.SetValue("KDiffuse", 0.25f);
                    foreach (var oro in metalesOro)
                    {
                        if (IsInFrustum(oro.GetMesh()))
                        {
                            oro.Technique("RenderSceneLight");
                            oro.Render();
                        }
                    }
                    Oceano.Render();

                }
                Player.Render();
                if (Player.Instance().puedoEnfrentarTiburon())
                {
                    // renderizo arma
                    arma.Effect(effect);
                    arma.Technique("RenderScene");
                    arma.Render();
                }
            }
            
            device.EndScene();

            if (!flagFuera || estaEnAlgunMenu)
            {
                device.SetRenderTarget(0, coralesBrillantes.GetSurfaceLevel(0));
                device.DepthStencilSurface = depthStencil;

                device.BeginScene();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                // dibujo 15 corales con luz
                int j = 0; //todo: poner 10
                while (j < 25)
                {
                    if (IsInFrustum(corales[j].GetMesh()))
                    {
                        corales[j].Technique("Bloom");
                        corales[j].Render();
                    }
                    j++;
                }

                foreach (var pez in peces)
                {
                    if (IsInFrustum(pez.GetMesh()))
                    {
                        pez.Technique("BloomMask");
                        pez.Render();
                    }
                }
                
                if (shark.estoyVivo() && IsInFrustum(shark.GetMesh()))
                {
                    shark.Technique("BloomMask");
                    shark.Render();
                }
                
                if (IsInFrustum(nave.obtenerMeshes()))
                {
                    nave.Technique("BloomMask");
                    nave.Render();
                }

                foreach (var oro in metalesOro)
                {
                    if (IsInFrustum(oro.GetMesh()))
                    {
                        oro.Technique("BloomMask");
                        oro.Render();
                    }
                }

                if (Player.Instance().puedoEnfrentarTiburon())
                {
                    // renderizo arma
                    arma.Effect(effect);
                    arma.Technique("BloomMask");
                    arma.Render();
                }

                device.EndScene();

                // aplico pasada de blur horizontal y vertical al FB de los corales q brillan
                
                device.SetRenderTarget(0, FBHorizontalBool.GetSurfaceLevel(0));
                device.DepthStencilSurface = depthStencil;

                device.BeginScene();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

                effect.Technique = "BlurHorizontal";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("fBCoralesBrillantes", coralesBrillantes);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                device.EndScene();
                
                
                device.SetRenderTarget(0, FBVerticalBloom.GetSurfaceLevel(0));
                device.DepthStencilSurface = depthStencil;

                device.BeginScene();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

                effect.Technique = "BlurVertical";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("fBCoralesBrillantes", FBHorizontalBool);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                
                device.EndScene();
                
            }
                

            // Especificamos que vamos a dibujar en pantalla
            device.SetRenderTarget(0, screenRenderTarget);
            device.DepthStencilSurface = screenDepthSurface;

            device.BeginScene();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
          
            if (estaEnAlgunMenu)
            {
                effect.Technique = "PostProcessMenu";
            }
            else if (flagFuera)
            {
                effect.Technique = "PostProcess";
            }
            else
            {
                effect.Technique = "PostProcessMar";
            }

            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, fullScreenQuad, 0);

            effect.SetValue("sceneFrameBuffer", renderTarget);
            effect.SetValue("verticalBlurFrameBuffer", FBVerticalBloom);

            // Dibujamos el full screen quad
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            Hud.Render();

            RenderFPS();
            RenderAxis();
            device.EndScene();

            device.Present();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecuci�n del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gr�ficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            shark.Dispose();
            efectoDesaparecer.Dispose();
            sonidoUnderwater.dispose();
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
            nave.Dispose();

            interiorNave.Dispose();
            mesaNave.Dispose();
            sogaInterior.Dispose();
            lamparaNave.Dispose();
            sillaInterior.Dispose();
            timonInterior.Dispose();
            Particulas.Dispose();
            Oceano.Dispose();
            Hud.Dispose();
            maskTexture.dispose();
            perlinTexture.dispose();
            arma.Dispose();
            effect.Dispose();
            renderTarget.Dispose();
            fullScreenQuad.Dispose();
            depthStencil.Dispose();
            coralesBrillantes.Dispose();
            FBVerticalBloom.Dispose();
            FBHorizontalBool.Dispose();
        }
        bool IsInFrustum(TgcMesh mesh)
        {
            var col = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
            if (col != TgcCollisionUtils.FrustumResult.OUTSIDE)
                return true;
            else
                return false;
        }

        bool IsInFrustum(List<TgcMesh> meshes)
        {
            bool isIn = false;
            foreach (TgcMesh mesh in meshes)
            {
                isIn = IsInFrustum(mesh);
                if (isIn)
                    break;
            }
            return isIn;
        }
    }
}