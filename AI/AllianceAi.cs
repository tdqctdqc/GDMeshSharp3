
public class AllianceAi
{
    public AllianceMilitaryAi MilitaryAi { get; private set; }
    public AllianceAi(Alliance alliance, Data data)
    {
        MilitaryAi = new AllianceMilitaryAi();
    }

    public void Calculate(Alliance alliance, LogicWriteKey key)
    {
        MilitaryAi.Calculate(key, alliance);
    }
}