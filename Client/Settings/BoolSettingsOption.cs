using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BoolSettingsOption : SettingsOption<bool>
{
    public BoolSettingsOption(string name, bool value) : base(name, value)
    {
    }

    public override Control GetControlInterface()
    {
        var hbox = new HBoxContainer();
        var label = new Label();
        hbox.AddChild(label);
        label.Text = $"{Name}: ";
        var check = new CheckBox();
        check.FocusMode = Control.FocusModeEnum.None;
        check.ToggleMode = true;
        check.Toggled += Set;
        hbox.AddChild(check);
        return hbox;
    }
}
