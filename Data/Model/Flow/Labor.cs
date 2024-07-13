
public class Labor : Flow
{
    public Labor()
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return 0f;
    }
}