
using System;
using System.Linq;
using Godot;

public partial class ConstructionPanel : PanelContainer
{
    private VBoxContainer _info;
    private VBoxContainer _interact;
    private ConstructionPanel()
        : base()
    {
    }
    public ConstructionPanel(Client c) 
    {
        var margin = new MarginContainer();
        AddChild(margin);
        var inner = margin.MakeContainer<VBoxContainer>();
        inner.FullRect();
        this.SelfModulate = Colors.Black;

        
        _info = inner.MakeScrollChild<VBoxContainer>(
            out var s1);
        _info.ExpandFill();
        s1.ExpandFill();

        _interact = inner.MakeScrollChild<VBoxContainer>(out var s2);
        _interact.ExpandFill();
        s2.ExpandFill();

        var mode = c.UiController.ModeOption.Options
            .OfType<ConstructionMode>()
            .First();
        mode.Cell.SettingChanged.SubscribeForNode(
            v =>
            {
                Draw(v.newVal, c);
            }, this);
    }

    private void Draw(Cell cell, Client c)
    {
        _info.ClearChildren();
        _interact.ClearChildren();
        var regime = cell.Controller.Get(c.Data);
        var localPlayer = c.Data.BaseDomain.PlayerAux.LocalPlayer;
        if (localPlayer.Regime.RefId != regime.Id) return;
        
        var usedLabor = 0f;
        var expectedLabor = 0f;
        
        var s = cell.GetSettlement(c.Data);
        if (s is not null)
        {
            usedLabor += s.Buildings.GetEnumModel(c.Data)
                .Sum(kvp => kvp.Key.Labor.TotalLabor() * kvp.Value);
            var inProgress = regime.MakeQueue.Queue
                .OfType<PlayerSettlementBuildingMakeProject>()
                .Where(p => p.Settlement.RefId == s.Id);
            expectedLabor += inProgress
                .Select(p => (SettlementBuilding)p.Making.Get(c.Data))
                .Sum(b => b.Labor.TotalLabor());
        }

        var rd = cell.GetResourceDeposit(c.Data);
        if (rd is not null)
        {
            var extraction = rd.Extraction.Get(c.Data);
            if (extraction is not null)
            {
                usedLabor += extraction.Labor.TotalLabor();
            }
            var inProgress = regime.MakeQueue.Queue
                .OfType<PlayerResourceExtractionMakeProject>()
                .Where(p => p.ResourceDeposit.RefId == rd.Id);
            expectedLabor += inProgress
                .Select(p => (ResourceExtractionBuilding)p.Making.Get(c.Data))
                .Sum(b => b.Labor.TotalLabor());
        }
        _info.CreateLabelAsChild($"Regime: " + regime.Name);
        _info.CreateLabelAsChild("Cell: " + cell.Id.ToString());
        var population = cell.GetPeep(c.Data).Size;
        _info.CreateLabelAsChild("Population: " + population);
        var freeLabor = population - (usedLabor + expectedLabor);
        _info.CreateLabelAsChild($"Free Labor: {freeLabor}");

        if (s is not null)
        {
            DrawSettlementBuildings(cell, c, s, regime, localPlayer);
        }
        
        if (rd is not null)
        {
            DrawExtractions(cell, c, rd, regime, localPlayer);
        }
    }

    private void DrawSettlementBuildings(Cell cell, Client c, Settlement s, Regime regime, Player localPlayer)
    {
        var settlementBuildingList = c.Data.Models
            .ModelsById
            .Values.OfType<SettlementBuilding>();
        foreach (var model in settlementBuildingList)
        {
            var vbox = new VBoxContainer();
            vbox.ExpandFill();
            var text = $"{model.Name} x {s.Buildings.Get(model).ToString()}";

            text += $"\n Labor: {model.Labor.TotalLabor()}";

            foreach (var (buildMaterial, amt) in model.Makeable.BuildCosts.GetEnumModel(c.Data))
            {
                text += $"\n {buildMaterial.Name}: {regime.Stock.Stock.Get(buildMaterial)}/{amt}";
            }

            var labeled = model.Icon.GetLabeledIcon<VBoxContainer>(text, 40f);
            vbox.AddChild(labeled);

            var buildingProjects = regime.MakeQueue.Queue
                .OfType<PlayerSettlementBuildingMakeProject>()
                .Where(p => p.Settlement.RefId == s.Id
                            && p.Making.RefId == model.Id);
            var buildingTotal = buildingProjects.Sum(b => b.Amount);
            var buildingFinished = buildingProjects.Sum(b => b.Fulfilled);

            if (regime.HasPrereqs(model))
            {
                var button = ButtonExt.GetButton(() =>
                {
                    var project = PlayerSettlementBuildingMakeProject.Construct(
                        1, s, regime, model);
                    var inner = new StartMakeProjectCommand(project, localPlayer.PlayerGuid);
                    var act = () => Draw(cell, c);
                    var com = CallbackCommand.Construct(inner, act, c);
                    c.HandleCommand(com);
                });
                button.Text = $"In progress: {buildingFinished} / {buildingTotal}";
                vbox.AddChild(button);
            }

            _interact.AddChild(vbox);
        }
    }

    private void DrawExtractions(Cell cell, Client c, ResourceDeposit rd, 
        Regime regime, Player localPlayer)
    {
        var resExtractionBuildings = c.Data.Models.GetModels<ResourceExtractionBuilding>()
            .Where(rx => rx.Resource(c.Data) == rd.Item.Get(c.Data));
        foreach (var model 
                 in resExtractionBuildings)
        {
            var vbox = new VBoxContainer();
            vbox.ExpandFill();
            var text = $"{model.Name}";

            text += $"\n Labor: {model.Labor.TotalLabor()}";

            foreach (var (buildMaterial, amt) in model.Makeable.BuildCosts.GetEnumModel(c.Data))
            {
                text += $"\n {buildMaterial.Name}: {regime.Stock.Stock.Get(buildMaterial)}/{amt}";
            }

            var labeled = model.Icon.GetLabeledIcon<VBoxContainer>(text, 40f);
            vbox.AddChild(labeled);

            var projects 
                = regime.MakeQueue.Queue
                .OfType<PlayerResourceExtractionMakeProject>()
                .Where(p =>
                {
                    
                    return p.ResourceDeposit.RefId == rd.Id
                        && p.Making.RefId == model.Id;
                });
            
            var (buildingTotal, buildingFinished) = 
                projects.Any()
                ? (projects.First().Amount,
                    projects.First().Fulfilled)
                : (0f, 0f);

            if (regime.HasPrereqs(model))
            {
                var button = ButtonExt.GetButton(() =>
                {
                    var project = PlayerResourceExtractionMakeProject.Construct(
                        rd, regime, model);
                    var inner = new StartMakeProjectCommand(project, localPlayer.PlayerGuid);
                    var act = () =>
                    {
                        Draw(cell, c);
                    };
                    var com = CallbackCommand.Construct(inner, act, c);
                    c.HandleCommand(com);
                });
                button.Text = $"In progress: {buildingFinished} / {buildingTotal}";
                vbox.AddChild(button);
            }

            _interact.AddChild(vbox);
        }
    }
}