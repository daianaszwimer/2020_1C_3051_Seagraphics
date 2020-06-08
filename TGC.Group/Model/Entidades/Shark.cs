using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Collision;

namespace TGC.Group.Model.Entidades
{
    class Shark : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        private int vida;

        //private Crafting conQueMeAtacan = null; 

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
            

            Move(goalPos, speed, ElapsedTime);
        }

        protected override void RenderEntity() { 
            mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() { }

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
            base.InteractEntity();
            if (Player.Instance().puedoEnfrentarTiburon())
            {
                //reducirVidaEn(conQueMeAtacan.danio);
                reducirVidaEn(10);
                Console.WriteLine("Me ataco");
            }
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
