using Godot;
using System;

public class ProcedureKey : Key, IWriteKey
{
    public ProcedureKey(ISession session) : base(session)
    {
    }

    public override bool HasRemotes()
    {
        return false;
    }
}
