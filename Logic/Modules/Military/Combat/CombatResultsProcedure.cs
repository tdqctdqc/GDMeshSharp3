using System.Collections.Generic;

public class CombatResultsProcedure : Procedure
{
    public List<CombatResult> Results { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Results.Count; i++)
        {
            var result = Results[i];
            var unit = result.Unit.Entity(key.Data);
            foreach (var kvp in result.LossesByTroopId)
            {
                unit.Troops.Remove(kvp.Key, kvp.Value);
            }
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}