using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IMeta<TMeta>
{
    TMeta Deserialize(byte[][] argsBytes);
    void Initialize(TMeta m, object[] args);
    object[] GetArgs(TMeta t);
}