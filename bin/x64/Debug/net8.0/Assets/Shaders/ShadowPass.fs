#version 460 core

#extension GL_ARB_bindless_texture : require
#extension GL_ARB_gpu_shader_int64 : require
#extension GL_ARB_shader_draw_parameters : require

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

	if (texture(DiffuseSampler, vs_out.TexCoord).a < 0.2)
	{
		discard;
	}
}  