using Godot;
using System;
using System.Collections.Generic;

public partial class GameUi : Ui
{
    public PromptManager Prompts { get; private set; }
    public TooltipManager TooltipManager { get; private set; }
    public PromptSidebar PromptSidebar { get; private set; }
    public void Process(float delta, ICameraController cam, ClientWriteKey key)
    {
        TooltipManager.Process(delta, cam.GetMousePosInMapSpace());
    }

    public static GameUi Construct(GameClient client, bool host, Data data, MapGraphics graphics)
    {
        var ui = SceneManager.Instance<GameUi>();
            // new GameUi(client);
        ui.Setup(host, data, client);
        return ui;
    }
    private GameUi() : base() 
    {
    }

    protected GameUi(IClient client) : base()
    {
    }

    public void Setup(bool host, Data data, GameClient client)
    {
        Setup(client);
        AddWindow(LoggerWindow.Get());
        AddWindow(ClientSettingsWindow.Get());
        AddWindow(EntityOverviewWindow.Get(data));
        AddWindow(SettingsWindow.Get(Game.I.Client.Settings));
        AddWindow(new RegimeOverviewWindow());
        AddWindow(RegimeAiOverviewWindow.Get(data));

        var mapOptions = new MapDisplayOptionsUi();
        mapOptions.Setup(client.Graphics, data);
        mapOptions.Position = Vector2.Down * 50f;
        AddChild(mapOptions);

        PromptSidebar = (PromptSidebar)FindChild("side");
        
        Prompts = new PromptManager(this, data);
        AddChild(GameUiTopBarToken.Get(host,client, data).Container);
        TooltipManager = new TooltipManager(data);
        AddChild(TooltipManager);
        
    }
}
