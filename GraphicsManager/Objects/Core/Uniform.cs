using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace GraphicsManager.Objects.Core;

public record Uniform<TVertex> where TVertex : struct
{
	public int Location { get; set; }
	public TVertex Value { get; set; }
}
