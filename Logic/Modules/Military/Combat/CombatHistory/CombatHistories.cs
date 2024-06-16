
using System.Collections.Generic;
using MessagePack;

public class CombatHistories : Entity
{
    public Dictionary<int, CombatHistory> Histories { get; private set; }

    public static CombatHistories Construct(GenWriteKey key)
    {
        var h = new CombatHistories(key.Data.IdDispenser.TakeId(),
            new Dictionary<int, CombatHistory>());
        key.Create(h);
        return h;
    }
    [SerializationConstructor] private CombatHistories(int id,
        Dictionary<int, CombatHistory> histories) : base(id)
    {
        Histories = histories;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}