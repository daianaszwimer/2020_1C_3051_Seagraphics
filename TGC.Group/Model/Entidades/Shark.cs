using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Collision;
using TGC.Core.Sound;
using TGC.Group.Model.Sounds;
using TGC.Group.Model.Gui;

namespace TGC.Group.Model.Entidades
{
    class Shark : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        private int vida;
        private float yMax;

        //private Crafting conQueMeAtacan = null; 
        private Tgc3dSound sound;
        private TgcStaticSound soundWin;

        private bool puedoSerAtacado = false;

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

        //Entity functions
        protected override void InitEntity()
        {
            mesh.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            if (ArrivedGoalPos())
                SetEscapeGoalPos();

            if (canDealDamage)
                Attack();
            
            Move(goalPos, speed, ElapsedTime, yMax);

            sound.Position = mesh.Position;
            if(!estaOculto)
            {
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
