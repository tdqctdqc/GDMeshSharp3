using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class 
    ClearFinishedConstructionsProcedure : Procedure
{
    public List<int> PolyCellIds { get; private set; }

    public static ClearFinishedConstructionsProcedure Construct()
    {
        return new ClearFinishedConstructionsProcedure(new List<int>());
    }

    [SerializationConstructor] private ClearFinishedConstructionsProcedure(List<int> polyCellIds)
    {
        PolyCellIds = polyCellIds;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var id in PolyCellIds)
        {
            var cell = PlanetDomainExt.GetPolyCell(id, key.Data);
            var poly = ((LandCell)cell).Polygon.Entity(key.Data);
            var r = poly.OwnerRegime.Entity(key.Data);
            key.Data.Infrastructure.CurrentConstruction.FinishConstruction(poly, cell, key);
        }
    }
}
