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

//Luz
float3 lightPos = float3(0, 100, 0);
float3 eyePos;
float KSpecular;
float shininess;
float3 ambientColor; //Color RGB para Ambient de la luz
float3 diffuseColor; //Color RGB para Diffuse de la luz
float3 specularColor; //Color RGB para Specular de la luz
float KAmbient; // Coeficiente de Ambient
float KDiffuse; // Coeficiente de Diffuse

//Input del Vertex Shader
struct VS_INPUT_VERTEX_LIGHT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR0;
    float2 Texture : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_VERTEX_LIGHT
{
    float4 Position : POSITION0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
    float2 Texture : TEXCOORD0;
};
//Vertex Shader
VS_OUTPUT_VERTEX_LIGHT vs_main_light(VS_INPUT_VERTEX_LIGHT input)
{
    VS_OUTPUT_VERTEX_LIGHT output;
    
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texture;
    
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

    output.WorldPosition = mul(input.Position, matWorld);

    return output;
}

//Pixel Shader
float4 ps_main_light(VS_OUTPUT_VERTEX_LIGHT input) : COLOR0
{
    input.WorldNormal = normalize(input.WorldNormal);

    float3 lightDirection = normalize(lightPos - input.WorldPosition);
    float3 viewDirection = normalize(eyePos - input.WorldPosition);
    float3 halfVector = normalize(lightDirection + viewDirection);

	// Obtener texel de la textura
    float4 color = tex2D(diffuseMap, input.Texture);

	//Componente Diffuse: N dot L
    float3 NdotL = dot(input.WorldNormal, lightDirection);
    float3 diffuseLight = KDiffuse * diffuseColor * max(0.0, NdotL);

	//Componente Specular: (N dot H)^shininess
    float3 NdotH = dot(input.WorldNormal, halfVector);
    float3 specularLight = ((NdotL <= 0.0) ? 0.0 : KSpecular) * specularColor * pow(max(0.0, NdotH), shininess);

    color = float4(saturate(ambientColor * KAmbient + diffuseLight) * color + specularLight, color.a);
    
    return color;
}

// -----------------------------------------------------------------
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_light();
        PixelShader = compile ps_3_0 ps_main_light();
    }
}