
public class TroopDomains : ModelPredefs<TroopDomain>
{
    public TroopDomain Land { get; private set; } = new();
    public TroopDomain Water { get; private set; } = new();
    public TroopDomain Air { get; private set; } = new();
}