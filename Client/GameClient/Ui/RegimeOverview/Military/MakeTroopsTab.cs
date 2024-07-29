using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class MakeTroopsTab : HBoxContainer, IUiDrawable
{
    private Func<Regime> _getRegime;
    private ItemListToken<Troop> _troops;
    private ItemListToken<ModelMakeProject> _projects;
    private NumSliderAndEntry _num;
    public MakeTroopsTab(Func<Regime> getRegime)
    {
        _getRegime = getRegime;
        Name = "Troops";
    }

    public void Draw(Client client)
    {
        this.ClearChildren();
        var regime = _getRegime();

        if (regime is null) return;
        var med = client.Settings.MedIconSize.Value;
        var units = regime.GetUnits(client.Data);
        var allTroopModels = client.Data.Models
            .GetModels<Troop>()
            .Where(t => regime.HasPrereqs(t));

        var d = new Dictionary<TroopType, float>();
        foreach (var unit in units)
        {
            foreach (var (troop, value) in unit.Troops.GetEnumModel(client.Data))
            {
                d.AddOrSum(troop.TroopType, value);
            }
        }

        var totalDeployed = IdCount<TroopType>.Construct(d);
        
        var totalAuthorized = IdCount<TroopType>.Sum(
            units.Select(u => u.Template.Get(client.Data).Troops).ToArray());
        
        var reserve = regime.Stock.Stock;
        _troops = new ItemListToken<Troop>(
            allTroopModels,
            t => $"{t.Name} " +
                 $"Deployed: {totalDeployed.Get(t.TroopType)} " +
                 $"Authorized: {totalAuthorized.Get(t.TroopType)} " +
                 $"Reserve: {reserve.Get(t)}",
            t => t.Icon.Texture,
            (int)med,
            false);
        _troops.ItemList.ExpandFill();

        var projects = regime.MakeQueue.Queue
            .OfType<ModelMakeProject>()
            .Where(m => m.Model.Get(client.Data) is Troop t);

        _projects = new ItemListToken<ModelMakeProject>(
            projects,
            p => $"{p.Model.Get(client.Data).Name}: {p.Fulfilled} / {p.Amount}",
            t => ((Troop)t.Model.Get(client.Data)).Icon.Texture,
            (int)med,
            true);
        _projects.ItemList.ExpandFill();
        
        var left = new VBoxContainer();
        left.ExpandFill();
        AddChild(left);
        var right = new VBoxContainer();
        right.ExpandFill();
        AddChild(right);

        left.CreateLabelAsChild("Troop Types");
        left.AddChild(_troops.ItemList);

        _num = new NumSliderAndEntry(
            "Amount", 100, 0, 10_000, 1);
        
        left.AddChild(_num);
        _troops.JustSelected += () =>
        {
            var selected = _troops.Selected;
            if (selected.Count != 1)
            {
                _num.SetRange(0, 0);
                _num.SetValue(0);
                return;
            }

            var t = selected.First();
            var have = totalDeployed.Get(t.TroopType);
            var authorized = totalAuthorized.Get(t.TroopType);
            var need = authorized - (have + reserve.Get(t));
            var max = Mathf.Max(10_000, need);
            _num.SetRange(0, max);
            _num.SetValue(need);
        };

        var makeBtn = ButtonExt.GetButton(() =>
        {
            var selected = _troops.Selected;
            if (selected.Count != 1)
            {
                _num.SetRange(0, 0);
                _num.SetValue(0);
                return;
            }
            var troop = selected.First();
            var num = _num.Value;
            var proj = ModelMakeProject.Construct(
                regime, troop, num);
            var com = new StartMakeProjectCommand(proj, 
                client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(com,
                () =>
                {
                    if (IsInstanceValid(this))
                    {
                        Draw(client);
                    }
                }, client);
            client.HandleCommand(outer);
        });
        makeBtn.Text = "Make Troops";
        left.AddChild(makeBtn);


        var setToNeed = ButtonExt.GetButton(() =>
        {
            var selected = _troops.Selected;
            if (selected.Count != 1)
            {
                _num.SetRange(0, 0);
                _num.SetValue(0);
                return;
            }
            var troop = selected.First();
            var need = totalAuthorized.Get(troop.TroopType)
                       - (reserve.Get(troop) + totalDeployed.Get(troop.TroopType));
            if (need <= 0f) return;
            _num.SetValue(need);
        });
        setToNeed.Text = "Set to needed quantity";
        left.AddChild(setToNeed);
        
        right.AddChild(_projects.ItemList);
        var cancel = ButtonExt.GetButton(() =>
        {
            var projs = _projects.Selected;
            if (projs.Count == 0) return;
            var proc = new AggregateProcedure(
                projs.Select(p => new CancelMakeProjectProcedure(regime.MakeRef(), p.Id)).ToArray());
            
            var com = new SendMessageCommand(proc, client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
            var outer = CallbackCommand.Construct(com,
                () =>
                {
                    if (IsInstanceValid(this))
                    {
                        Draw(client);
                    }
                }, client);
            client.HandleCommand(outer);
        });
        cancel.Text = "Cancel Project";
        right.AddChild(cancel);
    }
}