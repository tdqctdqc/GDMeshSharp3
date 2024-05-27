
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ArmyMode : UiMode
{
    public DefaultSettingsOption<Army> Army { get; private set; }
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
        Army.SettingChanged.Subscribe(n => Draw());
    }
    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse m)
        {
            MouseActions.Value.Process(m);
        }
        if (e is InputEventMouseButton mb 
            && mb.ButtonIndex == MouseButton.Left 
            && mb.Pressed == false)
        {
            SelectAndCycle(_client);
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
        foreach (var cell in army.GetCells(_client.Data))
        {
            highlight.Draw(mb =>
            {
                mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
            }, cell.RelTo);
        }
        if (army.GroupOrder is not null)
        {
            highlight.Draw(mb =>
            {
                army.GroupOrder.Draw(army, Vector2.Zero, mb, _client.Data);
            }, Vector2.Zero);
        }
        
        
    }

    private void SelectAndCycle(Client c)
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell != null)
        {
            var layerHolder = _client.GetComponent<MapGraphics>()
                .GraphicLayerHolder;
            var armiesOnCell = c.Data.Military.UnitAux
                .ArmiesByOccupancy[cell];
            if (armiesOnCell is null || armiesOnCell.Count() == 0)
            {
                Army.Set(null);
                return;
            }
            var armiesOrdered = armiesOnCell.OrderBy(a => a.Id).ToList();
            var selected = Army.Value;
            if (selected is not null && armiesOrdered.Contains(selected))
            {
                var index = armiesOrdered.IndexOf(selected);
                var next = armiesOrdered[(index + 1) % armiesOrdered.Count];
                Army.Set(next);
            }
            else
            {
                Army.Set(armiesOrdered[0]);
            }
        }
    }

    private void MakeMouseActions()
    {
        var drawArmyLine = new CellLineMouseAction(
            MouseButtonMask.Right,
            c =>
            {
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                return localAlliance.Members.RefIds.Contains(c.Controller.RefId);
            },
            _client.Data, _mouseOverHandler);
        drawArmyLine.MouseReleased += l =>
        {
            var army = Army.Value;
            if (army is null) return;
            var order = new LineOrder(l.Select(c => c.Id).ToHashSet(),
                new HashSet<int>(), false);
            var proc = new SetUnitOrderProcedure(army.MakeRef(),
                order);
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
            var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
            _client.HandleCommand(com);
        };


        var drawArmyAdvance = new CellHashMouseAction(
            _mouseOverHandler,
            (v) =>
            {
                var army = Army.Value;
                if (army is null) return false;
                if (army.GroupOrder is LineOrder l == false)
                {
                    return false;
                }

                var cell = v.prospect;
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                var res = false;
                if (localAlliance.Members.RefIds.Contains(v.prospect.Controller.RefId))
                {
                    return false;
                }

                if (v.already.Any(c => cell.Neighbors.Contains(c.Id)))
                {
                    return true;
                }

                if (l.LineCells.Any(c => cell.Neighbors.Contains(c)))
                {
                    return true;
                }

                return false;
            }, MouseButtonMask.Right, _client.Data);

        drawArmyAdvance.MouseReleased += advance =>
        {
            var army = Army.Value;
            if (army is null) return;
            if (army.GroupOrder is LineOrder l == false)
            {
                return;
            }

            var advanceZone = advance.Select(c => c.Id).ToHashSet();
            var exclusive = advanceZone.Except(l.AdvanceInto);
            
            if (exclusive.Any() == false)
            {
                advanceZone = l.AdvanceInto.Except(advanceZone).ToHashSet();
            }
            else
            {
                advanceZone = l.AdvanceInto.Union(advanceZone).ToHashSet();
            }
            
            var order = new LineOrder(l.LineCells.ToHashSet(),
                advanceZone, false);
            var proc = new SetUnitOrderProcedure(army.MakeRef(),
                order);
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;

            var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
            _client.HandleCommand(com);
        };


        var makeArmy = new CellMousePressAction(MouseButtonMask.Right,
            _mouseOverHandler, c =>
            {
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                return localAlliance.Members.RefIds.Contains(c.Controller.RefId);
            });
        makeArmy.MouseReleased += cell =>
        {
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
            var command = new CreateArmyCommand(cell.MakeRef(), localPlayer.PlayerGuid);
            _client.HandleCommand(command);
        };

        MouseActions = new ListSettingsOption<IMouseAction>(
            "Mouse Actions",
            new List<IMouseAction>{ drawArmyLine, 
                drawArmyAdvance, makeArmy },
            new List<string> { "Draw Army Line", 
                "Draw Army Advance", "Make Army" } );
    }
}