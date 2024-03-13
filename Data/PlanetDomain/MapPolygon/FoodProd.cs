using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class FoodProd
{
    public float BaseProd(Data data) 
        => Nums.Sum(kvp => kvp.Key.Get(data).BaseProd * kvp.Value);
    public float BaseLabor(Data data) 
        => Nums.Sum(kvp => kvp.Key.Get(data).BaseLabor * kvp.Value);
    public Dictionary<ModelRef<FoodProdTechnique>, float> Nums { get; private set; }

    public static FoodProd Construct()
    {
        return new FoodProd(new Dictionary<ModelRef<FoodProdTechnique>, float>());
    }

    [SerializationConstructor] private FoodProd(
        Dictionary<ModelRef<FoodProdTechnique>, float> nums)
    {
        Nums = nums;
    }

    public void Add(FoodProdTechnique tech, int num)
    {
        Nums.AddOrSum(tech.MakeRef(), num);
    }
    
}
