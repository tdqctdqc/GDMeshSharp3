
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.ArmyMode;

public class ArmyMode : UiMode
{
    public DefaultSettingsOption<Army> Army { get; private set; }
    private MapOverlayDrawer _armyOverlay, _cellOverlay;
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
            DrawArmyOverlay();
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
        var mg = _client.GetComponent<MapGraphics>();
        _armyOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _cellOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        _selectedArmyGraphic.QueueFree();
        mg.RemoveOverlay(_armyOverlay);
        mg.RemoveOverlay(_cellOverlay);
    }

    public override Control GetControl(Client client)
    {
        return new ArmyPanel(client);
    }

    private void Draw()
    {
        _cellOverlay.Clear();
        _mouseOverHandler.Highlight(_cellOverlay);
        MouseActions.Value.Highlight(_client, _cellOverlay);
    }

    private void DrawArmyOverlay()
    {
        _armyOverlay.Clear();
        var army = Army.Value;
        if (army is null) return;
        foreach (var cell in army.GetArmyMoveRadius(_client.Data))
        {
            _armyOverlay.Draw(mb =>
            {
                mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
            }, cell.RelTo);
        }
        _armyOverlay.Draw(mb =>
        {
            army.LineMission.Draw(army, Vector2.Zero, mb, _client.Data);
        }, Vector2.Zero);
        foreach (var order in army.OtherOrders)
        {
            _armyOverlay.Draw(mb =>
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