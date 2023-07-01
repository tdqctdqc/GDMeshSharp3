//
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public class Farm : ProductionBuildingModel
// {
//     public Farm() 
//         : base(BuildingType.Agriculture, ItemManager.Food, nameof(Farm), 
//             50, 
//             1000
//             )
//     {
//     }
//     public override int ProductionCap { get; } = 1500;
//     public override Dictionary<Item, int> BuildCosts { get; protected set; }
//         = new Dictionary<Item, int>
//         {
//             {ItemManager.Food, 10000}
//         };
//     public override Dictionary<PeepJob, int> JobLaborReqs { get; }
//         = new Dictionary<PeepJob, int>
//         {
//             {PeepJobManager.Farmer, 500}
//         };
//     protected override bool CanBuildInTriSpec(PolyTri t, Data data)
//     {
//         return t.Landform.IsLand
//                && t.Landform.MinRoughness < LandformManager.Mountain.MinRoughness;
//     }
//     public override bool CanBuildInPoly(MapPolygon p, Data data)
//     {
//         return p.IsLand;
//     }
// }
