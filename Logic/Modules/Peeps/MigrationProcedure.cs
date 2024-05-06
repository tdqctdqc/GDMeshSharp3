
using System.Collections.Generic;
using Godot;
using MessagePack;

public class MigrationProcedure : Procedure
{
    public List<(int from, int to, float amt)> Results { get; private set; }

    public static MigrationProcedure Construct()
    {
        return new MigrationProcedure(new List<(int from, int to, float amt)>());
    }
    [SerializationConstructor] private MigrationProcedure(List<(int from, int to, float amt)> results)
    {
        Results = results;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Results.Count; i++)
        {
            var result = Results[i];
            var from = PlanetDomainExt.GetPolyCell(result.from, key.Data);
            var to = PlanetDomainExt.GetPolyCell(result.to, key.Data);
            if (from.Controller.RefId != to.Controller.RefId) continue;
            var fromPeep = from.GetPeep(key.Data);
            var toPeep = to.GetPeep(key.Data);
            var amt = Mathf.Min(result.amt, fromPeep.Size);
            fromPeep.ShrinkSize(amt, key);
            toPeep.GrowSize(amt, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}