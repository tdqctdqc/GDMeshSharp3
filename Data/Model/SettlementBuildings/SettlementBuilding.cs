
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SettlementBuilding 
    : IModel, IMakeable, IIconed, ITechReqed
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public LaborComponent Labor { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public Icon Icon { get; private set; }
    public HashSet<Technology> Prereqs { get; private set; }
    public SettlementBuilding()
    {
    }

    public void MakeIcon()
    {
    }
    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }
}
