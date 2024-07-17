using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingList : ModelManager<SettlementBuilding>
{
    public Mine IronMine { get; private set; } = new ();
    public Mine CoalMine { get; private set; } = new ();
    public Mine HeavyMetalMine { get; private set; } = new ();
    public SettlementBuilding Factory { get; private set; } = new ();
    public SettlementBuilding TownHall { get; private set; } = new ();
    public SettlementBuilding Bank { get; private set; } = new ();
    public SettlementBuilding Barracks { get; private set; } = new ();
    public SettlementBuilding University { get; private set; } = new ();
    public BuildingList()
    {
    }
}
