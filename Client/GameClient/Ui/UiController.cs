
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class UiController : Node, IClientComponent
{
    private Client _client;
    public UiMode Mode => ModeOption.Value;
    public TypedSettingsOption<UiMode> ModeOption { get; private set; }
    public Node Node => this;

    public UiController(Client client)
    {
        _client = client;
        Disconnect += () => throw new Exception();
        var modes = new List<UiMode>
        {
            new BlankMode(client),
            new UnitMode(client),
            new PolyMode(client),
            // new TacticalMode(client),
            new DeploymentMode(client),
            // new HighlightCellsMode(client),
            new PathFindMode(client),
            new MilPlanningMode(client)
        };
        var names = modes.Select(m => m.GetType().Name).ToList();
        ModeOption = new TypedSettingsOption<UiMode>("Ui Mode",
            modes, names);
        ModeOption.SettingChanged.Subscribe(v =>
        {
            v.oldVal?.Clear();
        });
    }


    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        Mode?.Process(delta);
    }
}