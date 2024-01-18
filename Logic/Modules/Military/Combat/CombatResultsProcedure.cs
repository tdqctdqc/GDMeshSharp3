using System.Collections.Generic;
using Godot;
using MessagePack;

public class CombatResultsProcedure : Procedure
{
    public List<CombatResult> Results { get; private set; }

    public static CombatResultsProcedure Construct()
    {
        return new CombatResultsProcedure(new List<CombatResult>());
    }
    [SerializationConstructor] private CombatResultsProcedure(List<CombatResult> results)
    {
        Results = results;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Results.Count; i++)
        {
            var result = Results[i];
            var unit = result.Unit.Entity(key.Data);
            foreach (var kvp in result.LossesByTroopId)
            {
                
                var max = unit.Troops.Get(kvp.Key);
                unit.Troops.Remove(kvp.Key, Mathf.Min(max, kvp.Value));
            }

            // var newPos = unit.Position.Pos + result.ResultOffset;
            // newPos = newPos.ClampPosition(key.Data);
            // unit.SetPosition(MapPos.Construct(newPos, key.Data), key);
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}