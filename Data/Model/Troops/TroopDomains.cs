
public class TroopDomains : ModelManager<TroopDomain>
{
    public TroopDomain Land { get; private set; } = new();
    public TroopDomain Sea { get; private set; } = new();
    public TroopDomain Air { get; private set; } = new();
}