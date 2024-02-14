
public class MoveTypes : ModelList<MoveType>
{
    public InfantryMoveType InfantryMove { get; private set; }
        = new ();

    public StrategicMoveType StrategicMove { get; private set; }
        = new ();
}