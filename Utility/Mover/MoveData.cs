public class MoveData
{
    public int Id;
    public MoveType MoveType;
    public float MovePoints;
    public Alliance Alliance;

    public MoveData(int id, MoveType moveType, float movePoints, Alliance alliance)
    {
        Id = id;
        MoveType = moveType;
        MovePoints = movePoints;
        Alliance = alliance;
    }
}