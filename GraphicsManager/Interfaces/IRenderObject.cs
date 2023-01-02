using GraphicsManager.Enums;
using OpenTK.Mathematics;

namespace GraphicsManager.Interfaces;

public interface IRenderObject
{
	public ObjectAnchor Anchor { get; set; }
	public bool Loaded { get; }
	public void LoadToParent(IParent Parent, Window Window);
	public void Draw();
	public void Clean();
	public Vector2i Size { get; set; }
	public Vector2i Location { get; set; }
	public Vector2 SizeAsFloat { get; }
	public Vector2 LocationAsFloat { get; }
	public Vector2i Distance { get; }
	public IParent? Parent { get; }
	public Window? Window { get; }
	public bool Visible { get; set; }

	public event Func<IRenderObject, Task>? Clicked;
}
