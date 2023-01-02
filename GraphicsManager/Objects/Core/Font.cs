using System.Diagnostics;
using System.Reflection;
using SharpFont;

namespace GraphicsManager.Objects.Core;

public class Font
{
	private List<Face> _Faces = new();
	private Library lib;
	public IReadOnlyList<Face> Faces => _Faces.AsReadOnly();
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
		Font fontclass = new Font()
		{
			Assembly = Assembly,
			Embeded = true,
			Name = Font,
		};
		Library lib = new();
		string Base = "GraphicsManager.Resources.Fonts.";
		if (Assembly is not null) Base = string.Empty;
		byte[] f = (Assembly is null
			? Tools.GetResourceBytes(Base + Font)
			: Tools.GetResourceBytes(Assembly!, $"{Base}{Font}"));
		fontclass._Faces.Add(new Face(lib, f, 0));
		//load the fucking system fonts
		if (OperatingSystem.IsLinux())
		{
			try
			{
				Process proc = new()
				{
				};
				proc.Start();
				proc.WaitForExit();
				string[] files = proc.StandardOutput.ReadToEnd().Split($":{Environment.NewLine}");
				for (int i = 0; i < files.Length; i++)
				{
					fontclass._Faces.Add(new Face(lib, files[i], 0));
				}
			}
			catch
			{
			}
		}
		return fontclass;
	}
	
	public static Font MakeFontFromSystem()
	{
		
		Font fontclass = new()
		{
			Assembly = null,
			Embeded = false,
			Name = default!,
			lib = new()
		};
		if (OperatingSystem.IsLinux())
		{
			try
			{
				Process proc = new()
				{
					StartInfo = new()
					{
						FileName = "/bin/bash",
						Arguments = "-c \"fc-list ':' file\"",
						RedirectStandardOutput = true,
					}
				};
				proc.Start();
				proc.WaitForExit();
				string[] files = proc.StandardOutput.ReadToEnd().Split($": {Environment.NewLine}");
				for (int i = 0; i < files.Length; i++)
				{
					fontclass._Faces.Add(new Face(fontclass.lib, files[i], 0));
				}
			}
			catch
			{
			}
		}
		return fontclass;
	}

	public static Font MakeFontFromFile(string Font)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		Font fontclass = new Font()
		{
			Assembly = null,
			Embeded = false,
			Name = Font,
		};
		fontclass.lib = new();
		fontclass._Faces.Add(new Face(fontclass.lib, File.ReadAllBytes(Font), 0));
		//load the fucking system fonts
		if (OperatingSystem.IsLinux())
		{
			try
			{
				Process proc = new()
				{
				};
				proc.Start();
				proc.WaitForExit();
				string[] files = proc.StandardOutput.ReadToEnd().Split($":{Environment.NewLine}");
				for (int i = 0; i < files.Length; i++)
				{
					fontclass._Faces.Add(new Face(fontclass.lib, files[i], 0));
				}
			}
			catch
			{
			}
		}
		return fontclass;
	}

	public void SetFontFile(string Font)
	{
		_ = Font ?? throw new ArgumentNullException(nameof(Font));
		_Faces.RemoveAt(0);
		_Faces.Reverse();
		_Faces.Add(new Face(lib, File.ReadAllBytes(Font), 0));
		_Faces.Reverse();
		this.Assembly = null;
		this.Embeded = false;
		this.Name = Font;
	}
	
	public string Name { get; private set; } = default!;
	public bool Embeded { get; private set; }
	public Assembly? Assembly { get; private set; }
}
