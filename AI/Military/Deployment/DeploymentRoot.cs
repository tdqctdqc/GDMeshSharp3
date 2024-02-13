
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    public DeploymentRoot(DeploymentAi ai,
        LogicWriteKey key) : base(ai, key)
    {
    }
    
    public void MakeTheaters(DeploymentAi ai, LogicWriteKey key)
    {
        var cells = key.Data.Planet.PolygonAux
            .PolyCells.Cells.Values
            .Where(c => c.Controller.RefId == Regime.RefId)
            .ToArray();
        var unions = UnionFind.Find(cells,
            (p, q) => true, p => p.GetNeighbors(key.Data));
        var newTheaters = unions.Select(u =>
            new Theater(ai, u.ToHashSet(), key));
        foreach (var theater in newTheaters)
        {
            SubBranches.Add(theater);
            theater.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicWriteKey key)
    {
        var ai = key.Data.HostLogicData.RegimeAis[Regime.Entity(key.Data)]
            .Military.Deployment;
        
        var groups = key.Data.Military.UnitAux.UnitGroupByRegime[Regime.Entity(key.Data)];
        var taken = GetAssignments()
            .SelectMany(a => a.Groups)
            .ToHashSet();
       
        foreach (var g in groups)
        {
            if (taken.Contains(g.MakeRef()) == false)
            {
                Shuffle.Groups.Add(g);
            }
        }
    }
    
    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        throw new Exception();
    }
    

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.Entity(d).GetPolys(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}