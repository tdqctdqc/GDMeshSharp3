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
        var flagControl = new Control();
        var regimeFlagRect = new TextureRect();
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
        regimeFlagRect.StretchMode = TextureRect.StretchModeEnum.Scale;
        regimeFlagRect.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        regimeFlagRect.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        regimeFlagRect.Texture = regime.Template.Model(client.Data).Flag;
        regimeFlagRect.CustomMinimumSize = new Vector2(150f, 100f);
        _container.AddChild(regimeFlagRect);
        
        var seeAlliance = ButtonExt.GetButton(() =>
        {
            var alliance = regime.GetAlliance(client.Data);
            var w = client.WindowManager.OpenWindow<AllianceOverviewWindow>();
            w.Setup(alliance, client);
        });
        seeAlliance.Text = "See Alliance";
        _container.AddChild(seeAlliance);
        
        
        if (regime.IsPlayerRegime(client.Data) == false)
        {
            var chooseRegime = ButtonExt.GetButton(() =>
            {
                var com = new ChooseRegimeCommand(regime.MakeRef(),
                    client.Data.ClientPlayerData.LocalPlayerGuid);
                client.HandleCommand(com);
            });
            chooseRegime.Text = "Choose Regime";
            _container.AddChild(chooseRegime);

            if (client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(client.Data)
                is Regime playerRegime)
            {
                var target = regime.GetAlliance(client.Data);
                var proposeRival = ButtonExt.GetButton(() =>
                {
                    var prop = DeclareRivalProposal.Construct(playerRegime, target, client.Data);
                    var com = new MakeProposalCommand(default, prop);
                    client.Server.QueueCommandLocal(com);
                });
                proposeRival.Text = "Propose Rival";
                _container.AddChild(proposeRival);
            }
        }

        _container.CreateLabelAsChild("ALLIANCE: " + regime.GetAlliance(client.Data).Id);
        _container.CreateLabelAsChild("ALLIANCE LEADER: " 
                                      + regime.GetAlliance(client.Data).Leader.Entity(client.Data).Name
                                      + " " + regime.GetAlliance(client.Data).Leader.Entity(client.Data).Id);
        _container.CreateLabelAsChild("ALLIANCE MEMBERS");
        foreach (var ally in regime.GetAlliance(client.Data).Members.Items(client.Data))
        {
            if (ally == regime) continue;
            _container.CreateLabelAsChild(ally.Name);
        }
        _container.CreateLabelAsChild("RIVALS");
        foreach (var rival in regime.GetAlliance(client.Data).Rivals.Items(client.Data))
        {
            _container.CreateLabelAsChild(rival.Leader.Entity(client.Data).Name);
        }

    }
}
