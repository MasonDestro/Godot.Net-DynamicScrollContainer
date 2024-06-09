using Godot;
using System;

public partial class FramesPerSecond : Label
{
	public override void _Process(double delta)
	{
		Text = (1 / delta).ToString("F2");
	}
}
