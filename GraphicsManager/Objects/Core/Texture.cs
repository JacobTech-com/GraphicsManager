using GraphicsManager.Structs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace GraphicsManager.Objects.Core;

public class Texture
{
	public static readonly Shader TextureShader = new("RectangleTexture", true);

	public int handel;
	public Texture(byte[] File)
	{
		Image<Rgba32> image = Image.Load<Rgba32>(File);
		image.Mutate(x => x.Flip(FlipMode.Vertical));

		var pixels = new List<byte>(4 * image.Width * image.Height);

		for (int y = 0; y < image.Height; y++)
		{
			var row = image.GetPixelRowSpan(y);

			for (int x = 0; x < image.Width; x++)
			{
				pixels.Add(row[x].R);
				pixels.Add(row[x].G);
				pixels.Add(row[x].B);
				pixels.Add(row[x].A);
			}
		}

		handel = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, handel);
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
	}

	internal Texture(Label l, char charter, uint PixelHeight, Face face)
	{
		if (!Label._characters.ContainsKey(l)) Label._characters.Add(l, new Dictionary<uint, Character>());
		face.SetPixelSizes(0, PixelHeight);

		GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

		GL.ActiveTexture(TextureUnit.Texture0);
		face.SelectCharmap(Encoding.Unicode);
		ushort temp = ((ushort)charter);

		try
		{
			face.LoadChar(temp, LoadFlags.Render, LoadTarget.Normal);
			GlyphSlot glyph = face.Glyph;
			FTBitmap bitmap = glyph.Bitmap;

			handel = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, handel);
			GL.TexImage2D(TextureTarget.Texture2D, 0,
						  PixelInternalFormat.R8, bitmap.Width, bitmap.Rows, 0,
						  PixelFormat.Red, PixelType.UnsignedByte, bitmap.Buffer);


			Character cha = new()
			{
				Size = new Vector2(bitmap.Width, bitmap.Rows),
				Bearing = new Vector2(glyph.BitmapLeft, glyph.BitmapTop),
				Advance = (int)glyph.Advance.X.Value,
				Texture = this,
			};

			Label._characters[l].Add(temp, cha);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}

	public void LoadText()
	{
		GL.TextureParameter(handel, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
		GL.TextureParameter(handel, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		GL.TextureParameter(handel, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TextureParameter(handel, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
	}

	public void Load(int loc)
	{
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.EnableVertexAttribArray(loc);
		GL.VertexAttribPointer(loc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
	}

	public void Use(TextureUnit unit = TextureUnit.Texture0)
	{
		GL.ActiveTexture(unit);
		GL.BindTexture(TextureTarget.Texture2D, handel);
	}
}
