using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ItemProdBuildingConstructionPriority 
    : ConstructionPriority
{
    public Item ProducedItem { get; private set; }
    public ItemProdBuildingConstructionPriority(Item producedItem, Func<Data, Regime, float> getWeight) 
        : base(
            $"{producedItem.Name} Item Prod",
            getWeight)
    {
        ProducedItem = producedItem;
    }


    protected override float Utility(BuildingModel t)
    {
        return t.GetComponent<ItemProd>(p => p.ProdItem == ProducedItem).ProdCap;
    }

    protected override bool Relevant(BuildingModel t, Data d)
    {
        return t.GetComponent<ItemProd>(p => p.ProdItem == ProducedItem) != null;
    }
}
