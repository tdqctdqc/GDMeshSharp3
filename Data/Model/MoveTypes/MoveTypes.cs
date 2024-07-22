
public class MoveTypes : ModelPredefs<MoveType>
{
    public InfantryMoveType InfantryMove { get; private set; }
        = new ();

    public StrategicMoveType StrategicMove { get; private set; }
        = new ();
}