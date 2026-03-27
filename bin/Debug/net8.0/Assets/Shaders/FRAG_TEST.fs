#version 460 core

// ================================
// Inputs from Vertex Shader
// ================================
in VS_Out
{
    vec3 Normal;
    vec3 FragPos;
    vec3 CamPos;
    vec2 TexCoord;
    vec4 FragPosLightSpace;
    int DrawID;
} vs_out;

// ================================
// Uniforms
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
//layout(std430, binding = 1) buffer Lights
//{
//    Light light;
//};

uniform float Shininess;

// ================================
// Output
// ================================
out vec4 FragColor;

// ================================
// Main
// ================================
void main()
{
    // Simple base color per draw ID (just for testing multiple draws)
    vec3 baseColor = vec3(0.5 + 0.2, 0.2, 0.8 - 0.2);

    // Simple ambient + diffuse + specular lighting
    //vec3 N = normalize(vs_out.Normal);
    //vec3 L = normalize(light.position - vs_out.FragPos);
    //float diff = max(dot(N, L), 0.0);

    vec3 V = normalize(vs_out.CamPos - vs_out.FragPos);
    //vec3 H = normalize(L + V);
    //float spec = pow(max(dot(N, H), 0.0), Shininess);

    //vec3 color = light.ambient * baseColor + light.diffuse * diff * baseColor + light.specular * spec * vec3(1.0);

	vec3 color = vec3(1.0, 1.0, 1.0);

    // Gamma correction
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1.0);
}