using System;
using System.Collections.Generic;
using System.Linq;

public abstract class FoodProdTechnique : IModel
{
    public string Name { get; private set; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<IModelAttribute> Attributes { get; }
    public int Id { get; private set; }
    public int BaseProd { get; private set; }
    public int BaseLabor { get; private set; }
    public int Income { get; private set; }
    public Icon Icon { get; private set; }
    public PeepJob JobType { get; private set; }

    public FoodProdTechnique(string name, int baseProd, int baseLabor, int income, PeepJob jobType)
    {
        Name = name;
        BaseProd = baseProd;
        BaseLabor = baseLabor;
        Icon = Icon.Create(name, Icon.AspectRatio._1x1, 25f);
        Income = income;
        JobType = jobType;
        Attributes = new AttributeHolder<IModelAttribute>();
    }

    public abstract int NumForPoly(MapPolygon poly, Data data);
    public float FoodPerLabor() => BaseProd / BaseLabor;
}
