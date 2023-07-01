
using Godot;

public class PlayerRegimeDisplay
{
    public static Node Create(Data data)
    {
        var hbox = new HBoxContainer();
        var player = data.BaseDomain.PlayerAux.LocalPlayer;

        var regimeFlagRect = new TextureRect();
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        regimeFlagRect.Size = new Vector2(30f, 20f);

        regimeFlagRect.SubscribeUpdate(
            () =>
            {
                var playerRegime = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
                if (player.Regime.Fulfilled())
                {
                    regimeFlagRect.Texture = playerRegime.Entity().Template.Model().Flag;
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
            () => data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity()?.Name,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        hbox.AddChildWithVSeparator(regimeNameLabel);

        var icon = Icon.Create("Income", Icon.AspectRatio._1x1, 25f);

        var income = icon.MakeIconStatDisplay(data,
            () =>
            {
                var locRegime = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity();
                if (locRegime == null) return "";
                return Flow.Income.GetFlow(locRegime, data).ToString();
            },
            20f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        hbox.AddChildWithVSeparator(income);
        return hbox;
    }
}
