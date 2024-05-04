using System.Collections.Generic;
using Godot;


public abstract class ResourceExtractionBuilding : IModel, IIconed, IMakeable
{
    public string Name { get; private set; }
    public NaturalResource Resource { get; private set; }
    public int Id { get; private set; }
    public int BaseProd { get; private set; }
    public int BaseLabor { get; private set; }
    public int Income { get; private set; }
    public Icon Icon { get; private set; }
    public PeepJob JobType { get; private set; }
    public ProdComponent Prod { get; private set; }
    public MakeableAttribute Makeable { get; private set; }

    public ResourceExtractionBuilding(string name, 
        NaturalResource resource,
        int baseProd, 
        int baseLabor, int income, 
        PeepJob jobType,
        MakeableAttribute makeable)
    {
        Resource = resource;
        Name = name;
        BaseProd = baseProd;
        BaseLabor = baseLabor;
        Icon = Icon.Create(name, Vector2I.One);
        Income = income;
        JobType = jobType;
        Makeable = makeable;
        Prod = new ProdComponent(
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(
                new Dictionary<IModel, float> { { Resource, BaseProd } }),
            IdCount<PeepJob>.Construct(
                new Dictionary<PeepJob, float> { { JobType, BaseLabor } })
        );
    }

    public float OutputPerLabor() => BaseProd / BaseLabor;
    public abstract bool CanBuildInCell(Cell t, Data data);
}