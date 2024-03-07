using Godot;
using System;

public abstract class Update : Message
{
    protected Update()
    {
        
    }

    public abstract void Enact(ProcedureWriteKey key);
}
