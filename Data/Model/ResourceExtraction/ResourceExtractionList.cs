
public class ResourceExtractionList : ModelManager<ResourceExtractionBuilding>
{
    public Mine IronMine { get; private set; }
        = new Mine();

    public ResourceExtractionList()
    {
        
    }
}