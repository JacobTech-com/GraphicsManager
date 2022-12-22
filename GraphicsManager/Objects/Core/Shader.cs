using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace GraphicsManager.Objects.Core;

public class Shader : IDisposable
{
	public int Handle { get; }
	private readonly int VertexShader;
	private readonly int FragmentShader;
	private bool disposedValue = false;

	public Shader(string VertexShaderSource, string FragmentShaderSource, bool VertextBuiltIn = false, bool FragmentShaderBuiltIn = false, Assembly? Assembly = null)
	{
		VertexShader = GL.CreateShader(ShaderType.VertexShader);
		string Base = "GraphicsManager.Resources.Shaders.";
		if (Assembly is not null) Base = string.Empty;
		string vss = (VertextBuiltIn ? Tools.GetResourceString((Assembly == null ? typeof(Tools).Assembly : Assembly), $"{Base}{VertexShaderSource}") : File.ReadAllText(VertexShaderSource))!;
		string fss = (FragmentShaderBuiltIn ? Tools.GetResourceString((Assembly ?? typeof(Tools).Assembly), $"{Base}{FragmentShaderSource}") : File.ReadAllText(FragmentShaderSource))!;
		GL.ShaderSource(VertexShader, vss);

		FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(FragmentShader, fss);

		GL.CompileShader(VertexShader);

		string infoLogVert = GL.GetShaderInfoLog(VertexShader);
		if (infoLogVert != string.Empty)
			Console.WriteLine(infoLogVert);

		GL.CompileShader(FragmentShader);

		string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

		if (infoLogFrag != string.Empty)
			Console.WriteLine(infoLogFrag);

		Handle = GL.CreateProgram();

		GL.AttachShader(Handle, VertexShader);
		GL.AttachShader(Handle, FragmentShader);

		GL.LinkProgram(Handle);

		GL.DetachShader(Handle, VertexShader);
		GL.DetachShader(Handle, FragmentShader);
		GL.DeleteShader(FragmentShader);
		GL.DeleteShader(VertexShader);
	}

	public Shader(string ShaderSource, bool Embeded = false, Assembly? Assembly = null)
	{
		VertexShader = GL.CreateShader(ShaderType.VertexShader);
		string Base = "GraphicsManager.Resources.Shaders.";
		if (Assembly is not null) Base = string.Empty;
		GL.ShaderSource(VertexShader, (Embeded ? Tools.GetResourceString((Assembly ?? typeof(Tools).Assembly), $"{Base}{ShaderSource}.vert") : File.ReadAllText($"{ShaderSource}.vert")));

		FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(FragmentShader, (Embeded ? Tools.GetResourceString((Assembly ?? typeof(Tools).Assembly), $"{Base}{ShaderSource}.frag") : File.ReadAllText($"{ShaderSource}.frag")));

		GL.CompileShader(VertexShader);

		string infoLogVert = GL.GetShaderInfoLog(VertexShader);
		if (infoLogVert != string.Empty)
			Console.WriteLine(infoLogVert);

		GL.CompileShader(FragmentShader);

		string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

		if (infoLogFrag != string.Empty)
			Console.WriteLine(infoLogFrag);

		Handle = GL.CreateProgram();

		GL.AttachShader(Handle, VertexShader);
		GL.AttachShader(Handle, FragmentShader);

		GL.LinkProgram(Handle);

		GL.DetachShader(Handle, VertexShader);
		GL.DetachShader(Handle, FragmentShader);
		GL.DeleteShader(FragmentShader);
		GL.DeleteShader(VertexShader);
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}

	public void SetInt(string name, int value)
	{
		int location = GL.GetUniformLocation(Handle, name);

		GL.Uniform1(location, value);
	}

	public Shader Clone()
	{
		return (Shader)MemberwiseClone();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			GL.DeleteProgram(Handle);

			disposedValue = true;
		}
	}

	~Shader()
	{
		GL.DeleteProgram(Handle);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
