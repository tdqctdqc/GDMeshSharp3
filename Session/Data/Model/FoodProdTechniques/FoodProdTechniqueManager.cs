using System;
using System.Collections.Generic;
using System.Linq;

public class FoodProdTechniqueManager : IModelManager<FoodProdTechnique>
{
    public Dictionary<string, FoodProdTechnique> Models { get; private set;  }

    public static Farm Farm { get; private set; } = new Farm();
    public static Ranch Ranch { get; private set; } = new Ranch();
    public FoodProdTechniqueManager()
    {
        var techniques = GetType().GetStaticPropertiesOfType<FoodProdTechnique>();
        Models = techniques.ToDictionary(b => b.Name, b => b);
    }
}
