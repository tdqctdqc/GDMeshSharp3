using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class FoodProd
{
    public IdCount<FoodProdTechnique> Nums { get; private set; }

    public static FoodProd Construct()
    {
        return new FoodProd(IdCount<FoodProdTechnique>.Construct());
    }

    [SerializationConstructor] private FoodProd(
        IdCount<FoodProdTechnique> nums)
    {
        Nums = nums;
    }

    public void Add(FoodProdTechnique tech, float num)
    {
        Nums.Add(tech, num);
    }
    
}
