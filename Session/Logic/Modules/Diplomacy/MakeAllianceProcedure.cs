public class MakeAllianceProcedure : Procedure
{
    public EntityRef<Regime> Offerer { get; private set; }
    public EntityRef<Regime> Accepter { get; private set; }

    public static MakeAllianceProcedure Create(Regime declarer, Regime declaree)
    {
        return new MakeAllianceProcedure(declarer.MakeRef(), declaree.MakeRef());
    }
    private MakeAllianceProcedure(EntityRef<Regime> offerer, EntityRef<Regime> accepter)
    {
        Offerer = offerer;
        Accepter = accepter;
    }
    public override bool Valid(Data data)
    {
        return Offerer.CheckExists(data) && Accepter.CheckExists(data);
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var relation = Offerer.Entity().RelationWith(Accepter.Entity(), key.Data);
        relation.Set<bool>(nameof(RegimeRelation.Alliance), true, key);
    }
}