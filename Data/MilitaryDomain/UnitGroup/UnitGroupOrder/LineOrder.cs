
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineOrder : UnitGroupOrder
{
    public HashSet<int> LineCells { get; private set; }
    public HashSet<int> AdvanceInto { get; private set; }
    public bool Advance { get; private set; }
    public LineOrder(HashSet<int> lineCells, 
        HashSet<int> advanceInto,
        bool advance)
    {
        LineCells = lineCells;
        AdvanceInto = advanceInto;
        Advance = advance;
    }

    public override void Handle(Army g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        if (g.Cells.SetEquals(LineCells) == false)
        {
            proc.NewArmyPosesById.TryAdd(g.Id, LineCells.ToHashSet());
        }
    }
    
    
    public override void Draw(Army group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        var innerColor = group.Regime.Get(d).PrimaryColor;
        var outerColor = group.Regime.Get(d).PrimaryColor;
        var squareSize = 10f;
        var lineSize = 5f;
        var alliance = group.Regime.Get(d).GetAlliance(d);

        var natives = LineCells.Select(f => PlanetDomainExt.GetPolyCell(f, d));
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
        Army g, 
        CombatCalculator combat, LogicWriteKey key)
    {
        // if (Advance == false)
        // {
        //     return;
        // }
        var d = key.Data;
        if (g.Units.Count() == 0) return;
        var units = g.Units.Items(d).ToHashSet();
        var alliance = units.First().Regime.Get(d).GetAlliance(d);
        
        
    }
    public override string GetDescription(Data d)
    {
        return $"Deploying on line from {LineCells.First()}" +
               $" to {LineCells.Last()}";
    }
}