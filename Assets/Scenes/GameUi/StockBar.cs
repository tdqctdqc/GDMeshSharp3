using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class StockBar : HBoxContainer
{
    public StockBar(Client client, Data data)
    {
        // AddModel(client, data.Models.Items.ConstructionCap, data);
        // AddModel(client, data.Models.Items.IndustrialPower, data);
        // AddModel(client, data.Models.Items.Income, data);
        // AddModel(client, data.Models.Items.Research, data);
        // AddModel(client, data.Models.Items.MilitaryCap, data);
        //
        foreach (var item in data.Models.GetModels<Item>())
        {
            if(item is not Troop) AddModel(client, item, data);
        }
        
    }

    private StockBar()
    {
    }

    private void AddModel(Client client, IModel m, Data data)
    {
        this.AddChildWithVSeparator(new StockEntry(client, m));
    }

    private partial class StockEntry : HBoxContainer
    {
        private IModel _model;
        public StockEntry(Client client, IModel m)
        {
            _model = m;
            var r = client.GetComponent<MapGraphics>().SpectatingRegime;
            var h = client.Settings.MedIconSize.Value;
            var triggers = new RefAction[]
            {
                Game.I.Client.Notices.ChangedSpectatingRegime.Blank,
                client.Data.Notices.JustTicked.Blank,
                client.Data.Notices.FinishedTurnStartCalc
            };
            if (m is IIconed i)
            {
                i.Icon.MakeIconStatDisplay(this, client, GetStat, h, triggers);
            }
            else
            {
                NodeExt.MakeStatDisplay(this, client, GetStat, h, triggers);
            }

            MouseEntered += () =>
            {
                Game.I.Client.GetComponent<TooltipManager>()
                    .Prompt(TooltipFast(), null, this);
            };

            MouseExited += () =>
            {
                Game.I.Client.GetComponent<TooltipManager>()
                    .HideTooltip(this);
            };
        }
        

        private string GetStat()
        {
            var r = Game.I.Client.GetComponent<MapGraphics>().SpectatingRegime;
            if (r == null) return "";
            var stock = r.Stock.Stock.Get(_model);
            return $"{stock}";
        }


        private VBoxContainer TooltipFast()
        {
            var r = Game.I.Client.GetComponent<MapGraphics>().SpectatingRegime;

            var stock = r.Stock;
            var recurring = stock.RecurringCosts.Get(_model);
            var oneTime = stock.SingleTimeCosts.Get(_model);
            var produced = stock.Produced.Get(_model);
            var label = new Label();
            
            label.Text = $"Produced: {produced} " +
                         $"\nRecurring: {recurring} " +
                         $"\nOne Time: {oneTime}";
            var vbox = new VBoxContainer();
            vbox.AddChild(label);
            return vbox;
        }
    }
}