using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.ArmyMode;

public class DrawArmyAdvanceMouseAction : CellHashMouseAction
{
    private global::ArmyMode _mode;
    public DrawArmyAdvanceMouseAction(MouseOverHandler mouseOverHandler, 
        global::ArmyMode mode, Client client) 
        : base(mouseOverHandler, v => Valid(v, mode, client),
            MouseButtonMask.Right,
            client.Data)
    {
        _mode = mode;
        AddDefaultAction(() => SetAdvance(_cells, client));
        AddShiftAction(() => AddToAdvance(_cells, client));
        AddCtrlAction(() => TrimAdvance(_cells, client));
    }

    private static bool Valid((Cell prospect, HashSet<Cell> already) v,
        global::ArmyMode mode, 
        Client client)
    {
        var army = mode.Army.Value;
        if (army is null) return false;

        var cell = v.prospect;
        if (cell is LandCell == false) return false;
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var alliance = army.Regime.Get(client.Data).GetAlliance(client.Data);
        var res = false;
        if (alliance.Members.Contains(v.prospect.Controller))
        {
            return false;
        }

        if (army.LineMission.AdvanceInto.Contains(cell.MakeRef())
            || v.already.Contains(cell)
            || army.LineMission.AdvanceInto.Refs.Any(c => cell.Neighbors.Contains(cell.Id))
            || v.already.Any(c => cell.Neighbors.Contains(c.Id)))
        {
            return true;
        }

        if (army.LineMission.LineCells.Get<Cell, CellRef>(client.Data)
            .Any(c => cell.Neighbors.Contains(c.Id)))
        {
            return true;
        }

        return false;
    }

    private void SetAdvance(HashSet<Cell> advance, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;

        var advanceZone = new RefSet<CellRef>(advance
            .Select(c => c.MakeRef()).ToHashSet());
            
        var order = new LineMission(army.LineMission.LineCells,
            advanceZone, false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }
    private void AddToAdvance(HashSet<Cell> advance, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;

        var advanceZone = new RefSet<CellRef>(advance
            .Select(c => c.MakeRef()).ToHashSet());
            
        advanceZone = new RefSet<CellRef>(
            army.LineMission.AdvanceInto.Refs
                .Union(advanceZone.Refs).ToHashSet());
        var order = new LineMission(army.LineMission.LineCells,
            advanceZone, false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }
    
    private void TrimAdvance(HashSet<Cell> advance, Client client)
    {
        var army = _mode.Army.Value;
        if (army is null) return;
            
        var advanceZone = 
            army.LineMission.AdvanceInto
                .Refs.ToHashSet();
            
        var advanceUnions =
            UnionFind.Find<Cell, List<Cell>>(advanceZone
                    .Where(c => advance.Contains(c.Get(client.Data)) == false)
                    .Select(r => r.Get(client.Data)),
                (c, d) => true,
                c => c.GetNeighbors(client.Data));

        advanceZone = advanceUnions
            .Where(u 
                => u.Any(c 
                    => c.GetNeighbors(client.Data).Any(
                        n => army.LineMission.LineCells.Contains(n.MakeRef()))))
            .SelectMany(u => u)
            .Select(c => c.MakeRef()).ToHashSet();
        
        var order = new LineMission(army.LineMission.LineCells,
            new RefSet<CellRef>(advanceZone), 
            false);
        var proc = new SetUnitOrderProcedure(army.MakeRef(),
            order);
        var localPlayer = client.Data.BaseDomain.PlayerAux.LocalPlayer;

        var com = new SendMessageCommand(proc, localPlayer.PlayerGuid);
        client.HandleCommand(com);
    }
}