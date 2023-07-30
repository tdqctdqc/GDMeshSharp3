using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameUiTopBar : VBoxContainer, IClientComponent
{
    private Button _submitTurn;
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
    }
    public GameUiTopBar(Client client, bool host, Data data)
    {
        var frame = client.GetComponent<UiFrame>();
        frame.AddTopBar(this);
        
        var regimeInfoBar = new RegimeInfoBar(data, host);
        
        
        var general = new HBoxContainer();
        general.AddChild(regimeInfoBar);
        AddChild(general);

        general.AddWindowButton<RegimeAiOverviewWindow>("Regime Ais");
        
        _submitTurn = general.AddButton("Submit Turn", () =>
        {
            if (data.BaseDomain.PlayerAux.LocalPlayer.Regime.Fulfilled()
                && data.ClientPlayerData.MajorOrders != null 
                && data.ClientPlayerData.MinorOrders != null)
            {
                data.ClientPlayerData.SubmitOrders(data);
                _submitTurn.Text = "Turn Submitted";
                _submitTurn.Disabled = true;
            }
        });
        data.Notices.Ticked.SubscribeForNode(i =>
            {
                _submitTurn.Text = "Submit Turn";
                _submitTurn.Disabled = false;
            }, this);

        
        AddChild(new ItemBar(data));
    }

    private GameUiTopBar()
    {
    }

    Node IClientComponent.Node => this;
    
}