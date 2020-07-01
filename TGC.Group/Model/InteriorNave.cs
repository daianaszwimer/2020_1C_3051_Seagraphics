using System.Collections.Generic;
using TGC.Core.Textures;
using TGC.Core.Mathematica;
using TGC.Core.Geometry;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    class InteriorNave // es un singleton
    {
        private static InteriorNave _instance;
        private List<TGCBox> paredes;
        private TGCBox piso, techo;

        protected InteriorNave()
        {
        }

        public static InteriorNave Instance()
        {
            if (_instance == null)
            {
                _instance = new InteriorNave();
            }

            return _instance;
        }

        public List<TGCBox> obtenerParedes()
        {
            return paredes;
        }

        public void Effect(Effect effect)
        {
            foreach (var mesh in paredes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Effect = effect;
            }

            piso.Effect = effect;
            techo.Effect = effect;
        }

        public void Technique(string tec)
        {
            foreach (var mesh in paredes)
            {
                if (mesh == null)
                {
                    break;
                }
                mesh.Technique = tec;
            }
            piso.Technique = tec;
            techo.Technique = tec;
        }

        public void Init(string MediaDir)
        {
            paredes = new List<TGCBox>();
            var diffuseMap = TgcTexture.createTexture(MediaDir + "Textures//Lisas.bmp");

            // todo: usar planos para piso y techo?
            piso = TGCBox.fromExtremes(new TGCVector3(-100, -1, -100), new TGCVector3(100, 0, 100), diffuseMap);
            piso.Transform = TGCMatrix.Translation(piso.Position);

            techo = TGCBox.fromExtremes(new TGCVector3(-100, 100, -100), new TGCVector3(100, 101, 100), diffuseMap);
            techo.Transform = TGCMatrix.Translation(techo.Position);

            var paredSur = TGCBox.fromExtremes(new TGCVector3(-100, 0, -110), new TGCVector3(100, 100, -100), diffuseMap);
            paredSur.Transform = TGCMatrix.Translation(paredSur.Position);

            var paredNorte = TGCBox.fromExtremes(new TGCVector3(-100, 0, 100), new TGCVector3(100, 100, 110), diffuseMap);
            paredNorte.Transform = TGCMatrix.Translation(paredNorte.Position);

            var paredOeste = TGCBox.fromExtremes(new TGCVector3(-110, 0, -100), new TGCVector3(-100, 100, 100), diffuseMap);
            paredOeste.Transform = TGCMatrix.Translation(paredOeste.Position);

            var paredEste = TGCBox.fromExtremes(new TGCVector3(100, 0, -100), new TGCVector3(110, 100, 100), diffuseMap);
            paredEste.Transform = TGCMatrix.Translation(paredEste.Position);

            paredes.Add(paredSur);
            paredes.Add(paredNorte);
            paredes.Add(paredEste);
            paredes.Add(paredOeste);
        }

        public void Update()
        {

        }
        public void Render()
        {
            foreach (var mesh in paredes)
            {
                mesh.Render();
            }
            piso.Render();
            techo.Render();
        }
        public void Dispose()
        {
            foreach (var mesh in paredes)
            {
                mesh.Dispose();
            }
            piso.Dispose();
            techo.Dispose();
        }
    }
}
