
using System.Collections.Generic;
using MessagePack;

public class CombatHistory : Entity
{
    public Dictionary<int, CombatGraph> Graphs { get; private set; }
    public static CombatHistory Construct(GenKey key)
    {
        var h = new CombatHistory(key.Data.IdDispenser.TakeId(),
            new Dictionary<int, CombatGraph>());
        key.Create(h);
        return h;
    }
    [SerializationConstructor] private CombatHistory(int id,
        Dictionary<int, CombatGraph> graphs) : base(id)
    {
        Graphs = graphs;
    }

    public override void CleanUp(ProcedureKey key)
    {
        
    }
}