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

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey k && k.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
