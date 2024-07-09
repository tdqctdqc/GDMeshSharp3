namespace Ui.RegimeOverview;

public partial class EconomyTab : DrawTabContainer
{
    public EconomyTab(RegimeOverviewWindow parent, Client client) : base(client)
    {
        Name = "Economy";
        this.FullRect();
        AddTab(new StockTab(parent));
        AddTab(new FoodTab(parent));
        AddTab(new MakingTab(parent));
    }
    
}