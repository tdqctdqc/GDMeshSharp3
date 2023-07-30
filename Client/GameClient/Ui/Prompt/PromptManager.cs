
using System;
using System.Collections.Generic;
using Godot;

public partial class PromptManager : Node, IClientComponent
{
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    private Dictionary<Prompt, PromptWindow> _windows;
    private float _timer;
    private float _period = 1f;
    private Client _client;
    public PromptManager(Client client, Data data)
    {
        _client = client;
        _windows = new Dictionary<Prompt, PromptWindow>();
        data.BaseDomain.PlayerAux.SetLocalPlayer.SubscribeForNode(
            () =>
            {
                var p = data.BaseDomain.PlayerAux.LocalPlayer;
                if (p.Regime.Empty())
                {
                    AddPrompt(new ChooseRegimePrompt(data));
                }
            }, this);
        client.UiLayer.AddChild(this);
    }

    public void AddPrompt(Prompt prompt)
    {
        var icon = new PromptSideIcon();
        icon.Setup(prompt);
        Game.I.Client.GetComponent<UiFrame>().RightSidebar.AddChild(icon);
    }
    public void OpenPromptWindow(Prompt prompt)
    {
        var w = Game.I.Client.GetComponent<WindowManager>()
            .GetWindow<PromptWindow>();
        w.Setup(prompt);
        w.PopupCentered();
    }

    Node IClientComponent.Node => this;
}
