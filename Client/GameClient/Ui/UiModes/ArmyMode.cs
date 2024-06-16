
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
            Cycle();
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

    private void Cycle()
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell != null)
        {
            var armyGraphics = _client.GetComponent<MapGraphics>()
                .GraphicLayerHolder.ArmyGraphics;
            if (armyGraphics.ArmiesInOrder
                    .TryGetValue(cell, out var armiesOnCell)
                        == false
                    || armiesOnCell.Count() == 0)
            {
                Army.Set(null);
                return;
            }
            var selected = Army.Value;
            if (selected == armiesOnCell[0])
            {
                armyGraphics.CycleArmies(cell, _client);
            }
            Army.Set(armiesOnCell[0]);
        }
    }

    public void SelectOrCycle(Army army)
    {
        if (Army.Value == army)
        {
            Cycle();
            return;
        }
        Army.Set(army);
        var armyGraphics = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.ArmyGraphics;
        armyGraphics.SetArmyToTop(army, _client);
    }

    private void MakeMouseActions()
    {
        var drawArmyLine = GetDrawArmyOccupancyMouseAction();


        var drawArmyAdvance = GetDrawArmyAdvanceMouseAction();


        var makeArmy = GetMakeArmyMouseAction();

        MouseActions = new ListSettingsOption<IMouseAction>(
            "Mouse Actions",
            new List<IMouseAction>{ drawArmyLine, 
                drawArmyAdvance, makeArmy },
            new List<string> { "Draw Army Line", 
                "Draw Army Advance", "Make Army" } );
    }

    private CellMousePressAction GetMakeArmyMouseAction()
    {
        var makeArmy = new CellMousePressAction(MouseButtonMask.Right,
            _mouseOverHandler, c =>
            {
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                return localAlliance.Members.Contains(c.Controller);
            });
        makeArmy.MouseReleased += cell =>
        {
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
            var command = new CreateArmyCommand(cell.MakeRef(), localPlayer.PlayerGuid);
            _client.HandleCommand(command);
        };
        return makeArmy;
    }

    private CellHashMouseAction GetDrawArmyAdvanceMouseAction()
    {
        var drawArmyAdvance = new CellHashMouseAction(
            _mouseOverHandler,
            (v) =>
            {
                var army = Army.Value;
                if (army is null) return false;

                var cell = v.prospect;
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                var res = false;
                if (localAlliance.Members.Contains(v.prospect.Controller))
                {
                    return false;
                }

                if (v.already.Any(c => cell.Neighbors.Contains(c.Id)))
                {
                    return true;
                }

                if (army.LineMission.LineCells.Any(c => cell.Neighbors.Contains(c)))
                {
                    return true;
                }

                return false;
            }, MouseButtonMask.Right, _client.Data);

        drawArmyAdvance.MouseReleased += advance =>
        {
            var army = Army.Value;
            if (army is null) return;

            var advanceZone = advance.Select(c => c.Id).ToHashSet();
            var exclusive = advanceZone.Except(army.LineMission.AdvanceInto);

            if (exclusive.Any() == false)
            {
                advanceZone = army.LineMission.AdvanceInto.Except(advanceZone).ToHashSet();
            }
            else
            {
                advanceZone = army.LineMission.AdvanceInto.Union(advanceZone).ToHashSet();
            }

            var order = new LineMission(army.LineMission.LineCells.ToHashSet(),
                advanceZone, false);
            var proc = new SetUnitOrderProcedure(army.MakeRef(),
                order);
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;

            var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
            _client.HandleCommand(com);
        };
        return drawArmyAdvance;
    }

    private CellHashMouseAction GetDrawArmyOccupancyMouseAction()
    {
        var drawArmyLine = new CellHashMouseAction(
            _mouseOverHandler,
            c =>
            {
                var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
                var localAlliance = localPlayer.Regime.Get(_client.Data).GetAlliance(_client.Data);
                return localAlliance.Members.Contains(c.prospect.Controller);
            },
            MouseButtonMask.Right,
            _client.Data
        );
        drawArmyLine.MouseReleased += l =>
        {
            var army = Army.Value;
            if (army is null) return;

            HashSet<int> occupy;
            HashSet<int> advanceInto = army.LineMission.AdvanceInto.ToHashSet();
            var drawn = l.Select(c => c.Id).ToHashSet();
            var old = army.LineMission.LineCells;
            var exclusive = drawn
                .Where(c => old.Contains(c) == false);
            if (exclusive.Any())
            {
                var intersect = drawn.Intersect(old);
                if (intersect.Any())
                {
                    occupy = drawn.Concat(old)
                        .ToHashSet();
                }
                else
                {
                    occupy = drawn;
                }
            }
            else
            {
                occupy = old.Except(l.Select(c => c.Id)).ToHashSet();
            }

            var order = new LineMission(occupy,
                advanceInto, false);
            var proc = new SetUnitOrderProcedure(army.MakeRef(),
                order);
            var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
            var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
            _client.HandleCommand(com);
        };
        return drawArmyLine;
    }
}