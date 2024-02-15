
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Theater : DeploymentBranch
{
    public HashSet<int> HeldCellIds { get; private set; }

    public Theater (
        DeploymentAi ai,
        IEnumerable<PolyCell> cells,
        LogicWriteKey key) : base(ai, key)
    {
        HeldCellIds = cells.Select(c => c.Id).ToHashSet();
    }

    public void MakeFronts(DeploymentAi ai, LogicWriteKey key)
    {
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var frontlines = FrontFinder
            .FindFront(GetCells(key.Data).ToHashSet(),
                p =>
                {
                    if (p.Controller.IsEmpty()) return false;
                    var pAlliance = p.Controller.Entity(key.Data).GetAlliance(key.Data);
                    return alliance.IsRivals(pAlliance, key.Data);
                }, key.Data);
        foreach (var front in frontlines)
        {
            var frontline = new Frontline(front);
            var frontSegment = new HoldLineAssignment(ai, this, frontline, key);
            Assignments.Add(frontSegment);  
        }
    }
    

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(GetCells(d).Select(c => c.GetCenter()));
    }
}