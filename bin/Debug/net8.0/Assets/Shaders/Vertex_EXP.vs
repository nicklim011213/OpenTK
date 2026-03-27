#version 460 core

#extension GL_ARB_bindless_texture : require
#extension GL_ARB_gpu_shader_int64 : require
#extension GL_ARB_shader_draw_parameters : require

// ================================
// INPUTS
// ================================

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

// ================================
// OUTPUTS
// ================================

out VS
{
	vec3 Normal;
	vec3 FragPos;
	vec3 CamPos;
	vec2 TexCoord;
	vec4 FragPosLightSpace;
	flat int DrawID;
} vs_out;

// ================================
// UNIFORMS
// ================================

    uniform mat4 U_Perp;
    uniform mat4 U_View;
    uniform vec4 U_camPos;
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
	//MeshDrawData CurrentDraw = MeshDraw[gl_DrawID];
	MeshDrawData CurrentDraw = MeshDraw[0];
	
	vs_out.CamPos = U_camPos.xyz;
	vs_out.Normal = normalize(transpose(inverse(mat3(CurrentDraw.Model))) * aNormal);
	vs_out.TexCoord = aTexCoord;
	//vs_out.FragPos = vec3(CurrentDraw.Model * vec4(aPosition, 1.0));
	vs_out.FragPos = vec3(1.0, 0.0, 0.0);
	vs_out.FragPosLightSpace = U_LightSpaceMatrix * vec4(vs_out.FragPos, 1.0);
	
	//DrawID = gl_DrawID;
	DrawID = 0;
	
    gl_Position = U_Perp * U_View * CurrentDraw.Model* vec4(aPosition, 1.0);
}