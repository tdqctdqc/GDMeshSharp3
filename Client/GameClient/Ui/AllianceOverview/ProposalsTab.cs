using System.Linq;
using Godot;

namespace Ui.AllianceOverview;

public partial class ProposalsTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private AllianceOverviewWindow _parent;
    public ProposalsTab(AllianceOverviewWindow parent)
    {
        _parent = parent;
        Name = "Proposals";
        this.FullRect();
        _container = new VBoxContainer();
        _container.FullRect();
        AddChild(_container);
    }

    private ProposalsTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var alliance = _parent.Alliance;
        if (alliance is null) return;
        var proposals = alliance.PendingProposals(client.Data);
        foreach (var proposal in proposals)
        {
            _container.AddChild(proposal.GetDisplay(client.Data));
        }
    }
}