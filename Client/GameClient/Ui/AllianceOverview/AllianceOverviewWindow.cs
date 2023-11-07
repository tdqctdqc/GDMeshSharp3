
using Godot;
using Ui.AllianceOverview;

public partial class AllianceOverviewWindow : TabWindow
{
    private ProposalsTab _proposals;

    public AllianceOverviewWindow()
    {
        MinSize = new Vector2I(1000, 1000);
        _proposals = new ProposalsTab();
        AddTab(_proposals);
    }
    public void Setup(Alliance alliance, Client client)
    {
        if (alliance == null) return;
        _proposals.Setup(alliance, client);
    }
}