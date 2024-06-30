
using System;
using System.Linq;
using Godot;

public partial class FillArmyWindow : Window
{
    private VBoxContainer _inner;
    
    private FillArmyWindow()
    {
        this.MakeFreeable();
        Size = new Vector2I(500, 800);
        _inner = this.MakeWholeWindowScrollContainer<VBoxContainer>(
            new Vector2I(500, 800));
    }

    public static FillArmyWindow Get(Army army, Client c)
    {
        var w = new FillArmyWindow();
        w.Setup(army, c);
        return w;
    }

    public void Setup(Army army, Client c)
    {
        _inner.ClearChildren();

        var regime = army.Regime.Get(c.Data);
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer;
        var playerRegime = player
            .Regime.Get(c.Data);
        if (playerRegime == null) return;
        var playerAlliance = playerRegime.GetAlliance(c.Data);
        if (regime.GetAlliance(c.Data) != playerAlliance) return;

        var btns = new HBoxContainer();
        
        _inner.AddChild(btns);
        
        var hbox = new HBoxContainer();
        _inner.AddChild(hbox);

        var left = new VBoxContainer();
        hbox.AddChild(left);
        left.CreateLabelAsChild("In Reserve");
        var reserveUnits = new ItemMultiListToken<Unit>(
            regime
                    .GetUnits(c.Data)
                    .Where(u => c.Data.Military.UnitAux.UnitByGroup[u] == null),
            u => u.Template.Get(c.Data).Name,
            u => { },
            new Vector2(200f, 500f),
            u => u.GetMaxPowerTroop(c.Data).Icon.Texture,
            new Vector2I(20, 20)
        );
        left.AddChild(reserveUnits.ItemList);
        
        var right = new VBoxContainer();
        right.CreateLabelAsChild("In Army");
        hbox.AddChild(right);
        var armyUnits = new ItemMultiListToken<Unit>(
            army.Units.Entities(c.Data),
            u => u.Template.Get(c.Data).Name,
            u => { },
            new Vector2(200f, 500f),
            u => u.GetMaxPowerTroop(c.Data).Icon.Texture,
            new Vector2I(20, 20)
        );
        armyUnits.ItemList.SelectMode = ItemList.SelectModeEnum.Multi;
        right.AddChild(armyUnits.ItemList);
        
        var takeToArmy = ButtonExt.GetButton(() =>
        {
            if (reserveUnits.Selected.Count == 0) return;
            var procs = reserveUnits.Selected
                .Select(u => new SetUnitArmyProcedure(u.MakeRef(),
                    army.MakeRef())).ToArray<Message>();
            var inner = new SendMessagesCommand(procs, player.PlayerGuid);
            var cb = CallbackCommand.Construct(
                inner,
                () =>
                {
                    if (IsInstanceValid(this) == false
                        || this.Visible == false)
                    {
                        return;
                    }
                    Setup(army, c);
                },
                c);
            c.HandleCommand(cb);
        });
        takeToArmy.Text = "Take to Army";
        
        var sendToReserve = ButtonExt.GetButton(() =>
        {
            if (armyUnits.Selected.Count == 0) return;

            var procs = armyUnits.Selected
                .Select(u => new SetUnitArmyProcedure(u.MakeRef(),
                    ERef<Army>.GetEmpty())).ToArray<Message>();
            var inner = new SendMessagesCommand(procs, player.PlayerGuid);
            var cb = () => Setup(army, c);
            var com = CallbackCommand.Construct(inner, cb, c);
            c.HandleCommand(com);
        });
        sendToReserve.Text = "Send to Reserve";
        
        btns.AddChild(takeToArmy);
        btns.AddChild(sendToReserve);
    }
}