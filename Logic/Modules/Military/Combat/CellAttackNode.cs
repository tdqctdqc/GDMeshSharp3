
using System.Collections.Generic;
using Godot;
using MessagePack;

public class CellAttackNode : ICombatGraphNode, IUnitNode
{
    public int Id { get; }
    public CellRef From { get; private set; }
    public CellRef Target { get; private set; }
    public List<UnitCombatInfo> UnitInfos { get; private set; }

    public static CellAttackNode GetOrConstruct(
        Army army,
        Cell from,
        Cell target, CombatCalculator combat, Data d)
    {
        var key = new Vector2I(from.Id, target.Id);
        if (combat.Graph.CellAtkNodes.TryGetValue(key, out var id))
        {
            var atkNode = (CellAttackNode)combat.Graph
                .NodesById[id];
            combat.Graph.AddEdge(atkNode, army);
            return atkNode;
        }

        var node = new CellAttackNode(
            d.HostLogicData.CombatGraphIds.TakeId(d),
            from.MakeRef(),
            target.MakeRef(),
            new List<UnitCombatInfo>()
        );
        combat.Graph.AddNode(node);
        combat.Graph.CellAtkNodes.Add(key, node.Id);
        combat.Graph.AddEdge(node, army);
        var defNode = CellDefenseNode.GetOrConstruct(
            combat.Graph, target, d);
        combat.Graph.AddEdge(node, defNode);
        return node;
    }
    [SerializationConstructor] private CellAttackNode(int id, 
        CellRef from, CellRef target, 
        List<UnitCombatInfo> unitInfos)
    {
        Id = id;
        From = from;
        Target = target;
        UnitInfos = unitInfos;
    }

    public void Add(Unit unit, Data d)
    {
        var info = new UnitCombatInfo(unit, d);
        UnitInfos.Add(info);
    }
}