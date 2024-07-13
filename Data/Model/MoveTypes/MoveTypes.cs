
public class MoveTypes : ModelManager<MoveType>
{
    public InfantryMoveType InfantryMove { get; private set; }
        = new ();

    public StrategicMoveType StrategicMove { get; private set; }
        = new ();
}