using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ButtonExt
{
    public static Button GetButton(Action action)
    {
        var b = new Button();
        b.FocusMode = Control.FocusModeEnum.None;
        b.ButtonUp += action;
        return b;
    }
    public static Button GetButton(params Action[] action)
    {
        var b = new Button();
        b.FocusMode = Control.FocusModeEnum.None;
        for (var i = 0; i < action.Length; i++)
        {
            b.ButtonUp += action[i];
        }
        return b;
    }
    public static Button AddButton(this Node n, string name, Action action)
    {
        var button = ButtonExt.GetButton(action);
        button.Text = name;
        n.AddChild(button);
        return button;
    }
    public static void AddWindowButton<T>(this Node n, string name) where T : Window
    {
        var settingsWindowBtn
            = ButtonExt.GetButton(() => Game.I.Client
                .GetComponent<WindowManager>()
                .OpenWindow<T>());
        settingsWindowBtn.Text = name;
        n.AddChild(settingsWindowBtn);
    }
}
