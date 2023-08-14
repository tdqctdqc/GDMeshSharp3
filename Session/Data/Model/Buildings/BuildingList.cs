using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingList : ModelList<BuildingModel>
{
    public Mine IronMine { get; private set; } 
    public Mine CoalMine { get; private set; } 
    public Mine HeavyMetalMine { get; private set; } 
    public Factory Factory { get; private set; }
    public TownHall TownHall { get; private set; }
    public BuildingList(Items items, FlowList flows, PeepJobList jobs)
    {
        IronMine = new Mine(nameof(IronMine), items.Iron, items, jobs);
        CoalMine = new Mine(nameof(CoalMine), items.Coal, items, jobs);
        HeavyMetalMine = new Mine(nameof(HeavyMetalMine), items.HeavyMetal, items, jobs);
        Factory = new Factory(items, flows, jobs);
        TownHall = new TownHall(items, jobs);
    }
}
