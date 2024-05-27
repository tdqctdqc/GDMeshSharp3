using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyPeepAux
{
    public OneToOneIndexer<Cell, Peep> ByCell { get; private set; } 
    public PolyPeepAux(Data data)
    {
        ByCell = OneToOneIndexer.MakeForEntity<Cell, Peep>(p => p.Cell.Get(data), data);
    }
}