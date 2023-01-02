using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

	public Uniforms Uniforms { get; } = new() { Uniform4 = new() { new() { Location = 0, Value = new(0,0,0,1) } } };

	public bool Visible { get; set; } = true;

	public void Draw()
	{
		if (Visible && Loaded)
		{
			if (Texture is not null) Texture.Use();
			Shader.Use();
			for (int i = 0; i < Uniforms.Uniform4.Count; i++)
			{
				GL.Uniform4(Uniforms.Uniform4[i].Location, Uniforms.Uniform4[i].Value);
			}
			for (int i = 0; i < Uniforms.Uniform3.Count; i++)
			{
				GL.Uniform3(Uniforms.Uniform3[i].Location, Uniforms.Uniform3[i].Value);
			}
			for (int i = 0; i < Uniforms.Uniform2.Count; i++)
			{
				GL.Uniform2(Uniforms.Uniform2[i].Location, Uniforms.Uniform2[i].Value);
			}
			for (int i = 0; i < Uniforms.Uniform1.Count; i++)
			{
				GL.Uniform1(Uniforms.Uniform1[i].Location, Uniforms.Uniform1[i].Value);
			}
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
		if (Loaded) return;
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
		Loaded = true;
		Window.MouseDown += Window_MouseDown;
		Location = Location;
		Distance = new(Parent.Size.X - Size.X - Location.X, Parent.Size.Y - Size.Y - Location.Y);
	}

	public IParent? Parent { get; private set; }
	public Window? Window { get; private set; }

	private void Window_MouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
	{
		if (e.Button == MouseButton.Button1 &&
			Parent?.IntToFloat(Location.X) <= Parent?.IntToFloat((int)Parent?.MousePosition.X!) &&
			Parent?.IntToFloat(Size.X + Location.X) >= Parent?.IntToFloat((int)Parent?.MousePosition.X!) &&
			Parent?.IntToFloat(Location.Y + Size.Y, true) <= Parent?.IntToFloat((int)Parent?.MousePosition.Y!, true) &&
			Parent?.IntToFloat(Location.Y, true) >= Parent?.IntToFloat((int)Parent?.MousePosition.Y!, true))
		{
			if (Clicked is not null) Clicked.Invoke(this);
		}
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
				if (Loaded)
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
					if (Window is not null && Window.CanControleUpdate && Loaded) Window.DrawFrame();
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


	public event Func<IRenderObject, Task>? Clicked;

	public bool Loaded { get; private set; } = false;
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
			saf = new Vector2(Parent.IntToFloat(value.X + loc_.X, false), Parent.IntToFloat(value.Y + loc_.Y, true));
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
			laf = new Vector2(Parent.IntToFloat(value.X, false), Parent.IntToFloat(value.Y, true));
			temp[(Texture is null ? 6 : 10)] = laf.X;
			temp[(Texture is null ? 9 : 15)] = laf.X;
			temp[1] = laf.Y;
			temp[(Texture is null ? 10 : 16)] = laf.Y;
			saf = new Vector2(Parent.IntToFloat(Size.X + value.X, false), Parent.IntToFloat(Size.Y + value.Y, true));
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
