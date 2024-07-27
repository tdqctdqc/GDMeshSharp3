using System;
using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class ArmiesTab : HBoxContainer, IUiDrawable
{
    private Func<Regime> _getRegime;
    private VBoxContainer 
        _armyButtonsContainer,
        _armyInfoContainer, 
        _armiesContainer, _freeUnitsContainer;

    private Tree _freeUnits;
    private ItemListToken<Army> _armies;
    private ArmyTree _armyTree;
    private Label _freeUnitsLabel;
    public ArmiesTab(Func<Regime> getRegime)
    {
        Name = "Armies";
        _getRegime = getRegime;
        var left = new VBoxContainer();
        left.ExpandFill();
        AddChild(left);
        
        _armyInfoContainer = new VBoxContainer();
        _armyInfoContainer.ExpandFill(3);
        left.AddChild(_armyInfoContainer);

        _armyButtonsContainer = new VBoxContainer();
        _armyButtonsContainer.ExpandFill(1);
        left.AddChild(_armyButtonsContainer);
        
        _armiesContainer = new VBoxContainer();
        _armiesContainer.ExpandFill();
        AddChild(_armiesContainer);
        
        _freeUnitsContainer = new VBoxContainer();
        _freeUnitsContainer.ExpandFill();
        AddChild(_freeUnitsContainer);
    }

    private ArmiesTab()
    {
        
    }
    

    public void Draw(Client client)
    {
        _armiesContainer.ClearChildren();
        _armyButtonsContainer.ClearChildren();
        _armyInfoContainer.ClearChildren();
        _freeUnitsContainer.ClearChildren();

        var r = _getRegime();
        if (r is null) return;
        
        var armies = client.Data.GetAll<Army>()
            .Where(a => a.Regime.RefId == r.Id);
        var med = client.Settings.MedIconSize.Value;
        
        _armies =  new ItemListToken<Army>(
            armies, 
            a => $"Army {a.Id.ToString()} Units: {a.Units.Count()} " +
                 $"Strength: {a.GetPowerPoints(client.Data)}",
            a => a.Regime.Get(client.Data).Template.Get(client.Data).Flag.Texture,
            (int)med, 
            false
        );

        _armies.JustSelected += () => DrawArmyInfo(client);
        _armiesContainer.CreateLabelAsChild("Armies");
        _armiesContainer.AddChild(_armies.ItemList);
        _armies.ItemList.ExpandFill();
        
        _freeUnits = new UnitTree(0);
        _freeUnits.ExpandFill();
        _freeUnits.SelectMode = Tree.SelectModeEnum.Multi;
        var freeUnits = r
            .GetUnits(client.Data)
            .Where(u => client.Data.Military.UnitAux.UnitByGroup[u] == null);
        UnitTree.Add(_freeUnits, 
            freeUnits,
            0,
            client
        );
        _freeUnitsLabel = new Label();
        _freeUnitsLabel.Text = $"Available Units: {freeUnits.Count()}";
        var transferFreeUnitBtn = ButtonExt.GetButton(() =>
        {
            var selected = _freeUnits.GetSelectedEntities<Unit>(client.Data);
            if (selected.Count == 0) return;
            var army = _armies.Selected.Count == 1
                ? _armies.Selected.First() : null;
            if (army is null) return;
            foreach (var unit in selected)
            {
                var proc = new SetUnitArmyProcedure(unit.MakeRef(),
                    army.MakeRef());
                var inner = new SendMessageCommand(proc, 
                    client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
                
                var callback = () =>
                {
                    if (IsInstanceValid(this) == false)
                    {
                        return;
                    }
                    if (unit.GetArmy(client.Data) is not null)
                    {
                        _freeUnits.Remove(unit);
                    }
                    if (_armyTree.GetArmy(client.Data) == army
                        && army.Units.Contains(unit))
                    {
                        _armyTree.AddUnit(unit, client);
                    }
                    _armies.RefreshText();
                    SetFreeUnitsLabel(client);
                };
                var com = CallbackCommand.Construct(
                    inner, callback, client);
                client.HandleCommand(com);
            }
        });
        transferFreeUnitBtn.Text = "Transfer To Army";
        _freeUnitsContainer.AddChild(_freeUnitsLabel);
        _freeUnitsContainer.AddChild(_freeUnits);
        _freeUnitsContainer.AddChild(transferFreeUnitBtn);
        
    }

    public void SelectArmy(Army a)
    {
        _armies.Select(a);
    }

    private void DrawArmyInfo(Client c)
    {
        _armyInfoContainer.ClearChildren();
        _armyButtonsContainer.ClearChildren();
        if (_armies.Selected.Count != 1) return;
        var a = _armies.Selected.First();
        
        _armyTree = a.GetTree(0, c);
        _armyTree.SelectMode = Tree.SelectModeEnum.Multi;
        _armyTree.ExpandFill();
        
        var sendToReserve = ButtonExt.GetButton(() =>
        {
            var selected = _armyTree.GetSelectedEntities<Unit>(c.Data);

            var procs = selected.Select(u => new SetUnitArmyProcedure(u.MakeRef(),
                ERef<Army>.GetEmpty())).ToArray();
            var proc = new AggregateProcedure(procs);
            var com = new SendMessageCommand(proc, 
                c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this) == false) return;
                    foreach (var u in selected)
                    {
                        _armyTree.Remove(u);
                        UnitTree.Add(_freeUnits, u, 0, c);
                    }
                    _armies.RefreshText();
                    SetFreeUnitsLabel(c);
                }, c);
            c.HandleCommand(outer);
        });
        sendToReserve.Text = "Send to Reserve";

        var reinforceUnit = ButtonExt.GetButton(() =>
        {
            var selected = _armyTree.GetSelectedEntities<Unit>(c.Data);
            var proc = MilUtil.GetReinforceProc(_getRegime(), selected, c.Data);
            var com = new SendMessageCommand(proc, 
                c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        SelectArmy(a);
                        _armies.RefreshText();
                    }
                }, c);
            c.HandleCommand(outer);
        });
        reinforceUnit.Text = "Reinforce Unit";
        
        var reinforceArmy = ButtonExt.GetButton(() =>
        {
            var proc = MilUtil.GetReinforceProc(_getRegime(),
                a.Units.Entities(c.Data), c.Data);
            var com = new SendMessageCommand(proc, c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        SelectArmy(a);
                        _armies.RefreshText();
                    }
                }, c);
            c.HandleCommand(outer);
        });
        reinforceArmy.Text = "Reinforce Army";



        var reinforceTroop = ButtonExt.GetButton(() =>
        {
            var v = _armyTree.GetSelectedTroopAndUnit(c.Data);
            if (v.HasValue == false) return;
            var (u, t) = v.Value;
            var proc = MilUtil.GetReinforceProc(_getRegime(),
                u.Yield(), c.Data);
            var com = new SendMessageCommand(proc,
                c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        SelectArmy(a);                       
                        _armies.RefreshText();
                    }
                }, c);
            c.HandleCommand(outer);
        });
        reinforceTroop.Text = "Reinforce Troop";
        
        _armyInfoContainer.AddChild(_armyTree);
        _armyButtonsContainer.AddChild(sendToReserve);
        _armyButtonsContainer.AddChild(reinforceUnit);
        _armyButtonsContainer.AddChild(reinforceArmy);
    }

    private void SetFreeUnitsLabel(Client c)
    {
        var freeUnits = _getRegime()
            .GetUnits(c.Data)
            .Where(u => c.Data.Military.UnitAux.UnitByGroup[u] == null);
        _freeUnitsLabel.Text = $"Available Units: {freeUnits.Count()}";
    }
    
}