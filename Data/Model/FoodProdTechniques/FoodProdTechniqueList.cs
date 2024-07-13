using System;
using System.Collections.Generic;
using System.Linq;

public class FoodProdTechniqueList : ModelManager<FoodProdTechnique>
{
    public Farm Farm { get; private set; }
        = new();
    public Ranch Ranch { get; private set; }
        = new();
    public Fishery Fishery { get; private set; }
        = new();
    public FoodProdTechniqueList(PeepJobList jobs, Items items)
    {
    }
}
