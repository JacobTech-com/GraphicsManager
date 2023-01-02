namespace GraphicsManager.Enums;

[Flags]
public enum ObjectAnchor
{
	Left = 0b_0001,
	Top = 0b_0010,
	Right = 0b_0100,
	Bottom = 0b_1000,
	All = Left | Top | Right | Bottom,
}
