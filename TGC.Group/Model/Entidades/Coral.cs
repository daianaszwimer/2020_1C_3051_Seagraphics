using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;
using System.Collections.Generic;
using TGC.Core.Collision;

namespace TGC.Group.Model.Entidades
{
    class Coral : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        public Coral(TgcMesh mesh) : base(mesh, meshLookDir) { }

        protected override void InitEntity()
        {
            mesh.Position = new TGCVector3(10, -15, 5);
            mesh.Scale = new TGCVector3(0.2f, 0.2f, 0.2f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            mesh.Transform = TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.Identity * TGCMatrix.Translation(mesh.Position);
        }

        protected override void InteractEntity()
        {
            base.InteractEntity();
            if (estaOculto)
            {
                cambiarPosicion(mesh.Position.Y, 300, 100);
                estaOculto = false;
            }
            Recolectable.Recolectar(ElementoRecolectable.coral, 1);
            
        }

        protected override void RenderEntity() {
            //mesh.BoundingBox.Render();
        }

        protected override void chequearColision(List<Coral> corales, List<Metal> metales)
        {

            bool collided = false;
            foreach (var coral in corales)
            {
                var result = TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, coral.GetMesh().BoundingBox);
                if (result)
                {
                    collided = true;
                    break;
                }
            }
            //si colisiona lo muevo
            if (collided)
            {
                mesh.Position += new TGCVector3(5, 0, 5);
                return;
            }
            foreach (var metal in metales)
            {
                var result = TgcCollisionUtils.testAABBAABB(mesh.BoundingBox, metal.GetMesh().BoundingBox);
                if (result)
                {
                    collided = true;
                    break;
                }
            }
            //si colisiona lo muevo
            if (collided)
            {
                mesh.Position += new TGCVector3(13, 0, 13);
                return;
            }
        }
        protected override void DisposeEntity() { }
    }
}
