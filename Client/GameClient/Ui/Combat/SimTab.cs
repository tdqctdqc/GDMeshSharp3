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
    private ItemListToken<UnitTemplate> _templates;
    
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
        // chooseIfDefOuter.ExpandFill();
        
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
        
        var addNewUnit = ButtonExt.GetButton(
            () => AddNewUnit(new UnitCombatInfo(IdCount<Troop>.Construct(), c.Data),
                c.Data));
        addNewUnit.Text = "Add New Unit";

        var removeUnit = ButtonExt.GetButton(() => RemoveUnit(c));
        removeUnit.Text = "Remove Unit";

        var addTemplate = ButtonExt.GetButton(() => AddTemplate(c));
        addTemplate.Text = "Add Template";
        
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
        left.AddChild(chooseIfDefOuter);

        left.AddChild(_templates.ItemList);
        left.AddChild(addTemplate);
        left.AddChild(_troops.ItemList);
        left.AddChild(sliderOuter);
        left.AddChild(addNewUnit);
        left.AddChild(removeUnit);
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

    private void AddTemplate(Client client)
    {
        if (_templates.Selected.Count != 1) return;
        var template = _templates.Selected.First();
        var u = new UnitCombatInfo(template.Troops, client.Data);
        AddNewUnit(u, client.Data);
    }

    private void RemoveUnit(Client client)
    {
        var (graphic, list) = _chooseIfDef.ButtonPressed
            ? (_defendersGraphic, _defenders)
            : (_attackersGraphic, _attackers);
        var unit = graphic.Selected;
        if (unit is null) return;
        list.Remove(unit);
        foreach (var i in _attackers)
        {
            i.ClearLossesKills(Game.I.Client.Data);
        }
        foreach (var i in _defenders)
        {
            i.ClearLossesKills(Game.I.Client.Data);
        }
        DrawCenter();
    }

    private void AddNewUnit(UnitCombatInfo unit,
        Data data)
    {
        // var unit = new UnitCombatInfo(IdCount<Troop>.Construct(), data);
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
            i.ClearLossesKills(Game.I.Client.Data);
        }
        foreach (var i in _defenders)
        {
            i.ClearLossesKills(Game.I.Client.Data);
        }
        DrawCenter();
    }

    private void ConnectSignals()
    {
        _troops.JustSelected += DrawSelectedTroopInfo;
        _veg.JustSelected += DrawSelectedTroopInfo;
        _lf.JustSelected += DrawSelectedTroopInfo;
        _chooseIfDef.Pressed += DrawSelectedTroopInfo;
        _lf.JustSelected += DrawTerrainInfo;
        _veg.JustSelected += DrawTerrainInfo;
    }

    private void MakeModel(Client c)
    {
        var med = c.Settings.MedIconSize.Value;
        _chooseIfDef = new CheckBox();
        _lf = new ItemListToken<Landform>(
            c.Data.Models.GetModels<Landform>(),
            lf => lf.Name,
            lf => lf.GetColorTexture(med),
            (int)med,
            false
        );
        _lf.JustSelected += () => _landform = _lf.Selected.First();
        _lf.ItemList.ExpandFill();
        _lf.SelectAt(0);
        
        _veg = new ItemListToken<Vegetation>(
            c.Data.Models.GetModels<Vegetation>(),
            v => v.Name,
            v => v.GetColorTexture(med),
            (int)med,
            false
        );
        _veg.JustSelected += () => _vegetation = _veg.Selected.First();
        _veg.ItemList.ExpandFill();
        _veg.SelectAt(0);
        
        
        _troops = new ItemListToken<Troop>(
            c.Data.Models.GetModels<Troop>(),
            t => t.DisplayName,
            t => t.Icon.Texture,
            (int)med,
            false
        );
        _troops.ItemList.ExpandFill();
        _troops.SelectAt(0);
        
        _numSetting = new FloatSettingsOption(
            "Amount", 100f, 0f, 1000f, 1f, true);

        var regime = c.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(c.Data);
        _templates = new ItemListToken<UnitTemplate>(
            regime.GetUnitTemplates(c.Data),
            t => $"{t.Name}",
            t => t.GetMaxPowerTroop(c.Data).Icon.Texture,
            (int)med,
            false
        );
        _templates.ItemList.ExpandFill();
        _templates.SelectAt(0);
    }

    private void SetTroop()
    {
        if (_troops.Selected.Count != 1) return;
        var troop = _troops.Selected.First();
        var amt = _numSetting.Value;
        var unit = _chooseIfDef.ButtonPressed
            ? _defendersGraphic.Selected
            : _attackersGraphic.Selected;
        if (unit is null) return;
        unit.SetInitial(troop, amt, Game.I.Client.Data);

        foreach (var i in _attackers)
        {
            i.ClearLossesKills(Game.I.Client.Data);
        }
        foreach (var i in _defenders)
        {
            i.ClearLossesKills(Game.I.Client.Data);
        }
        DrawCenter();
    }

    public void Draw(Client client)
    {
        DrawSelectedTroopInfo();
        DrawTerrainInfo();
    }

    private void Calculate(Data d)
    {
        MilUtil.CalculateCombat(_attackers.ToArray(),
            _defenders.ToArray(),
            _lf.Selected.First(), _veg.Selected.First(), d);
        DrawCenter();
    }

    private void DrawSelectedTroopInfo()
    {
        _selectedTroopInfo.ClearChildren();
        if (_troops.Selected.Count != 1) return;
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var troop = _troops.Selected.First();
        var def = _chooseIfDef.ButtonPressed;

        var friendlies = def ? _defenders : _attackers;
        var targets = def ? _attackers : _defenders;
        var evasionMult = MilUtil.GetEvasionMult(_lf.Selected.First(),
            _veg.Selected.First(), def);
        
        var icon = troop.Icon.GetLabeledIcon<HBoxContainer>(
            $"{troop.DisplayName}", large);
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
        var echelonChances = MilUtil.GetEchelonChances(
            troop, _landform, _vegetation, targets.ToArray(),
            friendlies.ToArray());
        var totalChance = echelonChances.Sum();
        _selectedTroopInfo.CreateLabelAsChild($"Target Echelon Chances: ");
        for (var i = 0; i < echelonChances.Length; i++)
        {
            _selectedTroopInfo.CreateLabelAsChild(
                $"Echelon {i}: {echelonChances[i] / totalChance}");
        }
        
        _selectedTroopInfo.CreateLabelAsChild($"Base evasion: {troop.Evasion}");
        _selectedTroopInfo.CreateLabelAsChild($"Evasion mult: {evasionMult}");
        _selectedTroopInfo.CreateLabelAsChild($"Evasion chance: {troop.Evasion * evasionMult}");
    }

    private void DrawTerrainInfo()
    {
        _terrainInfo.ClearChildren();
        var lf = _lf.Selected.First();
        var veg = _veg.Selected.First();
        _terrainInfo.CreateLabelAsChild
            ($"Front length: {MilUtil.BaseFrontLength * lf.FrontLengthMult * veg.FrontLengthMult} ");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {lf.MovementCostMult * veg.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {lf.EvasionMult * veg.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Landform: {lf.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {lf.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {lf.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {lf.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Vegetation: {veg.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {veg.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {veg.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {veg.EvasionMult}");
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