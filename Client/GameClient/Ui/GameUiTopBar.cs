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
        
        var regimeInfoBar = new RegimeInfoBar(client, data, host);
        
        
        var general = new HBoxContainer();
        general.AddChild(regimeInfoBar);
        AddChild(general);

        general.AddWindowButton<RegimeAiOverviewWindow>("Regime Ais");
        general.AddWindowButton<MarketOverviewWindow>("Market");
        
        _submitTurn = general.AddButton("Submit Turn", () =>
        {
            if (data.BaseDomain.PlayerAux.LocalPlayer.Regime.Fulfilled()
                && data.ClientPlayerData.MajorOrders != null 
                && data.ClientPlayerData.MinorOrders != null)
            {
                var orders = data.ClientPlayerData.GetOrdersForThisTurn(data);

                var c = SubmitTurnCommand.Construct(orders, data.ClientPlayerData.LocalPlayerGuid);
                client.Server.QueueCommandLocal(c);

                _submitTurn.Text = "Turn Submitted";
                _submitTurn.Disabled = true;
            }
        });
        data.Notices.Ticked.SubscribeForNode(i =>
        {
            client.QueuedUpdates.Enqueue(
                () =>
                {
                    _submitTurn.Text = "Submit Turn";
                    _submitTurn.Disabled = false; 
                }
            );
        }, this);
        AddChild(new ItemBar(client, data));
        if (host)
        {
            var ordersReadyLabel = NodeExt.MakeStatDisplay(
                client, client.Data, () =>
                {
                    if (client.Logic is HostLogic log)
                    {
                        var ais = log.OrderHolder.GetNumAisReady(data);
                        var players = log.OrderHolder.GetNumPlayersReady(data);
                        return $"Ais ready: {ais.X} / {ais.Y}  " +
                               $"Players ready: {players.X} / {players.Y}";
                    }

                    return "";
                },
                10f, client.UiTick);
            AddChild(ordersReadyLabel);
        }
        
    }

    private GameUiTopBar()
    {
    }

    Node IClientComponent.Node => this;
    
}