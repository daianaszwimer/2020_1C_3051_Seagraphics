using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class SillaInterior
    {
        private TgcScene escenaMesa;
        private static SillaInterior _instance;
        protected SillaInterior()
        {
        }

        public static SillaInterior Instance()
        {
            if (_instance == null)
            {
                _instance = new SillaInterior();
            }

            return _instance;
        }

        public List<TgcMesh> meshes()
        {
            return escenaMesa.Meshes;
        }

        public void Init(TgcScene escena)
        {
            escenaMesa = escena;
            foreach (var mesh in escenaMesa.Meshes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Scale = new TGCVector3(0.7f, 0.7f, 0.7f);
                mesh.Position = new TGCVector3(40, 0, -40);
                mesh.Transform = TGCMatrix.Scaling(new TGCVector3(0.3f, 0.3f, 0.3f)) * TGCMatrix.Translation(mesh.Position);
            }
        }

        public void Effect(Effect effect)
        {
            foreach (var mesh in escenaMesa.Meshes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Effect = effect;
            }
        }

        public void Technique(string tec)
        {
            foreach (var mesh in escenaMesa.Meshes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Technique = tec;
            }
        }

        public void Update()
        {
        }

        public void Render()
        {
            foreach (var mesh in escenaMesa.Meshes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Render();
                //mesh.BoundingBox.Render();
            }
        }

        public void Dispose()
        {
            foreach (var mesh in escenaMesa.Meshes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Dispose();
            }
        }

    }
}
