using MessagePack;

public class DeclareWarProcedure : Procedure
{
    public EntityRef<Alliance> Declarer { get; private set; }
    public EntityRef<Alliance> Target { get; private set; }
    public static DeclareWarProcedure Create(Alliance declarer, Alliance declaree)
    {
        return new DeclareWarProcedure(declarer.MakeRef(), declaree.MakeRef());
    }
    [SerializationConstructor] private DeclareWarProcedure(EntityRef<Alliance> declarer, EntityRef<Alliance> target)
    {
        Declarer = declarer;
        Target = target;
    }
    public override bool Valid(Data data)
    {
        return Declarer.CheckExists(data) 
            && Target.CheckExists(data)
            && Declarer.Entity(data).Enemies.Contains(Target.Entity(data))
            && Declarer.Entity(data).AtWar.Contains(Target.Entity(data)) == false;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Declarer.Entity(key.Data).SetWar(Target.Entity(key.Data), key);
        Target.Entity(key.Data).SetWar(Declarer.Entity(key.Data), key);
    }
}
