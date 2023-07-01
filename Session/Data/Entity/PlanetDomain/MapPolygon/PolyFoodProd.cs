using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyFoodProd
{
    public int BaseProd(Data data) 
        => Nums.Sum(kvp => ((FoodProdTechnique)data.Models[kvp.Key]).BaseProd * kvp.Value);
    public int BaseLabor(Data data) 
        => Nums.Sum(kvp => ((FoodProdTechnique)data.Models[kvp.Key]).BaseLabor * kvp.Value);
    public Dictionary<int, int> Nums { get; private set; }

    public static PolyFoodProd Construct()
    {
        return new PolyFoodProd(new Dictionary<int, int>());
    }

    [SerializationConstructor] private PolyFoodProd(Dictionary<int, int> nums)
    {
        Nums = nums;
    }

    public void Add(FoodProdTechnique tech, int num)
    {
        Nums.AddOrSum(tech.Id, num);
    }
    

    public int Income(Data data)
    {
        return Nums.Sum(kvp => ((FoodProdTechnique) data.Models[kvp.Key]).Income * kvp.Value);
    }
}
