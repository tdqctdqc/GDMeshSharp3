
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TheaterBranch : DeploymentBranch
{
    public ERef<Theater> Theater { get; private set; }

    public TheaterBranch(ERef<Alliance> alliance, int id, HashSet<DeploymentBranch> subBranches, HashSet<GroupAssignment> assignments, ERef<Theater> theater) : base(alliance, id, subBranches, assignments)
    {
        Theater = theater;
    }

    public void MakeFronts(AllianceMilitaryAi ai, LogicKey key)
    {
        foreach (var frontline in Theater.Get(key.Data).Frontlines.Entities(key.Data))
        {
            var holdLine = FrontlineAssignment.Construct(ai.Deployment,
                this, frontline, key);
            Assignments.Add(holdLine);
        }
    }
    


    public override Cell GetCharacteristicCell(Data d)
    {
        return Theater.Get(d).Cells.First().Get(d);
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(Theater.Get(d).Cells
            .Select(c => c.Get(d).GetCenter()));
    }
}