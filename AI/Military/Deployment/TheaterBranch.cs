
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TheaterBranch : DeploymentBranch
{
    public Theater Theater { get; private set; }

    public TheaterBranch (
        Regime regime,
        Theater theater,
        LogicWriteKey key) : base(regime, key)
    {
        Theater = theater;
    }

    public void MakeFronts(RegimeMilitaryAi ai, LogicWriteKey key)
    {
        foreach (var frontline in Theater.Frontlines)
        {
            var holdLine = new HoldLineAssignment(ai.Deployment,
                this, frontline, key);
            Assignments.Add(holdLine);
        }
    }
    


    public override Cell GetCharacteristicCell(Data d)
    {
        return Theater.Cells.First();
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(Theater.Cells.Select(c => c.GetCenter()));
    }
}