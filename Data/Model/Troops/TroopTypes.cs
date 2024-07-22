
public class TroopTypes : ModelPredefs<TroopType>
{
    public TroopType Infantry { get; private set; }
        = new();
    public TroopType Artillery { get; private set; }
        = new();
    public TroopType MachineGun { get; private set; }
        = new();
}