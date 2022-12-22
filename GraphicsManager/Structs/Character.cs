using GraphicsManager.Objects.Core;
using OpenTK.Mathematics;

namespace GraphicsManager.Structs;

public struct Character
{
	public Texture Texture { get; set; }
	public Vector2 Size { get; set; }
	public Vector2 Bearing { get; set; }
	public int Advance { get; set; }
}
