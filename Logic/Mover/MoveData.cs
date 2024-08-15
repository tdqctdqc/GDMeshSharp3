public class MoveData
{
    public int Id;
    public MoveType MoveType;
    public float MovePoints;
    public Regime Regime;

    public MoveData(int id, MoveType moveType, float movePoints, Regime regime)
    {
        Id = id;
        MoveType = moveType;
        MovePoints = movePoints;
        Regime = regime;
    }
}