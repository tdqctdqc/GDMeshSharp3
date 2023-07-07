
using MessagePack;

public class DeclareWarProcedure : Procedure
{
    public EntityRef<Regime> Declarer { get; private set; }
    public EntityRef<Regime> Declaree { get; private set; }
    public static DeclareWarProcedure Create(Regime declarer, Regime declaree)
    {
        return new DeclareWarProcedure(declarer.MakeRef(), declaree.MakeRef());
    }
    [SerializationConstructor] private DeclareWarProcedure(EntityRef<Regime> declarer, EntityRef<Regime> declaree)
    {
        Declarer = declarer;
        Declaree = declaree;
    }
    public override bool Valid(Data data)
    {
        return Declarer.CheckExists(data) && Declaree.CheckExists(data);
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var relation = Declarer.Entity(key.Data).RelationWith(Declaree.Entity(key.Data), key.Data);
        relation.Set<bool>(nameof(RegimeRelation.AtWar), true, key);
    }
}
