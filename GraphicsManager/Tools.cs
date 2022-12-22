using System.Reflection;

namespace GraphicsManager;

public class Tools
{
	public static byte[] GetResourceBytes(Assembly Assembly, string Resource)
	{
		Stream str = Assembly.GetManifestResourceStream(Resource)!;
		MemoryStream ms = new();
		str.CopyTo(ms);
		str.Dispose();
		byte[] result = ms.ToArray();
		ms.Dispose();
		return result;
	}

	public static string GetResourceString(Assembly Assembly, string Resource)
	{
		Stream str = Assembly.GetManifestResourceStream(Resource)!;
		StreamReader sr = new(str);
		string result = sr.ReadToEnd();
		sr.Dispose();
		str.Dispose();
		return result;
	}

	public static byte[] GetResourceBytes(string Resource) => GetResourceBytes(typeof(Tools).Assembly, Resource);

	public static string GetResourceString(string Resource) => GetResourceString(typeof(Tools).Assembly, Resource);
}
