
using Godot;

public partial class IssueWindow : ClosableWindow
{
    private Client _client;
    private Container _container;

    private IssueWindow()
    {
        Size = Vector2I.One * 500;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        var scroll = new ScrollContainer();
        scroll.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        scroll.CustomMinimumSize = Vector2I.One * 500;
        scroll.AddChild(_container);
        AddChild(scroll);
        AboutToPopup += Draw;
    }
    
    public static IssueWindow Get(Client c)
    {
        var w = new IssueWindow();
        w._client = c;
        return w;
    }

    private void Draw()
    {
        _container.ClearChildren();
        var issues = _client.Data.ClientPlayerData.Issues;
        _container.AddButton("Clear", () =>
        {
            _client.Data.ClientPlayerData.Issues.Clear();
            Draw();
        });
        foreach (var issue in issues)
        {
            _container.AddButton(issue.GetType().Name + " " + issue.Message,
                () =>
                {
                    _client.UiController.ModeOption.Choose<BlankMode>();
                    issue.Draw(_client);
                    _client.Cam().JumpTo(issue.UnitPos);
                }
            );
        }
    }
}