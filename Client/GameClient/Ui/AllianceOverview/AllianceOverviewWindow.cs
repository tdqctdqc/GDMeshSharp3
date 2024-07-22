
using Godot;
using Ui.AllianceOverview;

public partial class AllianceOverviewWindow : TabWindow
{
    private ProposalsTab _proposals;
    public Alliance Alliance { get; private set; }
    public AllianceOverviewWindow(Client c) : base(c)
    {
        MinSize = new Vector2I(1000, 1000);
        _proposals = new ProposalsTab(this);
        AddTab(_proposals);
    }
    public void Setup(Alliance alliance)
    {
        Alliance = alliance;
    }

    public static void Open(Alliance alliance, Client client)
    {
        var w = new AllianceOverviewWindow(client);
        w.Setup(alliance);
        client.WindowHolder.OpenWindowFullSize(w);
    }
}