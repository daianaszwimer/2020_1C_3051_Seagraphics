using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Mathematica;
using TGC.Core.Geometry;
using TGC.Core.Direct3D;

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

        public List<TGCBox> obtenerMeshes()
        {
            return paredes;
        }

        public void Init(string MediaDir)
        {
            paredes = new List<TGCBox>();
            var diffuseMap = TgcTexture.createTexture(MediaDir + "Textures//Lisas.bmp");

            // todo: usar planos para piso y techo?
            piso = TGCBox.fromExtremes(new TGCVector3(-200, -1, -200), new TGCVector3(200, 0, 200), diffuseMap);
            piso.Transform = TGCMatrix.Translation(piso.Position);

            techo = TGCBox.fromExtremes(new TGCVector3(-200, 100, -200), new TGCVector3(200, 101, 200), diffuseMap);
            techo.Transform = TGCMatrix.Translation(techo.Position);

            var paredSur = TGCBox.fromExtremes(new TGCVector3(-200, 0, -210), new TGCVector3(200, 100, -200), diffuseMap);
            paredSur.Transform = TGCMatrix.Translation(paredSur.Position);

            var paredNorte = TGCBox.fromExtremes(new TGCVector3(-200, 0, 200), new TGCVector3(200, 100, 210), diffuseMap);
            paredNorte.Transform = TGCMatrix.Translation(paredNorte.Position);

            var paredOeste = TGCBox.fromExtremes(new TGCVector3(-210, 0, -200), new TGCVector3(-200, 100, 200), diffuseMap);
            paredOeste.Transform = TGCMatrix.Translation(paredOeste.Position);

            var paredEste = TGCBox.fromExtremes(new TGCVector3(200, 0, -200), new TGCVector3(210, 100, 200), diffuseMap);
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
