using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GraphicsManager.Objects;

public class Textbox : ITextureObject
{
	public event Func<IRenderObject, Task>? Clicked;
	public Rectangle Bounds;
	public Label Label;
	public IParent Parent { get; private set; }
	public Textbox(Texture texture = null!)
	{
		Bounds = new Rectangle(texture ?? new Texture(Tools.GetResourceBytes("GraphicsManager.Resources.Textures.Textbox.png")));
		Label = new Label()
		{
			Text = nameof(Textbox)
		};
	}
	public Vector2i Size { get { return Bounds.Size; } set { Bounds.Size = value; } }
	public Vector2i Location { get { return Bounds.Location; } set { Bounds.Location = value; } }
	public Vector2i Distance { get { return Bounds.Distance; }}
	public Texture Texture => Bounds.Texture!;

	public string Text { get => Label.Text; set => Label.Text = value; }
	public Font Font { get => Label.Font; set => Label.Font = value; }
	public Vector4 BackgroundColor { get => Bounds.Color; set => Bounds.Color = value; }
	public float[] Points
	{
		get
		{
			return Bounds.Points;
		}
		set
		{
			if (Label is not null && Parent is not null)
			{
				Label.Location = new Vector2i((int)Parent.FloatToInt(value[10] + ((-1 - Parent.IntToFloat(10)) * -1)), (int)Parent.FloatToInt(value[6] + (-1 - Parent.IntToFloat(5)) * -1, true));
			}
			Bounds.Points = value;
		}
	}

	public ObjectAnchor Anchor { get; set; } = ObjectAnchor.Left | ObjectAnchor.Top;

	private bool _loaded = false;
	public bool Loaded => _loaded;

	public bool Visible { get; set; } = true;

	public void Clean()
	{
		Bounds.Clean();
		Label.Clean();
	}

	public void Draw()
	{
		Bounds.Draw();
		Label.Draw();
	}
	public Window? Window { get; private set; }
	public void LoadToParent(IParent Parent, Window Window)
	{
		this.Parent = Parent;
		this.Window = Window;
		this.Parent.MouseDown += Parrent_MouseDown;
		this.Parent.KeyDown += Parrent_KeyDown;
		Bounds.LoadToParent(Parent, Window);
		Points = Points;
		Label.LoadToParent(Parent, Window);
		_loaded = true;
	}

	private void Parrent_KeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
	{
		if (use)
		{
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
	}

	bool use = false;

	private void Parrent_MouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
	{
		if (e.Button == MouseButton.Button1 && Location.X <= Parent?.MousePosition.X && Size.X + Location.X >= Parent?.MousePosition.X && Location.Y + Size.Y >= Parent?.MousePosition.Y && Location.Y <= Parent?.MousePosition.Y)
		{
			use = true;
			if (Clicked is not null) Clicked.Invoke(this);
		}
		else use = false;
	}

	public Vector2 LocationAsFloat { get { return Bounds.LocationAsFloat; } }
	public Vector2 SizeAsFloat { get { return Bounds.SizeAsFloat; } }
}
