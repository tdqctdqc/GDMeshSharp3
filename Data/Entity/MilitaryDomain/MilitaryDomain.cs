
public class MilitaryDomain
{
    public UnitAux UnitAux { get; private set; }
    public MilitaryDomain()
    {
    }

    public void Setup(Data data)
    {
        UnitAux = new UnitAux(data);
    }
}