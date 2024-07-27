using System;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class MakingTab : ScrollContainer, IUiDrawable
{
    private Container _container, _projectInfo;
    private RegimeOverviewWindow _parent;
    private ItemListToken<MakeProject> _projectsList;
    
    public MakingTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Making";
        this.ExpandFill();
        _container = new HBoxContainer();
        _container.ExpandFill();
        AddChild(_container);
    }

    private MakingTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;

        var left = new VBoxContainer();
        _container.AddChild(left);
        left.ExpandFill(1);
        _projectInfo = _container.MakeScrollChild<VBoxContainer>(
            out var rightScroll);
        rightScroll.ExpandFill(2);
        var med = client.Settings.MedIconSize.Value;
        var projects = regime.MakeQueue.Queue;
        
        _projectsList = new ItemListToken<MakeProject>(
            projects,
            p =>
            {
                return $"{p.Description(client.Data)} {p.Fulfilled.RoundTo2Digits()}/{p.Amount.RoundTo2Digits()}";
            },
            p => p.GetIcon(client.Data).Texture,
            (int)med,
            true
        );
        _projectsList.JustSelected += () => DrawProjectInfo(client);
        _projectsList.ItemList.ExpandFill(1);
        left.AddChild(_projectsList.ItemList);
    }

    private void DrawProjectInfo(Client client)
    {
        _projectInfo.ClearChildren();
        if (_projectsList.Selected.Count != 1) return;
        var project = _projectsList.Selected.First();
        var med = client.Settings.MedIconSize.Value;
        var large = client.Settings.LargeIconSize.Value;
        var title = project.Description(client.Data);
        _projectInfo.AddChild(project.GetIcon(client.Data).GetTextureRect(large));

        var titleLabel = _projectInfo.CreateLabelAsChild(title);

        _projectInfo.CreateLabelAsChild($"Amount: {project.Fulfilled}/{project.Amount}");
        _projectInfo.CreateLabelAsChild("Costs");
        var makeable = project.GetMakeable(client.Data);
        foreach (var (costModel, costAmt) in makeable.BuildCosts.GetEnumModel(client.Data))
        {
            var text = $"{costModel.Name}: {costAmt * project.Fulfilled}/{costAmt * project.Amount}";
            if (costModel is IIconed im)
            {
                _projectInfo.AddChild(im.Icon.GetLabeledIcon<HBoxContainer>(
                    text, med));
            }
            else
            {
                _projectInfo.CreateLabelAsChild(text);
            }
        }

        _projectInfo.AddButton("Cancel",
            () =>
            {
                var proc = new CancelMakeProjectProcedure(_parent.Regime.MakeRef(),
                    project.Id);
                var com = new SendMessageCommand(proc,
                    client.Data.ClientPlayerData.LocalPlayerGuid);
                var outer = CallbackCommand.Construct(com,
                    () =>
                    {
                        if (IsInstanceValid(this))
                        {
                            Draw(client);
                        }
                    }, client);
                client.HandleCommand(outer);
            }
        );


        _projectInfo.AddButton("Move Up",
            () =>
            {
                var index = _parent.Regime.MakeQueue.Queue.IndexOf(project);
                var newIndex = Mathf.Max(0, index - 1);
                var proc = new ChangeMakeProjectPriorityProcedure(
                    _parent.Regime.MakeRef(),
                    project.Id,
                    newIndex);
                var com = new SendMessageCommand(proc,
                    client.Data.ClientPlayerData.LocalPlayerGuid);
                var outer = CallbackCommand.Construct(com,
                    () =>
                    {
                        if (IsInstanceValid(this))
                        {
                            Draw(client);
                            _projectsList.Select(project);
                        }
                    }, client);
                client.HandleCommand(outer);
            }
        );
        
        _projectInfo.AddButton("Move Down",
            () =>
            {
                var index = _parent.Regime.MakeQueue.Queue.IndexOf(project);
                var newIndex = Mathf.Min(_parent.Regime.MakeQueue.Queue.Count - 1,
                    index + 1);
                var proc = new ChangeMakeProjectPriorityProcedure(
                    _parent.Regime.MakeRef(),
                    project.Id,
                    newIndex);
                var com = new SendMessageCommand(proc,
                    client.Data.ClientPlayerData.LocalPlayerGuid);
                var outer = CallbackCommand.Construct(com,
                    () =>
                    {
                        if (IsInstanceValid(this))
                        {
                            Draw(client);
                            _projectsList.Select(project);
                        }
                    }, client);
                client.HandleCommand(outer);
            }
        );
    }
}