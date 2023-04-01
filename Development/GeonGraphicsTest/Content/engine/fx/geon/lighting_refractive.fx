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

//Environment mapping
float3 EnvironmentMapSpecular;
float FresnelFactor = 1;
float RefractionIndex = 1;

// fog
float4 FogVector;
float3 FogColor;

// normal map texture
texture NormalMap;

// normal texture sampler
sampler2D NormalTextureSampler = sampler_state {
	Texture = (NormalMap);
	MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;  
    AddressU  = Clamp;
    AddressV  = Clamp;
};

samplerCUBE ReflectionCubeMapSampler = sampler_state
{
    texture = <ReflectionCubeMap>;  
	MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;  
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
	float4 TextureCoordinate : TEXCOORD0;
	float4 WorldPos : TEXCOORD1;
	float3 Normal : TEXCOORD2;
};

// vertex shader output
struct VertexShaderOutputTBN
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 TextureCoordinate : TEXCOORD0;
	float4 WorldPos : TEXCOORD1;
	float3 Normal : TEXCOORD2;
	float3 Tangent : TEXCOORD3;
	float3 Binormal : TEXCOORD4;
};

float ComputeFresnelFactor(float3 eyeVector, float3 worldNormal)
{
    float viewAngle = dot(normalize(eyeVector), worldNormal);
    return pow(max(1 - abs(viewAngle), 0), FresnelFactor);
}

// main vertex shader for flat lighting(no Uv-coords, no vertex colors)
VertexShaderOutputN VSVc(in VertexShaderInputVc input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(input.Color.rgb, input.Color.a * DiffuseColor.a);

	output.WorldPos = mul(input.Position, World);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	float fresnel = ComputeFresnelFactor(EyePosition - output.WorldPos.xyz, output.Normal);
	float fogFactor = saturate(dot(input.Position, FogVector));
	output.TextureCoordinate = float4(0, 0, fogFactor, fresnel);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputN VSUv(in VertexShaderInputUv input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color =  float4(1, 1, 1, DiffuseColor.a);

	output.WorldPos = mul(input.Position, World);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	float fresnel = ComputeFresnelFactor(EyePosition - output.WorldPos.xyz, output.Normal);
	float fogFactor = saturate(dot(input.Position, FogVector));
	output.TextureCoordinate = float4(input.TextureCoordinate, fogFactor, fresnel);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputN VSVcUv(in VertexShaderInputVcUv input)
{
	VertexShaderOutputN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color =  float4(input.Color.rgb, DiffuseColor.a);

	output.WorldPos = mul(input.Position, World);
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	float fresnel = ComputeFresnelFactor(EyePosition - output.WorldPos.xyz, output.Normal);
	float fogFactor = saturate(dot(input.Position, FogVector));
	output.TextureCoordinate = float4(input.TextureCoordinate, fogFactor, fresnel);
	return output;
}

// main vertex shader for flat lighting
VertexShaderOutputTBN VSUvTBN(in VertexShaderInputUvTBN input)
{
	VertexShaderOutputTBN output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(1, 1, 1, DiffuseColor.a);

	float fogFactor = saturate(dot(input.Position, FogVector));
	output.TextureCoordinate = float3(input.TextureCoordinate, fogFactor);
	output.WorldPos = mul(input.Position, World);
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

	float fogFactor = saturate(dot(input.Position, FogVector));
	output.TextureCoordinate = float3(input.TextureCoordinate, fogFactor);
	output.WorldPos = mul(input.Position, World);
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

float4 ReflectAndRefract(float3 Position, float3 Normal, float Fresnel) {
	
	float3 V = -normalize(EyePosition - Position);

	// Calculate reflection vector
	float3 Reflect = reflect(V, Normal);
	float3 Refract = -refract(V, Normal, RefractionIndex).xzy;
	Refract.z = -Refract.z;
	float4 ReflectColor = texCUBE(ReflectionCubeMapSampler, Reflect);
	float4 RefractColor = texCUBE(ReflectionCubeMapSampler, Refract);

	return float4(lerp(RefractColor, ReflectColor, Fresnel).xyz, ReflectColor.a);
}

// main pixel shader for flat lighting
float4 PS(VertexShaderOutputN input) : COLOR
{
	float3 N = normalize(input.Normal);

	// apply reflection
	float4 ref = ReflectAndRefract(input.WorldPos.xyz, N, input.TextureCoordinate.w);
	float3 retColor = ref.rgb;

	// apply emmissive
	retColor += EmissiveColor;

	// apply specular
    retColor += EnvironmentMapSpecular * ref.a;

	// apply fog
	retColor = lerp(retColor, FogColor, input.TextureCoordinate.z);
	
	// apply alpha & return final
	return float4(retColor, DiffuseColor.a);
}

// main pixel shader for normal-mapped lighting 
float4 PSTBN(VertexShaderOutputTBN input) : COLOR
{
	// get normal from texture
	float3 fragNormal = 2.0 * (tex2D(NormalTextureSampler, input.TextureCoordinate.xy).rgb) - 1.0;
	fragNormal.x *= -1;		// <-- fix X axis to be standard.

	//Calculate surface normal
	float3 N = normalize((fragNormal.z * input.Normal) + (fragNormal.x * input.Binormal) + (fragNormal.y * -input.Tangent));


	// apply reflection
	float fresnel = ComputeFresnelFactor(EyePosition - input.WorldPos.xyz, N);
	float4 ref = ReflectAndRefract(input.WorldPos.xyz, N, fresnel);
	float3 retColor = ref.rgb;

	// apply emmissive
	retColor += EmissiveColor;

	// apply specular
    retColor += EnvironmentMapSpecular * ref.a;

	// apply fog
	retColor = lerp(retColor, FogColor, input.TextureCoordinate.z);
	
	// apply alpha & return final
	return float4(retColor, DiffuseColor.a);
}

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile VS_SHADERMODEL vsname (); PixelShader = compile PS_SHADERMODEL psname(); } }

//Flat
TECHNIQUE(FlatVc, VSVc, PS);
TECHNIQUE(FlatUv, VSUv, PS);
TECHNIQUE(FlatVcUv, VSVcUv, PS);
//Normal map
TECHNIQUE(NormalUv, VSUvTBN, PSTBN);
TECHNIQUE(NormalVcUv, VSVcUvTBN, PSTBN);