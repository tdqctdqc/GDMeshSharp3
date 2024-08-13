
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IssueWindow : Window
{
    private Client _client;
    private static Issue _issue;
    private Container _panel;
    
    private IssueWindow(Client c)
    {
        _client = c;
        this.MakeFreeable();
        _panel = new PanelContainer();
        _panel.FullRect();
        AddChild(_panel);
        AboutToPopup += Draw;
        TreeExiting += () => _issue?.Clear(_client);
    }
    
    public static void Open(Client c)
    {
        var w = new IssueWindow(c);
        Game.I.Client.WindowHolder.OpenWindowFullSize(w);
    }

    private void Draw()
    {
        this.ClearChildren();
        var issues = _client.Data.ClientPlayerData.Issues;

        var hbox = new HBoxContainer();
        AddChild(hbox);
        hbox.FullRect();
        hbox.ExpandFill();
        var left = new VBoxContainer();
        left.ExpandFill();

        var control = new VBoxContainer();
        control.ExpandFill();
        left.AddChild(control);
        hbox.AddChild(left);

        var right = hbox.MakeScrollChild<VBoxContainer>(out var rScroll);
        rScroll.ExpandFill();
        right.ExpandFill();
        
        
        left.AddButton("Clear Issues", () =>
        {
            _issue?.Clear(_client);
            _client.Data.ClientPlayerData.Issues.Clear();
            Draw();
        });
        left.AddButton("Clear Overlay", () =>
        {
            _issue?.Clear(_client);
        });

        var tree = new Tree();
        tree.ExpandFill();
        right.AddChild(tree);
        tree.Columns = 4;
        var root = tree.CreateItem();
        var buttonActions = new Dictionary<TreeItem, Action>();
        var byTick = issues
            .SortBy(i => i.Tick)
            .OrderByDescending(i => i.Key);
        foreach (var (tick, tickIssues) in byTick)
        {
            var tickItem = root.CreateChild();
            tickItem.SetText(0, $"Tick {tick}: {tickIssues.Count}");
            var byType = tickIssues.SortBy(i => i.GetType());
            foreach (var (type, typeIssues) in byType)
            {
                var typeItem = tickItem.CreateChild();
                typeItem.SetText(1, $"{type.Name}: {typeIssues.Count}");

                foreach (var issue in typeIssues)
                {
                    var issueItem = typeItem.CreateChild();
                    issueItem.SetText(1, issue.Message);
                    issueItem.AddButton(2, Icon.Button1,
                        -1, false, issue.Message);
                    buttonActions.Add(issueItem, () =>
                    {
                        _issue?.Clear(_client);
                        _issue = issue;
                        // _client.UiController.ModeOption.Choose<BlankMode>();
                        issue.Draw(_client);
                        DrawInfo(control);
                        _client.Cam().JumpTo(issue.Pos);
                    });
                }
            }
        }
        
        tree.ButtonClicked += (item, column, id, index) =>
        {
            buttonActions[item].Invoke();
        };
        
        
        
        
        
    }

    private void DrawInfo(VBoxContainer control)
    {
        control.ClearChildren();
        if (_issue is null) return;
        control.CreateLabelAsChild(_issue.Message);
        var settings = _issue.GetSettings();
        foreach (var option in settings.SettingsOptions)
        {
            control.AddChild(option.GetControlInterface());
        }
    }
}