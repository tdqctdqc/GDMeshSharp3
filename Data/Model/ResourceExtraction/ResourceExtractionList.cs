
public class ResourceExtractionList : ModelList<ResourceExtractionBuilding>
{
    public Mine IronMine { get; private set; }

    public ResourceExtractionList(Items items, FlowList flows, 
        PeepJobList jobs)
    {
        IronMine = new Mine(nameof(IronMine), items.Iron, 
            items, jobs, flows);
    }
}