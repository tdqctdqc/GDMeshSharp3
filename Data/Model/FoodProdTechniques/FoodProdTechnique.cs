using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class FoodProdTechnique 
    : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }

    public float BaseProd() => Labor.Outputs.Contents
        .Single().Value;

    public float BaseLabor() => Labor.TotalLabor();
    public Icon Icon { get; private set; }
    public PeepJob JobType(Data d) => Labor.Jobs.GetEnumModel(d).Single().Key;
    public LaborComponent Labor { get; private set; }
    public FoodProdTechnique()
    {
        
    }

    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }

    public abstract float NumForCell(Cell cell, Data data);
    public float FoodPerLabor() => BaseProd() / BaseLabor();
}
