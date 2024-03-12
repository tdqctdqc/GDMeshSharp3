
using System;
using System.Collections.Generic;
using Godot;

public partial class PoliticalFillModule : PolyCellFillChunkGraphic
{
    public PoliticalChunkModule Parent { get; private set; }
    public PoliticalFillModule(PoliticalChunkModule parent,
        MapChunk chunk, 
        Data data)
        : base("Political", chunk, 
            LayerOrder.PolyFill, new Vector2(0f, 1f),
            data)
    {
        Parent = parent;
    }

    public override Color GetColor(Cell cell, Data d)
    {
        if (Parent.SelectedMode == PoliticalChunkModule.Mode.Regime)
        {
            if(cell.Controller.IsEmpty()) return Colors.Transparent;
            return cell.Controller.Get(d).GetMapColor();
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Alliance)
        {
            if(cell.Controller.IsEmpty()) return Colors.Transparent;
            return cell.Controller.Get(d).GetAlliance(d).Leader.Get(d).GetMapColor();
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Diplomacy)
        {
            if (cell.Controller.Fulfilled() == false) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Gray;
            if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.IsEmpty()) return Colors.Gray;
            var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Get(d);
            if (cell.Controller.RefId == playerRegime.Id) return Colors.Green;
            var playerAlliance = playerRegime.GetAlliance(d);
            var polyAlliance = cell.Controller.Get(d).GetAlliance(d);
            if (playerAlliance.Members.RefIds.Contains(cell.Controller.RefId)) 
                return Colors.SkyBlue;
            if (playerAlliance.IsAtWar(polyAlliance, d)) 
                return Colors.Red;
            if (playerAlliance.IsRivals(polyAlliance, d)) 
                return Colors.Orange;
            return Colors.Gray;
        }
        else throw new Exception();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        settings.SettingsOptions.Add(
            this.MakeTransparencySetting());
        
        return settings;
    }
}