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

    public ResourceExtractionBuilding()
    {
        
    }

    public float OutputPerLabor() => BaseProd / BaseLabor;
    public abstract bool CanBuildInCell(Cell t, Data data);
}