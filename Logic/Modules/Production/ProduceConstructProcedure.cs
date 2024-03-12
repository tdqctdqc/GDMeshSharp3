
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MessagePack;

public class ProduceConstructProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}
