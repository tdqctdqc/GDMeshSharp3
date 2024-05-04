using System;
using System.Collections.Generic;
using System.Linq;

public class FoodProdTechniqueList : ModelList<FoodProdTechnique>
{
    public Farm Farm { get; private set; }
    public Ranch Ranch { get; private set; }
    public Fishery Fishery { get; private set; }
    public FoodProdTechniqueList(PeepJobList jobs, Items items)
    {
        Farm = new Farm(jobs, items);
        Ranch = new Ranch(jobs, items);
        Fishery = new Fishery(jobs, items);
    }
}
