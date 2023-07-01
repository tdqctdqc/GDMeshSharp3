using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Construction
{
    public ModelRef<BuildingModel> Model { get; private set; }
    public PolyTriPosition Pos { get; private set; }
    public float TicksLeft { get; private set; }

    public Construction(ModelRef<BuildingModel> model, PolyTriPosition pos, float ticksLeft)
    {
        Model = model;
        Pos = pos;
        TicksLeft = ticksLeft;
    }

    public bool ProgressConstruction(float laborRatio, int ticks, ProcedureWriteKey key)
    {
        TicksLeft -= laborRatio * ticks;
        return TicksLeft <= 0;
    }

    public int TicksDone()
    {
        var left = Mathf.CeilToInt(Model.Model().NumTicksToBuild - TicksLeft);
        return Mathf.Max(0, left);
    }

}
