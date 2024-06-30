using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class SimTab : VBoxContainer, IUiDrawable
{
    private CombatInfo _info;
    private Landform _landform;
    private Vegetation _vegetation;
    private IdCount<Troop> _attackers, _defenders;
    
    public SimTab(CombatInfo info)
    {
        _info = info;
        _attackers = IdCount<Troop>.Construct();
        _defenders = IdCount<Troop>.Construct();
        
    }
    public void Draw(Client c)
    {
        this.ClearChildren();
        var med = c.Settings.MedIconSize.Value;
        var troops = new ItemListToken<Troop>(
            c.Data.Models.GetModels<Troop>().Values,
            t => $"{t.Name}",
            v => {},
            med * Vector2.One,
            t => t.Icon.Texture,
            (int)med * Vector2I.One
        );
        var defenders = new ItemListToken<(Troop troop, float amt)>(
            new List<(Troop, float)>(),
            v => $"{v.troop}: {v.amt}",
            v => {},
            med * Vector2.One,
            v => v.troop.Icon.Texture,
            (int)med * Vector2I.One
        );
        var attackers = new ItemListToken<(Troop troop, float amt)>(
            new List<(Troop, float)>(),
            v => $"{v.troop}: {v.amt}",
            v => {},
            med * Vector2.One,
            v => v.troop.Icon.Texture,
            (int)med * Vector2I.One
        );
        var lf = new ItemListToken<Landform>(
            c.Data.Models.GetModels<Landform>().Values,
            lf => lf.Name,
            lf => _landform = lf,
            med * Vector2.One
        );
        var veg = new ItemListToken<Vegetation>(
            c.Data.Models.GetModels<Vegetation>().Values,
            v => v.Name,
            v => _vegetation = v,
            med * Vector2.One
        );

        var chooseIfDefOuter = new HBoxContainer();
        chooseIfDefOuter.CreateLabelAsChild("Defender: ");
        var chooseIfDef = new CheckBox();
        chooseIfDefOuter.AddChild(chooseIfDef);
        
        var sliderOuter = new HBoxContainer();
        sliderOuter.CreateLabelAsChild("Amount: ");
        var slider = new HSlider();
        slider.MinValue = 0f;
        slider.MaxValue = 1000f;
        sliderOuter.AddChild(slider);
        var set = ButtonExt.GetButton(
            () => { });
        set.Text = "Set";
        sliderOuter.AddChild(set);


        var calc = ButtonExt.GetButton(() => { });
        calc.Text = "Calculate";
        calc.ExpandFill();

        var upper = new HBoxContainer();
        upper.ExpandFill();
        upper.AddChild(attackers.ItemList);
        upper.AddChild(defenders.ItemList);
        AddChild(upper);

        var lower = new HBoxContainer();
        lower.ExpandFill();
        lower.AddChild(troops.ItemList);
        var control = new VBoxContainer();
        control.ExpandFill();
        control.AddChild(chooseIfDefOuter);
        control.AddChild(sliderOuter);
        lower.AddChild(control);
        lower.AddChild(calc);
    }

    private void Calculate(Data d)
    {
        _info.Setup(
            _landform, 
            _vegetation, 
            new UnitCombatInfo[]
            {
                new (_attackers, d)
            },
            new UnitCombatInfo[]
            {
                new (_defenders, d)
            },
            false
        );
        MilUtil.CalculateCombat(_info.Attackers,
            _info.Defenders, _info.Landform, _info.Vegetation, d);
    }
}