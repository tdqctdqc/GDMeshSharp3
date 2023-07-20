using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class Proposal
{
    public int Id { get; private set; }
    public EntityRef<Regime> Proposer { get; private set; }
    public int Alliance { get; private set; }
    public HashSet<int> InFavor { get; private set; }
    public HashSet<int> Against { get; private set; }
    public HashSet<int> Abstain { get; private set; }

    [SerializationConstructor] protected Proposal(int id, EntityRef<Regime> proposer, int alliance, HashSet<int> inFavor, HashSet<int> against, HashSet<int> abstain)
    {
        Id = id;
        Proposer = proposer;
        Alliance = alliance;
        InFavor = inFavor;
        Against = against;
        Abstain = abstain;
    }
    public void SetId(int id)
    {
        Id = id;
    }
    public abstract bool GetDecisionForAi(Regime r, Data d);
    public abstract void Propose(ProcedureWriteKey key);
    public abstract void Resolve(bool accepted, ProcedureWriteKey key);
}
