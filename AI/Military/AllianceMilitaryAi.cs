
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    public Dictionary<Regime, HashSet<PolyCell>> AreasOfResponsibility { get; private set; }
    public AllianceMilitaryAi()
    {
        AreasOfResponsibility = new Dictionary<Regime, HashSet<PolyCell>>();
    }
    public void Calculate(LogicWriteKey key, Alliance alliance)
    {
        if (key.Data.Get<Alliance>(alliance.Id) == null)
        {
            throw new Exception();
        }
    }
}