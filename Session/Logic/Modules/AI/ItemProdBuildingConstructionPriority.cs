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
        : base(b => b is ItemProdBuildingModel p && p.ProdItem == producedItem,
            b => ((ItemProdBuildingModel)b).ProdCap,
            getWeight)
    {
        ProducedItem = producedItem;
    }

    
}
