#version 330
out vec4 outputColor;
in vec2 texCoord;
uniform sampler2D texture1;
void main(void)
{
    outputColor = texture(texture1, texCoord);
}