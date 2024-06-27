
using System;
using System.Linq;
using Godot;

public partial class ConstructionPanel : Panel
{
    private VBoxContainer _info;
    private VBoxContainer _buttons;
    private ConstructionPanel()
        : base()
    {
    }
    public ConstructionPanel(Client c) 
    {
        var inner = this.MakeContainer<VBoxContainer>(new Vector2(300f, 600f));
        this.SelfModulate = Colors.Black;

        var info = new Control();
        _info = info.MakeScroll<VBoxContainer>(new Vector2(300f, 200f));
        inner.AddChild(info);

        var buttons = new Control();
        _buttons = buttons.MakeScroll<VBoxContainer>(new Vector2(300f, 400f));
        inner.AddChild(buttons);

        var mode = c.UiController.ModeOption.Options
            .OfType<ConstructionMode>()
            .First();
        mode.Settlement.SettingChanged.SubscribeForNode(
            v =>
            {
                Draw(v.newVal, c);
            }, this);
    }

    private void Draw(Settlement s, Client c)
    {
        _info.ClearChildren();
        _buttons.ClearChildren();
        if (s is null) return;
        var regime = s.Cell.Get(c.Data).Controller.Get(c.Data);
        var localPlayer = c.Data.BaseDomain.PlayerAux.LocalPlayer;
        if (localPlayer.Regime.RefId != regime.Id) return;
        _info.CreateLabelAsChild($"{s.Tier.Get(c.Data).Name}: " + s.Name);
        _info.CreateLabelAsChild($"Regime: " + regime.Name);
        _info.CreateLabelAsChild("Cell: " + s.Cell.RefId.ToString());
        var population = s.Cell.Get(c.Data).GetPeep(c.Data).Size;
        _info.CreateLabelAsChild("Population: " + population);
        var usedLabor = s.Buildings.GetEnumModel(c.Data)
            .Where(kvp => kvp.Key.HasComponent<LaborComponent>())
            .Sum(kvp => kvp.Key.GetComponent<LaborComponent>().TotalLabor() * kvp.Value);
        var inProgress = regime.MakeQueue.Queue
            .OfType<PlayerBuildingMakeProject>()
            .Where(p => p.Settlement.RefId == s.Id);
        var expectedLabor = inProgress
            .Select(p => (SettlementBuildingModel)p.Making.Get(c.Data))
            .Where(b => b.HasComponent<LaborComponent>())
            .Sum(b => b.GetComponent<LaborComponent>().TotalLabor());

        var freeLabor = population - (usedLabor + expectedLabor);
        _info.CreateLabelAsChild($"Free Labor: {freeLabor}");
        
        var list = c.Data.Models.ModelsById
            .Values.OfType<SettlementBuildingModel>();
        foreach (var model in list)
        {
            var vbox = new VBoxContainer();
            var text = $"{model.Name} x {s.Buildings.Get(model).ToString()}";
            if (model.GetComponent<LaborComponent>() is LaborComponent l)
            {
                text += $"\n Labor: {model.GetComponent<LaborComponent>().TotalLabor()}";
            }
            foreach (var (buildMaterial, amt) in model.Makeable.BuildCosts.GetEnumModel(c.Data))
            {
                text += $"\n {buildMaterial.Name}: {regime.Stock.Stock.Get(buildMaterial)}/{amt}";
            }
            var labeled = model.Icon.GetLabeledIcon<VBoxContainer>(text, 40f);
            vbox.AddChild(labeled);
            var button = ButtonExt.GetButton(() =>
            {
                var project = PlayerBuildingMakeProject.Construct(
                    s, regime, model);
                var inner = new StartMakeProjectCommand(project, localPlayer.PlayerGuid);
                var act = () => Draw(s, c);
                var com = CallbackCommand.Construct(inner, act, c);
                c.HandleCommand(com);
            });
            var building = regime.MakeQueue.Queue
                .OfType<PlayerBuildingMakeProject>()
                .Where(p => p.Settlement.RefId == s.Id
                            && p.Making.RefId == model.Id);
            var buildingTotal = building.Sum(b => b.Amount);
            var buildingFinished = building.Sum(b => b.Fulfilled);
            button.Text = $"In progress: {buildingFinished} / {buildingTotal}";
            vbox.AddChild(button);
            _buttons.AddChild(vbox);
        }
    }

    
}