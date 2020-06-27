using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;

namespace TGC.Group.Model.Entidades
{
    class Metal : Entity
    {
        private TGCMatrix escalaBase;
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        public ElementoRecolectable Tipo { get; set; }

        public Metal(TgcMesh mesh) : base(mesh, meshLookDir) { }

        protected override void InitEntity()
        {
            escalaBase = TGCMatrix.Scaling(new TGCVector3(0.2f, 0.2f, 0.2f));
            mesh.Position = new TGCVector3(50, -15, 15);
            mesh.Scale = new TGCVector3(0.2f, 0.2f, 0.2f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            mesh.Transform = escalaBase * TGCMatrix.Identity * TGCMatrix.Translation(mesh.Position);
        }
        protected override void InteractEntity()
        {
            base.InteractEntity();
            if (estaOculto)
            {
                cambiarPosicion(mesh.Position.Y, 500, 60);
                estaOculto = false;
            }
            Recolectable.Recolectar(Tipo, 1);
        }

        protected override void RenderEntity() {
            //mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() { }

    }
}
