
public class TroopType : IModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public TroopDomain TroopDomain { get; private set; }
    
}