
public class UnitMoveTypes : ModelList<UnitMoveType>
{
    public InfantryMoveType InfantryMove { get; private set; }
        = new ();
}