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


// Mat Struct
struct Material_T {
	sampler2D Diffuse;
	sampler2D Specular;
	float Shininess;
};

// Structs
uniform Material_T material;

// Vertex Data Passthrough
in vec3 Normal;
in vec3 FragPos;
in vec3 CamPos;
in vec2 TexCoord;

// Result
out vec4 FragColor;

void main()
{
	// Get Diffuse and Specular Texture Data
    vec4 AlbedoMap = texture(material.Diffuse, TexCoord);
    if (AlbedoMap.a < 0.2)
        discard;
	vec4 SpecMap = texture(material.Specular, TexCoord);

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
    float SStr = pow(max(dot(N, H), 0.0), material.Shininess);
    vec3 S = light.specular * SStr * SpecMap.rgb;

	// Result
    vec3 result = A + D + S;
	
	// Gamma Correction
	result = pow(result, vec3(1.0/2.2));
	
    FragColor = vec4(result, AlbedoMap.a);
}
