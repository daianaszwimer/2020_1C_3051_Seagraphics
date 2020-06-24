using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Particle;

namespace TGC.Group.Model
{
    static class Particulas
    {
        struct Emisor
        {
            public ParticleEmitter particleEmitter;
            public TGCVector3 defaultPosition;
            public void SetOffset(TGCVector3 offset) { particleEmitter.Position = defaultPosition + offset; }
            public void SetPosition(TGCVector3 newPos) { defaultPosition = newPos; }
            public void Render(float ElapsedTime) { particleEmitter.render(ElapsedTime); }
            public void Dispose() { particleEmitter.dispose(); }
        }

        static Emisor[] Emitters;

        static public void Init(string MediaDir, int CantidadEmisores)
        {
            //Definir tamaño del array de emisores de particulas
            Emitters = new Emisor[CantidadEmisores];

            //Crear emisores de particular en posiciones randoms
            for (int i = 0; i < CantidadEmisores; i++)
            {
                
                ParticleEmitter particleEmitter = new ParticleEmitter(MediaDir + "\\Textures\\bubbles.png", 20);
                particleEmitter.CreationFrecuency = GetRandomFloat(.1f,.3f,i);
                particleEmitter.MaxSizeParticle = .25f;
                particleEmitter.MinSizeParticle = .1f;
                particleEmitter.ParticleTimeToLive = GetRandomFloat(7f,10f,i);
                particleEmitter.Speed = TGCVector3.Up * GetRandomFloat(9.5f,10f,i);
                particleEmitter.Playing = true;

                //Randomizar posicion
                particleEmitter.Position = GetRandomPosition(i);

                //Generar estructura de emisor
                Emisor emitter = new Emisor();
                emitter.particleEmitter = particleEmitter;
                emitter.defaultPosition = particleEmitter.Position;

                //Agregar emisor
                Emitters[i] = emitter;
            }
        }

        static public void Update(float time) {
            bool cambiarPosicion = FastMath.Sin(time / 2) >= 0.7;

            //Animar generacion de burbujas (que haya distancia horizontal entre ellas)
            Random r = new Random();
            for (int i= 1; i<Emitters.Length; i++)
            {
                if (cambiarPosicion)
                    Emitters[i].SetPosition(GetRandomPosition(i));
                float x = (float)r.NextDouble();
                float z = (float)r.NextDouble();
                Emitters[i].SetOffset(new TGCVector3(x, 0, z ) * 3);
            }
        }

        static public void Render(float elapsedTime)
        {
            foreach(Emisor emitter in Emitters)
                emitter.Render(elapsedTime);
        }


        static public void Dispose()
        {
            foreach (Emisor emitter in Emitters)
                emitter.Dispose();
        }

        //Internal functions
        static TGCVector3 GetRandomPosition(int seed = 1)
        {
            Random r = new Random(DateTime.Now.Millisecond + seed);
            var xOffset = r.Next(40, 250);
            var zOffset = r.Next(40, 250);
            var xSign = r.Next(-1, 1) >= 0 ? 1 : -1;
            var zSign = r.Next(-1, 1) >= 0 ? 1 : -1;
            TGCVector3 offset = new TGCVector3(xOffset * xSign, 0, zOffset * zSign);
            return new TGCVector3(0, -12, 0) + offset;
        }

        static float GetRandomFloat(float min, float max, int seed = 1)
        {
            Random r = new Random(DateTime.Now.Millisecond + seed);
            float fact = (float)r.NextDouble();
            return min + fact * (max - min);
        }
    }
}
