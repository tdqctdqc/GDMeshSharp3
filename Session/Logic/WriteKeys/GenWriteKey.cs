using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenWriteKey : CreateWriteKey
{
    public GenData GenData => (GenData) Data;
    public GenWriteKey(GenData data, ISession session) : base(data, session)
    {
    }
}