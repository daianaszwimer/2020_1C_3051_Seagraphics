using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model.Entidades
{
    class Coral : Entity
    {
        private TGCMatrix escalaBase;
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        public Recolectable Recolectable { get;  set; }

        public Coral(TgcMesh mesh) : base(mesh, meshLookDir) { }

        protected override void InitEntity()
        {
            escalaBase = TGCMatrix.Scaling(new TGCVector3(0.2f, 0.2f, 0.2f));
            mesh.Position = new TGCVector3(10, -15, 5);
            mesh.Scale = new TGCVector3(0.2f, 0.2f, 0.2f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            mesh.Transform = escalaBase * TGCMatrix.Identity * TGCMatrix.Translation(mesh.Position);
        }

        protected override void InteractEntity()
        {
            base.InteractEntity();
            Recolectable.Recolectar(ElementoRecolectable.coral, 1);    
            
        }

        protected override void RenderEntity() {
            mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() { }
    }
}
