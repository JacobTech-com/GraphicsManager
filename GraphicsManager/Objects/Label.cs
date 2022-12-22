using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using GraphicsManager.Objects.Core;
using GraphicsManager.Structs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using System.Reflection;
using Encoding = SharpFont.Encoding;

namespace GraphicsManager.Objects;

public class Label : IRenderObject
{
	public static readonly Shader DefaultTextShader = new("Label", true);
	public IParent? Parent { get; private set; }
	public ObjectAnchor Anchor { get; set; } = ObjectAnchor.Left | ObjectAnchor.Top;
	private Vector2 laf = new(), saf = new();

	public Vector2 LocationAsFloat { get { return laf; } }
	public Vector2 SizeAsFloat { get { return saf; } }
	public bool Visible { get; set; } = true;

	public static readonly Dictionary<Label, Dictionary<uint, Character>> _characters = new();
	private string text = string.Empty;
	public int VAO { get; set; }
	public int VBO { get; set; }
	public Vector2 DIR { get; set; } = new Vector2(1f, 0f);
	public string Text
	{
		get => text;
		set
		{
			if (Loaded)
			{
				Library lib = new();
				Face face = new(lib, Font.GetData(), 0);

				face.SetPixelSizes(0, PixelHeight);

				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

				face.SelectCharmap(Encoding.Unicode);
				if (!_characters.ContainsKey(this)) _characters.Add(this, new Dictionary<uint, Character>());
				foreach (char character in value)
				{
					if (_characters[this].ContainsKey(character) == false)
					{
						var f = new Texture(this, character, PixelHeight, face);
						f.LoadText();
					}
				}
			}
			text = value;
		}
	}
	public uint PixelHeight { get; set; } = 20;
	public float Scale { get; set; } = 1.2f;
	public Shader Shader { get; } = DefaultTextShader;
	public Font Font { get; set; } = Font.MakeEmbeddedFont("TektonPro-Regular.otf");

	public Vector4 Color { get; set; } = new Vector4(1, 1, 1, 1);
	public Vector2i Distance { get; private set; }
	private Vector2i loc_ = new();
	private Vector2i locc_ = new();
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
			locc_ = new(value.X + Parent.Position.X, value.Y + Parent.Position.Y);
		}
	}
	public Vector2i Size { get; set; }

	public void Clean()
	{

	}

	public void Draw()
	{
		if (Visible & loadd)
		{
			Shader.Use();
			GL.Enable(EnableCap.Blend);
			GL.Uniform4(2, Color);
			Matrix4 projectionM = Matrix4.CreateOrthographicOffCenter(0, Window!.Size.X, Window!.Size.Y, 0, -1.0f, 1.0f);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.UniformMatrix4(1, false, ref projectionM);

			GL.BindVertexArray(VAO);

			float angle_rad = (float)Math.Atan2(DIR.Y, DIR.X);
			Matrix4 rotateM = Matrix4.CreateRotationZ(angle_rad);
			Matrix4 transOriginM = Matrix4.CreateTranslation(new Vector3(locc_.X, locc_.Y, 0f));

			float char_x = 0.0f;

			Library lib = new();

			Face face = new(lib, Font.GetData(), 0);

			face.SetPixelSizes(0, PixelHeight);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

			GL.ActiveTexture(TextureUnit.Texture0);
			face.SelectCharmap(Encoding.Unicode);


			float hhh = 0f;
			foreach (char c in Text)
			{
				if (!_characters[this].ContainsKey(c)) break;
				Character ch = _characters[this][c];

				if (c == '\n')
				{
					hhh += PixelHeight;
					char_x = 0f;
				}
				else
				{
					float w = ch.Size.X * Scale;
					float h = ch.Size.Y * Scale;
					float xrel = char_x + ch.Bearing.X * Scale;
					float yrel = (ch.Size.Y - ch.Bearing.Y) * Scale;
					yrel += hhh;
					char_x += (ch.Advance >> 6) * Scale;

					Matrix4 scaleM = Matrix4.CreateScale(new Vector3(w, h, 1.0f));
					Matrix4 transRelM = Matrix4.CreateTranslation(new Vector3(xrel, yrel, 0.0f));

					Matrix4 modelM = scaleM * transRelM * rotateM * transOriginM;
					GL.UniformMatrix4(0, false, ref modelM);

					ch.Texture.Use();

					GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
				}
			}

			GL.Disable(EnableCap.Blend);
		}
	}
	public Window? Window { get; private set; }

	public void LoadToParent(IParent window, Window win)
	{
		Parent = window;
		Window = win;
		//X = window.FloatToInt(X, window.Size.X);
		//Y = window.FloatToInt(Y, window.Size.Y, true);
		Library lib = new();

		Face face = new(lib, Font.GetData(), 0);
		face.SetPixelSizes(0, PixelHeight);
		GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
		face.SelectCharmap(Encoding.Unicode);

		GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

		float[] vquad =
		{
			0.0f, -1.0f,   0.0f, 0.0f,
			0.0f,  0.0f,   0.0f, 1.0f,
			1.0f,  0.0f,   1.0f, 1.0f,
			0.0f, -1.0f,   0.0f, 0.0f,
			1.0f,  0.0f,   1.0f, 1.0f,
			1.0f, -1.0f,   1.0f, 0.0f
		};

		VBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, 4 * 6 * 4, vquad, BufferUsageHint.StaticDraw);

		VAO = GL.GenVertexArray();
		if (!_characters.ContainsKey(this)) _characters.Add(this, new Dictionary<uint, Character>());
		foreach (char character in Text)
		{
			if (_characters[this].ContainsKey(character) == false)
			{
				var f = new Texture(this, character, PixelHeight, face);
				f.LoadText();
			}
		}
		GL.BindVertexArray(VAO);
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * 4, 0);
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * 4, 2 * 4);

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.BindVertexArray(0);
		Text = text;
		loadd = true;
		Location = Location;
		Distance = new(window.Size.X - Size.X - Location.X, window.Size.Y - Size.Y - Location.Y);
	}

	private bool loadd = false;

	public event Func<IRenderObject, Task>? Clicked;

	public bool Loaded => loadd;
}
