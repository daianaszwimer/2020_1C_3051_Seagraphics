using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class SogaInterior
    {
        private TgcMesh mesh;
        private static SogaInterior _instance;
        protected SogaInterior()
        {
        }

        public static SogaInterior Instance()
        {
            if (_instance == null)
            {
                _instance = new SogaInterior();
            }

            return _instance;
        }
        public void Init(TgcMesh _mesh)
        {
            mesh = _mesh;
            mesh.Position = new TGCVector3(40, 28, 0);
            mesh.Transform = TGCMatrix.Scaling(new TGCVector3(0.015f, 0.015f, 0.015f)) * TGCMatrix.Translation(mesh.Position);
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
