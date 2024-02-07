
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
            r.MakeRef(), 
            new HashSet<DeploymentBranch>(),
            cells.Select(c => c.Id).ToHashSet(),
            reserve
        );
        return t;
    }
    [SerializationConstructor] private Theater(
        int id, 
        int parentId,
        ERef<Regime> regime, 
        HashSet<DeploymentBranch> branches,
        HashSet<int> heldCellIds, ReserveAssignment reserve) 
        : base(branches, regime, id, parentId, reserve)
    {
        HeldCellIds = heldCellIds;
    }

    public void MakeFronts(DeploymentAi ai, LogicWriteKey key)
    {
        var fronts = Branches.OfType<Front>().ToArray();
        var newFronts = fronts.Blob(ai, this, key);
        foreach (var front in fronts)
        {
            front.DissolveInto(ai, this, newFronts.AsEnumerable<DeploymentBranch>(), key);
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
        return GetCells(d).First();
    }

    public override void DissolveInto(DeploymentAi ai, 
        DeploymentTrunk parent, IEnumerable<DeploymentBranch> intos, LogicWriteKey key)
    {
        if (intos == null) throw new Exception();
        if (intos.Any(t => t is null)) throw new Exception();
        if (intos.Count() == 0)
        {
            throw new Exception();
        }
        var theaters = intos.OfType<Theater>();
        var allTheaterCells = theaters.SelectMany(t => t.GetCells(key.Data)).ToHashSet();
        
        foreach (var assgn in Branches.ToArray())
        {
            var wp = assgn.GetCharacteristicCell(key.Data);
            if (wp == null)
            {
                throw new Exception("no characteristic cell for " + assgn.GetType().Name);
            }
            var theater = theaters.FirstOrDefault(t => t.HeldCellIds.Contains(wp.Id));
            if (theater == null)
            {
                FloodFill<PolyCell>.FloodTilFirst(
                    wp,
                    c => c.Landform.Model(key.Data).IsLand,
                    c => c.GetNeighbors(key.Data),
                    c =>
                    {
                        if (allTheaterCells.Contains(c) == false) return false;
                        var t = theaters.First(t => t.HeldCellIds.Contains(c.Id));
                        theater = t;
                        return true;
                    });
            }

            if (theater != null)
            {
                assgn.SetParent(ai, theater, key);
            }
            else
            {
                // throw new Exception("theater destroyed");
                assgn.Disband(ai, key);
            }
        }
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(GetCells(d).Select(c => c.GetCenter()));
    }
}