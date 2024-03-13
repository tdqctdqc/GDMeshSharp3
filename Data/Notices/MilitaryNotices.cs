
public class MilitaryNotices
{
    public ValChangeAction<Unit, UnitGroup> UnitChangedGroup { get; private set; }
        = new();
    public ValChangeAction<Unit, MapPos> UnitChangedPos { get; private set; }
        = new();
    
}