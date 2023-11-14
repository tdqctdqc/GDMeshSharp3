
using Godot;

[MessagePack.Union(0, typeof(DeployOnLineOrder))]
[MessagePack.Union(1, typeof(DoNothingUnitOrder))]
[MessagePack.Union(2, typeof(GoToWaypointOrder))]
public abstract class UnitOrder : IPolymorph
{
    public abstract void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc);
    public abstract void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data);
}