
using System;
using Godot;

public partial class PoliticalFillModule : PolyCellFillChunkGraphic
{
    public PoliticalChunkModule Parent { get; private set; }
    public PoliticalFillModule(PoliticalChunkModule parent,
        MapChunk chunk, 
        Data data)
        : base("Political", chunk, LayerOrder.PolyFill, data)
    {
        Parent = parent;
    }

    public override Color GetColor(Cell cell, Data d)
    {
        if (Parent.SelectedMode == PoliticalChunkModule.Mode.Regime)
        {
            if(cell.Controller.IsEmpty()) return Colors.Transparent;
            return cell.Controller.Entity(d).GetMapColor();
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Alliance)
        {
            if(cell.Controller.IsEmpty()) return Colors.Transparent;
            return cell.Controller.Entity(d).GetAlliance(d).Leader.Entity(d).GetMapColor();
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Diplomacy)
        {
            if (cell.Controller.Fulfilled() == false) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Gray;
            if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.IsEmpty()) return Colors.Gray;
            var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
            if (cell.Controller.RefId == playerRegime.Id) return Colors.Green;
            var playerAlliance = playerRegime.GetAlliance(d);
            var polyAlliance = cell.Controller.Entity(d).GetAlliance(d);
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
}