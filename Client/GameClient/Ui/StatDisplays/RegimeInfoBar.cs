using Godot;

public partial class RegimeInfoBar : HBoxContainer
{
    public RegimeInfoBar(Client client, Data data, bool host)
    {
        var player = data.BaseDomain.PlayerAux.LocalPlayer;
        
        this.AddChildWithVSeparator(TickDisplay.Create(client, data));
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        this.AddChildWithVSeparator(hostClientLabel);
        
        var regimeFlagRect = new TextureRect();
        regimeFlagRect.Size = new Vector2(3f, 2f);
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;

        regimeFlagRect.SubscribeUpdate(
            () =>
            {
                var playerRegime = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
                if (player.Regime.Fulfilled())
                {
                    regimeFlagRect.Texture = playerRegime.Entity(data).Template.Model(data).Flag.Texture;
                }
                else
                {
                    regimeFlagRect.Texture = null;
                }
            },
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        AddChild(regimeFlagRect);
        
        var regimeNameLabel = new Label();
        StatLabel.Construct<string>(client, "", regimeNameLabel, 
            () => data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data)?.Name,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        this.AddChildWithVSeparator(regimeNameLabel);
        this.AddChildWithVSeparator(new RegimePeepsInfoBar(client, data));
    }
}
