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

texture textura_perlin;
sampler2D samplerPerlin = sampler_state
{
    Texture = (textura_perlin);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

float value;
//Input del Vertex Shader
struct VS_INPUT_VERTEX
{
    float4 Position : POSITION0;
    float3 Texture : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
    VS_OUTPUT_VERTEX output;

    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texture;
    return output;
}

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 fvBaseColor = tex2D(diffuseMap, input.Texture);
    float4 perlinColor = tex2D(samplerPerlin, input.Texture);
    if (perlinColor.r < value) // no quiero que sea ciclico
    {
        discard;
    }
    return fvBaseColor;

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
