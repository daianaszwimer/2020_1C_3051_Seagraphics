using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class TimonInterior
    {
        private TgcScene escenaMesa;
        private static TimonInterior _instance;
        protected TimonInterior()
        {
        }

        public static TimonInterior Instance()
        {
            if (_instance == null)
            {
                _instance = new TimonInterior();
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
                mesh.Scale = new TGCVector3(0.07f, 0.07f, 0.07f);
                mesh.Position = new TGCVector3(0, 15, 97);
                mesh.Transform = TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.Translation(mesh.Position);
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
