using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class Arma
    {
        protected TgcMesh mesh;
        public Arma(TgcMesh mesh)
        {
            this.mesh = mesh;
            mesh.Transform = TGCMatrix.Scaling(new TGCVector3(0.5f, 0.5f, 0.5f));
        }

        public void Update(float ElapsedTime)
        {
            // logica pero no hay ninguna
        }

        public void Render()
        {
            mesh.Render();
        }

        public void Dispose()
        {
            mesh.Dispose();
        }

    }
}
