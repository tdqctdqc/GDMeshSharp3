using System;
using System.Collections.Generic;
using System.Linq;

public class SetRegimeProcedure : Procedure
{
    public EntityRef<Player> Player { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public override bool Valid(Data data)
    {
        return Regime.Entity().IsPlayerRegime(data) == false;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        throw new NotImplementedException();
    }
}
