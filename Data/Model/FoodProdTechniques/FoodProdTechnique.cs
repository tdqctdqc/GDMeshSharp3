using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class FoodProdTechnique 
    : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public int BaseProd { get; private set; }
    public int BaseLabor { get; private set; }
    public Icon Icon { get; private set; }
    public PeepJob JobType { get; private set; }
    public LaborComponent Labor { get; private set; }
    public FoodProdTechnique(string name, int baseProd, 
        int baseLabor,
        PeepJob jobType, Items items)
    {
        Name = name;
        BaseProd = baseProd;
        BaseLabor = baseLabor;
        Labor = new LaborComponent(
            IdCount<IModel>.Construct(new Dictionary<IModel, float>()), 
            IdCount<IModel>.Construct(
                new Dictionary<IModel, float>
                {
                    { items.Food, baseProd }
                }), 
            IdCount<PeepJob>.Construct(
                new Dictionary<PeepJob, float>
                {
                    { jobType, baseLabor }
                })
        );
        Icon = Icon.Create(name, Vector2I.One);
        JobType = jobType;
    }

    public abstract float NumForCell(Cell cell, Data data);
    public float FoodPerLabor() => BaseProd / BaseLabor;
}
