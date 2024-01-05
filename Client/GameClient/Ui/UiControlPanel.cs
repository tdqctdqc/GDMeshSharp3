
using System;
using Godot;

public partial class UiControlPanel : Panel, IClientComponent
{
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }
    private ScrollContainer _scroll;
    private UiControlPanel()
    {
    }

    public UiControlPanel(Client client)
    {
        SelfModulate = Colors.Black;
        CustomMinimumSize = new Vector2(300f, 600f);
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _scroll = new ScrollContainer();
        _scroll.AnchorsPreset =  (int)LayoutPreset.FullRect;
        MouseFilter = MouseFilterEnum.Stop;
        _scroll.CustomMinimumSize = new Vector2(300f, 600f);
        AddChild(_scroll);
        var vbox = new VBoxContainer();
        vbox.AnchorsPreset = (int)LayoutPreset.FullRect;
        vbox.CustomMinimumSize = new Vector2(300f, 600f);
        _scroll.AddChild(vbox);
        var options = client.UiController.ModeOption.GetControlInterface();
        vbox.AddChild(options);
        
        client.GetComponent<UiFrame>().RightSidebar.AddChild(this);
    }

    public void Process(float delta)
    {
        
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        _scroll._GuiInput(@event);
        GetViewport().SetInputAsHandled();
    }
}