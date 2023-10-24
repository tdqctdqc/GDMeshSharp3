
public abstract class UnitOrder : IPolymorph
{
    public abstract void Handle(UnitGroup g, Data d, HandleUnitOrdersProcedure proc);
}