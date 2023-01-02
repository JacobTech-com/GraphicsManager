using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GraphicsManager.Objects;

public class Textbox : IRenderObject
{
	private RoundedRectangle _bounds, _inside;
	private Label _label;
	public Textbox()
	{
		_bounds = new RoundedRectangle();
		_inside = new RoundedRectangle();
		_label = new Label();
	}

	public int Radius { get => _bounds.Radius; set { _bounds.Radius = value; _inside.Radius = value; } }
	public int Border { get; set; } = 2;
	public int Smoothness { get => _bounds.Smoothness; set { _bounds.Smoothness = value; _inside.Smoothness = value; } }
	public ObjectAnchor Anchor { get => _bounds.Anchor; set { _bounds.Anchor = value; _inside.Anchor = value; _label.Anchor = value; } }
	public Font Font { get => _label.Font; set => _label.Font = value; }
	public string Text { get => _label.Text; set => _label.Text = value; }
	public bool Loaded { get; private set; } = false;
	public Vector2i Size
	{
		get
		{
			return _bounds.Size;
		}
		set
		{
			_bounds.Size = value;
			_inside.Size = new(value.X - (Border * 2), value.Y - (Border * 2));
		}
	}
	public Vector2i Location { 
		get => _bounds.Location; 
		set
		{
			_bounds.Location = value;
			_label.Location = new(value.X + Radius + 5, value.Y + Size.Y - Radius - 5);
			_inside.Location = new(value.X + Border, value.Y + Border);
		}
	}
	public Vector2 SizeAsFloat { get => _bounds.SizeAsFloat; }
	public Vector2 LocationAsFloat { get => _bounds.LocationAsFloat; }
	public Vector2i Distance { get => _bounds.Distance; }
	public IParent? Parent { get; private set; } = null;
	public Window? Window { get; private set; } = null;

	public Color4 InsideColor
	{
		get
		{
			Uniform<Vector4> u4 = _inside.Uniforms.Uniform4.Where(u => u.Location == 0).First();
			if (u4 is null) u4 = new() { Location = 0, Value = new(1, 1, 0, 1) };
			return new Color4(u4.Value.X, u4.Value.X, u4.Value.X, u4.Value.X);
		}
		set
		{
			Uniform<Vector4> u4 = _inside.Uniforms.Uniform4.Where(u => u.Location == 0).First();
			if (u4 is not null) _inside.Uniforms.Uniform4.Remove(u4);
			if (u4 is null) u4 = new() { Location = 0 };
			u4.Value = new(value.R, value.G, value.B, value.A);
			_inside.Uniforms.Uniform4.Add(u4);
		}
	}
	public Color4 BorderColor
	{
		get
		{
			Uniform<Vector4> u4 = _bounds.Uniforms.Uniform4.Where(u => u.Location == 0).First();
			if (u4 is null) u4 = new() { Location = 0, Value = new(1, 1, 0, 1) };
			return new Color4(u4.Value.X, u4.Value.X, u4.Value.X, u4.Value.X);
		}
		set
		{
			Uniform<Vector4> u4 = _bounds.Uniforms.Uniform4.Where(u => u.Location == 0).First();
			if (u4 is not null) _bounds.Uniforms.Uniform4.Remove(u4);
			if (u4 is null) u4 = new() { Location = 0 };
			u4.Value = new(value.R, value.G, value.B, value.A);
			_bounds.Uniforms.Uniform4.Add(u4);
		}
	}
	public bool Visible { get; set; } = true;
	public event Func<IRenderObject, Task>? Clicked;

	public void Clean()
	{
		_bounds.Clean();
		_inside.Clean();
		_label.Clean();
	}

	public void Draw()
	{
		if (!Visible || !Loaded) return;
		_bounds.Draw();
		_inside.Draw();
		_label.Draw();
	}

	public void LoadToParent(IParent Parent, Window Window)
	{
		if (Loaded) return;
		this.Parent = Parent;
		this.Window = Window;
		this.Window.MouseDown += Window_MouseDown;
		this.Window.KeyDown += Window_KeyDown;
		Loaded = true;
		_bounds.LoadToParent(Parent, Window);
		_inside.LoadToParent(Parent, Window);
		_label.LoadToParent(Parent, Window);
		Location = Location;
	}
	private bool use = false;
	private void Window_KeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
	{
		if (!use) return;
		if (obj.Key == Keys.CapsLock || obj.Key == Keys.Menu || obj.Key == Keys.LeftSuper || obj.Key == Keys.RightSuper || obj.Key == Keys.End || obj.Key == Keys.Home || obj.Key == Keys.PageDown || obj.Key == Keys.PageUp || obj.Key == Keys.Insert || obj.Key == Keys.Up || obj.Key == Keys.Down || obj.Key == Keys.Left || obj.Key == Keys.Right) return;
		if (obj.Key == Keys.Backspace)
		{
			if (!(Text.Length > 0)) return;
			Text = Text.Remove(Text.Length - 1, 1);
		}
		else if (obj.Key == Keys.Delete)
		{
			if (!(Text.Length > 0)) return;
			Text = Text.Remove(Text.Length - 1, 1);
		}
		else if (obj.Shift)
		{
			if (obj.Key == Keys.Enter || obj.Key == Keys.KeyPadEnter) Text += '\n';
			else if (obj.Key == Keys.LeftShift || obj.Key == Keys.KeyPadEnter || obj.Key == Keys.Enter || obj.Key == Keys.End || obj.Key == Keys.Down) return;
			else Text += ((char)obj.Key).ToString().ToUpper();
		}
		else if (obj.Command || obj.Alt || obj.Control) { }
		else
		{
			Text += ((char)obj.Key).ToString().ToLower();
		}
	}

	private void Window_MouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
	{
		if (e.Button == MouseButton.Button1 &&
			Parent?.IntToFloat(Location.X) <= Parent?.IntToFloat((int)Parent?.MousePosition.X!) &&
			Parent?.IntToFloat(Size.X + Location.X) >= Parent?.IntToFloat((int)Parent?.MousePosition.X!) &&
			Parent?.IntToFloat(Location.Y + Size.Y, true) <= Parent?.IntToFloat((int)Parent?.MousePosition.Y!, true) &&
			Parent?.IntToFloat(Location.Y, true) >= Parent?.IntToFloat((int)Parent?.MousePosition.Y!, true))
		{
			use = true;
			if (Clicked is not null) Clicked.Invoke(this);
		}
		else use = false;
	}
}
