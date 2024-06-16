using System;
using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class ArmiesTab : HBoxContainer, IUiDrawable
{
    private global::MilitaryWindow _parent;
    private VBoxContainer _armyInfoContainer, 
        _armiesContainer, _freeUnitsContainer;

    private ItemMultiListToken<Unit> _freeUnits;
    private ArmyTree _armyTree;
    public ArmiesTab(global::MilitaryWindow parent)
    {
        _parent = parent;
        
        _armyInfoContainer = new VBoxContainer();
        _armyInfoContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        _armyInfoContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_armyInfoContainer);

        _armiesContainer = new VBoxContainer();
        _armiesContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        _armiesContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_armiesContainer);
        
        _freeUnitsContainer = new VBoxContainer();
        _freeUnitsContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        _freeUnitsContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_freeUnitsContainer);
    }

    private ArmiesTab()
    {
        
    }


    public void Draw(Client c)
    {
        _armiesContainer.ClearChildren();
        _armyInfoContainer.ClearChildren();
        _freeUnitsContainer.ClearChildren();
        
        var r = _parent.Regime;
        if (r is null) return;
        
        
        
        var armies = c.Data.GetAll<Army>()
            .Where(a => a.Regime.RefId == r.Id);
        
        var armiesToken =  new ItemListToken<Army>(
            armies, 
            a => a.Id.ToString(),
            a => DrawArmyInfo(a, c),
            Vector2.One * 20f
        );
        armiesToken.ItemList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        armiesToken.ItemList.SizeFlagsVertical = SizeFlags.ExpandFill;
        _armiesContainer.AddChild(armiesToken.ItemList);
        
        
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
            var army = armiesToken.Selected;
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
        _freeUnits.ItemList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _freeUnits.ItemList.SizeFlagsVertical = SizeFlags.ExpandFill;
        _freeUnitsContainer.AddChild(transferFreeUnitBtn);
        _freeUnitsContainer.AddChild(_freeUnits.ItemList);
    }

    private void DrawArmyInfo(Army a, 
        Client c)
    {
        _armyInfoContainer.ClearChildren();
        if (a is null) return;
        
        _armyTree = a.GetTree(c.Data);
        _armyTree.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _armyTree.SizeFlagsVertical = SizeFlags.ExpandFill;
        
        
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
        
        
        _armyInfoContainer.AddChild(sendToReserve);
        _armyInfoContainer.AddChild(_armyTree);
    }
    
}