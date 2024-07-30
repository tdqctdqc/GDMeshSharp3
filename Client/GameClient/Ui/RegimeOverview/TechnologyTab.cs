using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class TechnologyTab : ScrollContainer, IUiDrawable
{
    private Container _container, _currentResearchInfo,
        _availableResearchInfo, _alreadyResearchedInfo;
    private RegimeOverviewWindow _parent;

    public TechnologyTab(RegimeOverviewWindow parent)
    {
        Name = "Technology";
        _parent = parent;
        _container = new HBoxContainer();
        _container.ExpandFill();
        AddChild(_container);
    }
    public void Draw(Client client)
    {
        _container.ClearChildren();

        var side = ContainerExt.MakeScroll<VBoxContainer>(out var sideScroll);
        _container.AddChild(sideScroll);
        side.ExpandFill();
        sideScroll.ExpandFill(1);
        side.CreateLabelAsChild("Available Technologies");

        _availableResearchInfo = side.MakeScrollChild<VBoxContainer>(out var availScroll);
        availScroll.ExpandFill();
        _availableResearchInfo.ExpandFill();
        
        var availableTechs = client.Data.Models.GetModels<Technology>()
            .Where(t => t.AvailableToResearch(_parent.Regime, client.Data));

        var availableList = new ItemListToken<Technology>(
            availableTechs,
            t => t.DisplayName,
            false
        );
        availableList.JustSelected += () =>
        {
            var avail = availableList.Selected.Single();
            DrawTechInfo(avail, _availableResearchInfo, client);
        };
        availableList.SelectAt(0);
        side.AddChild(availableList.ItemList);
        availableList.ItemList.ExpandFill();
        var researchBtn = side.AddButton("Set Research",
            () =>
            {
                var tech = availableList.Selected.Single();
                var proc = new SetResearchProcedure(
                    _parent.Regime.MakeRef(),
                    tech.MakeRef());
                var inner = new SendMessageCommand(proc, client.Data.ClientPlayerData.LocalPlayerGuid);
                var com = CallbackCommand.Construct(inner, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        DrawInfo(client);
                    }
                }, client);
                client.HandleCommand(com);
            });
        
        _currentResearchInfo = ContainerExt.MakeScroll<VBoxContainer>(out var infoScroll);
        infoScroll.ExpandFill(3);
        _container.AddChild(infoScroll);

        var right = ContainerExt.MakeScroll<VBoxContainer>(out var rightScroll);
        right.ExpandFill(1);
        rightScroll.ExpandFill();

        _alreadyResearchedInfo = right.MakeScrollChild<VBoxContainer>(out var alreadyScroll);
        alreadyScroll.ExpandFill();
        _alreadyResearchedInfo.ExpandFill();
        
        right.CreateLabelAsChild("Researched Technologies");
         
        var alreadyResearched = _parent.Regime
            .GetTechnologies(client.Data)
            .Select(t => t.Get(client.Data));
        
        var alreadyResearchedList = new ItemListToken<Technology>(
            alreadyResearched,
            t => t.DisplayName,
            false
        );
        alreadyResearchedList.JustSelected += () =>
        {
            var already = alreadyResearchedList.Selected.Single();
            DrawTechInfo(already, _alreadyResearchedInfo, client);
        };
        alreadyResearchedList.ItemList.ExpandFill();
        alreadyResearchedList.SelectAt(0);
        right.AddChild(alreadyResearchedList.ItemList);
        _container.AddChild(rightScroll);
        
        DrawInfo(client);
    }

    private void DrawInfo(Client c)
    {
        _currentResearchInfo.ClearChildren();

        var researchModel = c.Data.Models.Items.Research;
        var researchProduced = _parent.Regime
            .Stock.Produced.Get(researchModel);
        var researchRecurring = _parent.Regime
            .Stock.RecurringCosts.Get(researchModel);
        var researchSingle = _parent.Regime
            .Stock.SingleTimeCosts.Get(researchModel);
        _currentResearchInfo.CreateLabelAsChild($"Research produced: {researchProduced.RoundTo2Digits()}");
        _currentResearchInfo.CreateLabelAsChild($"Research consumed recurring: {researchRecurring.RoundTo2Digits()}");
        _currentResearchInfo.CreateLabelAsChild($"Research consumed single time: {researchSingle.RoundTo2Digits()}");
        _currentResearchInfo.CreateLabelAsChild($"Research for technology: {(researchProduced - (researchRecurring + researchSingle)).RoundTo2Digits()}");
        
        
        //todo
        // var tech = _parent.Regime.Technology;
        //
        //
        // for (var i = 0; i < tech.Progresses.Count; i++)
        // {
        //     var t = tech.Current.Get(c.Data);
        //     var e = tech.Progresses[tech.Current];
        //     
        //     _currentResearchInfo.CreateLabelAsChild($"Researching: {t.DisplayName}");
        //     _currentResearchInfo.CreateLabelAsChild($"Progress: {e} / {t.ResearchCost}");
        // }
    }

    private void DrawTechInfo(Technology t, Container container, Client c)
    {
        container.ClearChildren();
        container.CreateLabelAsChild($"{t.Name}");
        container.CreateLabelAsChild($"Category: {t.Category.Name}");
        container.CreateLabelAsChild($"Cost: {t.ResearchCost}");

        var med = c.Settings.MedIconSize.Value;
        
        var models = t.GetModelsWithPrereq(c.Data);

        if (models.Any())
        {
            container.CreateLabelAsChild("Required for: ");
        }
        
        foreach (var model in models)
        {
            if (model is IIconed i)
            {
                container.AddChild(i.Icon.GetLabeledIcon<HBoxContainer>(
                    model.Name, med));
            }
            else
            {
                container.CreateLabelAsChild(model.Name);
            }
        }
    }
}