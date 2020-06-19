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

// variable de fogs
float4 ColorFog;
float4 CameraPos;
float StartFogDistance;
float EndFogDistance;
float Density;

//Specular
float3 lightPos = float3(10, 100, 10);
float3 eyePos;
float kS;
float shininess;

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
    float4 Normal : POSITION1;
    float4 WorldPosition : POSITION2;
    float2 Texture : TEXCOORD0;
    float4 PosView : COLOR0;
};

//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
    VS_OUTPUT_VERTEX output;

	//Proyectar posicion
    output.Position = mul(input.Position, matWorldViewProj);
    output.Normal = normalize(mul(input.Position, matInverseTransposeWorld));
    output.WorldPosition = mul(input.Position, matWorld);
    output.Texture = input.Texture;
    output.PosView = mul(input.Position, matWorldView);
    return output;
}

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 color = tex2D(diffuseMap, input.Texture);
    
    //SPECULAR
    if(kS > 0)
    {
        float3 L = normalize(lightPos - input.WorldPosition.xyz);
        float3 V = normalize(eyePos - input.WorldPosition.xyz);
        float3 H = normalize(L + V);
        float3 NdotH = dot(input.Normal.xyz, H);
        float3 light = kS * float3(1, 1, 1) * pow(max(0.0, NdotH), shininess);
        color += float4(light, 1);
    }
    
    //FOG
    float zn = StartFogDistance;
    float zf = EndFogDistance;

    
    if (input.PosView.z > zf)
    {
        color = ColorFog;
    }
    else
    {
		// combino fog y textura
        float1 total = zf - zn;
        float1 resto = input.PosView.z - zn;
        float1 proporcion = resto / total;
        
        float1 r = lerp(color.r, ColorFog.r, proporcion);
        float1 g = lerp(color.g, ColorFog.g, proporcion);
        float1 b = lerp(color.b, ColorFog.b, proporcion);
        float1 a = 1;
        
        color = float4(r, g, b, a);
    }
    
    return color;
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