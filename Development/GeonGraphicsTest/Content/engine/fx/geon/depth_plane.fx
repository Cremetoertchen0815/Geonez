#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_2_0
	#define PS_SHADERMODEL ps_2_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix LightViewProjection;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	
	float Depth : TEXCOORD0;
};

VertexShaderOutput MainVS(float4 Position : POSITION0)
{
	VertexShaderOutput output;

	output.Position = mul(Position, LightViewProjection);
	output.Depth = output.Position.z / output.Position.w;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return float4(input.Depth, 0, 0, 0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};