using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PromptSidebar : ScrollContainer
{
    private VBoxContainer _vBox;
    public PromptSidebar()
    {
        CustomMinimumSize = new Vector2(100f, 500f);
        _vBox = new VBoxContainer();
        _vBox.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_vBox);
        var p = new Panel();
        p.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(p);
    }

    public void AddPromptIcon(PromptSideIcon icon)
    {
        _vBox.AddChild(icon);
    }
}
