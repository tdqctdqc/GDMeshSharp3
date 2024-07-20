using System.Collections.Generic;
using System.Linq;
using Godot;


public abstract class ResourceExtractionBuilding 
    : IModel, IIconed, IMakeable, ITechReqed
{
    public string Name { get; private set; }
    public NaturalResource Resource(Data d) 
        => (NaturalResource)Labor.Outputs
            .GetEnumModel(d)
            .GetOnly().Key;
    public int Id { get; private set; }
    public float BaseProd() => Labor.Outputs.Contents.Single().Value;
    public float BaseLabor() => Labor.TotalLabor();
    public float Income { get; private set; }
    public Icon Icon { get; private set; }
    public LaborComponent Labor { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public HashSet<Technology> Prereqs { get; private set; }
    public ResourceExtractionBuilding()
    {
        
    }

    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }

    public float OutputPerLabor() => BaseProd() / BaseLabor();
    public abstract bool CanBuildInCell(Cell t, Data data);
}