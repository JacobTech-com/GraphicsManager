using GraphicsManager.Enums;
using GraphicsManager.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace GraphicsManager;

public class Window : NativeWindow , IParent
{
	public IParent? Parent { get; } = null;
	public Vector2 LocationAsFloat { get; } = new Vector2(0f, 0f);
	public Window(NativeWindowSettings nativeWindowSettings) : base(nativeWindowSettings)
	{
	}

	public Window() : base(new NativeWindowSettings())
	{
		
	}
	public Vector2i Position { get; } = new Vector2i(0, 0);
	public Color4 BackgroundColor { get; set; } = new Color4(0, 0, 0, 255);

	public ICollection<IRenderObject> Controls { get; } = new List<IRenderObject>();
#region Cool Math Things
	public float[] RctToFloat(int x, int y, int Width, int Height, bool hastexture = false, float z = 0.0f)
	{
		if (hastexture)
		{
			return new float[20] {
				IntToFloat(x + Width), IntToFloat(y, true), z, 1.0f, 1.0f,// top r
                IntToFloat(x + Width), IntToFloat(y + Height, true), z, 1.0f, 0.0f,//b r
                IntToFloat(x), IntToFloat(y + Height, true), z, 0.0f, 0.0f,//bot l
                IntToFloat(x), IntToFloat(y, true), z, 0.0f, 1.0f// top l
            };
		}
		else
		{
			return new float[12] {
				IntToFloat(x + Width), IntToFloat(y, true), z,// top r
                IntToFloat(x + Width), IntToFloat(y + Height, true), z, //b r
                IntToFloat(x), IntToFloat(y + Height, true), z, //bot l
                IntToFloat(x), IntToFloat(y, true), z,// top l
            };
		}
	}

	public Vector3 PointToVector(int x, int y, float z = 0.0f)
	{
		return new Vector3(IntToFloat(x), IntToFloat(y, true), z);
	}

	public float IntToFloat(int p, bool Invert = false)
	{
		int Size = (Invert ? this.Size.Y : this.Size.X);
		double half = Math.Round((double)Size / (double)2, 1);
		double Per = Math.Round((double)1 / half, 15);
		if (p == half) return 0.0f;
		if (Invert)
		{
			if (p > half) return (float)(((double)(p - half) * Per) * -1);
			else return (float)(1 - (p * Per));
		}
		else
		{
			if (p > half) return (float)((double)(p - half) * Per);
			else return (float)((1 - (p * Per)) * -1);
		}
	}

	public float FloatToInt(float p, bool Invert = false)
	{
		int Size = (Invert ? this.Size.Y : this.Size.X);
		double half = Math.Round((double)Size / (double)2, 15);
		if (p == 0) return (int)half;
		if (Invert)
		{
			if (p < 0)
			{
				p *= -1;
				p++;
				return (float)(half * p);
			}
			else
			{
				return (float)(half - (p * half));
			}
		}
		else
		{
			if (p < 0)
			{
				p *= -1;
				p++;
				return (float)(Size - (half * p));
			}
			else
			{
				return (float)(p * half + half);
			}
		}
	}
#endregion

	public void ParentResize(ResizeEventArgs e)
	{
		if (e.Width == 0 && e.Height == 0) return;
		base.OnResize(e);
		GL.Viewport(0, 0, e.Width, e.Height);
		foreach (IRenderObject Control in Controls)
		{
			if (!Control.Loaded) continue;
			bool top = (Control.Anchor & ObjectAnchor.Top) == ObjectAnchor.Top;
			bool left = (Control.Anchor & ObjectAnchor.Left) == ObjectAnchor.Left;
			bool right = (Control.Anchor & ObjectAnchor.Right) == ObjectAnchor.Right;
			bool bottom = (Control.Anchor & ObjectAnchor.Bottom) == ObjectAnchor.Bottom;
			if (!top && !bottom) { Control.Anchor |= ObjectAnchor.Top; top = true; }
			if (!left && !right) { Control.Anchor |= ObjectAnchor.Left; left = true; }
			int lx = (left ? Control.Location.X : Size.X - Control.Distance.X - Control.Size.X);
			int ly = (top ? Control.Location.Y : Size.Y - Control.Distance.Y - Control.Size.Y);
			int sy = (bottom ? Size.Y - Control.Distance.Y - ly : Control.Size.Y);
			int sx = (right ? Size.X - Control.Distance.X - lx : Control.Size.X);
			Control.Size = new(sx, sy);
			Control.Location = new(lx, ly);
			if (Control is IParent parent)
			{
				parent.ParentResize(e);
			}
		}
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		ParentResize(e);
	}

	public Rendertype Rendertype { get; set; } = Rendertype.ControlUpdates;

	internal bool CanControleUpdate { get; private set; } = true;

	public int FPS { get; set; } = 0;

	public void StartRender()
	{
		Context.MakeCurrent();
		ProcessEvents();
		DrawFrame();
		while (Exists && IsVisible && !IsExiting)
		{
			ProcessEvents();
			bool u = (Rendertype & Rendertype.ControlUpdates) == Rendertype.ControlUpdates;
			if (!u) DrawFrame();
			Thread.Sleep(8);
		}
	}

	public void DrawFrame()
	{
		GL.ClearColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, (BackgroundColor.A * -1) + 1);
		IEnumerable<IRenderObject> needload = Controls.Where(a => a.Loaded == false);
		
		if (needload.Any())
		{
			foreach (IRenderObject obj in needload)
			{
				obj.LoadToParent(this, this);
			}
		}

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		foreach (IRenderObject obj in Controls)
		{
			if (obj.Loaded) obj.Draw();
		}
		Context.SwapBuffers();
		
	}
}