
public class TroopTypes : ModelList<TroopType>
{
    public TroopType Infantry { get; private set; }
    public TroopTypes()
    {
        Infantry = new Infantry();
    }
    
}