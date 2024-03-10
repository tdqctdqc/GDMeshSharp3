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
    
    public static Button GetToggleButton(Action pressed,
        Action unpressed)
    {
        var b = new Button();
        b.ToggleMode = true;
        b.FocusMode = Control.FocusModeEnum.None;
        b.ButtonUp += () =>
        {
            if (b.ButtonPressed)
            {
                pressed();
            }
            else
            {
                unpressed();
            }
        };
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

    public static void AddIntButton(this Node n, string name, Action<int> action)
    {
        var hbox = new HBoxContainer();
        var spinbox = new SpinBox();
        spinbox.Rounded = true;
        spinbox.Step = 1;
        spinbox.MinValue = 0;
        spinbox.MaxValue = 1_000_000;
        spinbox.UpdateOnTextChanged = true;
        spinbox.FocusMode = Control.FocusModeEnum.None;
        hbox.AddButton(name, () => action.Invoke((int)spinbox.Value));
        hbox.AddChild(spinbox);
        n.AddChild(hbox);
    }
}
