using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class 
    ClearFinishedConstructionsProcedure : Procedure
{
    public List<PolyTriPosition> Positions { get; private set; }

    public static ClearFinishedConstructionsProcedure Construct()
    {
        return new ClearFinishedConstructionsProcedure(new List<PolyTriPosition>());
    }

    [SerializationConstructor] private ClearFinishedConstructionsProcedure(List<PolyTriPosition> positions)
    {
        Positions = positions;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var pos in Positions)
        {
            var poly = pos.Poly(key.Data);
            var r = poly.Regime.Entity(key.Data);
            key.Data.Infrastructure.CurrentConstruction.FinishConstruction(poly, pos, key);
        }
    }
}
