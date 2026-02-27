#version 450 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 model;

// Generic Uniform Data
layout(std140, binding = 0) uniform CameraData
{
    mat4 perp;
    mat4 view;
    vec4 camPos;
};

out vec3 Normal;
out vec3 FragPos;
out vec3 CamPos;
out vec2 TexCoord;

void main()
{
	CamPos = camPos.xyz;
	Normal = normalize(transpose(inverse(mat3(model))) * aNormal);
	TexCoord = aTexCoord;
	FragPos = vec3(model * vec4(aPosition, 1.0));
    gl_Position = perp * view * model* vec4(aPosition, 1.0);
}