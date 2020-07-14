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

        public bool estoyVivo() { return !meAtaco; }

        //Config
        const float NORMAL_SPEED = 20f;
        const float ESCAPE_SPEED = NORMAL_SPEED * 1.4f;
        const float DAMAGE = 30f;
        float speed = NORMAL_SPEED;
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
            mesh.Scale = new TGCVector3(0.2f, 0.2f, 0.2f);
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
            
                Move(goalPos, speed, ElapsedTime);

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

        protected override void ResetMove()
        {
            SetRandomGoalPos();
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
            var y = FastMath.Min(yMax, Player.Instance().Position().Y);

            TGCVector3 playerPos = Player.Instance().Position();
            playerPos.Y = y;

            mesh.Position = playerPos + new TGCVector3(x, 0, z) * 250f;
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

        public void setearAlturaMaxima(float nivelDelAgua) { yMax = nivelDelAgua; }
        //Internal functions

        private void SetRandomGoalPos()
        {
            int seed = DateTime.Now.Millisecond;
            Random r = new Random(seed);
            var sign = r.Next(-1, 1) >= 0 ? 1 : -1;
            var x = (float)r.NextDouble() * sign;
            var y = (float)r.NextDouble();
            var z = (float)r.NextDouble() * sign;

            y = FastMath.Min(y, 0.25f);

            goalPos = new TGCVector3(x, -y, z) * distanceToEscape;
            goalPos.Y = FastMath.Min(FastMath.Max(goalPos.Y, 10), 61f); //para que nade por debajo del suelo ni por encima del agua
        }

        private void SetPlayerGoalPos() { 
            TGCVector3 newGoal = Player.Instance().Position();
            newGoal.Y = FastMath.Min(FastMath.Max(Player.Instance().Position().Y, 10), 61f);
            goalPos = newGoal;

            speed = NORMAL_SPEED;
        }

        private void SetEscapeGoalPos()
        {
            TGCVector3 dir = mesh.Position - Player.Instance().Position();
            dir = TGCVector3.Normalize(dir);
            goalPos = dir * distanceToEscape;

            goalPos.Y = FastMath.Min(FastMath.Max(goalPos.Y, 10), 61f);

            speed = ESCAPE_SPEED;
      }

        private bool ArrivedGoalPos() { return Math.Abs((goalPos - mesh.Position).Length()) < 0.5f; }

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
