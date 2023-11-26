using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ClosableWindow : Window
{
    public ClosableWindow()
    {
        this.MakeCloseable();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouse m && m.Position.InBox(Vector2.Zero, Size) == false)
        {
            // Game.I.Client.GetComponent<MapGraphics>()?.InputCatcher.HandleInput(m);
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey k && k.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
