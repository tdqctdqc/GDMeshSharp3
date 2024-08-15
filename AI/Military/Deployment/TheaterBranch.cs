
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TheaterBranch : DeploymentBranch
{
    public ERef<Theater> Theater { get; private set; }

    public TheaterBranch(ERef<Alliance> alliance, int id, 
        HashSet<DeploymentBranch> subBranches, 
        HashSet<ArmyAssignment> assignments, 
        ERef<Theater> theater) : base(alliance, id, subBranches, assignments)
    {
        Theater = theater;
    }

    public void MakeFrontAssignments(AllianceMilitaryAi ai, LogicKey key)
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

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        Theater.Get(d).Draw(mb, relTo, d);
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(Theater.Get(d).Cells
            .Select(c => c.Get(d).GetCenter()));
    }
}