using Godot;

public partial class SpectatingRegimeInfoBar : HBoxContainer
{
    private TextureRect _flag;
    private Label _name;
    private Data _data;
    public SpectatingRegimeInfoBar(Client client, Data data, bool host)
    {
        _data = data;
        this.AddChildWithVSeparator(TickDisplay.Create(client, data));
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        this.AddChildWithVSeparator(hostClientLabel);
        
        _flag = new TextureRect();
        _flag.Size = new Vector2(3f, 2f);
        _flag.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
        AddChild(_flag);
        _name = new Label();
        this.AddChildWithVSeparator(_name);
        this.AddChildWithVSeparator(new RegimePeepsInfoBar(client, data));

        client.Notices
            .ChangedSpectatingRegime.SubscribeForNode(ChangedSpectator, this);
        var mapGraphics = client.GetComponent<MapGraphics>();
        ChangedSpectator(mapGraphics.SpectatingRegime);
    }

    private void ChangedSpectator(Regime spectating)
    {
        _flag.Texture = spectating.Template.Model(_data).Flag.Texture;
        var localPlayerRegime = _data.BaseDomain.PlayerAux.LocalPlayer.Regime
            .Entity(_data);
        if (localPlayerRegime == spectating)
        {
            _name.Text = spectating.Name;
        }
        else
        {
            _name.Text = $"(S) {spectating.Name}";
        }
    }
}
