using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameUiTopBarToken : ButtonBarToken
{
    private Button _submitTurn;
    public static GameUiTopBarToken Get(bool host, GameClient client, Data data)
    {
        var g = new GameUiTopBarToken();
        Create<GameUiTopBarToken, HBoxContainer>(g);
        g.Setup(host,client, data);
        return g;
    }
    
    public void Setup(bool host, GameClient client, Data data)
    {
        AddWindowButton<EntityOverviewWindow>("Entities");
        AddWindowButton<ClientSettingsWindow>("Client Settings");
        AddWindowButton<LoggerWindow>("Logger");
        AddWindowButton<RegimeAiOverviewWindow>("Regime Ais");
        _submitTurn = AddButton("Submit Turn", () =>
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
        data.Notices.Ticked.Subscribe(i =>
        {
            _submitTurn.Text = "Submit Turn";
            _submitTurn.Disabled = false;
        });
        
        
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        Container.AddChildWithVSeparator(hostClientLabel);
        Container.AddChildWithVSeparator(TickDisplay.Create(data));
        Container.AddChildWithVSeparator(PlayerRegimeDisplay.Create(data));

        var itemBar = new ItemBar();
        itemBar.Setup(data);
        Container.AddChildWithVSeparator(itemBar);

        var peepsInfo = new RegimePeepsInfoBar();
        peepsInfo.Setup(data);
        Container.AddChildWithVSeparator(peepsInfo);
    }
}