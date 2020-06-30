using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    class Arma
    {
        protected TgcMesh mesh;
        private FPSCamara cam;
        private TGCVector3 posOffset = new TGCVector3(-5f, -4f, -10f);
        private TGCQuaternion rotOffset = TGCQuaternion.RotationAxis(new TGCVector3(0f, 0f, 1f), FastMath.ToRad(90f)) * TGCQuaternion.RotationAxis(TGCVector3.Up, FastMath.ToRad(90f));
        public Arma(TgcMesh mesh)
        {
            this.mesh = mesh;
            mesh.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
            cam = Player.Instance().GetCamara();
        }

        public void Update()
        {
            TGCQuaternion camRot = cam.GetRotation();
            TGCMatrix plrTransform = TGCMatrix.Translation(Player.Instance().Position());
            mesh.Transform = TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.RotationTGCQuaternion(rotOffset) * TGCMatrix.Translation(posOffset) * TGCMatrix.RotationTGCQuaternion(camRot) * plrTransform;
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
