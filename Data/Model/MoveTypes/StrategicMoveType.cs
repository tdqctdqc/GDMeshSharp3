
public class StrategicMoveType : MoveType
{
    public StrategicMoveType() 
        : base(true, 200f, nameof(StrategicMoveType))
    {
    }

    protected override float TerrainCostInstantaneous(PolyCell pt, Data d)
    {
        return 1f;
    }

    public override bool TerrainPassable(PolyCell p, Data d)
    {
        return true;
    }
}