using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingList : ModelManager<SettlementBuilding>
{
    public Mine IronMine { get; private set; } = new ();
    public Mine CoalMine { get; private set; } = new ();
    public Mine HeavyMetalMine { get; private set; } = new ();
    public Factory Factory { get; private set; } = new ();
    public TownHall TownHall { get; private set; } = new ();
    public Bank Bank { get; private set; } = new ();
    public Barracks Barracks { get; private set; } = new ();
    public BuildingList()
    {
    }
}
