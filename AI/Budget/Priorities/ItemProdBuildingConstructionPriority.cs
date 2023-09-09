using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ItemProdBuildingConstructionPriority : BuildingConstructionPriority
{
    public Item ProducedItem { get; private set; }
    public ItemProdBuildingConstructionPriority(Item producedItem, Func<Data, Regime, float> getWeight) 
        : base(
            $"{producedItem.Name} Item Prod",
            b => b.GetComponent<ItemProd>(p => p.ProdItem == producedItem) != null,
            b => b.GetComponent<ItemProd>(p => p.ProdItem == producedItem).ProdCap,
            getWeight)
    {
        ProducedItem = producedItem;
    }

    
}
