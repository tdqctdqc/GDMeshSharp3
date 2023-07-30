using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class UiFrame : VBoxContainer, IClientComponent
{
    public VBoxContainer TopBars { get; private set; }
    public VBoxContainer LeftSidebar { get; private set; }
    public VBoxContainer RightSidebar { get; private set; }
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    public UiFrame(Client client)
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.FullRect);
        TopBars = new VBoxContainer();
        AddChild(TopBars);
        var sidebars = new HBoxContainer();
        sidebars.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(sidebars);

        sidebars.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        sidebars.SetAnchorsPreset(LayoutPreset.HcenterWide);
            
        LeftSidebar = new VBoxContainer();
        LeftSidebar.SetAnchorsPreset(LayoutPreset.LeftWide);
        sidebars.AddChild(LeftSidebar);

        var filler = new Control();
        filler.GrowHorizontal = GrowDirection.Both;
        filler.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        filler.MouseFilter = MouseFilterEnum.Ignore;
        sidebars.AddChild(filler);
        
        
        RightSidebar = new VBoxContainer();
        RightSidebar.SetAnchorsPreset(LayoutPreset.RightWide);
        sidebars.AddChild(RightSidebar);

        client.UiLayer.AddChild(this);
    }

    public void AddTopBar(Node topBar)
    {
        TopBars.AddChild(topBar);
    }
    Node IClientComponent.Node => this;
}
