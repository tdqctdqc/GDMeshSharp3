
using Godot;

public class DefendAction : CombatAction
{
    public static float RetreatDist { get; private set; } = 30f;

    public override Unit[] GetCombatGraphTargets(Unit u, Data d)
    {
        return null;
    }

    public override CombatResult CalcResult(Unit u, CombatCalculator.CombatCalcData cData, Data d)
    {
        var defendNode = cData.DefendNodes[u];
        var result = CombatResult.Construct(u, cData, d);
        
        if (defendNode.Held == false)
        {
            var dir = Vector2.Zero;
            var attackers = cData.Graph.GetNeighbors(defendNode);
            foreach (var atk in attackers)
            {
                var edge = cData.Graph.GetEdge(defendNode, atk);

                dir += u.Position.Pos.GetOffsetTo(atk.Unit.Position.Pos, d)
                       * atk.Unit.GetPowerPoints(d) * edge.AttackerProportion;
            }

            dir = dir.Normalized();
            result.ResultOffset = -dir * RetreatDist;
        }

        return result;
    }
}