﻿using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Crafting;
using TGC.Core.Input;

namespace TGC.Group.Model.Entidades
{
    class Fish : Entity
    {
        static TGCVector3 meshLookDir = new TGCVector3(-1, 0, 0);

        public Recolectable Recolectable { get; set; }

        //Config
        const float speed = 7.5f;
        const float distanceToMove = 70f;

        //Internal vars
        TGCVector3 goalPos = TGCVector3.Empty;

        public Fish(TgcMesh mesh) : base(mesh, meshLookDir) { }

        protected override void InitEntity()
        {
            mesh.Scale = new TGCVector3(0.3f, 0.3f, 0.3f);
        }

        protected override void UpdateEntity(float ElapsedTime)
        {
            if (ArrivedGoalPos()) 
                SetRandomGoalPos();

            Move(goalPos, speed, ElapsedTime);
        }

        protected override void InteractEntity()
        {
            base.InteractEntity();
            Recolectable.Recolectar(ElementoRecolectable.fish, 1);
        }

        protected override void RenderEntity() {
            mesh.BoundingBox.Render();
        }

        protected override void DisposeEntity() { }


        //Internal functions

        private void SetRandomGoalPos()
        {
            Random r = new Random();
            var x = (float) r.NextDouble();
            var y = (float) r.NextDouble();
            var z = (float) r.NextDouble();
            goalPos = new TGCVector3(x, y, z) * distanceToMove;
        }

        private bool ArrivedGoalPos() { return Math.Abs( (goalPos - mesh.Position).Length() ) < 0.1f; }
    }
}
