
public class ClientNotices
{
    public RefAction<Regime> ChangedSpectatingRegime { get; set; }
    public RefAction<int> PostTick { get; private set; }
    public ClientNotices()
    {
        ChangedSpectatingRegime = new RefAction<Regime>();
        PostTick = new RefAction<int>();
    }
}