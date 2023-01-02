using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsManager.Objects.Core;

public class Uniforms
{
	public List<Uniform<Vector4>> Uniform4 { get; set; } = new();
	public List<Uniform<Vector3>> Uniform3 { get; set; } = new();
	public List<Uniform<Vector2>> Uniform2 { get; set; } = new();
	public List<Uniform<float>> Uniform1 { get; set; } = new();
}
