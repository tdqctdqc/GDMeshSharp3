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
        var allTroopModels = client.Data.Models.GetModels<Troop>().Values;
        var totalDeployed = IdCount<Troop>.Sum(
            units.Select(u => u.Troops).ToArray());
        var totalAuthorized = IdCount<Troop>.Sum(
            units.Select(u => u.Template.Get(client.Data).Troops)
                .ToArray());
        var reserve = regime.Stock.Stock;
        _troops = new ItemListToken<Troop>(
            allTroopModels,
            t => $"{t.Name} " +
                 $"Deployed: {totalDeployed.Get(t)} " +
                 $"Authorized: {totalAuthorized.Get(t)} " +
                 $"Reserve: {reserve.Get(t)}",
            t => { },
            t => t.Icon.Texture,
            (int)med);
        _troops.ItemList.ExpandFill();

        var projects = regime.MakeQueue.Queue
            .OfType<ModelMakeProject>()
            .Where(m => m.Model(client.Data) is Troop t);

        _projects = new ItemListToken<ModelMakeProject>(
            projects,
            p => $"{p.Model(client.Data).Name}: {p.Fulfilled} / {p.Amount}",
            p => { },
            t => ((Troop)t.Model(client.Data)).Icon.Texture,
            (int)med);
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
        _troops.JustSelected += t =>
        {
            var have = totalDeployed.Get(t);
            var authorized = totalAuthorized.Get(t);
            var need = authorized - (have + reserve.Get(t));
            var max = Mathf.Max(10_000, need);
            _num.SetRange(0, max);
            _num.SetValue(need);
        };

        var makeBtn = ButtonExt.GetButton(() =>
        {
            var troop = _troops.Value;
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
            var troop = _troops.Value;
            var need = totalAuthorized.Get(troop)
                       - (reserve.Get(troop) + totalDeployed.Get(troop));
            if (need <= 0f) return;
            _num.SetValue(need);
        });
        setToNeed.Text = "Set to needed quantity";
        left.AddChild(setToNeed);
        
        right.AddChild(_projects.ItemList);
        var cancel = ButtonExt.GetButton(() =>
        {
            var proj = _projects.Value;
            if (proj == null) return;
            var proc = new CancelMakeProjectProcedure(regime.MakeRef(),
                proj.Id);
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