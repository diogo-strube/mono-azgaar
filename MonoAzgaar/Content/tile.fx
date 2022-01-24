#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D OceanTexture;
float animationTime;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler2D OceanTextureSampler = sampler_state
{
	Texture = <OceanTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
	float3 myPosition : TEXCOORD1;
	float4 Color: COLOR0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	if (color.b * 1.5 > color.r + color.g)
	{   // overlap animated waves only on what is bluish (just for fun as this approach is bugged)
		color += tex2D(OceanTextureSampler, float2(input.TextureCoordinates.x + sin(animationTime / 40.0) * 2, input.TextureCoordinates.y + cos(animationTime / 40.0)) * 2) * 0.2;
	}
	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};