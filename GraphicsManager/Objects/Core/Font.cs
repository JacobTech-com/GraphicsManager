using System.Reflection;

namespace GraphicsManager.Objects.Core;

public class Font
{
	public void SetEmbeddedFont(string Font, Assembly? Assembly = null)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		this.Assembly = Assembly;
		this.Embeded = true;
		this.Name = Font;
	}

	public static Font MakeEmbeddedFont(string Font, Assembly? Assembly = null)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		return new Font()
		{
			Assembly = Assembly,
			Embeded = true,
			Name = Font,
		};
	}

	public static Font MakeFontFromFile(string Font)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		return new Font()
		{
			Assembly = null,
			Embeded = false,
			Name = Font,
		};
	}

	public void SetFontFile(string Font)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		this.Assembly = null;
		this.Embeded = false;
		this.Name = Font;
	}

	public byte[] GetData()
	{
		if (Embeded)
		{
			string Base = "GraphicsManager.Resources.Fonts.";
			if (Assembly is not null) Base = string.Empty;
			return (Assembly is null ? Tools.GetResourceBytes(Base + Name) : Tools.GetResourceBytes(Assembly!, $"{Base}{Name}"));
		}
		return File.ReadAllBytes(Name);
	}

	public string Name { get; private set; } = "shal be default";
	public bool Embeded { get; private set; }
	public Assembly? Assembly { get; private set; }
}
