using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class MoistureGenerator : Generator
{
    public GenData Data { get; private set; }
    private GenKey _key;
    public MoistureGenerator()
    {
    }

    public override GenReport Generate(GenKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        Data = key.GenData;
        report.StartSection();
        SetPolyMoistures();
        report.StopSection("SetPolyMoistures");
        
        report.StartSection();
        MoistureFlow();
        report.StopSection("BuildRiversDrainGraph");
        return report;
    }
    private void SetPolyMoistures()
    {
        var massBaseMoistures = new Dictionary<GenMass, float>();
        foreach (var genMass in Data.GenAuxData.Masses)
        {
            massBaseMoistures.Add(genMass, 
                Game.I.Random.RandfRange(-.2f, .2f));
        }
        
        var scale = Data.GenMultiSettings.MoistureSettings.Scale.Value;
        var equatorDistMultWeight = Data.GenMultiSettings
            .MoistureSettings.EquatorDistMoistureMultWeight.Value;
        var polys = Data.GetAll<MapPolygon>();

        var distsFromWater = 
            Data.GenAuxData.Cells
            .ToDictionary(c => c,
                c => c.Plate.Mass.GenContinent.IsLand
                    ? 1f
                    : int.MaxValue);
        var unhandled = Data.GenAuxData.Cells
            .Where(c => c.Plate.Mass.GenContinent.IsLand)
            .ToHashSet();
        
        var frontier = Data.GenAuxData.Cells
            .Where(c => c.Plate.Mass.GenContinent.IsLand == false)
            .ToHashSet();

        var iter = 2f;
        while (unhandled.Count > 0 && frontier.Count > 0)
        {
            var newFrontier = new HashSet<GenCell>();
            foreach (var cell in frontier)
            {
                var neighbors = cell
                    .Neighbors
                    .Intersect(unhandled);
                if (neighbors.Any() == false)
                {
                    continue;
                }
                foreach (var landCell in neighbors)
                {
                    newFrontier.Add(landCell);
                    distsFromWater[landCell] = iter;
                }
                unhandled.ExceptWith(newFrontier);
            }
            frontier = newFrontier;
            iter += 4f;
        }
        var avgDim = (Data.Planet.Height + Data.Planet.Width) / 2f;

        Parallel.ForEach(polys, p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.Y);
            var latitudeMult = (1f - equatorDistMultWeight) 
                          + equatorDistMultWeight * (1f - distFromEquator / (Data.Planet.Height / 2f));
            var genCell = Data.GenAuxData.PolyGenCells[p];
            var baseScore =
                    (avgDim / 20_000f) / distsFromWater[genCell]
                + massBaseMoistures[genCell.Plate.Mass]
                ;
            var score = scale 
                        * latitudeMult 
                        * baseScore;
            score = Mathf.Clamp(score, 0f, 1f);
            p.SetMoisture(score, _key);
        });
    }

    private void MoistureFlow()
    {
        var riverFlowPerMoisture = Data.GenMultiSettings.MoistureSettings.RiverFlowPerMoisture.Value;
        var baseRiverFlowCost = Data.GenMultiSettings.MoistureSettings.BaseRiverFlowCost.Value;
        var roughnessMult = Data.GenMultiSettings.MoistureSettings.RiverFlowCostRoughnessMult.Value;
        Parallel.ForEach(Data.Planet.MapAux.LandSea.Landmasses, doLandmass);
        
        void doLandmass(Landmass lm)
        {
            var edges = lm.Polys
                .SelectMany(p => p.Neighbors.Entities(Data).Select(n => p.GetEdge(n, Data)))
                .Distinct()
                .Where(e => e.HighPoly.Get(Data).IsLand && e.LowPoly.Get(Data).IsLand);
            var coastEdges = edges.Where(e => e.IsLandToSeaEdge(Data));
            var covered = coastEdges.ToHashSet();
            var curr = coastEdges.ToHashSet();
            var nodes = curr.ToDictionary(e => e, e => new DrainGraphNode<MapPolygonEdge>(e));
            
            while (curr.Count > 0)
            {
                var adjs = curr.SelectMany(c => c.GetIncidentEdges(Data))
                    .Distinct()
                    .Where(e => covered.Contains(e) == false 
                                && e.HighPoly.Get(Data).IsLand && e.LowPoly.Get(Data).IsLand);
                curr = adjs.ToHashSet();
                if (adjs.Any() == false) break;
                foreach (var adj in adjs)
                {
                    var coveredNeighborEdges = adj.GetIncidentEdges(Data).Where(covered.Contains);
                    if (coveredNeighborEdges.Any() == false) continue;
                    var drainTo = coveredNeighborEdges.OrderBy(getCost).First();
                    var node = new DrainGraphNode<MapPolygonEdge>(adj);
                    node.DrainsTo = drainTo;
                    nodes.Add(adj, node);
                }
                covered.AddRange(adjs);
            }
            
            foreach (var kvp in nodes)
            {
                var node = kvp.Value;
                var m = node.Element.GetAvgMoisture(Data) * riverFlowPerMoisture;
                while (node != null)
                {
                    node.Element.IncrementFlow(m, _key);
                    if (node.DrainsTo != null && nodes.ContainsKey(node.DrainsTo))
                    {
                        node = nodes[node.DrainsTo];
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        

        float getCost(MapPolygonEdge edge)
        {
            return edge.GetAvgRoughness(Data) * roughnessMult
                   + baseRiverFlowCost;
        }
    }
}