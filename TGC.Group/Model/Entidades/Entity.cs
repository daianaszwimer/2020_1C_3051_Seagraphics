using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;
using TGC.Core.Sound;
using TGC.Group.Model.Sounds;
using System.Collections.Generic;

namespace TGC.Group.Model.Entidades
{
    class Entity
    {
        protected TgcMesh mesh;
        protected TGCVector3 defaultLookDir; //direccion a la que esta mirando el mesh al meterlo en escena
        protected TGCQuaternion rotation;
        protected float MAX_CLICK_DISTANCE = 35f;
        public Recolectable Recolectable { get; } = Recolectable.Instance();
        // todos los elementos inicialmente se muestran
        // cuando se clickean se pasa a true y despues se reutiliza y se muestra el elemento en otro lado
        // y el valor nuevamente pasa a false
        protected bool estaOculto { get; set; } = false;
        // solo el tiburon lo tiene en true
        public bool necesitaArmaParaInteractuar { get; set; } = false;
        public TgcStaticSound agarrarEfecto;

        public Entity(TgcMesh mesh, TGCVector3 defaultLookDir) {
            if (!necesitaArmaParaInteractuar)
            {
                agarrarEfecto = new TgcStaticSound();
                agarrarEfecto.loadSound(SoundsManager.Instance().mediaDir + "Sounds\\grab.wav", SoundsManager.Instance().sound);
            }
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
            if (!estaOculto)
            {
                mesh.Render();
                RenderEntity();
            }
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
            agarrarEfecto.dispose();
            DisposeEntity();
        }

        public virtual void Interact() {
            if (!estaOculto)
            {
                if (!necesitaArmaParaInteractuar || (necesitaArmaParaInteractuar && Player.Instance().puedoEnfrentarTiburon()))
                {
                    agarrarEfecto.play(false);
                    if (!necesitaArmaParaInteractuar)
                    {
                        // si se mata con arma ponemos mas suspenso y aplicamos perlin noise
                        estaOculto = true;
                    }
                    InteractEntity();
                }
            }
        }

        public void cambiarPosicion(TGCVector3 nuevaPosicion)
        {
            mesh.Position = nuevaPosicion;
        }

        public void cambiarPosicion(TGCVector3 nuevaPosicion, List<Coral> corales, List<Metal> metales)
        {
            cambiarPosicion(nuevaPosicion);
            // ver si colisiona con otros elementos y moverlo, no lo hacemos hasta que no colisione por perfomance, las chances son pocas
            // solo lo hacemos con oro y corales
            chequearColision(corales, metales);
        }

        protected virtual void chequearColision(List<Coral> corales, List<Metal> metales) { }

        /// <summary>
        /// Se cambia la posicion a una random dados unos parametros opcionales
        /// </summary>
        public void cambiarPosicion(float yPos = -1f, int maxDistance = 75, int maxOffset = 60)
        {
            var seed = DateTime.Now.Millisecond;
            Random r = new Random(seed);
            var distance = r.Next(50, maxDistance);
            var offset = r.Next(0, maxOffset);

            //Randomizar posicion
            var sign = r.Next(-1, 1) >= 0 ? 1 : -1;
            TGCVector3 nuevaPosicion = -Player.Instance().GetLookDir() * distance;
            nuevaPosicion.X += sign * offset;
            sign = r.Next(-1, 1) >= 0 ? 1 : -1;
            offset = r.Next(0, maxOffset);
            nuevaPosicion.Z += sign * offset;
            nuevaPosicion.Y = yPos < 0 ? yPos : nuevaPosicion.Y;

            cambiarPosicion(nuevaPosicion);
        }

        //Override functions
        protected virtual void InitEntity() { }
        protected virtual void UpdateEntity(float ElapsedTime) { }
        protected virtual void RenderEntity() { }
        protected virtual void DisposeEntity() { }
        protected virtual void InteractEntity() { }

        //Getters
        public TgcMesh GetMesh() { return mesh; }
        public TGCVector3 Position() { return mesh.Position; }
        public float GetMaxClickDistance() { return MAX_CLICK_DISTANCE; }
        //Common functinos
        /// <param name="goalPos">Posicion en el mundo a la que se quiere llegar</param>
        /// <param name="speed">Velocidad a la que la entidad se mueve</param>
        protected void Move(TGCVector3 goalPos, float speed, float ElapsedTime, float limiteY = -1f)
        {
            TGCVector3 dir = TGCVector3.Normalize(goalPos - mesh.Position);
            LookAt(dir);

            TGCVector3 movement = dir * speed * ElapsedTime;
            mesh.Position += movement;

            if (limiteY != -1 && mesh.Position.Y >= (limiteY - 30f))
            {
                // si hay limite lo pongo ahi, un poco mas abajo
                mesh.Position = new TGCVector3(mesh.Position.X, limiteY - 30f, mesh.Position.Z);
            }
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
            rotVector.Z = 0;

            rotation = TGCQuaternion.RotationAxis(rotVector, angle);
        }
    }
}
