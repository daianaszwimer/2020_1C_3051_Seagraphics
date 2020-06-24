using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Group.Model.Crafting;
using TGC.Group.Model.Gui;
using TGC.Group.Model.Entidades;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.Sounds;

namespace TGC.Group.Model
{
    class Player
    {
        //Gameplay vars
        private Inventory inventory = Inventory.Instance();
        private FPSCamara Camara;

        private float oxygen = 100f;
        private float health = 100f;
        private bool estaEnNave;

        private const float OXYGEN_LOSS_SPEED = 10f;
        private const float OXYGEN_RECOVER_SPEED = OXYGEN_LOSS_SPEED * 3.2f;
        private const float OXYGEN_MAX = 100f;
        private const float OXYGEN_DAMAGE = 5f;
        private const float HEALTH_MAX = 100f;
        private const float WATER_LEVEL = 10f; //When players reaches a position above this level, then recovers oxygen.

        private const float MIN_Y_POS = -10f; //nivel del piso

        TgcD3dInput Input;

        //Dev vars
        private bool godmode = false;

        private bool puedoEnfretarTiburon = false;


        //Transformations vars
        public TGCBox mesh { get; set; }
        private TGCVector3 size = new TGCVector3(2, 5, 2);
        private TGCQuaternion rotation = TGCQuaternion.Identity;
        private TGCVector3 posicionInteriorNave = new TGCVector3(0, 50, 0);

        //Config vars
        private float speed = 25f; //foward and horizontal speed
        private float vspeed = 10f; //vertical speed

        // como no anda el keypressed, lo hacemos a mano con estas variables:
        private bool presionoO = false;
        private bool presionoI = false;

        // sonidos
        private TgcStaticSound colision;
        private TgcStaticSound walking;

        private static Player _instance;

        protected Player()
        {
            walking = new TgcStaticSound();
            walking.loadSound(SoundsManager.Instance().mediaDir + "Sounds\\walking.wav", SoundsManager.Instance().sound);
            colision = new TgcStaticSound();
            colision.loadSound(SoundsManager.Instance().mediaDir + "Sounds\\colision.wav", SoundsManager.Instance().sound);
        }

        public static Player Instance()
        {
            if (_instance == null)
            {
                _instance = new Player();
            }

            return _instance;
        }

        public void SetInput(TgcD3dInput Input)
        {
            this.Input = Input;
        }

        public bool puedoEnfrentarTiburon() { return puedoEnfretarTiburon; }

        public void enfrentarTiburon()
        {
            puedoEnfretarTiburon = true;
        }

        //Tgc functions

        public TGCVector3 Position() { return mesh.Position; }
        public TgcBoundingAxisAlignBox BoundingBox() { return mesh.BoundingBox; }

        public void Init(FPSCamara Camara) {
            mesh = TGCBox.fromSize(size, null);
            mesh.Position = posicionInteriorNave;
            this.Camara = Camara;
        }

        /// <summary>
        /// Update del jugador cuando esta FUERA de la nave.
        /// </summary>
        public void Update(float ElapsedTime, ref bool _estaEnNave) {
            estaEnNave = _estaEnNave;
            CheckInputs(ElapsedTime, ref _estaEnNave);
            GameplayUpdate(ElapsedTime);
            UpdateTransform();
        }

        /// <summary>
        /// Update del jugador cuando esta DENTRO de la nave.
        /// </summary>
        /*public void Update(FPSCamara Camara, float ElapsedTime, List<TgcMesh> Paredes)
        {
            estaEnNave = true;
            CheckInputs(Camara, ElapsedTime, Paredes);
            GameplayUpdate(ElapsedTime);
            UpdateTransform();
        }*/

        public void Render() { }

        private void CheckInputs(float ElapsedTime, ref bool estaEnNave_)
        {
            int w = Input.keyDown(Key.W) ? 1 : 0;
            int s = Input.keyDown(Key.S) ? 1 : 0;
            int d = Input.keyDown(Key.D) ? 1 : 0;
            int a = Input.keyDown(Key.A) ? 1 : 0;
            int space = Input.keyDown(Key.Space) ? 1 : 0;
            int ctrl = Input.keyDown(Key.LeftControl) ? 1 : 0;
            bool o = Input.keyDown(Key.O);
            bool i = Input.keyDown(Key.I);

            // nuestra "implementacion" del key pressed (porque nos da false todo el tiempo el de TGC)
            if (o)
            {
                o = false;
                presionoO = true;
            }
            else
            {
                if (presionoO)
                {
                    o = true;
                    presionoO = false;
                }
            }

            if (i)
            {
                i = false;
                presionoI = true;
            }
            else
            {
                if (presionoI)
                {
                    i = true;
                    presionoI = false;
                }
            }

            float fmov = w - s; //foward movement
            float hmov = a - d; //horizontal movement
            float vmov = space - ctrl; //vertical movement

            //Check for in-ship movement            
            var LookDir = Camara.LookDir();
            var LeftDir = Camara.LeftDir();

            if (estaEnNave)
            {
                LookDir.Y = 0;
                LeftDir.Y = 0;
                vmov = 0;
            }

            //Move player
            TGCVector3 movement = LookDir * fmov * speed + Camara.LeftDir() * hmov * speed + TGCVector3.Up * vmov * vspeed;
            movement *= ElapsedTime;
            movement.Y = mesh.Position.Y + movement.Y < MIN_Y_POS ? 0 : movement.Y;

            Move(movement, ElapsedTime);

            if (i)
            {
                if (!Hud.IsCurrentStatus(Hud.Status.MainMenu) && !Hud.IsCurrentStatus(Hud.Status.GameOver))
                {
                    if (!Hud.IsCurrentStatus(Hud.Status.Inventory) && !Hud.IsCurrentStatus(Hud.Status.Crafting))
                        if (!estaEnNave)
                            Hud.ChangeStatus(Hud.Status.Inventory);
                        else
                            Hud.ChangeStatus(Hud.Status.Crafting);
                    else
                        Hud.ChangeStatus(Hud.Status.Gameplay);
                }
            }

            if (o)
            {
                if (!Hud.IsCurrentStatus(Hud.Status.MainMenu) && !Hud.IsCurrentStatus(Hud.Status.GameOver))
                {
                    estaEnNave_ = !estaEnNave_;
                    walking.stop();
                    if (estaEnNave_)
                    {
                        // todo: guardar la posicion en la que estaba para que cuando vuelva, ponerlo en esa posicion anterior
                        // posiciono dentro de nave
                        mesh.Position = posicionInteriorNave;
                    }

                    Hud.ChangeStatus(Hud.Status.Gameplay);
                }
            }

            //Dev
            bool p = Input.keyDown(Key.P);
            if (p) { godmode = !godmode; GodMode(godmode); }
        }

        private void Move(TGCVector3 movement, float ElapsedTime) {
            TGCVector3 lastPos = mesh.Position;
            mesh.Position += movement;
            colision.stop();
            if (estaEnNave)
            {
                //Check for collisions
                bool collided = false;
                List<TGCBox> meshes = InteriorNave.Instance().obtenerParedes();
                foreach (var pared in meshes)
                {
                    var result = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, pared.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro ||
                        result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collided = true;
                        break;
                    }
                }
                //If any collision then go to last position.
                if (collided)
                {
                    mesh.Position = lastPos;
                    colision.play(true);
                }
                if (mesh.Position != lastPos)
                    walking.play(true);
                else
                    walking.stop();
            } else
            {
                // todo: manejamos que la colision de la nave haga que el player entre y sacamos lo del press en la "o"?
                bool collided = false;
                List<TgcMesh> meshes = Nave.Instance().obtenerMeshes();
                foreach (var meshNave in meshes)
                {
                    var result = TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, meshNave.BoundingBox);
                    if (result)
                    {
                        collided = true;
                        break;
                    }
                }
                //If any collision then go to last position.
                if (collided)
                {
                    mesh.Position = lastPos;
                    colision.play(true);
                    return;
                }

                List<Entity> entities = Entities.GetEntities();
                foreach (var entity in entities)
                {
                    var result = TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, entity.GetMesh().BoundingBox);
                    if (result)
                    {
                        collided = true;
                        break;
                    }
                }
                //If any collision then go to last position.
                if (collided)
                {
                    mesh.Position = lastPos;
                    colision.play(true);
                    return;
                }
                // colisiones contra elementos del mar
            }
        }

        public void Dispose() { 
            mesh.Dispose();
            colision.dispose();
            walking.dispose();
        }

        public void UpdateTransform() { mesh.Transform = TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.Translation(mesh.Position); }   


        //Gameplay functions
        private void GameplayUpdate(float ElapsedTime)
        {
            if (!godmode)
            {
                if (IsOutsideWater()) RecoverOxygen(ElapsedTime); else LoseOxygen(ElapsedTime);
            }

            if (IsDead())
            {
                Hud.ChangeStatus(Hud.Status.GameOver);
            }
        }

        private void LoseOxygen(float ElapsedTime) { 
            oxygen = Math.Max(0, oxygen - OXYGEN_LOSS_SPEED * ElapsedTime);
            if (oxygen == 0) GetDamage(OXYGEN_DAMAGE * ElapsedTime);
        }

        public void GetHeal(float amount) { health = Math.Min(100f, health + amount); }
        public void GetDamage(float amount) { health = Math.Max(0, health - amount); }
        public bool IsDead() { return health <= 0; }
        private void RecoverOxygen(float ElapsedTime) { oxygen = Math.Min(OXYGEN_MAX, oxygen + OXYGEN_RECOVER_SPEED * ElapsedTime); }
        private bool IsOutsideWater() { return estaEnNave || mesh.Position.Y > WATER_LEVEL; }

        public float Oxygen() { return oxygen; }
        public float MaxOxygen() { return OXYGEN_MAX; }
        public float Health() { return health; }
        public float MaxHealth() { return HEALTH_MAX; }

        public Inventory GetInventory() { return inventory; }
        public TGCVector3 GetLookDir() { return Camara.LookDir(); }

        //Dev functions
        private void GodMode(bool enabled)
        {
            if (enabled)
            {
                health = 100f;
                oxygen = 100f;
            }
        }

        public void Curarme()
        {
            health = 100f;
        }
    }
}
