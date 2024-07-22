
public class ResourceExtractionList : ModelPredefs<ResourceExtractionBuilding>
{
    public Mine IronMine { get; private set; }
        = new Mine();
    public Mine CoalMine { get; private set; }
        = new Mine();
    public ResourceExtractionList()
    {
        
    }
}