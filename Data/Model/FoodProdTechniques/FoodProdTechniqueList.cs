using System;
using System.Collections.Generic;
using System.Linq;

public class FoodProdTechniqueList : ModelList<FoodProdTechnique>
{
    public Farm Farm { get; private set; }
    public Ranch Ranch { get; private set; }
    public Fishery Fishery { get; private set; }
    public FoodProdTechniqueList(PeepJobList jobs)
    {
        Farm = new Farm(jobs);
        Ranch = new Ranch(jobs);
        Fishery = new Fishery(jobs);
    }
}
