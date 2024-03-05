using System;
using Godot;

public partial class PoliticalBordersModule : PolyCellBorder
{
    public PoliticalChunkModule Parent { get; private set; }

    public PoliticalBordersModule(PoliticalChunkModule parent,
        MapChunk chunk, 
        Data data)
        : base("Political Border", 
            chunk, new Vector2(0f, 1f),
            LayerOrder.PolyFill, data)
    {
        Parent = parent;
    }

    protected override bool InUnion(Cell p1, Cell p2, Data data)
    {
        return p1.Controller.RefId == p2.Controller.RefId;
    }

    protected override float GetThickness(Cell m, Cell n, Data data)
    {
        if (Parent.SelectedMode == PoliticalChunkModule.Mode.Regime)
        {
            return 5f;
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Alliance
                 || Parent.SelectedMode == PoliticalChunkModule.Mode.Diplomacy)
        {
            if (m.Controller.RefId == -1 || n.Controller.RefId == -1) 
                return 5f;
            if (m.Controller.Entity(data).GetAlliance(data) 
                == n.Controller.Entity(data).GetAlliance(data))
            {
                return 2.5f;
            }
            return 5f;
        }
        else throw new Exception();
    }

    protected override Color GetColor(Cell p1, Data data)
    {
        if (Parent.SelectedMode == PoliticalChunkModule.Mode.Regime)
        {
            return p1.Controller.Entity(data).PrimaryColor;
        }
        else if (Parent.SelectedMode == PoliticalChunkModule.Mode.Alliance
                 || Parent.SelectedMode == PoliticalChunkModule.Mode.Diplomacy)
        {
            if(p1.Controller.Fulfilled() == false) return Colors.Transparent;
            var allianceLeader = p1.Controller.Entity(data).GetAlliance(data).Leader.Entity(data);
            return allianceLeader.PrimaryColor;
        }
        else throw new Exception();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }
}