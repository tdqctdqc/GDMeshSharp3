
using Godot;

public class DoNothingUnitOrder : UnitOrder
{
    public override void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc)
    {
        
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data)
    {
        return;
    }
}