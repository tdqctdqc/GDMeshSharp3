
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
        var theaters = Branches.OfType<Theater>().ToArray();
        foreach (var theater in theaters)
        {
            theater.Disband(ai, key);
            // theater.DissolveInto(ai, theaters, key);
        } 
        var newTheaters = theaters.Blob(
            ai, Regime.Entity(key.Data), key);
        
        foreach (var theater in newTheaters)
        {
            theater.SetParent(ai, this, key);
            ai.AddNode(theater);
            theater.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicWriteKey key)
    {
        var ai = key.Data.HostLogicData.RegimeAis[Regime.Entity(key.Data)]
            .Military.Deployment;
        
        
        var groups = key.Data.Military.UnitAux.UnitGroupByRegime[Regime.Entity(key.Data)];
        var taken = GetAssignments()
            .SelectMany(a => a.Groups.Groups)
            .ToHashSet();
        
       
        foreach (var g in groups)
        {
            if (taken.Contains(g.MakeRef()) == false)
            {
                Reserve.Groups.AddUnassigned(ai, g, key);
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
    public override void DissolveInto(DeploymentAi ai, IEnumerable<DeploymentBranch> into, LogicWriteKey key)
    {
        throw new Exception();
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.Entity(d).GetPolys(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}