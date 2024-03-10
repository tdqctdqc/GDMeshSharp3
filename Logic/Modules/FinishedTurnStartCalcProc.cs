using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinishedTurnStartCalcProc : Procedure
{
    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Notices.FinishedTurnStartCalc.Invoke();
    }
}
