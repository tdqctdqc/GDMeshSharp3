using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class FoodProdTechnique : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public int BaseProd { get; private set; }
    public int BaseLabor { get; private set; }
    public int Income { get; private set; }
    public Icon Icon { get; private set; }
    public PeepJob JobType { get; private set; }

    public FoodProdTechnique(string name, int baseProd, 
        int baseLabor, int income, 
        PeepJob jobType)
    {
        Name = name;
        BaseProd = baseProd;
        BaseLabor = baseLabor;
        Icon = Icon.Create(name, Vector2I.One);
        Income = income;
        JobType = jobType;
    }

    public abstract float NumForCell(Cell cell, Data data);
    public float FoodPerLabor() => BaseProd / BaseLabor;
}
