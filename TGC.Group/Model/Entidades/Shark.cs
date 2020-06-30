using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Collision;
using TGC.Core.Sound;
using TGC.Group.Model.Sounds;
using TGC.Group.Model.Gui;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Textures;

namespace TGC.Group.Model.Entidades
{
    class Shark : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        private int vida;
        private float yMax;
        private float time = 0;
        //private Crafting conQueMeAtacan = null; 
        private Tgc3dSound sound;
        private TgcStaticSound soundWin;
        private Effect efectoDesaparecer;
        private bool meAtaco = false;

        private bool puedoSerAtacado;

        public void puedenAtarcame() // puedenAtacarme(Crafting arma)
        {
            //conQueMeAtacan = arma
            this.puedoSerAtacado = true;
        }

        private void reducirVidaEn(int cantidad)
        {
            this.vida -= cantidad;
        }

        public bool estoyVivo() { return this.vida > 0; }

        //Config
        const float DAMAGE = 30f;
        const float speed = 10f;
        const float distanceToEscape = 300f;

        //Internal vars
        TGCVector3 goalPos = TGCVector3.Empty;

        bool canDealDamage = true;

        public Shark(TgcMesh mesh) : base(mesh, meshLookDir) 
        {
            this.vida = 500;
            necesitaArmaParaInteractuar = true;
            agarrarEfecto = new TgcStaticSound();
            agarrarEfecto.loadSound(SoundsManager.Instance().mediaDir + "Sounds\\stab.wav", SoundsManager.Instance().sound);
            soundWin = new TgcStaticSound();
            soundWin.loadSound(SoundsManager.Instance().mediaDir + "Sounds\\win.wav", SoundsManager.Instance().sound);
        }

        public void setearPerlin(TgcTexture texture)
        {
            efectoDesaparecer.SetValue("textura_perlin", texture.D3dTexture);
        }
        public void setearEfectoPerlin(Effect _effect)
        {
            efectoDesaparecer = _effect;
        }

        //Entity functions
        protected override void InitEntity()
        {
            mesh.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            time += ElapsedTime;
            // solo setear si se esta usando el efecto?
            if (time < 1.5)
            {
                // si supera 1 no tiene sentido porque ningun canal tiene mas de uno (ver el if en el efecto)
                // 1.5 es para darle un delta porque si hacemos > 1 nunca va a llegar a 1
                efectoDesaparecer.SetValue("value", time);
            }
            // me fijo si se llego al tiempo y pongo estaOculto = true;
            if (!meAtaco)
            {
                if (ArrivedGoalPos())
                    SetEscapeGoalPos();

                if (canDealDamage)
                    Attack();
            
                Move(goalPos, speed, ElapsedTime, yMax);

                sound.Position = mesh.Position;
            
                sound.play(true);
            }
        }

        protected override void RenderEntity() { 
            //mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() {
            sound.dispose();
            soundWin.dispose();
        }

        //Gamemodel functions
        public void Spawn()
        {
            //Reset vars
            canDealDamage = true;
            SetPlayerGoalPos();

            //Position shark
            Random r = new Random();
            var sign = r.Next(-1, 1) >= 0 ? 1 : -1;
            var x = (float)r.NextDouble() * sign;
            var z = (float)r.NextDouble() * sign;
            
            mesh.Position = Player.Instance().Position() + new TGCVector3(x, 0, z) * 100f;
        }

        protected override void InteractEntity()
        {
            /*if (Player.Instance().puedoEnfrentarTiburon() && estoyVivo())*/
            // muestro efecto para que "desaparezca"
            // cuando el tiempo llega a x, ahi lo desaparezco
            mesh.Effect = efectoDesaparecer;
            mesh.Technique = "RenderScene";
            time = 0;
            meAtaco = true;
            sound.stop();
            Player.Instance().usarArma();
            //reducirVidaEn(conQueMeAtacan.danio);
            //reducirVidaEn(10);
            //Console.WriteLine("Me ataco");
            // player gano
            Hud.ChangeStatus(Hud.Status.Win);
            soundWin.play();
            
        }

        public void setearSonido(Tgc3dSound _sound)
        {
            sound = _sound;
            sound.MinDistance = 20f;
        }

        public void setearAlturaMaxima(float yMax_)
        {
            yMax = yMax_;
        }

        //Internal functions

        private void SetPlayerGoalPos() { goalPos = Player.Instance().Position(); }

        private void SetEscapeGoalPos()
        {
            TGCVector3 dir = mesh.Position - Player.Instance().Position();
            dir = TGCVector3.Normalize(dir);
            dir.Y = 0;
            goalPos = dir * distanceToEscape;
      }

        private bool ArrivedGoalPos() { return Math.Abs((goalPos - mesh.Position).Length()) < 0.1f; }

        private void Attack()
        {
            //Chequear colisiones
            var hitPlayer = TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, Player.Instance().BoundingBox());
            if (hitPlayer)
            {
                Player.Instance().GetDamage(DAMAGE);
                canDealDamage = false;
                SetEscapeGoalPos();
            }
        }
    }
}
