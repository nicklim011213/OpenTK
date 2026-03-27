#version 460 core

#extension GL_ARB_bindless_texture : require
#extension GL_ARB_gpu_shader_int64 : require
#extension GL_ARB_shader_draw_parameters : require

// ================================
// INPUTS
// ================================

layout (location = 0) in vec3 aPosition;
layout (location = 2) in vec2 aTexCoord;

// ================================
// OUTPUTS
// ================================

out VS_Out
{
	vec2 TexCoord;
	flat int DrawID;
} vs_out;

// ================================
// UNIFORMS
// ================================
    uniform mat4 U_LightSpaceMatrix;

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
	MeshDrawData CurrentDraw = MeshDraw[gl_DrawID];
	
	vs_out.DrawID = gl_DrawID;
	vs_out.TexCoord = aTexCoord;
	
    //gl_Position = U_LightSpaceMatrix * CurrentDraw.Model * vec4(aPosition, 1.0);
	gl_Position = U_LightSpaceMatrix * CurrentDraw.Model * vec4(aPosition, 1.0);
}