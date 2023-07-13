
using Godot;

public class RegimeInfoBar
{
    public static Node Create(Data data)
    {
        var hbox = new HBoxContainer();
        var player = data.BaseDomain.PlayerAux.LocalPlayer;
        var regimeFlagRect = new TextureRect();
        regimeFlagRect.Size = new Vector2(15f, 10f);
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;

        regimeFlagRect.SubscribeUpdate(
            () =>
            {
                var playerRegime = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
                if (player.Regime.Fulfilled())
                {
                    regimeFlagRect.Texture = playerRegime.Entity(data).Template.Model(data).Flag;
                }
                else
                {
                    regimeFlagRect.Texture = null;
                }
            },
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        hbox.AddChild(regimeFlagRect);
        
        var regimeNameLabel = new Label();
        StatLabel.Construct<string>("", regimeNameLabel, 
            () => data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data)?.Name,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        hbox.AddChildWithVSeparator(regimeNameLabel);

        var icon = Icon.Create("Income", Icon.AspectRatio._1x1, 25f);

        var income = NodeExt.MakeFlowStatDisplay(FlowManager.Income, 
            data, 
            10f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        hbox.AddChildWithVSeparator(income);
        
        var conCap = NodeExt.MakeFlowStatDisplay(FlowManager.ConstructionCap, 
            data, 
            10f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        hbox.AddChildWithVSeparator(conCap);
        
        var indPower = NodeExt.MakeFlowStatDisplay(FlowManager.IndustrialPower, 
            data, 
            10f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        hbox.AddChildWithVSeparator(indPower);
        return hbox;
    }
}
