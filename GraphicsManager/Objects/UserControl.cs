using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace GraphicsManager.Objects;

public class UserControl : IRenderObject, IParent
{
	private Rectangle _bounds;

	public UserControl()
	{
		_bounds = new Rectangle();
		_bounds.Clicked += _bounds_Clicked;
	}

	private Task _bounds_Clicked(IRenderObject arg)
	{
		_ = Clicked?.Invoke(arg)!;
		return Task.CompletedTask;
	}

	public ICollection<IRenderObject> Controls { get; } = new List<IRenderObject>();
	public ObjectAnchor Anchor { get => _bounds.Anchor; set => _bounds.Anchor = value; }
	public Uniforms Uniforms { get => _bounds.Uniforms; }
	public bool Visible { get => _bounds.Visible; set => _bounds.Visible = value; }
	public Vector2i Size { get => _bounds.Size; set => _bounds.Size = value; }
	public Vector2 SizeAsFloat { get => _bounds.SizeAsFloat; }
	public Vector2i Location { get => _bounds.Location; set => _bounds.Location = value; }
	public Vector2i Position => Location;
	public Vector2 LocationAsFloat { get => _bounds.LocationAsFloat; }
	public Vector2i Distance { get => _bounds.Distance; }
	public event Func<IRenderObject, Task>? Clicked;
	public event Action<MouseButtonEventArgs> MouseDown;
	public event Action<KeyboardKeyEventArgs> KeyDown;

	public IParent? Parent { get; private set; }
	public Window? Window { get; private set; }
	public bool Loaded { get; private set; } = false;

	public Vector2 MousePosition => Window!.MousePosition;

	public void LoadToParent(IParent Parent, Window Window)
	{
		if (Loaded) return;
		this.Parent = Parent;
		this.Window = Window;
		Loaded = true;
		_bounds.LoadToParent(Parent, Window);
		foreach (IRenderObject obj in Controls) 
		{
			obj.LoadToParent(this, Window);
		}
	}

	public void Draw()
	{
		if (Loaded)
		{
			_bounds.Draw();
			IEnumerable<IRenderObject> needload = Controls.Where(a => a.Loaded == false);

			if (needload.Any())
			{
				foreach (IRenderObject Control in needload)
				{
					Control.LoadToParent(this, Window!);
				}
			}
			foreach (IRenderObject Control in Controls)
			{
				Control.Draw();
			}
		}
	}

	public void Clean()
	{
		foreach (IRenderObject Control in Controls)
		{
			Control.Clean();
		}
		_bounds.Clean();
	}

	public void ParentResize(ResizeEventArgs e)
	{
		if (e.Width == 0 && e.Height == 0) return;
		foreach (IRenderObject Control in Controls)
		{
			if (Control.Loaded)
			{
				bool top = (Control.Anchor & ObjectAnchor.Top) == ObjectAnchor.Top;
				bool left = (Control.Anchor & ObjectAnchor.Left) == ObjectAnchor.Left;
				bool right = (Control.Anchor & ObjectAnchor.Right) == ObjectAnchor.Right;
				bool bottom = (Control.Anchor & ObjectAnchor.Bottom) == ObjectAnchor.Bottom;
				if (!top && !bottom) { Control.Anchor |= ObjectAnchor.Top; top = true; }
				if (!left && !right) { Control.Anchor |= ObjectAnchor.Left; left = true; }
				int lx = (left ? Control.Location.X : Size.X - Control.Distance.X - Control.Size.X);
				int ly = (top ? Control.Location.Y : Size.Y - Control.Distance.Y - Control.Size.Y);
				int sy = (bottom ? Size.Y - Control.Distance.Y - ly : Control.Size.Y);
				int sx = (right ? Size.X - Control.Distance.X - lx : Control.Size.X);
				Control.Size = new(sx, sy);
				Control.Location = new(lx, ly);
				if (Control is IParent parent)
				{
					parent.ParentResize(e);
				}
			}
		}
	}

	#region Cool Math Things
	public float[] RctToFloat(int x, int y, int Width, int Height, bool hastexture = false, float z = 0.0f)
	{
		if (hastexture)
		{
			return new float[20] {
				IntToFloat(x + Width), IntToFloat(y, true), z, 1.0f, 1.0f,// top r
                IntToFloat(x + Width), IntToFloat(y + Height, true), z, 1.0f, 0.0f,//b r
                IntToFloat(x), IntToFloat(y + Height, true), z, 0.0f, 0.0f,//bot l
                IntToFloat(x), IntToFloat(y, true), z, 0.0f, 1.0f// top l
            };
		}
		else
		{
			return new float[12] {
				IntToFloat(x + Width), IntToFloat(y, true), z,// top r
                IntToFloat(x + Width), IntToFloat(y + Height, true), z, //b r
                IntToFloat(x), IntToFloat(y + Height, true), z, //bot l
                IntToFloat(x), IntToFloat(y, true), z,// top l
            };
		}
	}

	public Vector3 PointToVector(int x, int y, float z = 0.0f)
	{
		return new Vector3(IntToFloat(x), IntToFloat(y, true), z);
	}

	public float IntToFloat(int p, bool Invert = false)
	{
		p += (Invert ? Location.Y : Location.X);
		IParent? tempp = Parent;
		while (tempp is not null)
		{
			p += (Invert ? tempp.Position.Y : tempp.Position.X);
			tempp = tempp.Parent;
		}
		int Size = (Invert ? Window!.Size.Y : Window!.Size.X);
		double half = Math.Round((double)Size / (double)2, 1);
		double Per = Math.Round((double)1 / half, 15);
		if (p == half) return 0.0f;
		if (Invert)
		{
			if (p > half) return (float)(((double)(p - half) * Per) * -1);
			else return (float)(1 - (p * Per));
		}
		else
		{
			if (p > half) return (float)((double)(p - half) * Per);
			else return (float)((1 - (p * Per)) * -1);
		}
	}

	public float FloatToInt(float p, bool Invert = false)
	{
		p += (Invert ? LocationAsFloat.Y : LocationAsFloat.X);
		IParent? tempp = Parent;
		while (tempp is not null)
		{
			p += (Invert ? tempp.LocationAsFloat.Y : tempp.LocationAsFloat.X);
			tempp = tempp.Parent;
		}

		int Size = (Invert ? Window!.Size.Y : Window!.Size.X);
		double half = Math.Round((double)Size / (double)2, 15);
		if (p == 0) return (int)half;
		if (Invert)
		{
			if (p < 0)
			{
				p *= -1;
				p++;
				return (float)(half * p);
			}
			else
			{
				return (float)(half - (p * half));
			}
		}
		else
		{
			if (p < 0)
			{
				p *= -1;
				p++;
				return (float)(Size - (half * p));
			}
			else
			{
				return (float)(p * half + half);
			}
		}
	}
	#endregion
}
