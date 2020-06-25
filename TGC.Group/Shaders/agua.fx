/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

								   //Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

float time;
float color;

//Input del Vertex Shader
struct VS_INPUT_VERTEX
{
    float4 Position : POSITION0;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX
{
    float4 Position : POSITION0;
};

//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
    VS_OUTPUT_VERTEX output;
    
    float yPos = saturate(sin(input.Position.x + time) + cos(input.Position.z + time)) * 20;
    //yPos = max(yPos, -10);
    
    input.Position.y += yPos;
    output.Position = mul(input.Position, matWorldViewProj);
    
    return output;
}

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
    return float4(0.3, 0.7, 0.7, 0.7);
}

// ------------------------------------------------------------------
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}