#version 450 core

layout(binding = 0) uniform sampler2D Diffuse;

in vec2 TexCoord;

void main()
{           
	if (texture(Diffuse, TexCoord).a < 0.2)
	{
		discard;
	}
    //gl_FragDepth = gl_FragCoord.z;
}  