
using Godot;

public class MilitaryDomain
{
    public UnitAux UnitAux { get; private set; }
    public SingletonCache<CombatHistory> CombatHistories { get; private set; }
    public MilitaryDomain()
    {
    }

    public void Setup(Data data)
    {
        UnitAux = new UnitAux(data);
        CombatHistories = new SingletonCache<CombatHistory>(data);
    }
}