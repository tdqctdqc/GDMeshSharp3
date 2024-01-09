public class MoveData
{
    public int Id;
    public MoveType MoveType;
    public float MovePoints;
    public bool GoThruHostile;
    public Alliance Alliance;

    public MoveData(int id, MoveType moveType, float movePoints, bool goThruHostile, Alliance alliance)
    {
        Id = id;
        MoveType = moveType;
        MovePoints = movePoints;
        GoThruHostile = goThruHostile;
        Alliance = alliance;
    }
}