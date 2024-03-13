using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyPeepAux
{
    public Indexer<Cell, Peep> ByCell { get; private set; } 
    public PolyPeepAux(Data data)
    {
        ByCell = Indexer.MakeForEntity<Cell, Peep>(p => p.Cell.Get(data), data);
    }
}