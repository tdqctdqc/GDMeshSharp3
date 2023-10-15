
public class AllianceAi
{
    public Alliance Alliance { get; private set; }
    public AllianceMilitaryAi MilitaryAi { get; private set; }

    public AllianceAi(Alliance alliance, Data data)
    {
        Alliance = alliance;
        MilitaryAi = new AllianceMilitaryAi(alliance);
    }
}