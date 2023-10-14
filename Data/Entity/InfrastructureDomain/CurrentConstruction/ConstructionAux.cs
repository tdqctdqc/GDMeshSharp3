using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructionAux
{
    public RefAction<Construction> StartedConstruction { get; private set; }
    public RefAction<Construction> EndedConstruction { get; private set; }

    public ConstructionAux(Data data)
    {
        StartedConstruction = new RefAction<Construction>();
        EndedConstruction = new RefAction<Construction>();
    }
}
