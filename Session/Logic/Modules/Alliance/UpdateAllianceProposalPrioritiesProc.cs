using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class UpdateAllianceProposalPrioritiesProc : Procedure
{
    public List<int> AllianceIds { get; private set; }
    public List<int> ProposalIds { get; private set; }
    public List<float> NewPriorities { get; private set; }

    public static UpdateAllianceProposalPrioritiesProc Construct()
    {
        return new UpdateAllianceProposalPrioritiesProc(new List<int>(), new List<int>(), new List<float>());
    }
    [SerializationConstructor] private UpdateAllianceProposalPrioritiesProc(List<int> allianceIds, List<int> proposalIds, List<float> newPriorities)
    {
        AllianceIds = allianceIds;
        ProposalIds = proposalIds;
        NewPriorities = newPriorities;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < AllianceIds.Count; i++)
        {
            var alliance = key.Data.Society.Alliances[AllianceIds[i]];
            
            var proposal = alliance.AllianceProposals.FirstOrDefault(p => p.Id == ProposalIds[i]);
            if (proposal == null) continue;
            var newPriority= NewPriorities[i];
            proposal.UpdatePriority(newPriority, key);
        }
    }
}
