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
    public static SettingsControls Construct(ISettings settings)
    {
        
        var s = new SettingsControls();
        s.Setup(settings);
        return s;
    }
    private void Setup(ISettings settings)
    {
        Name = settings.Name;
        foreach (var option in settings.Options)
        {
            SetupOption(option);
        }
    }
    private void SetupOption(ISettingsOption option)
    {
        var l = new Label();
        l.Text = option.Name + ":";
        _vBox.AddChild(l);
        _vBox.AddChild(option.GetControlInterface());
    }
}
