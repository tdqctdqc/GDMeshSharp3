using System;
using System.Collections.Generic;
using System.Linq;

public class FoodProdTechniqueList : ModelList<FoodProdTechnique>
{
    public Farm Farm { get; private set; } = new ();
    public Ranch Ranch { get; private set; } = new ();
}
