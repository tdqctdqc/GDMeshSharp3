using System.Linq;
using Godot;

namespace Ui.AllianceOverview;

public partial class ProposalsTab : ScrollContainer
{
    private VBoxContainer _container;
    public ProposalsTab()
    {
        Name = "Proposals";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(_container);
    }
    
    public void Setup(Alliance alliance, Client client)
    {
        _container.ClearChildren();
        var proposals = alliance.GetProposals(client.Data);
        foreach (var proposal in proposals)
        {
            _container.AddChild(proposal.GetDisplay(client.Data));
        }
    }
}