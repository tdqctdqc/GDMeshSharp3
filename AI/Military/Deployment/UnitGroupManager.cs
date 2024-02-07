// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// public class UnitGroupManager
// {
//     public HashSet<ERef<UnitGroup>> Groups { get; private set; }
//
//     public static UnitGroupManager Construct(ERef<Regime> r, int nodeId)
//     {
//         return new UnitGroupManager(r, nodeId, new HashSet<ERef<UnitGroup>>());
//     }
//     public UnitGroupManager(ERef<Regime> regime, int nodeId, HashSet<ERef<UnitGroup>> groups)
//     {
//         Regime = regime;
//         NodeId = nodeId;
//         Groups = groups;
//     }
//
//     public bool Contains(UnitGroup g)
//     {
//         return Groups.Contains(g.Id);
//     }
//
//     public void AddUnassigned(DeploymentAi ai, UnitGroup g, Data d)
//     {
//         var node = GetNode(ai);
//         node.AddGroup(ai, g, d);
//     }
//     
//     public void Add(DeploymentAi ai, UnitGroup g)
//     {
//         
//     }
//     public void Remove(DeploymentAi ai, UnitGroup g)
//     {
//         
//     }
//
//     public IEnumerable<UnitGroup> Get(Data d)
//     {
//         return Groups.Select(g => g.Entity(d));
//     }
//
//     public int Count()
//     {
//         return Groups.Count;
//     }
//
//     private GroupAssignment GetNode(DeploymentAi ai)
//     {
//         return (GroupAssignment)ai.GetNode(NodeId);
//     }
// }