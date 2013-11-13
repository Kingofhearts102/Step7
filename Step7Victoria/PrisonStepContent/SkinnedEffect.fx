float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;

float3 LightAmbient = float3(0.05, 0.05, 0.10);
float3 Light1Location = float3(568, 246, 1036);
float3 Light1Color = float3(1, 1, 1);
float3 Light2Location = float3(821, 224, 941);
float3 Light2Color = float3(14.29, 45, 43.94);
float3 Light3Location = float3(824, 231, 765);
float3 Light3Color = float3(82.5, 0, 0);

float3 Gain = float3(1, 1, 1);

texture Texture;

// Maximum number of bone matrices we can render using shader 2.0 in a single pass.
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 57
float4x4 Bones[MaxBones];

sampler Sampler = sampler_state
{
    Texture = <Texture>;

    MinFilter = LINEAR;
    MagFilter = LINEAR;
    
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.TexCoord = input.TexCoord;

	// Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;

	float4 worldPosition = mul(mul(input.Position, skinTransform), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    float3 color = LightAmbient;

	float3 normal = normalize(mul(mul(input.Normal, skinTransform), World));

	float3 L1 = normalize(Light1Location - worldPosition);
	color += saturate(dot(L1, normal)) * Light1Color;

	float3 L2 = Light2Location - worldPosition;
	float L2distance = length(L2);
	L2 /= L2distance;
    color += saturate(dot(L2, normal)) / L2distance * Light2Color;

	float3 L3 = Light3Location - worldPosition;
	float L3distance = length(L3);
	L3 /= L3distance;
    color += saturate(dot(L3, normal)) / L3distance * Light3Color;
	
    output.Color = float4(color, 1);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color * tex2D(Sampler, input.TexCoord) * float4(Gain, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}