using System;
using System.Collections.Generic;
using System.Linq;

public class FoodAndPopGrowthProcedure : Procedure
{
    public Dictionary<int, int> Growths { get; private set; }
    public Dictionary<int, int> FoodConsumptions { get; private set; }
    public FoodAndPopGrowthProcedure(Dictionary<int, int> growths,
        Dictionary<int, int> foodConsumptions)
    {
        Growths = growths;
        FoodConsumptions = foodConsumptions;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        DoGrowth(key);
        DoFoodConsumption(key);
        var tick = key.Data.Tick;
        
        //todo make history update itself in its own procedure
        foreach (var r in key.Data.Society.Regimes.Entities)
        {
            r.History.PeepHistory.Update(tick, r, key);
        }
        
    }

    private void DoFoodConsumption(ProcedureWriteKey key)
    {
        foreach (var kvp in FoodConsumptions)
        {
            var regime = key.Data.Society.Regimes[kvp.Key];
            var cons = kvp.Value;
            regime.Items.Remove(ItemManager.Food, cons);
        }
    }
    private void DoGrowth(ProcedureWriteKey key)
    {
        foreach (var kvp in Growths)
        {
            var peep = key.Data.Society.PolyPeeps[kvp.Key];
            var growth = kvp.Value;
            //todo divide by class
            if (growth < 0)
            {
                peep.ShrinkSize(-growth, key);
            }
            
            if (growth > 0)
            {
                peep.GrowSize(growth, key);
            }
        }
    }
}