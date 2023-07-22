using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public struct TriBool
{
    public static TriBool True => new TriBool(1);
    public static TriBool False => new TriBool(0);
    public static TriBool Undecided => new TriBool(-1);
    [SerializationConstructor] public TriBool(int state)
    {
        State = state;
    }

    public TriBool(bool val)
    {
        State = val ? 1 : 0;
    }
    public int State { get; private set; }
    public bool IsUndecided() => State == -1;
    public bool IsTrue() => State == 1;
    public bool IsFalse() => State == 0;
}


public static class TriBoolExt
{
    public static TriBool And(this TriBool t1, TriBool t2)
    {
        if (t1.IsUndecided() || t2.IsUndecided()) return TriBool.Undecided;
        if (t1.IsFalse() || t2.IsFalse()) return TriBool.False;
        return TriBool.True;
    }
}
