using System;
using System.Collections.Generic;
using System.Linq;

public class SetEnemyProcedure : Procedure
{
    public int DeclarerAllianceId { get; private set; }
    public int TargetAllianceId { get; private set; }

    public SetEnemyProcedure(int declarerAllianceId, int targetAllianceId)
    {
        DeclarerAllianceId = declarerAllianceId;
        TargetAllianceId = targetAllianceId;
    }

    public override bool Valid(Data data)
    {
        var declarer = data.Society.Alliances[DeclarerAllianceId];
        var target = data.Society.Alliances[TargetAllianceId];
        return declarer != target && declarer.Enemies.Contains(target) == false;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var declarer = key.Data.Society.Alliances[DeclarerAllianceId];
        var target = key.Data.Society.Alliances[TargetAllianceId];
        declarer.SetEnemy(target, key);
        target.SetEnemy(declarer, key);
    }
}
