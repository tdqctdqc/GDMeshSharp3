
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
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var oldFrontSegs = GetChildrenOfType<FrontSegment>()
            .ToArray();
        foreach (var oldFrontSeg in oldFrontSegs)
        {
            oldFrontSeg.Correct(ai, this, key);
        }
        
        var coveredFaces = GetChildrenOfType<FrontSegment>()
            .SelectMany(f => f.Frontline.Faces)
            .ToHashSet();

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
            if (coveredFaces.Contains(front[0])) continue;
            var frontSegment = FrontSegment.Construct(ai, Regime,
                front, false, key);
            ai.AddNode(frontSegment);
            frontSegment.SetParent(ai, this, key);
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