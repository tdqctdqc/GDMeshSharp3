using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ResolveAllianceProposalProcedure : Procedure
{
    public bool Accepted { get; private set; }
    public int Id { get; private set; }
    public int Alliance { get; private set; }

    public ResolveAllianceProposalProcedure(bool accepted, int id, int alliance)
    {
        Accepted = accepted;
        Id = id;
        Alliance = alliance;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        // GD.Print("Looking for Proposal id " + Id);
        var alliance = key.Data.Society.Alliances[Alliance];
        AllianceProposal prop;
        prop = alliance.AllianceProposals.First(p => p.Id == Id);
        prop.Resolve(Accepted, key);
        var removed = alliance.AllianceProposals.Remove(prop);
        if (removed == false) throw new Exception();
    }
}
