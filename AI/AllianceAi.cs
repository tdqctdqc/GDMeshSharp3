
public class AllianceAi
{
    private Alliance _alliance;
    public AllianceMilitaryAi Military { get; private set; }
    public DiplomacyAi Diplomacy { get; private set; }

    public AllianceAi(Alliance alliance, Data data)
    {
        _alliance = alliance;
        Military = new AllianceMilitaryAi(alliance, data);
        Diplomacy = new DiplomacyAi(alliance);
    }

    public void CalculateMajor(RegimeTurnOrders orders,
        Alliance alliance, LogicWriteKey key)
    {
        Military.Calculate(key, alliance);
        Diplomacy.Calculate(orders, key);
    }

    public void CalculateMinor(LogicWriteKey key)
    {
        Military.CalculateMinor(key, _alliance);
    }
}