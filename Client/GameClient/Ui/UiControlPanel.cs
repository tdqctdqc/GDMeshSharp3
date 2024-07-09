
using System;
using Godot;

public partial class UiControlPanel : Panel
{
    private UiControlPanel()
    {
    }

    public UiControlPanel(Client client)
    {
        SelfModulate = Colors.Black;
        CustomMinimumSize = new Vector2(300f, 600f);
        
        var vbox = this.MakeScrollChild<VBoxContainer>(out var scroll);
        vbox.FullRect();
        var options = client.UiController.ModeOption
            .GetControlInterface();
        vbox.AddChild(options);
    }

    public void Process(float delta)
    {
        
    }
    
}