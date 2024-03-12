
using Godot;

public class BuildingProd : BuildingModelComponent
{
    public IModel Produced { get; private set; }
    public int ProdCap { get; private set; }

    public BuildingProd(IModel produced, int prodCap)
    {
        Produced = produced;
        ProdCap = prodCap;
    }

    public override void Work(Cell cell, float staffingRatio, 
        ProcedureWriteKey key)
    {
        if (cell.Controller.IsEmpty()) return;
        var regime = cell.Controller.Get(key.Data);
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        regime.Store.Add(Produced, ProdCap * staffingRatio);
    }

}