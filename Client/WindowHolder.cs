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
    public void OpenWindow(Window w)
    {
        AddChild(w);
        w.PopupCenteredClamped();
    }
}
