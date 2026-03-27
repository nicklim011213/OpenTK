#version 450 core
out vec4 FragColor;
  
in vec2 TexCoords;

layout(binding = 0) uniform sampler2D screenTexture;

void main()
{ 
    vec4 InputFrame = texture(screenTexture, TexCoords);
	//FragColor = vec4(1 - InputFrame.rgb, InputFrame.a);
	FragColor = InputFrame;
}