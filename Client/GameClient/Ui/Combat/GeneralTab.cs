using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class GeneralTab : HBoxContainer, IUiDrawable
{
    private CombatInfo _info;
    private VBoxContainer _terrainInfo;
    private CombatResultsGraphic _defendersGraphic, 
        _attackersGraphic, _attackerTotalsGraphic, _defenderTotalsGraphic;

    public GeneralTab(CombatInfo info, Client c)
    {
        Name = "General";
        _info = info;
        Arrange(c);
    }

    private void Arrange(Client c)
    {
        _terrainInfo = new VBoxContainer();
        _terrainInfo.ExpandFill();

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
        
        _attackersGraphic = new CombatResultsGraphic();
        _attackersGraphic.ExpandFill();
        byUnitTab.AddChild(_attackersGraphic);
        _defendersGraphic = new CombatResultsGraphic();
        _defendersGraphic.ExpandFill();
        byUnitTab.AddChild(_defendersGraphic);
        AddChild(middle);

        _attackerTotalsGraphic = new CombatResultsGraphic();
        _attackerTotalsGraphic.ExpandFill();
        totalsTab.AddChild(_attackerTotalsGraphic);
        _defenderTotalsGraphic = new CombatResultsGraphic();
        _defendersGraphic.ExpandFill();
        totalsTab.AddChild(_defenderTotalsGraphic);
        
        right.AddChild(_terrainInfo);
        AddChild(right);
    }

    

    public void Draw(Client client)
    {
        
        DrawTerrainInfo();
        DrawCenter();
    }
    private void DrawTerrainInfo()
    {
        _terrainInfo.ClearChildren();

        _terrainInfo.CreateLabelAsChild
            ($"Front length: {MilUtil.BaseFrontLength * _info.Landform.FrontLengthMult * _info.Vegetation.FrontLengthMult} ");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_info.Landform.MovementCostMult * _info.Vegetation.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_info.Landform.EvasionMult * _info.Vegetation.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Landform: {_info.Landform.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {_info.Landform.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_info.Landform.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_info.Landform.EvasionMult}");
        
        _terrainInfo.CreateLabelAsChild
            ($"Vegetation: {_info.Vegetation.Name}");
        _terrainInfo.CreateLabelAsChild
            ($"Front length mult: {_info.Vegetation.FrontLengthMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Movement mult: {_info.Vegetation.MovementCostMult}");
        _terrainInfo.CreateLabelAsChild
            ($"Evasion mult: {_info.Vegetation.EvasionMult}");
    }


    private void DrawCenter()
    {
        _attackersGraphic.Draw(_info.Attackers, Game.I.Client.Data);
        _defendersGraphic.Draw(_info.Defenders, Game.I.Client.Data);

        var attackerTotals = UnitCombatInfo.Sum(_info.Attackers, Game.I.Client.Data);
        var defenderTotals = UnitCombatInfo.Sum(_info.Defenders, Game.I.Client.Data);
        _attackerTotalsGraphic.Draw(attackerTotals.Yield(), Game.I.Client.Data);
        _defenderTotalsGraphic.Draw(defenderTotals.Yield(), Game.I.Client.Data);
    }
    
}