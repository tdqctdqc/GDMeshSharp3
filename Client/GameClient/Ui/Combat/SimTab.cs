using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class SimTab : HBoxContainer, IUiDrawable
{
    private Landform _landform;
    private Vegetation _vegetation;
    private UnitCombatInfo _attackers, _defenders;
    private ItemListToken<Landform> _lf;
    private ItemListToken<Vegetation> _veg;
    private ItemListToken<Troop> _troops;
    
    private VBoxContainer _selectedTroopInfo;
    private CheckBox _chooseIfDef;
    private VBoxContainer _terrainInfo;
    private CombatResultsGraphic _defendersGraphic;
    private CombatResultsGraphic _attackersGraphic;
    private FloatSettingsOption _numSetting;

    public SimTab(Client c)
    {
        Name = "Simulation Settings";
        _attackers = new UnitCombatInfo(IdCount<Troop>.Construct(), c.Data);
        _defenders = new UnitCombatInfo(IdCount<Troop>.Construct(), c.Data);
        
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
        sliderOuter.ExpandFill();

        var set = ButtonExt.GetButton(SetTroop);
        set.Text = "Set";
        sliderOuter.AddChild(set);

        var calc = ButtonExt.GetButton(() => Calculate(c.Data));
        calc.Text = "Calculate";
        
        var left = new VBoxContainer();
        left.ExpandFill();
        left.SizeFlagsStretchRatio = 1f;

        var middle = new HBoxContainer();
        middle.ExpandFill();
        middle.SizeFlagsStretchRatio = 3f;

        var right = new VBoxContainer();
        right.ExpandFill();
        right.SizeFlagsStretchRatio = 1f;
        
        left.AddChild(_selectedTroopInfo);
        left.AddChild(_troops.ItemList);
        left.AddChild(chooseIfDefOuter);
        left.AddChild(sliderOuter);
        left.AddChild(calc);        
        AddChild(left);

        _attackersGraphic = new CombatResultsGraphic();
        _attackersGraphic.ExpandFill();
        middle.AddChild(_attackersGraphic);
        _defendersGraphic = new CombatResultsGraphic();
        _defendersGraphic.ExpandFill();
        middle.AddChild(_defendersGraphic);
        AddChild(middle);
        
        right.AddChild(_terrainInfo);
        right.AddChild(_lf.ItemList);
        right.AddChild(_veg.ItemList);
        AddChild(right);
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
            new Vector2(100f, 100f),
            lf => lf.GetColorTexture(med),
            (int)med * Vector2I.One
        );
        _lf.ItemList.ExpandFill();
        _lf.SelectAt(0);
        
        _veg = new ItemListToken<Vegetation>(
            c.Data.Models.GetModels<Vegetation>().Values,
            v => v.Name,
            v => _vegetation = v,
            new Vector2(100f, 100f),
            v => v.GetColorTexture(med),
            (int)med * Vector2I.One
        );
        _veg.ItemList.ExpandFill();
        _veg.SelectAt(0);
        
        
        _troops = new ItemListToken<Troop>(
            c.Data.Models.GetModels<Troop>().Values,
            t => t.Name,
            v => { },
            med * Vector2.One,
            t => t.Icon.Texture,
            (int)med * Vector2I.One
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
            _defenders.SetInitial(troop, amt, Game.I.Client.Data);
            _attackers.ClearLossesKills();
        }
        else
        {
            _attackers.SetInitial(troop, amt, Game.I.Client.Data);
            _defenders.ClearLossesKills();
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
        MilUtil.CalculateCombat(_attackers.Yield().ToArray(),
            _defenders.Yield().ToArray(),
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
        _attackersGraphic.Draw(_attackers.Yield(), Game.I.Client.Data);
        _defendersGraphic.Draw(_defenders.Yield(), Game.I.Client.Data);
    }
    
}