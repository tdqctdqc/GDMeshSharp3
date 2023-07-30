using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ButtonExt
{
    public static Button AddButton(this Node n, string name, Action action)
    {
        var button = new Button();
        button.ButtonUp += action;
        button.Text = name;
        n.AddChild(button);
        return button;
    }
    public static void AddWindowButton<T>(this Node n, string name) where T : Window
    {
        var settingsWindowBtn
            = new Button();
        settingsWindowBtn.Text = name;
        settingsWindowBtn.ButtonUp += () => Game.I.Client
            .GetComponent<WindowManager>()
            .OpenWindow<T>();
        n.AddChild(settingsWindowBtn);
    }
}
