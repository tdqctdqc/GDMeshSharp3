
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.ArmyMode;

public class ArmyMode : UiMode
{
    public DefaultSettingsOption<Army> Army { get; private set; }
    private HashSet<Cell> _moveRadiusCache;
    private Client _client;
    private Node2D _selectedArmyGraphic;
    private MouseOverHandler _mouseOverHandler;
    public ListSettingsOption<IMouseAction> MouseActions { get; private set; }
    public ArmyMode(Client client) : base(client, "Army")
    {
        _client = client;
        _mouseOverHandler = new MouseOverHandler(client.Data,
            c => c is not RiverCell);
        _mouseOverHandler.ChangedCell += c => Draw();
        _mouseOverHandler.ChangedSecondClosest += c => Draw();

        MakeMouseActions();
        
        Army = new DefaultSettingsOption<Army>("Army",
            null);
        Army.SettingChanged.Subscribe(n =>
        {
            GD.Print("setting");
            _moveRadiusCache = n.newVal?.GetArmyMoveRadius(_client.Data);
            Draw();
        });
    }
    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse m)
        {
            if (e is InputEventMouseButton mb
                && mb.ButtonIndex == MouseButton.Left
                && e.IsPressed() == false)
            {
                Army.Set(null);
            }
            MouseActions.Value.Process(m);
        }
    }

    public override void Enter()
    {
        var mb = new MeshBuilder();
        mb.AddCircle(Vector2.Zero, 20f, 20, new Color(Colors.Yellow, .5f));
        _selectedArmyGraphic = mb.GetMeshInstance();
        _selectedArmyGraphic.ZIndex = 99;
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        _selectedArmyGraphic.QueueFree();
    }

    private void Draw()
    {
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Clear();
        _mouseOverHandler.Highlight();
        MouseActions.Value.Highlight(_client);
        OverlayForArmy();
        UnitTooltip();
    }
    private void UnitTooltip()
    {
        
    }

    private void OverlayForArmy()
    {
        var army = Army.Value;
        if (army is null) return;
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        
        foreach (var cell in _moveRadiusCache)
        {
            highlight.Draw(mb =>
            {
                mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
            }, cell.RelTo);
        }
        highlight.Draw(mb =>
        {
            army.LineMission.Draw(army, Vector2.Zero, mb, _client.Data);
        }, Vector2.Zero);
        foreach (var order in army.OtherOrders)
        {
            highlight.Draw(mb =>
            {
                order.Draw(army, Vector2.Zero, mb, _client.Data);
            }, Vector2.Zero);
        }
    }
    
    
    

    private void MakeMouseActions()
    {
        var drawArmyLine = new DrawArmyLineMouseAction(this, _mouseOverHandler, _client);
        var drawArmyAdvance = new DrawArmyAdvanceMouseAction(_mouseOverHandler, this, _client);
        var makeArmy = new MakeArmyMouseAction(_mouseOverHandler, _client);

        MouseActions = new ListSettingsOption<IMouseAction>(
            "Mouse Actions",
            new List<IMouseAction>{ drawArmyLine, 
                drawArmyAdvance, makeArmy },
            new List<string> { "Draw Army Line", 
                "Draw Army Advance", "Make Army" } );
    }
}