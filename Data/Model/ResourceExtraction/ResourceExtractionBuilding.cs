using System.Collections.Generic;
using Godot;


public abstract class ResourceExtractionBuilding : IModel, IIconed, IMakeable
{
    public string Name { get; private set; }

    public NaturalResource Resource(Data d) 
        => (NaturalResource)Labor.Outputs
            .GetEnumModel(d)
            .GetOnly().Key;
    public int Id { get; private set; }
    public int BaseProd { get; private set; }
    public int BaseLabor { get; private set; }
    public float Income { get; private set; }
    public Icon Icon { get; private set; }
    public LaborComponent Labor { get; private set; }
    public MakeableAttribute Makeable { get; private set; }

    public ResourceExtractionBuilding(string name, 
        NaturalResource resource,
        int baseProd, 
        int baseLabor, float income, 
        PeepJob jobType,
        MakeableAttribute makeable)
    {
        Name = name;
        BaseProd = baseProd;
        BaseLabor = baseLabor;
        Icon = Icon.Create(name, Vector2I.One);
        Income = income;
        Makeable = makeable;
        Labor = new LaborComponent(
            IdCount<Item>.Construct(),
            IdCount<Item>.Construct(
                new Dictionary<Item, float> { { resource, BaseProd } }),
            IdCount<PeepJob>.Construct(
                new Dictionary<PeepJob, float> { { jobType, BaseLabor } })
        );
    }

    public float OutputPerLabor() => BaseProd / BaseLabor;
    public abstract bool CanBuildInCell(Cell t, Data data);
}