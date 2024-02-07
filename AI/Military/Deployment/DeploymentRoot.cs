
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentTrunk
{
    public static DeploymentRoot Construct(Regime r, DeploymentAi ai,
        Data d)
    {
        var id = ai.DeploymentTreeIds.TakeId(d);
        var reserve = ReserveAssignment.Construct(ai, id, 
            r.MakeRef(), d);
        var v = new DeploymentRoot(
            new HashSet<DeploymentBranch>(),
            r.MakeRef(), 
            reserve,
            id);
        return v;
    }
    [SerializationConstructor] private DeploymentRoot(
        HashSet<DeploymentBranch> branches,
        ERef<Regime> regime, 
        ReserveAssignment reserve, 
        int id) 
        : base(branches, regime, id, -1, reserve)
    {
    }
    
    public void MakeTheaters(DeploymentAi ai, LogicWriteKey key)
    {
        var oldTheaters = Branches.OfType<Theater>().ToArray();
        var oldTheaterCells = oldTheaters.SelectMany(t => t.GetCells(key.Data)).ToHashSet();
        var unclaimedCells = key.Data.Planet.PolygonAux
            .PolyCells.Cells.Values
            .OfType<LandCell>()
            .Where(c => c.Controller.RefId == Regime.RefId)
            .Except(oldTheaterCells).ToArray();
        var unions = UnionFind.Find(unclaimedCells,
            (p, q) => true, p => p.GetNeighbors(key.Data));

        var newTheaters = unions.Select(u =>
            Theater.Construct(ai, Regime.Entity(key.Data),
                u.ToHashSet(), key));
            
        foreach (var theater in newTheaters)
        {
            ai.AddNode(theater);
            theater.SetParent(ai, ai.Root, key);
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
                Reserve.AddGroup(ai, g, key.Data);
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
    
    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        return false;
    }

    public override bool PushGroup(DeploymentAi ai, GroupAssignment transferFrom, LogicWriteKey key)
    {
        return false;
    }
    public override void DissolveInto(DeploymentAi ai, DeploymentTrunk parent, IEnumerable<DeploymentBranch> into, LogicWriteKey key)
    {
        throw new Exception();
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.Entity(d).GetPolys(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}