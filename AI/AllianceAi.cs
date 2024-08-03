
public class AllianceAi
{
    public ERef<Alliance> Alliance { get; private set; }
    public AllianceMilitaryAi Military { get; private set; }
    public DiplomacyAi Diplomacy { get; private set; }

    public AllianceAi(ERef<Alliance> alliance, AllianceMilitaryAi military, DiplomacyAi diplomacy)
    {
        Alliance = alliance;
        Military = military;
        Diplomacy = diplomacy;
    }

    public static AllianceAi Construct(Alliance alliance, Data data)
    {
        return new AllianceAi(alliance.MakeRef(),
            AllianceMilitaryAi.Construct(alliance, data),
            new DiplomacyAi());
    }

    public void CalculateMajor(RegimeTurnOrders orders,
        Alliance alliance, LogicKey key)
    {
        Military.Calculate(key, alliance);
        Diplomacy.Calculate(alliance, orders, key);
    }

    public void CalculateMinor(LogicKey key)
    {
        Military.CalculateMinor(key, Alliance.Get(key.Data));
    }
}