
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineMission : ArmyMission
{
    public HashSet<int> LineCells { get; private set; }
    public HashSet<int> AdvanceInto { get; private set; }
    public bool Advance { get; private set; }
    public LineMission(HashSet<int> lineCells, 
        HashSet<int> advanceInto,
        bool advance)
    {
        LineCells = lineCells;
        AdvanceInto = advanceInto;
        Advance = advance;
    }

    public override void Handle(Army g, LogicWriteKey key,
        HandleUnitMissionsProcedure proc)
    {
        proc.NewArmyPosesById.TryAdd(g.Id, LineCells.ToHashSet());
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
            .Select(f => PlanetDomainExt.GetPolyCell(f, d))
            .ToArray();
        foreach (var n in natives)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.Offset(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Blue, .5f));
        }
        
        foreach (var landCell in AdvanceInto)
        {
            var c = PlanetDomainExt.GetPolyCell(landCell, d);
            mb.DrawPolygon(c.RelBoundary.Select(p => relTo.Offset(p + c.RelTo, d)).ToArray(),
                new Color(Colors.Red, .5f));
        }
    }

    public override void RegisterCombatActions(
        Army army, 
        CombatCalculator combat, LogicWriteKey key)
    {
        var d = key.Data;
        
        if (army.Units.Count() == 0) return;
        var alliance = army.Regime.Get(d).GetAlliance(d);

        var cells = LineCells.Select(i => PlanetDomainExt.GetPolyCell(i, key.Data));
        var adjacentAdvanceCells = cells
            .SelectMany(c => c.Neighbors)
            .Distinct()
            .Where(n => AdvanceInto.Contains(n)
                && PlanetDomainExt.GetPolyCell(n, d)
                    .Controller.Get(d).GetAlliance(d)
                    .IsAtWar(alliance, d))
            .Select(n => PlanetDomainExt.GetPolyCell(n, d));
        
        foreach (var advanceCell in adjacentAdvanceCells)
        {
            ArmyAttackEdge.ConstructAndAddToGraph(army, advanceCell, combat, key.Data);
        }
    }

    public override bool CleanUp(Army army, ProcedureWriteKey key)
    {
        var alliance = army.Regime.Get(key.Data).GetAlliance(key.Data);
        var lost = army.LineMission.LineCells
            .Where(c => PlanetDomainExt.GetPolyCell(c, key.Data)
                .FriendlyControlled(alliance, key.Data) == false)
            .ToArray();
        army.LineMission.AdvanceInto.UnionWith(lost);
        army.LineMission.LineCells.ExceptWith(lost);
        army.Cells.ExceptWith(lost);
        var conquered = army.LineMission.AdvanceInto
            .Where(i => PlanetDomainExt.GetPolyCell(i, key.Data)
                .FriendlyControlled(alliance, key.Data))
            .ToArray();
        army.LineMission.LineCells.UnionWith(conquered);
        army.LineMission.AdvanceInto.ExceptWith(conquered);
        army.Cells.UnionWith(army.LineMission.LineCells);
        return true;
    }

    public override string GetDescription(Data d)
    {
        return $"Deploying";
    }
}