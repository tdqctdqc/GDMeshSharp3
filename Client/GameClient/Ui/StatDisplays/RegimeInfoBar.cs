using Godot;

public partial class RegimeInfoBar : HBoxContainer
{
    private TextureRect _flag;
    private Label _name;
    private Data _data;
    public RegimeInfoBar(Client client, Data data, bool host)
    {
        _data = data;
        this.AddChildWithVSeparator(TickDisplay.Create(client, data));
        var hostClientLabel = new Label();
        hostClientLabel.Text = host ? "Host" : "Client";
        this.AddChildWithVSeparator(hostClientLabel);
        
        _flag = new TextureRect();
        _flag.Size = new Vector2(3f, 2f);
        _flag.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;

        _flag.AddClickUpAction(MouseButton.Left, () =>
            {
                var spect = client.GetComponent<MapGraphics>()
                    .SpectatingRegime;
                if (spect is not null)
                {
                    RegimeOverviewWindow.Open(spect, client);
                }
            }
        );
        
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
        _flag.Texture = spectating.Template.Get(_data).Flag.Texture;
        var localPlayerRegime = _data.BaseDomain.PlayerAux.LocalPlayer.Regime
            .Get(_data);
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
