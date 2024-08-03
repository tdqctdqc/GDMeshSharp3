using System;
using Godot;
namespace Ui.RegimeOverview;

public partial class MilitaryAiTab : VBoxContainer, IUiDrawable
{
    private Func<Regime> _getRegime;

    public MilitaryAiTab(Func<Regime> getRegime)
    {
        _getRegime = getRegime;
        Name = "Ai";
    }

    public void Draw(Client client)
    {
        this.ClearChildren();
        var regime = _getRegime();
        if (regime.IsPlayerRegime(client.Data)) return;
        var ai = client.Data.HostLogicData.RegimeAis[regime]
            .Military;
        this.CreateLabelAsChild("Force Composition");
        foreach (var (meta, (needed, have)) 
                 in ai.ForceComposition.GetCurrentAndNeededTotals(regime, client.Data))
        {
            this.CreateLabelAsChild($"{meta.Name}: {have} / {needed}");
        }
        
    }
}