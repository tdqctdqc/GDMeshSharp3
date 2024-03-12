using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class GeneralTab : ScrollContainer
{
    private VBoxContainer _container;
    public override void _Ready()
    {
        base._Ready();
    }

    public GeneralTab()
    {
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        AddChild(_container);
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
    }
    public void Setup(Regime regime, Client client)
    {
        Name = regime.Name;
        _container.ClearChildren();

        var flag = regime.Template.Get(client.Data).Flag;
        var flagTexture = flag.GetTextureRect(100f);
        _container.AddChild(flagTexture);
        
        var seeAlliance = ButtonExt.GetButton(() =>
        {
            var alliance = regime.GetAlliance(client.Data);
            var w = client.WindowManager.OpenWindow<AllianceOverviewWindow>();
            w.Setup(alliance, client);
        });
        seeAlliance.Text = "See Alliance";
        _container.AddChild(seeAlliance);
        var spectating = client.GetComponent<MapGraphics>()
            .SpectatingRegime;
        var localPlayerRegime = client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(client.Data);
        if (regime != spectating)
        {
            var spectateRegime = ButtonExt.GetButton(() =>
            {
                client.GetComponent<MapGraphics>().SpectateRegime(regime);
            });
            spectateRegime.Text = "Spectate Regime";
            _container.AddChild(spectateRegime);
        }
        
        
        if (regime != spectating)
        {
            if (regime.IsMajor && localPlayerRegime != regime)
            {
                var chooseRegime = ButtonExt.GetButton(() =>
                {
                    var com = new ChooseRegimeCommand(regime.MakeRef(),
                        client.Data.ClientPlayerData.LocalPlayerGuid);
                    client.HandleCommand(com);
                });
                chooseRegime.Text = "Choose Regime";
                _container.AddChild(chooseRegime);
            }
        }

        var spectatingAlliance = spectating.GetAlliance(client.Data);
        var spectatingAllianceLeader = spectatingAlliance.Leader.Get(client.Data);
        
        var regimeAlliance = regime.GetAlliance(client.Data);
        var regimeAllianceLeader = regimeAlliance.Leader.Get(client.Data);
        
        if (regime != spectating
            && spectatingAllianceLeader == spectating)
        {
            var target = regime.GetAlliance(client.Data);
            var declareRival = ButtonExt.GetButton(() =>
            {
                var proc = new DeclareRivalProcedure(
                    spectatingAlliance.Id,
                    target.Id);
                var com = new DoProcedureCommand(proc, 
                    client.Data.ClientPlayerData.LocalPlayerGuid);
                client.Server.QueueCommandLocal(com);
            });
            declareRival.Text = "Declare Rival";
            _container.AddChild(declareRival);
        }

        _container.CreateLabelAsChild("ALLIANCE: " + regime.GetAlliance(client.Data).Id);
        _container.CreateLabelAsChild("ALLIANCE LEADER: " 
                                      + regime.GetAlliance(client.Data).Leader.Get(client.Data).Name
                                      + " " + regime.GetAlliance(client.Data).Leader.Get(client.Data).Id);
        _container.CreateLabelAsChild("ALLIANCE MEMBERS");
        foreach (var ally in regime.GetAlliance(client.Data).Members.Items(client.Data))
        {
            if (ally == regime) continue;
            _container.CreateLabelAsChild(ally.Name);
        }
        _container.CreateLabelAsChild("RIVALS");
        foreach (var rival in regime.GetAlliance(client.Data).GetRivals(client.Data))
        {
            _container.CreateLabelAsChild(rival.Leader.Get(client.Data).Name);
        }

        
        _container.CreateLabelAsChild("AT WAR");
        foreach (var rival in regime.GetAlliance(client.Data).GetAtWar(client.Data))
        {
            _container.CreateLabelAsChild(rival.Leader.Get(client.Data).Name);
        }
    }
}
