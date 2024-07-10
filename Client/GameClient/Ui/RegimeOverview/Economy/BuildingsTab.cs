using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class BuildingsTab : ScrollContainer, IUiDrawable
{
    private RegimeOverviewWindow _parent;
    private Container _container, _info;
    private ItemListToken<Settlement> _settlementList;
    private ItemListToken<SettlementBuildingModel> _settlementBuildingList;
    private ItemListToken<ResourceExtractionBuilding> _resourceExtractionList;
    
    public BuildingsTab(RegimeOverviewWindow parent)
    {
        this.ExpandFill();
        Name = "Buildings";
        _parent = parent;
        _container = new HBoxContainer();
        _container.FullRect();
        _container.ExpandFill();
        AddChild(_container);
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var med = client.Settings.MedIconSize.Value;
        var left = new VBoxContainer();
        left.ExpandFill(1);
        _container.AddChild(left);
        _info = _container.MakeScrollChild<VBoxContainer>(
            out var infoScroll);
        infoScroll.ExpandFill(3);
        _info.ExpandFill();
        var regime = _parent.Regime;
        var cells = regime.GetCells(client.Data);
        var settlements = cells
            .Where(c => c.HasSettlement(client.Data))
            .Select(c => c.GetSettlement(client.Data));
        var settlementBuildings = settlements
            .SelectMany(s => s.Buildings.GetEnumModel(client.Data))
            .SortInto(v => v.Key, v => v.Value);
        
        var resourceDeposits = cells
            .Where(c => c.HasResourceDeposit(client.Data))
            .Select(c => c.GetResourceDeposit(client.Data));
        var resourceExtractions = resourceDeposits
            .Where(rd => rd.Extraction.Fulfilled())
            .Select(rd => rd.Extraction.Get(client.Data))
            .SortInto(rx => rx, rx => 1f);



        _settlementList = new ItemListToken<Settlement>(
            settlements,
            s => $"{s.Name} Population: {s.Cell.Get(client.Data).GetPeep(client.Data).Size}",
            s => s.Tier.Get(client.Data).Icon.Texture,
            (int)med,
            false
        );
        _settlementList.JustSelected += () => DrawSettlementInfo(client);
        _settlementList.ItemList.ExpandFill();
        left.CreateLabelAsChild("Settlements");
        left.AddChild(_settlementList.ItemList);

        _settlementBuildingList = new ItemListToken<SettlementBuildingModel>(
            settlementBuildings.Keys,
            m => $"{m.Name}: {settlementBuildings[m]}",
            s => s.Icon.Texture,
            (int)med,
            false
        );
        _settlementBuildingList.JustSelected += () => DrawSettlementBuildingInfo(client);
        _settlementBuildingList.ItemList.ExpandFill();
        left.CreateLabelAsChild("Settlement Buildings");
        left.AddChild(_settlementBuildingList.ItemList);
        
        _resourceExtractionList = new ItemListToken<ResourceExtractionBuilding>(
            resourceExtractions.Keys,
            m => $"{m.Name}: {resourceExtractions[m]}",
            s => s.Icon.Texture,
            (int)med,
            
            false
        );
        _resourceExtractionList.JustSelected += () => DrawResourceExtractionBuildingInfo(client);
        _resourceExtractionList.ItemList.ExpandFill();
        left.CreateLabelAsChild("Resource Extraction Buildings");
        left.AddChild(_resourceExtractionList.ItemList);
    }

    private void DrawSettlementInfo(Client c)
    {
        
    }
    
    private void DrawSettlementBuildingInfo(Client c)
    {
        
    }
    
    private void DrawResourceExtractionBuildingInfo(Client c)
    {
        
    }
}