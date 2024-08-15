using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class GeneralTab : ScrollContainer,
    IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public override void _Ready()
    {
        base._Ready();
    }

    public GeneralTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        _container = new VBoxContainer();
        AddChild(_container);
        _container.FullRect();
    }

    private GeneralTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        _container.ExpandFill();
        var regime = _parent.Regime;
        if (regime is null) return;
        Name = regime.Name;

        var top = new HBoxContainer();
        _container.AddChild(top);
        
        var flagContainer = new VBoxContainer();
        var flag = regime.Template.Get(client.Data).Flag;
        var flagTexture = flag.GetTextureRect(100f);
        flagContainer.AddChild(flagTexture);
        flagContainer.CreateLabelAsChild(regime.Name);
        flagContainer.CreateLabelAsChild($"{(regime.IsMajor ? "Major" : "Minor")} Power");
        top.AddChild(flagContainer);

        var middle = new HBoxContainer();
        middle.ExpandFill();
        _container.AddChild(middle);
        
        var left = new VBoxContainer();
        left.ExpandFill();
        middle.AddChild(left);
        var right = new VBoxContainer();
        right.ExpandFill();
        middle.AddChild(right);
        
        
        var rivals = regime.GetRivals(client.Data);
        if (rivals.Count() > 0)
        {
            right.CreateLabelAsChild("Rivals");

            var rivalsContainer = right.MakeScrollChild<HBoxContainer>(
                out var rivalsScroll);
            rivalsScroll.ExpandFill();
            
            foreach (var rival in rivals)
            {
                var rivalContainer = new VBoxContainer();
                var rivalFlag = rival.Template.Get(client.Data).Flag;
                var rivalFlagTexture = rivalFlag.GetTextureRect(50f);
                rivalFlagTexture.AddClickUpAction(MouseButton.Left,
                    () => RegimeOverviewWindow.Open(rival, client));
                rivalContainer.AddChild(rivalFlagTexture);
                rivalContainer.CreateLabelAsChild(rival.Name);
                rivalContainer.CreateLabelAsChild($"{(rival.IsAtWar(rival, client.Data) ? "At War" : "At Peace")}");
                rivalsContainer.AddChild(rivalContainer);
            }
        }
        
        
        var spectating = client.GetComponent<MapGraphics>()
            .SpectatingRegime;
        var localPlayerRegime = client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(client.Data);
        if (regime != spectating)
        {
            var spectateRegime = ButtonExt.GetButton(() =>
            {
                client.GetComponent<MapGraphics>().SpectateRegime(regime);
                Draw(client);
            });
            spectateRegime.Text = "Spectate Regime";
            left.AddChild(spectateRegime);
        }
        
        
        if (regime != spectating)
        {
            if (regime.IsMajor && localPlayerRegime != regime)
            {
                var chooseRegime = ButtonExt.GetButton(() =>
                {
                    var inner = new ChooseRegimeCommand(regime.MakeRef(),
                        client.Data.ClientPlayerData.LocalPlayerGuid);
                    var com = CallbackCommand.Construct(
                        inner, () =>
                        {
                            if(IsInstanceValid(this)) Draw(client);
                        }, client);
                    client.HandleCommand(com);
                });
                chooseRegime.Text = "Choose Regime";
                left.AddChild(chooseRegime);
            }
        }

        
        
        if (regime != spectating)
        {
            if (spectating.IsRivals(regime, client.Data)
                == false)
            {
                var declareRival = ButtonExt.GetButton(() =>
                {
                    var proc = new DeclareRivalProcedure(
                        spectating.MakeRef(),
                        regime.MakeRef());
                    var com = new SendMessageCommand(proc, 
                        client.Data.ClientPlayerData.LocalPlayerGuid);
                    var outer = CallbackCommand.Construct(
                        com, () =>
                        {
                            if(IsInstanceValid(this)) Draw(client);
                        }, client);
                    
                    client.Server.QueueCommandLocal(outer);
                });
                declareRival.Text = "Declare Rival";
                left.AddChild(declareRival);
            }
            else if(spectating.IsAtWar(regime, client.Data)
                    == false)
            {
                var declareRival = ButtonExt.GetButton(() =>
                {
                    var proc = new DeclareWarProcedure(
                        regime.MakeRef(),
                        spectating.MakeRef());
                    var com = new SendMessageCommand(proc, 
                        client.Data.ClientPlayerData.LocalPlayerGuid);
                    var outer = CallbackCommand.Construct(
                        com, () =>
                        {
                            if(IsInstanceValid(this)) Draw(client);
                        }, client);
                    client.Server.QueueCommandLocal(outer);
                });
                declareRival.Text = "Declare War";
                left.AddChild(declareRival);
            }
        }

        left.AddChild(new VSeparator());

        var territory = regime
            .GetCells(client.Data).ToArray();
        left.CreateLabelAsChild($"Number of cells: {territory.Count()}");
        var settlements = territory.Where(c => c.HasSettlement(client.Data))
            .Select(c => c.GetSettlement(client.Data));
        left.CreateLabelAsChild($"Number of settlements: {settlements.Count()}");
        var numBuildings = settlements.Sum(s => s.Buildings.Contents.Sum(kvp => kvp.Value));
        left.CreateLabelAsChild($"Number of buildings: {numBuildings}");

        left.AddChild(new VSeparator());
        
        var peeps = regime.GetPeeps(client.Data);
        var pop = peeps.Sum(p => p.Size);
        left.CreateLabelAsChild($"Population: {pop}");
        var urban = settlements.Sum(s => s.Cell.Get(client.Data).GetPeep(client.Data).Size);
        var rural = pop - urban;
        left.CreateLabelAsChild($"Urban: {urban}");
        left.CreateLabelAsChild($"Rural: {rural}");

        left.AddChild(new VSeparator());
        
        var units = regime.GetUnits(client.Data).ToArray();
        var armies = client.Data.GetAll<Army>()
            .Count(a => a.Regime.RefId == regime.Id);
        var totalPower = units.Sum(u => u.GetPowerPoints(client.Data));
        left.CreateLabelAsChild($"Armies: {armies}");
        left.CreateLabelAsChild($"Units: {units.Count()}");
        left.CreateLabelAsChild($"Total Military Power: {totalPower}");
        
    }
}
