#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// world / view / projection matrix
matrix WorldViewProjection;

// world matrix
matrix World;

// inverse transpose world matrix
float3x3 WorldInverseTranspose;

float3 EyePosition;

// diffuse color
float4 DiffuseColor = float4(1, 1, 1, 1);

// emissive
float3 EmissiveColor = float3(0, 0, 0);

// specular
float SpecularPower;
float3 SpecularColor = float3(1, 1, 1);

// fog
float4 FogVector;
float3 FogColor;

// main texture
texture AlbedoMap;

// normal map texture
texture NormalMap;

// are we using texture?
bool AlbedoEnabled = false;

#define ShadowFilterSamples 5

// max lights count
#define MAX_LIGHTS_COUNT 3


// light sources.
// note: 
//	- lights with range 0 = directional lights (in which case light pos is direction).
//	- lights with intensity 0 = disabled lights.
float3 LightDiffuseA;
float3 LightDirectionA;
float3 LightSpecularA;
float3 LightDiffuseB;
float3 LightDirectionB;
float3 LightSpecularB;
float3 LightDiffuseC;
float3 LightDirectionC;
float3 LightSpecularC;

matrix ShadowViewProjection;
float DepthBias = 0;

// how many active lights we have
int ActiveLightsCount = 0;

// main texture sampler
sampler2D MainTextureSampler = sampler_state {
	Texture = (AlbedoMap);
};

// normal texture sampler
sampler2D NormalTextureSampler = sampler_state {
	Texture = (NormalMap);
	MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;  
    AddressU  = Clamp;
    AddressV  = Clamp;
};

// normal texture sampler
sampler2D ShadowMapSampler = sampler_state {
	Texture = (ShadowMap);
	MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;  
    AddressU  = Clamp;
    AddressV  = Clamp;
};

// vertex shader input N
struct VertexShaderInput
{
	float4 Position : SV_POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderInputVc
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
};

struct VertexShaderInputVcUv
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderInputUv
{
	float4 Position : SV_POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : NORMAL0;
};

// TBN
struct VertexShaderInputTBN
{
	float4 Position : SV_POSITION0;
	float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
};

struct VertexShaderInputVcTBN
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
};

struct VertexShaderInputVcUvTBN
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
};

struct VertexShaderInputUvTBN
{
	float4 Position : SV_POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
};

// vertex shader output
struct VertexShaderOutputN
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 TextureCoordinate : TEXCOORD0;
	float4 WorldPos : TEXCOORD1;
	float4 ShadowPos : TEXCOORD2;
	float3 Normal : TEXCOORD3;
};

// vertex shader output
struct VertexShaderOutputTBN
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 TextureCoordinate : TEXCOORD0;
	float4 WorldPos : TEXCOORD1;
	float4 ShadowPos : TEXCOORD2;
	float3 Normal : TEXCOORD3;
	float3 Tangent : TEXCOORD4;
	float3 Binormal : TEXCOORD5;
};

// main vertex shader for flat lighting(no Uv-coords, no vertex colors)
VertexShaderOutputN VSVc(in VertexShaderInputVc input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(input.Color.rgb, input.Color.a * DiffuseColor.a);

	output.TextureCoordinate = float3(0, 0, saturate(dot(input.Position, FogVector)));
	output.WorldPos = mul(input.Position, World);
	output.ShadowPos = mul(output.WorldPos, ShadowViewProjection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputN VSUv(in VertexShaderInputUv input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color =  float4(1, 1, 1, DiffuseColor.a);

	output.TextureCoordinate = float3(input.TextureCoordinate, saturate(dot(input.Position, FogVector)));
	output.WorldPos = mul(input.Position, World);
	output.ShadowPos = mul(output.WorldPos, ShadowViewProjection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputN VSVcUv(in VertexShaderInputVcUv input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color =  float4(input.Color.rgb, DiffuseColor.a);

	output.TextureCoordinate = float3(input.TextureCoordinate, saturate(dot(input.Position, FogVector)));
	output.WorldPos = mul(input.Position, World);
	output.ShadowPos = mul(output.WorldPos, ShadowViewProjection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputTBN VSUvTBN(in VertexShaderInputUvTBN input)
{
	VertexShaderOutputTBN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(1, 1, 1, DiffuseColor.a);

	output.TextureCoordinate = float3(input.TextureCoordinate, saturate(dot(input.Position, FogVector)));
	output.WorldPos = mul(input.Position, World);
	output.ShadowPos = mul(output.WorldPos, ShadowViewProjection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Binormal = mul(input.Binormal, WorldInverseTranspose);
	output.Tangent = mul(input.Tangent, WorldInverseTranspose);
	
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputTBN VSVcUvTBN(in VertexShaderInputVcUvTBN input)
{
	VertexShaderOutputTBN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(input.Color.rgb, input.Color.a * DiffuseColor.a);

	output.TextureCoordinate = float3(input.TextureCoordinate, saturate(dot(input.Position, FogVector)));
	output.WorldPos = mul(input.Position, World);
	output.ShadowPos = mul(output.WorldPos, ShadowViewProjection);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Binormal = mul(input.Binormal, WorldInverseTranspose);
	output.Tangent = mul(input.Tangent, WorldInverseTranspose);
	
	return output;
}

struct ColorPair
{
    float3 Diffuse;
    float3 Specular;
};


ColorPair ComputeLights(float3 eyePos, float3 worldNormal, float3 shadowContribution)
{
    float3x3 lightDiffuse = 0;
    float3x3 lightSpecular = 0;
	float3 V = normalize(eyePos);
    float3 diffuse = 0;
    float3 specular = 0;

    for (int i = 0; i < ActiveLightsCount; i++)
    {
        lightDiffuse[i] = float3x3(LightDiffuseA * shadowContribution, LightDiffuseB, LightDiffuseC)[i];
        lightSpecular[i] = float3x3(LightSpecularA * shadowContribution, LightSpecularB, LightSpecularC)[i];

		float3 L = -normalize(float3x3(LightDirectionA, LightDirectionB, LightDirectionC)[i]);
		diffuse[i] = saturate(dot(L, worldNormal));
        specular[i] = pow(saturate(dot(worldNormal, normalize(L + V))), SpecularPower);
    }

    ColorPair result;
    
    result.Diffuse  = mul(diffuse,  lightDiffuse) * DiffuseColor.rgb + EmissiveColor;
    result.Specular = mul(specular,  lightSpecular) * SpecularColor;

    return result;
}

	// Calculates the shadow term using PCF with edge tap smoothing
float CalcShadowTermSoftPCF(float fLightDepth, float ndotl, float2 vTexCoord, int iSqrtSamples)
{

    float fShadowTerm = 0.0f;
    
    float variableBias = DepthBias;

    float shadowMapSize = 2048;

    float fRadius = iSqrtSamples - 1; //mad(iSqrtSamples, 0.5, -0.5);//(iSqrtSamples - 1.0f) / 2;

	if (vTexCoord.x < 0 || vTexCoord.x > 1 || vTexCoord.y < 0 || vTexCoord.y > 1) return 1;

    for (float y = -fRadius; y <= fRadius; y++)
    {
        for (float x = -fRadius; x <= fRadius; x++)
        {
            float2 vOffset = 0;
            vOffset = float2(x, y);
            vOffset /= shadowMapSize;
            //vOffset *= 2;
            //vOffset /= variableBias*200;
            float2 vSamplePoint = vTexCoord + vOffset;

            float fDepth = tex2D(ShadowMapSampler, vSamplePoint).x;
            float fSample = (fLightDepth <= fDepth + variableBias);
            
            // Edge tap smoothing
            float xWeight = 1;
            float yWeight = 1;
            
            if (x == -fRadius)
                xWeight = 1 - frac(vTexCoord.x * shadowMapSize);
            else if (x == fRadius)
                xWeight = frac(vTexCoord.x * shadowMapSize);
                
            if (y == -fRadius)
                yWeight = 1 - frac(vTexCoord.y * shadowMapSize);
            else if (y == fRadius)
                yWeight = frac(vTexCoord.y * shadowMapSize);
                
            fShadowTerm += fSample * xWeight * yWeight;
        }
    }
    
    fShadowTerm /= (fRadius*fRadius*4);
    
    return saturate(fShadowTerm);
}


// main pixel shader for flat lighting
float4 PS(VertexShaderOutputN input) : COLOR
{
	// pixel color to return
	float4 retColor = AlbedoEnabled ? tex2D(MainTextureSampler, input.TextureCoordinate.xy) : 1.0f;
	float3 N = normalize(input.Normal);

	// apply vertex lighting
	retColor *= input.Color;

	// process directional lights
    ColorPair lightResult = ComputeLights(EyePosition - input.WorldPos.xyz, N, 1);

	// apply diffuse
	retColor.rgb *= lightResult.Diffuse;

	// apply specular
	retColor.rgb += lightResult.Specular * retColor.a;

	// apply fog
	retColor.rgb = lerp(retColor.rgb, FogColor * retColor.a, input.TextureCoordinate.z);
	
	// apply alpha
	retColor.a *= DiffuseColor.a;
	
	// return final
	return retColor;
}

// main pixel shader for flat lighting with shadows
float4 PS_S(VertexShaderOutputN input) : COLOR
{
	// pixel color to return
	float4 retColor = AlbedoEnabled ? tex2D(MainTextureSampler, input.TextureCoordinate.xy) : 1.0f;
	float3 N = normalize(input.Normal);

	// apply vertex lighting
	retColor *= input.Color;

	float2 ShadowTexCoord = mad(0.5f , input.ShadowPos.xy / input.ShadowPos.w , float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	// Get the current depth stored in the shadow map
    float ourdepth = (input.ShadowPos.z / input.ShadowPos.w);

	float shadowContribution = CalcShadowTermSoftPCF(ourdepth, dot(N, LightDirectionA), ShadowTexCoord, ShadowFilterSamples);	

	// process directional lights
    ColorPair lightResult = ComputeLights(EyePosition - input.WorldPos.xyz, N, shadowContribution);

	// apply diffuse
	retColor.rgb *= lightResult.Diffuse;

	// apply specular
	retColor.rgb += lightResult.Specular * retColor.a;

	// apply fog
	retColor.rgb = lerp(retColor.rgb, FogColor * retColor.a, input.TextureCoordinate.z);
	
	// apply alpha
	retColor.a *= DiffuseColor.a;
	
	// return final
	return retColor;
}

// main pixel shader for normal-mapped lighting 
float4 PSTBN(VertexShaderOutputTBN input) : COLOR
{
	// pixel color to return
	float4 retColor = AlbedoEnabled ? tex2D(MainTextureSampler, input.TextureCoordinate.xy) : 1.0f;

	// apply vertex lighting
	retColor *= input.Color;

	// get normal from texture
	float3 fragNormal = 2.0 * (tex2D(NormalTextureSampler, input.TextureCoordinate.xy).rgb) - 1.0;
	fragNormal.x *= -1;		// <-- fix X axis to be standard.

	//Calculate surface normal
	float3 N = normalize((fragNormal.z * input.Normal) + (fragNormal.x * input.Binormal) + (fragNormal.y * -input.Tangent));

	// process directional lights
    ColorPair lightResult = ComputeLights( EyePosition - input.WorldPos.xyz, N, 1);

	// apply diffuse
	retColor.rgb *= lightResult.Diffuse;

	// apply specular
	retColor.rgb += lightResult.Specular * retColor.a;

	// apply fog
	retColor.rgb = lerp(retColor.rgb, FogColor * retColor.a, input.TextureCoordinate.z);
	
	// apply alpha
	retColor.a *= DiffuseColor.a;
	
	// return final
	return retColor;
}

// main pixel shader for normal-mapped lighting with shadows
float4 PSTBN_S(VertexShaderOutputTBN input) : COLOR
{
	// pixel color to return
	float4 retColor = AlbedoEnabled ? tex2D(MainTextureSampler, input.TextureCoordinate.xy) : 1.0f;

	// apply vertex lighting
	retColor *= input.Color;

	// get normal from texture
	float3 fragNormal = 2.0 * (tex2D(NormalTextureSampler, input.TextureCoordinate.xy).rgb) - 1.0;
	fragNormal.x *= -1;		// <-- fix X axis to be standard.

	//Calculate surface normal
	float3 N = normalize((fragNormal.z * input.Normal) + (fragNormal.x * input.Binormal) + (fragNormal.y * -input.Tangent));

	float2 ShadowTexCoord = mad(0.5f , input.ShadowPos.xy / input.ShadowPos.w , float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	// Get the current depth stored in the shadow map
    float ourdepth = (input.ShadowPos.z / input.ShadowPos.w);

	float shadowContribution = CalcShadowTermSoftPCF(ourdepth, dot(N, LightDirectionA), ShadowTexCoord, ShadowFilterSamples);	

	// process directional lights
    ColorPair lightResult = ComputeLights( EyePosition - input.WorldPos.xyz, N, shadowContribution);

	// apply diffuse
	retColor.rgb *= lightResult.Diffuse;

	// apply specular
	retColor.rgb += lightResult.Specular * retColor.a;

	// apply fog
	retColor.rgb = lerp(retColor.rgb, FogColor * retColor.a, input.TextureCoordinate.z);
	
	// apply alpha
	retColor.a *= DiffuseColor.a;
	
	// return final
	return retColor;
}



#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile VS_SHADERMODEL vsname (); PixelShader = compile PS_SHADERMODEL psname(); } }

//Flat, no shadow
TECHNIQUE(FlatNoShadowVc, VSVc, PS);
TECHNIQUE(FlatNoShadowUv, VSUv, PS);
TECHNIQUE(FlatNoShadowVcUv, VSVcUv, PS);
//Flat, shadow
TECHNIQUE(FlatShadowVc, VSVc, PS_S);
TECHNIQUE(FlatShadowUv, VSUv, PS_S);
TECHNIQUE(FlatShadowVcUv, VSVcUv, PS_S);
//Normal map, no shadow
TECHNIQUE(NormalNoShadowUv, VSUvTBN, PSTBN);
TECHNIQUE(NormalNoShadowVcUv, VSVcUvTBN, PSTBN);
//Normal map, shadow
TECHNIQUE(NormalShadowUv, VSUvTBN, PSTBN_S);
TECHNIQUE(NormalShadowVcUv, VSVcUvTBN, PSTBN_S);