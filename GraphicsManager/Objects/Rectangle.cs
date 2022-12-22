using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsManager.Objects;

public class Rectangle : ITextureObject
{
	public static readonly Shader DefaultShader = new("Rectangle", true);

	public ObjectAnchor Anchor { get; set; } = ObjectAnchor.Left | ObjectAnchor.Top;

	public Texture? Texture { get; private set; }

	public Rectangle(Texture? texture = null)
	{
		Texture = texture;
		if (Points_ is null)
		{
			bool tex = (Texture is null);
			Points_ = new float[(tex ? 12 : 20)];
			if (!tex)
			{
				Points[3] = 1.0f;
				Points[4] = 1.0f;
				Points[8] = 1.0f;
				Points[19] = 1.0f;
			}
		}
	}

	public Vector4 Color { get; set; }

	public bool Visible { get; set; } = true;

	public void Draw()
	{
		if (Visible)
		{
			if (Texture is not null) Texture.Use();
			Shader.Use();
			GL.Uniform4(0, Color);
			if (Texture is not null)
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			}
			GL.BindVertexArray(ArrayObject);
			GL.DrawElements(PrimitiveType.Triangles, Indexs.Length, DrawElementsType.UnsignedInt, 0);
			if (Texture is not null) GL.Disable(EnableCap.Blend);
		}
	}

	public void Clean()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.DeleteBuffer(BufferObject);
	}

	public void LoadToParent(IParent Parent, Window Window)
	{
		this.Parent = Parent;
		this.Window = Window;
		int pos = Points.Length - 3;
		if (Texture is not null) pos -= 2;
		pos = 4;
		if (Texture is not null) pos += 2;
		
		BufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, BufferObject);
		ArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(ArrayObject);
		int add = 3;
		if (Texture is not null) add = 5;
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, add * sizeof(float), 0);
		if (Texture is not null)
		{
			Shader = Texture.TextureShader;
			Texture.Load(Shader.GetAttribLocation("aTexCoord"));
		}
		GL.EnableVertexAttribArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, BufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, Hint);
		GL.BindVertexArray(ArrayObject);
		ElementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, Indexs.Length * sizeof(uint), Indexs, Hint);
		load = true;
		loadd = true;
		
		Window.MouseDown += Window_MouseDown;
		Location = Location;
		Distance = new(Parent.Size.X - Size.X - Location.X, Parent.Size.Y - Size.Y - Location.Y);
	}

	public IParent? Parent { get; private set; }
	public Window? Window { get; private set; }

	private void Window_MouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
	{
		if (Clicked is not null && e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button1 && Location.X <= Parent?.MousePosition.X && Size.X + Location.X >= Parent?.MousePosition.X && Location.Y + Size.Y >= Parent?.MousePosition.Y && Location.Y <= Parent?.MousePosition.Y) Clicked.Invoke(this);
	}

	~Rectangle()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.DeleteBuffer(BufferObject);
	}

	public Shader Shader { get; set; } = DefaultShader;
	public int ElementBufferObject { get; private set; }
	public int BufferObject { get; private set; }
	public int ArrayObject { get; private set; }
	private float[] Points_;
	private Vector2i size_ = new(), loc_ = new();
	bool load = false;

	public float[] Points
	{
		get
		{
			return Points_;
		}
		set
		{
			Points_ = value;
			try
			{
				if (load)
				{
					int add = 3;
					if (Texture is not null) add = 5;
					GL.BindBuffer(BufferTarget.ArrayBuffer, BufferObject);
					GL.BindVertexArray(ArrayObject);
					GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, add * sizeof(float), 0);
					GL.EnableVertexAttribArray(0);
					GL.BindBuffer(BufferTarget.ArrayBuffer, BufferObject);
					GL.BufferData(BufferTarget.ArrayBuffer, Points_.Length * sizeof(float), Points_, Hint);
					GL.BindVertexArray(ArrayObject);
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
					GL.BufferData(BufferTarget.ElementArrayBuffer, Indexs.Length * sizeof(uint), Indexs, Hint);
				}
			}
			catch (AccessViolationException v)
			{
				Console.WriteLine(v.Message);
			}
		}
	}

	public uint[] Indexs { get; set; } = new uint[6] { 0, 1, 3, 1, 2, 3 };
	public BufferUsageHint Hint { get; set; } = BufferUsageHint.StaticDraw;

	private bool loadd = false;

	public event Func<IRenderObject, Task>? Clicked;

	public bool Loaded => loadd;
	public Vector2i Distance { get; private set; }

	public Vector2i Size
	{
		get
		{
			return size_;
		}
		set
		{
			size_ = value;
			if (Window is null || Parent is null) return;
			float[] temp = Points;
			saf = new Vector2(Window.IntToFloat(value.X + loc_.X + Parent.Position.X, false), Window.IntToFloat(value.Y + loc_.Y + Parent.Position.Y, true));
			temp[0] = saf.X;
			temp[(Texture is null ? 3 : 5)] = saf.X;
			temp[(Texture is null ? 4 : 6)] = saf.Y;
			temp[(Texture is null ? 7 : 11)] = saf.Y;
			Points = temp;
		}
	}

	public Vector2i Location
	{
		get
		{
			return loc_;
		}
		set
		{
			loc_ = value;
			if (Window is null || Parent is null) return;
			float[] temp = Points;
			laf = new Vector2(Window.IntToFloat(value.X + Parent.Position.X, false), Window.IntToFloat(value.Y + Parent.Position.Y, true));
			temp[(Texture is null ? 6 : 10)] = laf.X;
			temp[(Texture is null ? 9 : 15)] = laf.X;
			temp[1] = laf.Y;
			temp[(Texture is null ? 10 : 16)] = laf.Y;
			saf = new Vector2(Window.IntToFloat(Size.X + value.X + Parent.Position.X, false), Window.IntToFloat(Size.Y + value.Y + Parent.Position.Y, true));
			temp[0] = saf.X;
			temp[(Texture is null ? 3 : 5)] = saf.X;
			temp[(Texture is null ? 4 : 6)] = saf.Y;
			temp[(Texture is null ? 7 : 11)] = saf.Y;
			Points = temp;
		}
	}
	private Vector2 laf = new(), saf = new();

	public Vector2 LocationAsFloat { get { return laf; } }
	public Vector2 SizeAsFloat { get { return saf; } }
}
