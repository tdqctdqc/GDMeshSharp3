
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineMission : ArmyMission
{
    public RefSet<CellRef> LineCells { get; private set; }
    public RefSet<CellRef> AdvanceInto { get; private set; }
    public bool Advance { get; private set; }
    public LineMission(RefSet<CellRef> lineCells, 
        RefSet<CellRef> advanceInto,
        bool advance)
    {
        LineCells = lineCells;
        AdvanceInto = advanceInto;
        Advance = advance;
    }

    public override void Handle(Army g, LogicKey key,
        HandleUnitMissionsProcedure proc)
    {
        if (LineCells.Count() == 0)
        {
            LineCells.Add(g.Cells.Refs, key);
        }

        var moveTo = Mover.MoveArmy(g, key.Data);
        proc.NewArmyPosesById.TryAdd(g.Id, moveTo);
    }
    
    
    public override void Draw(Army group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        var innerColor = group.Regime.Get(d).PrimaryColor;
        var outerColor = group.Regime.Get(d).PrimaryColor;
        var squareSize = 10f;
        var lineSize = 5f;
        var alliance = group.Regime.Get(d).GetAlliance(d);

        var natives = LineCells
            .Get<Cell, CellRef>(d)
            .ToArray();
        foreach (var n in natives)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.Offset(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Blue, .5f));
        }
        
        foreach (var c in AdvanceInto.Get<Cell, CellRef>(d))
        {
            mb.DrawPolygon(c.RelBoundary.Select(p => relTo.Offset(p + c.RelTo, d)).ToArray(),
                new Color(Colors.Red, .5f));
        }
    }

    public override void RegisterCombatActions(
        Army army, 
        CombatCalculator combat, LogicKey key)
    {
        var d = key.Data;
        
        if (army.Units.Count() == 0) return;
        var alliance = army.Regime.Get(d).GetAlliance(d);
        var cells = LineCells.Get<Cell, CellRef>(d);
        
        foreach (var cell in cells)
        {
            foreach (var neighbor in cell.GetNeighbors(d))
            {
                if (AdvanceInto.Contains(neighbor.Id) == false)
                {
                    continue;
                }

                if (neighbor.Controller.Get(d).GetAlliance(d)
                        .IsAtWar(alliance, d) == false)
                {
                    continue;
                }

                CellAttackNode.GetOrConstruct(army, 
                    cell, neighbor,
                    combat, d);
            }
        }
    }

    public override bool CleanUp(Army army, ProcedureKey key)
    {
        var alliance = army.Regime.Get(key.Data).GetAlliance(key.Data);
        var lost = army.LineMission.LineCells.Refs
            .Where(c => c.Get(key.Data).FriendlyControlled(alliance, key.Data) == false)
            .ToArray();
        army.LineMission.AdvanceInto.Add(lost, key);
        army.LineMission.LineCells.Remove(lost, key);
        var conquered = army.LineMission.AdvanceInto.Refs
            .Where(i => i.Get(key.Data).FriendlyControlled(alliance, key.Data))
            .ToArray();
        army.LineMission.LineCells.Add(conquered, key);
        army.LineMission.AdvanceInto.Remove(conquered, key);

        var advanceUnions = 
            UnionFind.Find<Cell, HashSet<Cell>>(army.LineMission.AdvanceInto
                .Get<Cell, CellRef>(key.Data),
                (c,d) => true,
                c => c.GetNeighbors(key.Data));
        foreach (var advanceUnion in advanceUnions)
        {
            if(advanceUnion.Any(c => c.GetNeighbors(key.Data)
                   .Any(n => LineCells.Contains(n.MakeRef()))
                        == false
               )
            )
            {
                army.LineMission.LineCells.Remove(advanceUnion.Select(u => u.MakeRef()), key);
            }
        }
        return true;
    }

    public override string GetDescription(Data d)
    {
        return $"Deploying";
    }
}