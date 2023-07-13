using System;
using System.Collections.Generic;
using System.Linq;

public class SetEnemyProcedure : Procedure
{
    public int DeclarerRegimeId { get; private set; }
    public int TargetRegimeId { get; private set; }

    public SetEnemyProcedure(int declarerRegimeId, int targetRegimeId)
    {
        DeclarerRegimeId = declarerRegimeId;
        TargetRegimeId = targetRegimeId;
    }

    public override bool Valid(Data data)
    {
        var declarer = data.Society.Regimes[DeclarerRegimeId];
        var target = data.Society.Regimes[TargetRegimeId];
        var rel = declarer.RelationWith(target, data);
        return rel.Alliance == false 
               && rel.Enemies == false;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var declarer = key.Data.Society.Regimes[DeclarerRegimeId];
        var target = key.Data.Society.Regimes[TargetRegimeId];
        var rel = declarer.RelationWith(target, key.Data);
        rel.SetEnemies(true, key);
    }
}
