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

    private ItemMultiListToken<Unit> _freeUnits;
    private ItemListToken<Army> _armies;
    private ArmyTree _armyTree;
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
    

    public void Draw(Client c)
    {
        _armiesContainer.ClearChildren();
        _armyButtonsContainer.ClearChildren();
        _armyInfoContainer.ClearChildren();
        _freeUnitsContainer.ClearChildren();

        var r = _getRegime();
        if (r is null) return;
        
        var armies = c.Data.GetAll<Army>()
            .Where(a => a.Regime.RefId == r.Id);
        
        _armies =  new ItemListToken<Army>(
            armies, 
            a => a.Id.ToString(),
            a => DrawArmyInfo(a, c),
            a => a.Regime.Get(c.Data).Template.Get(c.Data).Flag.Texture
        );
        _armies.ItemList.ExpandFill();
        _armiesContainer.AddChild(_armies.ItemList);
        
        
        _freeUnits = new ItemMultiListToken<Unit>(
            r
                .GetUnits(c.Data)
                .Where(u => c.Data.Military.UnitAux.UnitByGroup[u] == null),
            u => u.Template.Get(c.Data).Name,
            u => { },
            new Vector2(200f, 500f),
            u => u.GetMaxPowerTroop(c.Data).Icon.Texture,
            new Vector2I(20, 20)
        );
        var transferFreeUnitBtn = ButtonExt.GetButton(() =>
        {
            if (_freeUnits.Selected.Count == 0) return;
            var army = _armies.Value;
            if (army is null) return;
            foreach (var unit in _freeUnits.Selected)
            {
                var proc = new SetUnitArmyProcedure(unit.MakeRef(),
                    army.MakeRef());
                var inner = new SendMessageCommand(proc, 
                    c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
                
                var action = () =>
                {
                    if (IsInstanceValid(_freeUnits.ItemList) == false
                        || IsInstanceValid(_armyTree) == false)
                    {
                        return;
                    }
                    if (unit.GetArmy(c.Data) is not null)
                    {
                        _freeUnits.Remove(unit.Yield());
                    }
                    if (_armyTree.GetArmy(c.Data) == army
                        && army.Units.Contains(unit))
                    {
                        _armyTree.AddUnit(unit, c.Data);
                    }
                };
                var com = CallbackCommand.Construct(
                    inner, action, c);
                c.HandleCommand(com);
            }
        });
        transferFreeUnitBtn.Text = "Transfer To Army";
        _freeUnits.ItemList.ExpandFill();
        _freeUnitsContainer.AddChild(transferFreeUnitBtn);
        _freeUnitsContainer.AddChild(_freeUnits.ItemList);
    }

    public void SelectArmy(Army a)
    {
        _armies.Select(a);
    }

    private void DrawArmyInfo(Army a, 
        Client c)
    {
        _armyInfoContainer.ClearChildren();
        _armyButtonsContainer.ClearChildren();
        if (a is null) return;
        
        _armyTree = a.GetTree(0, c.Data);
        _armyTree.ExpandFill();
        
        var sendToReserve = ButtonExt.GetButton(() =>
        {
            var selected = _armyTree.GetSelectedUnit(c.Data);
            
            if (selected is Unit u)
            {
                var proc = new SetUnitArmyProcedure(u.MakeRef(),
                    ERef<Army>.GetEmpty());
                var com = new SendMessageCommand(proc, 
                    c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
                c.HandleCommand(com);
                _armyTree.RemoveUnit(u);
                _freeUnits.Add(u);
            }
        });
        sendToReserve.Text = "Send to Reserve";

        var reinforceUnit = ButtonExt.GetButton(() =>
        {
            var unit = _armyTree.GetSelectedUnit(c.Data);
            if (unit is null) return;
            var proc = new ReinforceUnitProcedure(unit.MakeRef());
            var com = new SendMessageCommand(proc, c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        SelectArmy(a);
                    }
                }, c);
            c.HandleCommand(outer);
        });
        reinforceUnit.Text = "Reinforce Unit";
        
        var reinforceArmy = ButtonExt.GetButton(() =>
        {
            var proc = new ReinforceArmyProcedure(a.MakeRef());
            var com = new SendMessageCommand(proc, c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(
                com, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        SelectArmy(a);
                    }
                }, c);
            c.HandleCommand(outer);
        });
        reinforceArmy.Text = "Reinforce Army";
        
        
        _armyInfoContainer.AddChild(_armyTree);
        _armyButtonsContainer.AddChild(sendToReserve);
        _armyButtonsContainer.AddChild(reinforceUnit);
        _armyButtonsContainer.AddChild(reinforceArmy);
    }
    
}