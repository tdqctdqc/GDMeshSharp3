using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SampleProc1 : Procedure
{
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        GD.Print("enacting procedure 1");
    }
}