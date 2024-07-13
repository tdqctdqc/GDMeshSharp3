using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingList : ModelManager<SettlementBuilding>
{
    public Mine IronMine { get; private set; } 
    public Mine CoalMine { get; private set; } 
    public Mine HeavyMetalMine { get; private set; } 
    public Factory Factory { get; private set; }
    public TownHall TownHall { get; private set; }
    public Bank Bank { get; private set; }
    public Barracks Barracks { get; private set; }
    public BuildingList(Items items, FlowList flows, 
        PeepJobList jobs)
    {
        IronMine = new Mine();
        CoalMine = new Mine();
        HeavyMetalMine = new Mine();
        Factory = new Factory();
        TownHall = new TownHall();
        Bank = new Bank();
        Barracks = new Barracks();
    }
}
