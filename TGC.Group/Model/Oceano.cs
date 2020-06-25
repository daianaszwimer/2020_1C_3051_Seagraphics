using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;

namespace TGC.Group.Model
{
    static class Oceano
    {
        static VertexBuffer vertexBuffer;
        static int totalVertices;
        static Effect effect;
        static public void Init(TGCVector3 centro, int nivelTeselado, float escalaXZ, string ShadersDir)
        {
            //Inicializar efecto
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "agua.fx");
            effect.Technique = "RenderScene";

            //Inicializar vertex buffer
            int nivelTeseladoX = nivelTeselado;
            int nivelTeseladoZ = nivelTeselado;
            totalVertices = 6 * nivelTeseladoX * nivelTeseladoZ;
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionOnly), totalVertices,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionOnly.Format, Pool.Default);

            //Inicializar y llenar data para meterla en el vertexBuffer
            int dataIndex = 0;
            CustomVertex.PositionOnly[] data = new CustomVertex.PositionOnly[totalVertices];

            centro.X = centro.X * escalaXZ - nivelTeseladoX / 2 * escalaXZ;
            centro.Z = centro.Z * escalaXZ - nivelTeseladoZ / 2 * escalaXZ;

            for (var i = 0; i < nivelTeseladoX; i++)
            {
                for (var j = 0; j < nivelTeseladoZ; j++)
                {
                    //Vertices
                    var v1 = new TGCVector3(centro.X + i * escalaXZ, centro.Y, centro.Z + j * escalaXZ);
                    var v2 = new TGCVector3(centro.X + i * escalaXZ, centro.Y, centro.Z + (j + 1) * escalaXZ);
                    var v3 = new TGCVector3(centro.X + (i + 1) * escalaXZ, centro.Y, centro.Z + j * escalaXZ);
                    var v4 = new TGCVector3(centro.X + (i + 1) * escalaXZ, centro.Y, centro.Z + (j + 1) * escalaXZ);

                    //Cargar triangulo 1
                    data[dataIndex] = new CustomVertex.PositionOnly(v1);
                    data[dataIndex + 1] = new CustomVertex.PositionOnly(v2);
                    data[dataIndex + 2] = new CustomVertex.PositionOnly(v4);

                    //Cargar triangulo 2
                    data[dataIndex + 3] = new CustomVertex.PositionOnly(v1);
                    data[dataIndex + 4] = new CustomVertex.PositionOnly(v4);
                    data[dataIndex + 5] = new CustomVertex.PositionOnly(v3);

                    dataIndex += 6;
                }
            }

            //Meter en vertex buffer
            vertexBuffer.SetData(data, 0, LockFlags.None);
        }

        static public void Update(float time)
        {
            effect.SetValue("time", time);
        }

        static public void Render()
        {
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            TGCShaders.Instance.SetShaderMatrix(effect, TGCMatrix.Identity);
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionOnly.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
            effect.EndPass();
            effect.End();
        }

        static public void Dispose()
        {
            vertexBuffer.Dispose();
        }

    }
}
