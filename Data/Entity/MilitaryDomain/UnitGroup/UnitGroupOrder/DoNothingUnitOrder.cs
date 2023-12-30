
using Godot;

public class DoNothingUnitOrder : UnitOrder
{
    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        return;
    }
}