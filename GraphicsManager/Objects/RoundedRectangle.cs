using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.Intrinsics.X86;

namespace GraphicsManager.Objects;

public class RoundedRectangle : IRenderObject
{
	public static readonly Shader DefaultShader = Rectangle.DefaultShader;

	public ObjectAnchor Anchor { get; set; } = ObjectAnchor.Left | ObjectAnchor.Top;
	private const int sn = 4, r = 5;
	private int sn_ = sn, r_ = r;

	public RoundedRectangle()
	{
		Points_ = new float[36 + (((sn - 1) * 4) * 3)];
	}

	public Uniforms Uniforms { get; set; } = new() { Uniform4 = new() { new() { Location = 0, Value = new(0,0,0,1) } } };
	public int Radius
	{
		get
		{
			return r_;
		}
		set
		{
			r_ = value;
			Location = Location;
		}
	}

	public int Smoothness 
	{
		get
		{
			return sn_;
		}
		set
		{
			sn_ = value;
			List<uint> Indexs = this.Indexs.ToList().GetRange(0, 30);
			uint wall = 5;
			uint last = 12;
			for (uint i = 0; i < 4; i++)
			{
				if (value == 1)
				{
					Indexs.Add(i);
					Indexs.Add(wall);
					wall++;
					if (wall == 12) wall = 4;
					Indexs.Add(wall);
					wall++;
				}
				else
				{
					for (uint j = 0; j < value; j++)
					{
						Indexs.Add(i);
						if (j == 0 || j == value - 1)
						{
							if (wall == 12) wall = 4;
							Indexs.Add(wall);
							wall++;
						}
						else
						{
							Indexs.Add(last);
							last++;
						}
						Indexs.Add(last);

					}
					last++;
				}
			}
			this.Indexs = Indexs.ToArray();
			Location = Location;
		}
	}

	public bool Visible { get; set; } = true;

	public void Draw()
	{
		if (Visible && Loaded)
		{
			GL.Enable(EnableCap.Multisample);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
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
			GL.BindVertexArray(ArrayObject);
			
			GL.DrawElements(PrimitiveType.Triangles, Indexs.Length, DrawElementsType.UnsignedInt, 0);
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
		pos = 4;
		
		BufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, BufferObject);
		ArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(ArrayObject);
		int add = 3;
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, add * sizeof(float), 0);
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

	~RoundedRectangle()
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

	public uint[] Indexs { get; set; } = new uint[30 + ((sn * 4) * 3)] { 0, 1, 3, 1, 2, 3, 0, 3, 4, 0, 4, 5, 0, 1, 7, 0, 6, 7, 1, 2, 9, 1, 8, 9, 2, 3, 11, 2, 10, 11,
	0, 5, 12,
	0, 12, 13,
	0, 13, 14,
	0, 6, 14,
	
	1, 7, 15,
	1, 15, 16,
	1, 16, 17,
	1, 8, 17,
	
	2, 9, 18,
	2, 18, 19,
	2, 19, 20,
	2, 10, 20,
	
	3, 11, 21,
	3, 21, 22,
	3, 22, 23,
	3, 4, 23};
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
			Location = Location;
			saf = new Vector2(Parent.IntToFloat(value.X + loc_.X, false), Parent.IntToFloat(value.Y + loc_.Y, true));
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
			List<float> temp;
			saf = new Vector2(Parent.IntToFloat(Size.X + loc_.X, false), Parent.IntToFloat(Size.Y + loc_.Y, true));
			Vector3 _0 = Parent.PointToVector(value.X + Size.X - Radius, value.Y + Radius, 0);
			Vector3 _1 = Parent.PointToVector(value.X + Size.X - Radius, value.Y + Size.Y - Radius, 0);
			Vector3 _2 = Parent.PointToVector(value.X + Radius, value.Y + Size.Y - Radius, 0);
			Vector3 _3 = Parent.PointToVector(value.X + Radius, value.Y + Radius, 0);
			Vector3 _4 = Parent.PointToVector(value.X + Radius, value.Y, 0);
			Vector3 _5 = Parent.PointToVector(value.X + Size.X - Radius, value.Y, 0);
			Vector3 _6 = Parent.PointToVector(value.X + Size.X, value.Y + Radius, 0);
			Vector3 _7 = Parent.PointToVector(value.X + Size.X, value.Y + Size.Y - Radius, 0);
			Vector3 _8 = Parent.PointToVector(value.X + Size.X - Radius, value.Y + Size.Y, 0);
			Vector3 _9 = Parent.PointToVector(value.X + Radius, value.Y + Size.Y, 0);
			Vector3 _10 = Parent.PointToVector(value.X, value.Y + Size.Y - Radius, 0);
			Vector3 _11 = Parent.PointToVector(value.X, value.Y + Radius, 0);
			int[] ff = new int[]
			{
				value.X + Size.X - Radius,
				value.X + Size.X - Radius,
				value.X + Radius,
				value.X + Radius,
				value.Y + Radius,
				value.Y + Size.Y - Radius,
				value.Y + Size.Y - Radius,
				value.Y + Radius
			};
			float rotation = 90f / (Smoothness);
			temp = new()
			{
				_0.X, _0.Y, _0.Z, _1.X, _1.Y, _1.Z, _2.X, _2.Y, _2.Z, _3.X, _3.Y, _3.Z, _4.X, _4.Y, _4.Z, _5.X, _5.Y, _5.Z, _6.X, _6.Y, _6.Z, _7.X, _7.Y, _7.Z, _8.X, _8.Y, _8.Z, _9.X, _9.Y, _9.Z, _10.X, _10.Y, _10.Z, _11.X, _11.Y, _11.Z
			};
			for (int j = 0; j < 4; j++)
			{
				int start = 90 - (j * 90);
				for (int i = 0; i < Smoothness - 1; i++)
				{
					var degpre = (rotation * (i + 1));
					var deg = start - degpre;
					double y = ff[j + 4] - (Math.Sin(MathHelper.DegreesToRadians(deg)) * Radius);
					double x = (Math.Cos(MathHelper.DegreesToRadians(deg)) * Radius) + ff[j];
					Vector3 tri = Parent.PointToVector((int)x, (int)y, 0f);
					temp.Add(tri.X);
					temp.Add(tri.Y);
					temp.Add(tri.Z);
				}
			}
			Points = temp.ToArray();
		}
	}
	private Vector2 laf = new(), saf = new();

	public Vector2 LocationAsFloat { get { return laf; } }
	public Vector2 SizeAsFloat { get { return saf; } }
}
