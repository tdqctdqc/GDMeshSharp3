using Godot;
using System;

public abstract class Procedure : Message
{
    protected Procedure()
    {
        
    }
    
    public abstract bool Valid(Data data);
}

