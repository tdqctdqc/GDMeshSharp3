
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Theater : CompoundDeploymentBranch
{
    public HashSet<int> HeldCellIds { get; private set; }
    public Theater(int id, ERef<Regime> regime, 
        HashSet<DeploymentBranch> assignments,
        HashSet<int> heldCellIds) 
        : base(assignments, regime, id)
    {
        HeldCellIds = heldCellIds;
    }

    public void MakeFronts(LogicWriteKey key)
    {
        var fronts = Assignments.OfType<Front>().ToArray();
        var newFronts = Blobber.Blob(fronts, this, key);
        foreach (var front in fronts)
        {
            front.DissolveInto(fronts, key);
            front.Disband(key);
        }
        
        foreach (var newFront in newFronts)
        {
            newFront.SetParent(this, key);
            newFront.MakeSegments(key);
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

    public override void DissolveInto(IEnumerable<DeploymentBranch> intos, LogicWriteKey key)
    {
        if (intos == null) throw new Exception();
        if (intos.Any(t => t is null)) throw new Exception();
        if (intos.Count() == 0)
        {
            throw new Exception();
        }
        var theaters = intos.OfType<Theater>();
        foreach (var assgn in Assignments)
        {
            var wp = assgn.GetCharacteristicCell(key.Data);
            var theater = theaters.First(t => t.HeldCellIds.Contains(wp.Id));
            assgn.SetParent(theater, key);
        }
    }
}