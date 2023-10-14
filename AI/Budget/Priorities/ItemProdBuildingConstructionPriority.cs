using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ItemProdBuildingConstructionPriority : ConstructionPriority
{
    public Item ProducedItem { get; private set; }
    public ItemProdBuildingConstructionPriority(Item producedItem, Func<Data, Regime, float> getWeight) 
        : base(
            $"{producedItem.Name} Item Prod",
            getWeight,
            b => b.GetComponent<ItemProd>(p => p.ProdItem == producedItem) != null,
            b => b.GetComponent<ItemProd>(p => p.ProdItem == producedItem).ProdCap)
    {
        ProducedItem = producedItem;
    }
}
