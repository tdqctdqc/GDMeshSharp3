
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DoNothingUnitGroupOrder : UnitGroupOrder
{
    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        var cells = group.Units.Items(d)
            .Select(u => u.Position.GetCell(d)).Distinct();
        var crossSize = 10f;
        foreach (var c in cells)
        {
            var relPos = relTo.GetOffsetTo(c.GetCenter(), d);
            mb.AddLine(relPos - Vector2.One * crossSize,
                relPos + Vector2.One * crossSize,
                group.Color, 3f);
            mb.AddLine(relPos + new Vector2(1f, -1f) * crossSize,
                relPos + new Vector2(-1f, 1f) * crossSize,
                group.Color, 3f);
        }
        return;
    }

    public override void RegisterCombatActions(CombatCalculator combat, LogicWriteKey key)
    {
        
    }

    public override string GetDescription(Data d)
    {
        return "Doing nothing";
    }
}