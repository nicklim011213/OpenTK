#version 450 core

// Light SSBO
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

layout(std430, binding = 0) buffer Lights
{
    Light light;
};


// Mats
uniform float Shininess;
layout(binding = 0)	uniform sampler2D Diffuse;
layout(binding = 1) uniform	sampler2D Specular;

// Shadows
layout(binding = 2) uniform sampler2D shadowMap;


// Vertex Data Passthrough
in vec3 Normal;
in vec3 FragPos;
in vec3 CamPos;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

// Result
out vec4 FragColor;

float ShadowCalc(vec4 FragPosLightSpace, vec3 N, vec3 L)
{
    vec3 ProjCoords = FragPosLightSpace.xyz / FragPosLightSpace.w;
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

void main()
{
	// Get Diffuse and Specular Texture Data
    vec4 AlbedoMap = texture(Diffuse, TexCoord);
    if (AlbedoMap.a < 0.2)
        discard;
	vec4 SpecMap = texture(Specular, TexCoord);

    // Ambient
    vec3 A = light.ambient * AlbedoMap.rgb;

    // Diffuse
    vec3 N = normalize(Normal);
    vec3 L = normalize(light.position - FragPos);
    float DStr = max(dot(L, N), 0.0);
    vec3 D = light.diffuse * DStr * AlbedoMap.rgb;

    // Specular
    vec3 V = normalize(CamPos - FragPos);
	vec3 H = normalize(L + V);
    float SStr = pow(max(dot(N, H), 0.0), Shininess);
    vec3 S = light.specular * SStr * SpecMap.rgb;

	// Shadow
	float ShStr = ShadowCalc(FragPosLightSpace, N, L);

	// Result
    vec3 result = A + (1.0 - ShStr) * (D + S);
	
	// Gamma Correction
	result = pow(result, vec3(1.0/2.2));
	
    FragColor = vec4(result, AlbedoMap.a);
}