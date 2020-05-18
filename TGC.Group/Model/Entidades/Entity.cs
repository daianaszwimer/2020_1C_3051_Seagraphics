﻿using Microsoft.DirectX.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Entidades
{
    class Entity
    {
        protected TgcMesh mesh;
        protected TGCVector3 defaultLookDir; //direccion a la que esta mirando el mesh al meterlo en escena
        protected TGCQuaternion rotation;

        public Entity(TgcMesh mesh, TGCVector3 defaultLookDir) { 
            this.mesh = mesh;
            this.defaultLookDir = defaultLookDir;
        }

        //GameModel functions
        public void Init() { 
            Entities.Add(this);
            InitEntity();
        }

        public void Update(float ElapsedTime) {
            UpdateEntity(ElapsedTime);
        }
        public void Render() { 
            mesh.Render(); 
            RenderEntity(); 
        }

        public void Effect(Effect effect)
        {
            mesh.Effect = effect;
        }

        public void Technique(string tec)
        {
            mesh.Technique = tec;
        }

        public void Dispose() { 
            Entities.Remove(this);
            mesh.Dispose();
            DisposeEntity();
        }

        public virtual void Interact() {
            InteractEntity();
        }


        //Override functions
        protected virtual void InitEntity() { }
        protected virtual void UpdateEntity(float ElapsedTime) { }
        protected virtual void RenderEntity() { }
        protected virtual void DisposeEntity() { }
        protected virtual void InteractEntity() { }

        //Getters
        public TgcMesh GetMesh() { return mesh; }


        //Common functinos
        /// <param name="goalPos">Posicion en el mundo a la que se quiere llegar</param>
        /// <param name="speed">Velocidad a la que la entidad se mueve</param>
        protected void Move(TGCVector3 goalPos, float speed, float ElapsedTime)
        {
            TGCVector3 dir = TGCVector3.Normalize(goalPos - mesh.Position);
            LookAt(dir);

            TGCVector3 movement = dir * speed * ElapsedTime;
            mesh.Position += movement;

            mesh.Transform = TGCMatrix.Scaling(mesh.Scale) * TGCMatrix.RotationTGCQuaternion(rotation) * TGCMatrix.Translation(mesh.Position);
        }

        /// <summary>
        /// Devuelve la rotacion que se debe aplicar a la matriz de transformacion para que la entidad apunte hacia una direccion.
        /// </summary>
        /// <param name="lookDir">Vector normalizado que define la direccion a la que debe mirar la entidad.</param>
        private void LookAt(TGCVector3 lookDir)
        {
            float angle = FastMath.Acos(TGCVector3.Dot(defaultLookDir, lookDir));
            TGCVector3 rotVector = TGCVector3.Cross(defaultLookDir, lookDir);
            rotation = TGCQuaternion.RotationAxis(rotVector, angle);
        }
    }
}
