
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Theater : DeploymentTrunk
{
    public HashSet<int> HeldCellIds { get; private set; }

    public static Theater Construct(
        DeploymentAi ai,
        Regime r, 
        IEnumerable<PolyCell> cells,
        LogicWriteKey key)
    {
        var id = ai.DeploymentTreeIds.TakeId(key.Data);
        var reserve = ReserveAssignment.Construct(ai, id, r.MakeRef(), key.Data);
        var t = new Theater(
            id,
            -1,
            r.MakeRef(), new HashSet<DeploymentBranch>(),
            cells.Select(c => c.Id).ToHashSet(),
            reserve
        );
        return t;
    }
    [SerializationConstructor] private Theater(
        int id, 
        int parentId,
        ERef<Regime> regime, 
        HashSet<DeploymentBranch> assignments,
        HashSet<int> heldCellIds, ReserveAssignment reserve) 
        : base(assignments, regime, id, parentId, reserve)
    {
        HeldCellIds = heldCellIds;
    }

    public void MakeFronts(DeploymentAi ai, LogicWriteKey key)
    {
        var fronts = Branches.OfType<Front>().ToArray();
        var newFronts = fronts.Blob(ai, this, key);
        foreach (var front in fronts)
        {
            front.DissolveInto(ai, newFronts.AsEnumerable<DeploymentBranch>(), key);
            front.Disband(ai, key);
        }
        foreach (var newFront in newFronts)
        {
            newFront.MakeSegments(ai, key);
        }
    }
    

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
    
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d)
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }

    public override void DissolveInto(DeploymentAi ai, IEnumerable<DeploymentBranch> intos, LogicWriteKey key)
    {
        if (intos == null) throw new Exception();
        if (intos.Any(t => t is null)) throw new Exception();
        if (intos.Count() == 0)
        {
            throw new Exception();
        }
        var theaters = intos.OfType<Theater>();
        foreach (var assgn in Branches)
        {
            var wp = assgn.GetCharacteristicCell(key.Data);
            var theater = theaters.First(t => t.HeldCellIds.Contains(wp.Id));
            assgn.SetParent(ai, theater, key);
        }
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(GetCells(d).Select(c => c.GetCenter()));
    }
}