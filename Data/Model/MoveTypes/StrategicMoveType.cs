
public class StrategicMoveType : MoveType
{
    public StrategicMoveType() 
        : base(true, 200f, nameof(StrategicMoveType))
    {
    }

    protected override float TerrainCostInstantaneous(Cell pt, Data d)
    {
        return 1f;
    }

    public override bool TerrainPassable(Cell p, Data d)
    {
        return true;
    }
}