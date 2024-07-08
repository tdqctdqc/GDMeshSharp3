using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class BudgetTab : ScrollContainer, IUiDrawable
{
    private Container _container, _priorityInfo;
    private RegimeOverviewWindow _parent;
    public BudgetTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Budget";
        _container = new HBoxContainer();
        AddChild(_container);
        _container.FullRect();
    }

    private BudgetTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        _container.ExpandFill();
        var small = client.Settings.SmallIconSize.Value;
        var regime = _parent.Regime;
        if (regime is null) return;
        var ais = client.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;

        var leftScroll = new ScrollContainer();
        leftScroll.ExpandFill(1);
        var left = new VBoxContainer();
        left.ExpandFill();
        leftScroll.AddChild(left);
        _container.AddChild(leftScroll);

        var priorityScroll = new ScrollContainer();
        priorityScroll.ExpandFill(1);
        _container.AddChild(priorityScroll);
        _priorityInfo = new VBoxContainer();
        _priorityInfo.ExpandFill();
        priorityScroll.AddChild(_priorityInfo);
        
        var budgetTree = new BudgetTree(budget.Root, client.Data);
        budgetTree.SelectedBudgetNode += n =>
        {
            if (n is PriorityNode p)
            {
                DrawPriorityInfo(p, client);
            }
        };
        budgetTree.ExpandFill();
        left.AddChild(budgetTree);
        
        left.CreateLabelAsChild("Prices");

        var priceScroll = new ScrollContainer();
        priceScroll.ExpandFill();
        left.AddChild(priceScroll);
        var priceContainer = new VBoxContainer();
        priceScroll.AddChild(priceContainer);
        foreach (var (model, price) in budget.Root.RelativePrices())
        {
            if (model is IIconed i)
            {
                var entry = i.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{model.Name}: {price}", small);
                priceContainer.AddChild(entry);
            }
            else
            {
                priceContainer.CreateLabelAsChild($"{model.Name}: {price}");
            }
        }
    }

    private void DrawPriorityInfo(PriorityNode node, Client c)
    {
        _priorityInfo.ClearChildren();
        var priority = node.Priority;
        if (priority == null) return;
        var regime = _parent.Regime;
        var ais = c.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        
        var wishlist = priority.GetWishlist(regime, c.Data);
        var small = c.Settings.SmallIconSize.Value;
        var med = c.Settings.MedIconSize.Value;
        
        
        _priorityInfo.CreateLabelAsChild($"Credit: {node.Credit.GetCredit()}");
        _priorityInfo.CreateLabelAsChild($"Weight: {node.GetTreeWeight(c.Data)}");
        var made = node.MadeByTick;

        var madeScroll = new ScrollContainer();
        madeScroll.ExpandFill();
        var madeBox = new VBoxContainer();
        madeScroll.AddChild(madeBox);
        _priorityInfo.AddChild(madeScroll);
        
        foreach (var (tick, tickMade) in made.OrderByDescending(kvp => kvp.Key))
        {
            madeBox.CreateLabelAsChild("Tick " + tick);
            foreach (var (model, amtMade) in tickMade.GetEnumModel(c.Data))
            {
                if (model is IIconed i)
                {
                    madeBox.AddChild(i.Icon.GetLabeledIcon<HBoxContainer>(
                        $"{model.Name}: {amtMade}",
                        med));
                }
                else
                {
                    madeBox.CreateLabelAsChild($"{model.Name}: {amtMade}");
                }
            }
        }
        
        
        _priorityInfo.CreateLabelAsChild($"Wishlist");

        var wishlistContainer = new HBoxContainer();

        var wishlistInfoScroll = new ScrollContainer();
        wishlistInfoScroll.ExpandFill();
        var wishlistInfo = new VBoxContainer();
        wishlistInfoScroll.AddChild(wishlistInfo);
        
        
        var wishlistItems = new ItemListToken<IModel>(
            wishlist.Keys,
            m => $"{m.Name}: {wishlist[m]}",
            m => { },
            m => m is IIconed i ? i.Icon.Texture : new Texture2D(),
            (int)med
        );
        wishlistContainer.AddChild(wishlistItems.ItemList);
        wishlistContainer.AddChild(wishlistInfoScroll);

        void drawWishlistItemInfo(IModel model)
        {
            wishlistInfo.ClearChildren();
            var amt = wishlist[model];
            if (model is IIconed i)
            {
                wishlistInfo.AddChild(i.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{model.Name}: {amt}",
                    med));
            }
            else
            {
                wishlistInfo.CreateLabelAsChild($"{model.Name}: {amt}");
            }
            
            wishlistInfo.CreateLabelAsChild("Costs");
            var makeable = (IMakeable)model;
            foreach (var (costModel, costAmt) in makeable.Makeable.BuildCosts.GetEnumModel(c.Data))
            {
                if (costModel is IIconed iCost)
                {
                    wishlistInfo.AddChild(iCost.Icon.GetLabeledIcon<HBoxContainer>(
                        $"{costModel.Name}: {costAmt * amt}",
                        med));
                }
                else
                {
                    wishlistInfo.CreateLabelAsChild($"{costModel.Name}: {costAmt * amt}");
                }
            }
        }
    }
}