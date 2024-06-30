using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public partial class SettingsWindow : ConfirmationDialog
{
    protected SettingsWindow()
    {
        Size = Vector2I.One * 800;
        this.MakeFreeable();
    }
    public static SettingsWindow Get(MultiSettings multi)
    {
        var sw = SceneManager.Instance<SettingsWindow>();
        multi.Settings.ForEach(s => sw.Setup(s));
        return sw;
    }
    protected void Setup(ISettings settings)
    {
        var tabs = new TabContainer();
        tabs.Size = Size;
        AddChild(tabs);
        var controls = SettingsControls.Construct(settings, Vector2I.One * 500);
        controls.Name = settings.Name;
        tabs.AddChild(controls);
    }
    protected void Setup(MultiSettings multi)
    {
        var tabs = new TabContainer();
        tabs.Size = Size;
        AddChild(tabs);
        foreach (var settings in multi.Settings)
        {
            var controls = SettingsControls.Construct(settings, Vector2I.One * 500);
            controls.Name = settings.Name;
            tabs.AddChild(controls);
        }
    }
}