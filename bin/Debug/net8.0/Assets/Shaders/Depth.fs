#version 460 core

// ================================
// INPUTS
// ================================

in VS_Out
{
	vec2 TexCoord;
	flat int DrawID;
} vs_out;

// ================================
// Structs
// ================================

struct MeshDrawData
{
	mat4 Model;
	uint64_t DiffuseTexture;
	uint64_t SpecularTexture;
};
	
// ================================
// Buffers
// ================================

layout (std430, binding = 0) buffer MeshDrawDataBuffer
{
	MeshDrawData MeshDraw[];
};

// ================================
// MAIN
// ================================

void main()
{           

	MeshDrawData CurrentDraw = MeshDraw[vs_out.DrawID];
	sampler2D DiffuseSampler = sampler2D(CurrentDraw.DiffuseTexture);

	if (texture(DiffuseSampler, TexCoord).a < 0.2)
	{
		discard;
	}
}  