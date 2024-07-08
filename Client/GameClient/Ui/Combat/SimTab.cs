using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class SimTab : HBoxContainer, IUiDrawable
{
    private Landform _landform;
    private Vegetation _vegetation;
    private List<UnitCombatInfo> _attackers, _defenders;
    private ItemListToken<Landform> _lf;
    private ItemListToken<Vegetation> _veg;
    private ItemListToken<Troop> _troops;
    
    private VBoxContainer _selectedTroopInfo;
    private CheckBox _chooseIfDef;
    private VBoxContainer _terrainInfo;
    
    private CombatResultsGraphic _defendersGraphic, 
        _attackersGraphic, _attackerTotalsGraphic, _defenderTotalsGraphic;
    private FloatSettingsOption _numSetting;

    public SimTab(Client c)
    {
        Name = "Simulation Settings";
        _attackers = new List<UnitCombatInfo>
        {
            new (IdCount<Troop>.Construct(), c.Data)
        };
        _defenders = new List<UnitCombatInfo>
        {
            new (IdCount<Troop>.Construct(), c.Data)
        };
        
        MakeModel(c);
        ConnectSignals();
        Arrange(c);
    }

    private void Arrange(Client c)
    {
        var chooseIfDefOuter = new HBoxContainer();
        chooseIfDefOuter.CreateLabelAsChild("Defender: ");
        chooseIfDefOuter.AddChild(_chooseIfDef);
        chooseIfDefOuter.ExpandFill();
        
        _selectedTroopInfo = new VBoxContainer();
        _selectedTroopInfo.ExpandFill();
        
        _terrainInfo = new VBoxContainer();
        _terrainInfo.ExpandFill();

        var sliderOuter = _numSetting.GetControlInterface();

        var set = ButtonExt.GetButton(SetTroop);
        set.Text = "Set";
        sliderOuter.AddChild(set);

        var calc = ButtonExt.GetButton(() => Calculate(c.Data));
        calc.Text = "Calculate";
        
        var addNewUnit = ButtonExt.GetButton(() => AddNewUnit(c.Data));
        addNewUnit.Text = "Add New Unit";
        
        var left = new VBoxContainer();
        left.ExpandFill();
        left.SizeFlagsStretchRatio = 1f;

        
        
        var middle = new TabContainer();
        middle.ExpandFill();
        middle.SizeFlagsStretchRatio = 3f;
        var byUnitTab = new HBoxContainer();
        byUnitTab.Name = "By Unit";
        byUnitTab.ExpandFill();
        var totalsTab = new HBoxContainer();
        totalsTab.Name = "Totals";
        totalsTab.ExpandFill();
        middle.AddChild(byUnitTab);
        middle.AddChild(totalsTab);

        var right = new VBoxContainer();
        right.ExpandFill();
        right.SizeFlagsStretchRatio = 1f;
        
        left.AddChild(_selectedTroopInfo);
        left.AddChild(_troops.ItemList);
        left.AddChild(chooseIfDefOuter);
        left.AddChild(sliderOuter);
        left.AddChild(addNewUnit);
        left.AddChild(calc);        
        AddChild(left);
        
        _attackersGraphic = new CombatResultsGraphic();
        _attackersGraphic.ExpandFill();
        _attackersGraphic.Draw(_attackers, c.Data);
        byUnitTab.AddChild(_attackersGraphic);
        _defendersGraphic = new CombatResultsGraphic();
        _defendersGraphic.ExpandFill();
        _attackersGraphic.Draw(_defenders, c.Data);
        byUnitTab.AddChild(_defendersGraphic);
        AddChild(middle);

        _attackerTotalsGraphic = new CombatResultsGraphic();
        _attackerTotalsGraphic.ExpandFill();
        totalsTab.AddChild(_attackerTotalsGraphic);
        _defenderTotalsGraphic = new CombatResultsGraphic();
        _defendersGraphic.ExpandFill();
        totalsTab.AddChild(_defenderTotalsGraphic);
        
        right.AddChild(_terrainInfo);
        right.AddChild(_lf.ItemList);
        right.AddChild(_veg.ItemList);
        AddChild(right);
    }

    private void AddNewUnit(Data data)
    {
        var unit = new UnitCombatInfo(IdCount<Troop>.Construct(), data);
        if (_chooseIfDef.ButtonPressed)
        {
            _defenders.Add(unit);
        }
        else
        {
            _attackers.Add(unit);
        }
        foreach (var i in _attackers)
        {
            i.ClearLossesKills();
        }
        foreach (var i in _defenders)
        {
            i.ClearLossesKills();
        }
        DrawCenter();
    }

    private void ConnectSignals()
    {
        _troops.JustSelected += v => DrawSelectedTroopInfo();
        _veg.JustSelected += v => DrawSelectedTroopInfo();
        _lf.JustSelected += v => DrawSelectedTroopInfo();
        _chooseIfDef.Pressed += DrawSelectedTroopInfo;
        _lf.JustSelected += v => DrawTerrainInfo();
        _veg.JustSelected += v => DrawTerrainInfo();
    }

    private void MakeModel(Client c)
    {
        var med = c.Settings.MedIconSize.Value;
        _chooseIfDef = new CheckBox();
        _lf = new ItemListToken<Landform>(
            c.Data.Models.GetModels<Landform>().Values,
            lf => lf.Name,
            lf => _landform = lf,
            lf => lf.GetColorTexture(med),
            (int)med
        );
        _lf.ItemList.ExpandFill();
        _lf.SelectAt(0);
        
        _veg = new ItemListToken<Vegetation>(
            c.Data.Models.GetModels<Vegetation>().Values,
            v => v.Name,
            v => _vegetation = v,
            v => v.GetColorTexture(med),
            (int)med
        );
        _veg.ItemList.ExpandFill();
        _veg.SelectAt(0);
        
        
        _troops = new ItemListToken<Troop>(
            c.Data.Models.GetModels<Troop>().Values,
            t => t.Name,
            v => { },
            t => t.Icon.Texture,
            (int)med
        );
        _troops.ItemList.ExpandFill();
        _troops.SelectAt(0);
        
        _numSetting = new FloatSettingsOption(
            "Amount", 100f, 0f, 1000f, 1f, true);
    }

    private void SetTroop()
    {
        var troop = _troops.Value;
        if (troop is null) return;
        var amt = _numSetting.Value;
        if (_chooseIfDef.ButtonPressed)
        {
            _defendersGraphic.Selected.SetInitial(troop, amt, Game.I.Client.Data);
        }
        else
        {
            _attackersGraphic.Selected.SetInitial(troop, amt, Game.I.Client.Data);
        }
        foreach (var i in _attackers)
        {
            i.ClearLossesKills();
        }
        foreach (var i in _defenders)
        {
            i.ClearLossesKills();
        }
        DrawCenter();
    }

    public void Draw(Client c)
    {
        DrawSelectedTroopInfo();
        DrawTerrainInfo();
    }

    private void Calculate(Data d)
    {
        MilUtil.CalculateCombat(_attackers.ToArray(),
            _defenders.ToArray(),
            _lf.Value, _veg.Value, d);
        DrawCenter();
    }

    private void DrawSelectedTroopInfo()
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var troop = _troops.Value;
        var def = _chooseIfDef.ButtonPressed;
        var evasionMult = MilUtil.GetEvasionMult(_lf.Value,
            _veg.Value, def);
        
        _selectedTroopInfo.ClearChildren();
        var icon = troop.Icon.GetLabeledIcon<HBoxContainer>(
            $"{troop.Name}", large);
        _selectedTroopInfo.AddChild(icon);

        _selectedTroopInfo.CreateLabelAsChild
            ($"HP: {troop.Hitpoints}");
        _selectedTroopInfo.CreateLabelAsChild
            ($"Soft attack: {troop.SoftAttack}");
        _selectedTroopInfo.CreateLabelAsChild
            ($"Hardness: {troop.Hardness}");
        _selectedTroopInfo.CreateLabelAsChild
            ($"Accuracy: {troop.Accuracy}");
        _selectedTroopInfo.CreateLabelAsChild
            ($"Echelon: {troop.Echelon}");
        
        _selectedTroopInfo.CreateLabelAsChild($"Base evasion: {troop.Evasion}");
        _selectedTroopInfo.CreateLabelAsChild($"Evasion mult: {evasionMult}");
        _selectedTroopInfo.CreateLabelAsChild($"Evasion chance: {troop.Evasion * evasionMult}");
    }

    private void DrawTerrainInfo()
    {
        _terrainInfo.ClearChildren();

        _terrainInfo.CreateLabelAsChild
            ($"Front length: {MilUtil.BaseFrontLength * _lf.Value.FrontLengthMult * _veg.Value.FrontLengthMult} ");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_lf.Value.MovementCostMult * _veg.Value.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_lf.Value.EvasionMult * _veg.Value.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Landform: {_lf.Value.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {_lf.Value.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_lf.Value.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_lf.Value.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Vegetation: {_veg.Value.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {_veg.Value.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_veg.Value.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_veg.Value.EvasionMult}");
    }


    private void DrawCenter()
    {
        _attackersGraphic.Draw(_attackers, Game.I.Client.Data);
        _defendersGraphic.Draw(_defenders, Game.I.Client.Data);

        var attackerTotals = UnitCombatInfo.Sum(_attackers, Game.I.Client.Data);
        var defenderTotals = UnitCombatInfo.Sum(_defenders, Game.I.Client.Data);
        _attackerTotalsGraphic.Draw(attackerTotals.Yield(), Game.I.Client.Data);
        _defenderTotalsGraphic.Draw(defenderTotals.Yield(), Game.I.Client.Data);
    }
    
}