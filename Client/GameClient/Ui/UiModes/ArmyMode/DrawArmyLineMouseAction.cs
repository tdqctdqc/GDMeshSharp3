using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.ArmyMode;

public class DrawArmyLineMouseAction : CellHashMouseAction
{
    private global::ArmyMode _mode;
    public DrawArmyLineMouseAction(global::ArmyMode armyMode,
        MouseOverHandler mouseOverHandler,
        Client client) 
        : base(mouseOverHandler, v 
                => Valid(v, client),
            MouseButtonMask.Right, client.Data)
    {
        _mode = armyMode;
        AddDefaultAction(() => DrawNewLine(_cells, client));
        AddShiftAction(() => AddToLine(_cells, client));
        AddCtrlAction(() => TrimLine(_cells, client));
    }

    private static bool Valid((Cell prospect, HashSet<Cell> already) v,
        Client client)
    {
        if (v.prospect is LandCell == false) return false;
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localAlliance = localPlayer.Regime.Get(client.Data).GetAlliance(client.Data);
        return localAlliance.Members.Contains(v.prospect.Controller);
    }
    private void DrawNewLine(HashSet<Cell> l, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;
        var drawn = l.Select(c => c.MakeRef()).ToHashSet();
        var order = new LineMission(
            new RefSet<CellRef>(drawn),
            new RefSet<CellRef>(new HashSet<CellRef>()), false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }

    private void AddToLine(HashSet<Cell> l, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;

        HashSet<CellRef> occupy;
        var old = army.LineMission.LineCells
            .Refs;
        var drawn = l.Select(c => c.MakeRef()).ToHashSet();
            
        var adj = old.Intersect(drawn).Any()
                  || old.SelectMany(c => c.Get(client.Data).GetNeighbors(client.Data)
                  ).Any(l.Contains);
        if (adj == false) return;
        occupy = drawn.Concat(old)
            .ToHashSet();
        var order = new LineMission(
            new RefSet<CellRef>(occupy),
            new RefSet<CellRef>(army.LineMission.AdvanceInto.Refs.ToHashSet()), false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }
    
    private void TrimLine(HashSet<Cell> l, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;

        var old = army.LineMission.LineCells
            .Refs;
        var drawn = l.Select(c => c.MakeRef()).ToHashSet();
        HashSet<CellRef> occupy = old.Except(drawn).ToHashSet();
        if (occupy.Count == 0) return;
        var advanceUnions =
            UnionFind.Find<Cell, List<Cell>>(army.LineMission.AdvanceInto.Get<Cell, CellRef>(client.Data),
                (c, d) => true,
                c => c.GetNeighbors(client.Data));

        var newAdvance = advanceUnions
            .Where(u 
                => u.Any(c 
                    => c.GetNeighbors(client.Data).Any(
                        n => occupy.Contains(n.MakeRef()))))
            .SelectMany(u => u)
            .Select(c => c.MakeRef()).ToHashSet();
            
        var order = new LineMission(
            new RefSet<CellRef>(occupy),
            new RefSet<CellRef>(newAdvance), false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }
}