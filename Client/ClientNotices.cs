
public class ClientNotices
{
    public RefAction<Regime> ChangedSpectatingRegime { get; set; }

    public ClientNotices()
    {
        ChangedSpectatingRegime = new RefAction<Regime>();
    }
}