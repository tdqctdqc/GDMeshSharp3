using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettingsControls : ScrollContainer
{
    private VBoxContainer _vBox;
    private SettingsControls()
    {
        _vBox = new VBoxContainer();
        AddChild(_vBox);
    }
    public static SettingsControls Construct(ISettings settings, Vector2I size)
    {
        
        var s = new SettingsControls();
        s.Setup(settings, size);
        return s;
    }
    private void Setup(ISettings settings, Vector2I size)
    {
        Name = settings.Name;
        foreach (var option in settings.SettingsOptions)
        {
            SetupOption(option, size);
        }
    }
    private void SetupOption(ISettingsOption option, Vector2I size)
    {
        var l = new Label();
        l.Text = option.Name + ":";
        _vBox.AddChild(l);
        _vBox.CustomMinimumSize = size;
        _vBox.AddChild(option.GetControlInterface());
    }
}
