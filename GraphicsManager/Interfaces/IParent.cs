using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace GraphicsManager.Interfaces;

public interface IParent
{
	public Vector2i Size { get; }
	public void Resize(ResizeEventArgs e);
	public float[] RctToFloat(int x, int y, int Width, int Height, bool hastexture = false, float z = 0.0f);
	public float IntToFloat(int p, bool Invert = false);
	public float FloatToInt(float p, bool Invert = false);
	public event Action<MouseButtonEventArgs> MouseDown;
	public event Action<KeyboardKeyEventArgs> KeyDown;
	public Vector2 MousePosition { get; }

	public Vector2i Position { get; }
}
