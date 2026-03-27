#version 460 core

#extension GL_ARB_bindless_texture : require
#extension GL_ARB_gpu_shader_int64 : require

// ================================
// Structs
// ================================

struct Light
{
    vec3 position;
    float _pad0;
    vec3 ambient;
    float _pad1;
    vec3 diffuse;
    float _pad2;
    vec3 specular;
    float _pad3;
};

struct MeshDrawData
{
	mat4 Model;
	uint64_t DiffuseTexture;
	uint64_t SpecularTexture;
};

// ================================
// INPUTS
// ================================

in VS_Out
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

uniform float Shininess;

// ================================
// Buffers
// ================================

layout (std430, binding = 0) buffer MeshDrawDataBuffer
{
	MeshDrawData MeshDraw[];
};

layout(std430, binding = 1) buffer Lights
{
    Light light;
};

// =================================
// Other
// =================================

layout(binding = 2) uniform sampler2D shadowMap;
out vec4 FragColor;

// =================================
// Shadow Mapping (Func)
// =================================

float ShadowCalc(vec4 FragPosLightSpace, vec3 N, vec3 L)
{
    vec3 ProjCoords = vs_out.FragPosLightSpace.xyz / vs_out.FragPosLightSpace.w;
    ProjCoords = ProjCoords * 0.5 + 0.5;

	float ClosestDepth = texture(shadowMap, ProjCoords.xy).r;
	float CurrentDepth = ProjCoords.z;

	float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);  

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMap, ProjCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += CurrentDepth - bias > pcfDepth ? 1.0 : 0.0;        
		}    
	}
	shadow /= 9.0;
	return shadow;
}

// =================================
// Main
// =================================

void main()
{
	MeshDrawData CurrentDraw = MeshDraw[vs_out.DrawID];

	sampler2D DiffuseSampler = sampler2D(CurrentDraw.DiffuseTexture);
	sampler2D SpecularSampler = sampler2D(CurrentDraw.SpecularTexture);

	// Get Diffuse and Specular Texture Data
    vec4 AlbedoMap = texture(DiffuseSampler, vs_out.TexCoord);
    if (AlbedoMap.a < 0.2)
        discard;
	vec4 SpecMap = texture(SpecularSampler, vs_out.TexCoord);

    // Ambient
    vec3 A = light.ambient * AlbedoMap.rgb;

    // Diffuse
    vec3 N = normalize(vs_out.Normal);
    vec3 L = normalize(light.position - vs_out.FragPos);
    float DStr = max(dot(L, N), 0.0);
    vec3 D = light.diffuse * DStr * AlbedoMap.rgb;

    // Specular
    vec3 V = normalize(vs_out.CamPos - vs_out.FragPos);
	vec3 H = normalize(L + V);
    float SStr = pow(max(dot(N, H), 0.0), Shininess);
    vec3 S = light.specular * SStr * SpecMap.rgb;

	// Shadow
	//float ShStr = ShadowCalc(vs_out.FragPosLightSpace, N, L);

	// Result
    //vec3 result = A + (1.0 - ShStr) * (D + S);
	vec3 result = A + D + S;
	
	// Gamma Correction
	result = pow(result, vec3(1.0/2.2));
	
    FragColor = vec4(result, AlbedoMap.a);
}