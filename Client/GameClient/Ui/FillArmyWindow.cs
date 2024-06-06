
using System;
using System.Linq;
using Godot;

public partial class FillArmyWindow : Window
{
    private VBoxContainer _inner;
    private Action _redraw;
    public FillArmyWindow()
    {
        this.MakeHideable();
        Size = new Vector2I(500, 800);
        _inner = this.MakeScrollContainer<VBoxContainer>(
            new Vector2I(500, 800));
    }

    public override void _Process(double delta)
    {
        if (_redraw is not null)
        {
            _redraw();
            _redraw = null;
        }
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
            army.Units.Items(c.Data),
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
            
            foreach (var unit in reserveUnits.Selected)
            {
                var proc = new SetUnitGroupProcedure(unit.MakeRef(),
                    army.MakeRef());
                var com = new SendMessageCommand(proc, player.PlayerGuid);
                c.HandleCommand(com);
            }
            
            _redraw = () => Setup(army, c);
        });
        takeToArmy.Text = "Take to Army";
        
        var sendToReserve = ButtonExt.GetButton(() =>
        {
            if (armyUnits.Selected.Count == 0) return;
            foreach (var unit in armyUnits.Selected)
            {
                var proc = new SetUnitGroupProcedure(unit.MakeRef(),
                    ERef<Army>.GetEmpty());
                var com = new SendMessageCommand(proc, player.PlayerGuid);
                c.HandleCommand(com);
            }
            
            _redraw = () => Setup(army, c);
        });
        sendToReserve.Text = "Send to Reserve";
        
        btns.AddChild(takeToArmy);
        btns.AddChild(sendToReserve);
    }
}