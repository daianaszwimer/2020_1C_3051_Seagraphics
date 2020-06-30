using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class LamparaNave
    {
        private TgcMesh mesh;

        public LamparaNave(TgcMesh _mesh)
        {
            mesh = _mesh;
            mesh.Position = new TGCVector3(0, 100, 0);
            mesh.Transform = TGCMatrix.Scaling(new TGCVector3(2, 2, 2)) * TGCMatrix.Translation(mesh.Position);
        }

        public void Effect(Effect effect)
        {
            mesh.Effect = effect;
        }

        public void Technique(string tec)
        {
            mesh.Technique = tec;
        }

        public void Update()
        {
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
