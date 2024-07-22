using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class WindowHolder : Node, IClientComponent
{
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }
    public WindowHolder(Client client)
    {
       client.UiLayer.AddChild(this);
    }
    public void OpenWindowFullSize(Window w)
    {
        AddChild(w);
        w.Size = DisplayServer.WindowGetSize();
        w.PopupCenteredClamped(null, .9f);
    }
    public void OpenWindow(Window w)
    {
        AddChild(w);
        w.PopupCenteredClamped(w.Size);
    }
}
