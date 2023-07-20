using System;
using System.Collections.Generic;
using System.Linq;

public class ResolveDipProposalProc : Procedure
{
    public bool Accepted { get; private set; }
    public int Id { get; private set; }
    public int Alliance0 { get; private set; }
    public int Alliance1 { get; private set; }

    public ResolveDipProposalProc(bool accepted, int id, int alliance0, int alliance1)
    {
        Accepted = accepted;
        Id = id;
        Alliance0 = alliance0;
        Alliance1 = alliance1;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var alliance0 = key.Data.Society.Alliances[Alliance0];
        var alliance1 = key.Data.Society.Alliances[Alliance1];
        var prop = alliance0.DiplomacyProposals.First(p => p.Id == Id);
        prop.Resolve(Accepted, key);
        var removed = alliance0.DiplomacyProposals.Remove(prop);
        if (removed == false) throw new Exception();
        removed = alliance1.DiplomacyProposals.Remove(prop);
        if (removed == false) throw new Exception();
    }
}
