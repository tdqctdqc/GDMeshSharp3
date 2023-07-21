using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class UpdateAllianceProposalPrioritiesProc : Procedure
{
    public List<int> ProposalIds { get; private set; }
    public List<float> NewPriorities { get; private set; }

    public static UpdateAllianceProposalPrioritiesProc Construct()
    {
        return new UpdateAllianceProposalPrioritiesProc(new List<int>(), new List<float>());
    }
    [SerializationConstructor] private UpdateAllianceProposalPrioritiesProc(List<int> proposalIds, List<float> newPriorities)
    {
        ProposalIds = proposalIds;
        NewPriorities = newPriorities;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (int i = 0; i < ProposalIds.Count; i++)
        {
            var proposalId = ProposalIds[i];
            if (key.Data.Handles.Proposals.ContainsKey(proposalId) == false) continue;
            key.Data.Handles.Proposals[proposalId].UpdatePriority(NewPriorities[i], key);
        }
    }
}
