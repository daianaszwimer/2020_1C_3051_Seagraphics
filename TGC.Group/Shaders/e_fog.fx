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

texture sceneFrameBuffer;
sampler SceneFrameBuffer =
sampler_state
{
    Texture = <sceneFrameBuffer>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture fBCoralesBrillantes;
sampler FBCoralesBrillantes =
sampler_state
{
    Texture = (fBCoralesBrillantes);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

texture verticalBlurFrameBuffer;
sampler VerticalBlurFrameBuffer =
sampler_state
{
    Texture = (verticalBlurFrameBuffer);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

texture textura_mascara;
sampler2D samplerMascara = sampler_state
{
    Texture = (textura_mascara);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};

// variable de fogs
float4 ColorFog;
float4 CameraPos;
float StartFogDistance;
float EndFogDistance;
float Density;

float nivelAgua;
float health;

//Luz
float3 lightPos = float3(10, 100, 10);
float3 eyePos;
float KSpecular;
float shininess;
float3 ambientColor; //Color RGB para Ambient de la luz
float3 diffuseColor; //Color RGB para Diffuse de la luz
float3 specularColor; //Color RGB para Specular de la luz
float KAmbient; // Coeficiente de Ambient
float KDiffuse; // Coeficiente de Diffuse
float screen_dx, screen_dy;
static const int radius = 7;
static const int kernelSize = 15;
static const float kernel[kernelSize] =
{
    0.000489, 0.002403, 0.009246, 0.02784, 0.065602, 0.120999, 0.174697, 0.197448, 0.174697, 0.120999, 0.065602, 0.02784, 0.009246, 0.002403, 0.000489
};

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
    float4 WorldPosition : TEXCOORD1;
    float4 PosView : COLOR0;
};
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
    float4 PosView : COLOR0;
};

struct VS_INPUT_BLOOM
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
};
struct VS_OUTPUT_BLOOM
{
    float4 Position : POSITION0;
    float4 MeshPosition : TEXCOORD1;
    float2 Texture : TEXCOORD0;
};
struct VS_INPUT_BLUR
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
};
struct VS_OUTPUT_BLUR
{
    float4 Position : POSITION0;
    float2 Texture : TEXCOORD0;
};
//Vertex Shader
VS_OUTPUT_VERTEX vs_main(VS_INPUT_VERTEX input)
{
    VS_OUTPUT_VERTEX output;

    output.Position = mul(input.Position, matWorldViewProj);
    output.WorldPosition = mul(input.Position, matWorld);
    output.Texture = input.Texture;
    output.PosView = mul(input.Position, matWorldView);
    return output;
}

//Vertex Shader
VS_OUTPUT_VERTEX_LIGHT vs_main_light(VS_INPUT_VERTEX_LIGHT input)
{
    VS_OUTPUT_VERTEX_LIGHT output;
    
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texture;
    output.PosView = mul(input.Position, matWorldView);
    
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

    output.WorldPosition = mul(input.Position, matWorld);

    return output;
}
VS_OUTPUT_BLOOM vs_bloom(VS_INPUT_BLOOM input)
{
    VS_OUTPUT_BLOOM output;
    
    output.MeshPosition = input.Position;
    output.Position = mul(input.Position, matWorldViewProj);
    output.Texture = input.Texture;

    return output;
}
VS_OUTPUT_BLUR vs_blur(VS_INPUT_BLUR input)
{
    VS_OUTPUT_BLUR output;
    
    output.Position = input.Position;
    output.Texture = input.Texture;

    return output;
}

float4 fogEffect(float positionViewZ, float4 fvBaseColor, float yPos)
{
    float zn = StartFogDistance;
    float zf = EndFogDistance;

    if (positionViewZ < zn || yPos > nivelAgua)
        return fvBaseColor;
    else if (positionViewZ > zf)
    {
        fvBaseColor = ColorFog;
        return fvBaseColor;
    }
    else
    {
		// combino fog y textura
        float1 total = zf - zn;
        float1 resto = positionViewZ - zn;
        float1 proporcion = resto / total;
        float1 r = lerp(fvBaseColor.r, ColorFog.r, proporcion);
        float1 g = lerp(fvBaseColor.g, ColorFog.g, proporcion);
        float1 b = lerp(fvBaseColor.b, ColorFog.b, proporcion);
        return float4(r, g, b, 1);
    }
}

//Pixel Shader
float4 ps_main(VS_OUTPUT_VERTEX input) : COLOR0
{
    float4 fvBaseColor = tex2D(diffuseMap, input.Texture);
    return fogEffect(input.PosView.z, fvBaseColor, input.WorldPosition.y);

}

float4 ps_bloom(VS_OUTPUT_BLOOM input) : COLOR0
{
    float4 texel = tex2D(diffuseMap, input.Texture);
    float4 colorNegro = float4(0, 0, 0, 1);
    
    float distanciaCentro = distance(input.MeshPosition.xyz, float3(0, -20, 0));
    float fact = saturate(distanciaCentro / 120);
    float4 color = lerp(colorNegro, texel, fact);
    return color;
}

float4 ps_mask(VS_OUTPUT_VERTEX input) : COLOR0
{
    return 0;
}

float4 vs_blur_vertical(VS_OUTPUT_BLUR input) : COLOR0
{
    float4 verticalSum = float4(0, 0, 0, 1);
    for (int y = 0; y < kernelSize; y++)
    {
        float2 delta = float2(0, (y - radius + 1) / screen_dy);
        verticalSum += tex2D(FBCoralesBrillantes, input.Texture + delta) * kernel[y];
    }
    
    return verticalSum;
}
float4 vs_blur_horizontal(VS_OUTPUT_BLUR input) : COLOR0
{
    float4 horizontalSum = float4(0, 0, 0, 1);
    for (int x = 0; x < kernelSize; x++)
    {
        float2 delta = float2((x - radius + 1) / screen_dx, 0);
        horizontalSum += tex2D(FBCoralesBrillantes, input.Texture + delta) * kernel[x];
    }
    return horizontalSum;
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
    
    return fogEffect(input.PosView.z, color, input.WorldPosition.y);
}

//Input del Vertex Shader
struct VS_INPUT_POSTPROCESS
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_POSTPROCESS
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

//Vertex Shader
VS_OUTPUT_POSTPROCESS VSPostProcess(VS_INPUT_POSTPROCESS input)
{
    VS_OUTPUT_POSTPROCESS output;

	// Propagamos la posicion, ya que esta en espacio de pantalla
    output.Position = input.Position;

	// Propagar coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

//Pixel Shader
float4 PSPostProcess(VS_OUTPUT_POSTPROCESS input) : COLOR0
{
    float4 color = tex2D(SceneFrameBuffer, input.TextureCoordinates);
    return color;
}
float4 PSPostProcessMar(VS_OUTPUT_POSTPROCESS input) : COLOR0
{
    float4 color = tex2D(SceneFrameBuffer, input.TextureCoordinates);
    float4 colorMascara = tex2D(samplerMascara, input.TextureCoordinates);
    float4 bloomColor = tex2D(VerticalBlurFrameBuffer, input.TextureCoordinates);
    bloomColor *= 2;
    
    //Escala de gris si tiene poca vida
    float fact = clamp(health * 1.5 / 100,0.35,1);
    color.rgb = lerp(color.r, color.rgb, fact);
    
    //Bloom + mascara
    color = colorMascara ? colorMascara : color + bloomColor;
    
    return color;
}
float4 PSPostProcessMenu(VS_OUTPUT_POSTPROCESS input) : COLOR0
{
    float4 color = tex2D(SceneFrameBuffer, input.TextureCoordinates);
    float4 bloomColor = tex2D(VerticalBlurFrameBuffer, input.TextureCoordinates);
    bloomColor *= 2;
    return color + bloomColor;

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
technique RenderSceneLight
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main_light();
        PixelShader = compile ps_3_0 ps_main_light();
    }
}

technique Bloom
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_bloom();
        PixelShader = compile ps_3_0 ps_bloom();
    }
}

technique BlurVertical
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_blur();
        PixelShader = compile ps_3_0 vs_blur_vertical();
    }
}
technique BlurHorizontal
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_blur();
        PixelShader = compile ps_3_0 vs_blur_horizontal();
    }
}

technique BloomMask
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_mask();
    }
}

technique PostProcess
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSPostProcess();
        PixelShader = compile ps_3_0 PSPostProcess();
    }
}

technique PostProcessMar
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSPostProcess();
        PixelShader = compile ps_3_0 PSPostProcessMar();
    }
}
technique PostProcessMenu
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSPostProcess();
        PixelShader = compile ps_3_0 PSPostProcessMenu();
    }
}