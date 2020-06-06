using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model.Entidades
{
    class Fish : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        public Recolectable Recolectable { get; set; }

        //Config
        const float speed = 7.5f;
        const float distanceToMove = 100f;

        //Internal vars
        TGCVector3 goalPos = TGCVector3.Empty;

        public Fish(TgcMesh mesh) : base(mesh, meshLookDir) { }

        protected override void InitEntity()
        {
            mesh.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            if (ArrivedGoalPos()) 
                SetRandomGoalPos();

            Move(goalPos, speed, ElapsedTime);
        }

        protected override void InteractEntity()
        {
            base.InteractEntity();
            Recolectable.Recolectar(ElementoRecolectable.fish, 1);
        }

        protected override void RenderEntity() {
            mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() { }


        //Internal functions

        private void SetRandomGoalPos()
        {
            int seed = DateTime.Now.Millisecond + Int32.Parse(mesh.Name);
            Random r = new Random(seed);
            var sign = r.Next(-1, 1) >= 0 ? 1 : -1;
            var x = (float) r.NextDouble() * sign;
            var y = (float) r.NextDouble();
            var z = (float) r.NextDouble() * sign;

            y = FastMath.Min(y, 0.3f);

            goalPos = new TGCVector3(x, y, z) * distanceToMove;
        }

        private bool ArrivedGoalPos() { return Math.Abs( (goalPos - mesh.Position).Length() ) < 0.1f; }
    }
}
