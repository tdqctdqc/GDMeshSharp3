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
    private GenWriteKey _key;
    public MoistureGenerator()
    {
    }

    public override GenReport Generate(GenWriteKey key)
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
        var scale = Data.GenMultiSettings.MoistureSettings.Scale.Value;
        var plateMoistures = new ConcurrentDictionary<GenPlate, float>();
        var equatorDistMultWeight = Data.GenMultiSettings.MoistureSettings.EquatorDistMoistureMultWeight.Value;
        var roughnessCostMult = Data.GenMultiSettings.MoistureSettings.MoistureFlowRoughnessCostMult.Value;
        Parallel.ForEach(Data.GenAuxData.Plates, p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.Y);
            var altMult = (1f - equatorDistMultWeight) 
                          + equatorDistMultWeight * (1f - distFromEquator / (Data.Planet.Height / 2f));
            var polyGeos = p.Cells.SelectMany(c => c.PolyGeos).ToList();
            var count = polyGeos.Count;
            var waterCount = polyGeos.Where(g => g.IsWater()).Count();
            var score = scale * altMult * waterCount / count;
            plateMoistures.TryAdd(p, score);
        });


        float maxFriction = 0f;
        float averageFriction = 0f;
        int iter = 0;
        for (int i = 0; i < 3; i++)
        {
            diffuse();
        }
        var landPlateMoistureShaping = Data.GenMultiSettings.MoistureSettings
            .LandPlateMoistureShaping.Value;
        Parallel.ForEach(Data.GenAuxData.Plates, setPlateMoistures);
        void setPlateMoistures(GenPlate plate)
        {
            var polys = plate.Cells.SelectMany(c => c.PolyGeos).ToList();
            foreach (var cell in plate.Cells)
            {
                foreach (var poly in cell.PolyGeos)
                {
                    if (poly.IsWater()) poly.SetMoisture(1f, _key);
                    else
                    {
                        var moisture = plateMoistures[plate] + Game.I.Random.RandfRange(-.1f, .1f);
                        var newMoisture = Mathf.Pow(moisture, 1f / landPlateMoistureShaping);
                        if (float.IsNaN(newMoisture) == false)
                        {
                            moisture = newMoisture;
                        }
                        poly.SetMoisture(Mathf.Clamp(moisture, 0f, 1f), _key);
                    }
                }
            }
        }
        
        
        void diffuse()
        {
            Data.GenAuxData.Plates.ForEach(p =>
            {
                var oldScore = plateMoistures[p];
                
                var newScore = p.Neighbors.Select(n =>
                {
                    var mult = 1f;
                    if (Data.GenAuxData.FaultLines.TryGetFault(p, n, out var fault))
                    {
                        mult = 1f - fault.Friction * roughnessCostMult;
                        maxFriction = Mathf.Max(maxFriction, fault.Friction);
                        averageFriction += fault.Friction;
                        iter++;
                    }
                    return mult * plateMoistures[n];
                }).Average();

                if (newScore > oldScore)
                {
                    plateMoistures[p] = newScore;
                }
            });
        }
    }

    private void MoistureFlow()
    {
        var riverFlowPerMoisture = Data.GenMultiSettings.MoistureSettings.RiverFlowPerMoisture.Value;
        var baseRiverFlowCost = Data.GenMultiSettings.MoistureSettings.BaseRiverFlowCost.Value;
        var roughnessMult = Data.GenMultiSettings.MoistureSettings.RiverFlowCostRoughnessMult.Value;
        Parallel.ForEach(Data.Planet.PolygonAux.LandSea.Landmasses, doLandmass);
        
        void doLandmass(Landmass lm)
        {
            var edges = lm.Polys
                .SelectMany(p => p.Neighbors.Items(Data).Select(n => p.GetEdge(n, Data)))
                .Distinct()
                .Where(e => e.HighPoly.Entity(Data).IsLand && e.LowPoly.Entity(Data).IsLand);
            var coastEdges = edges.Where(e => e.IsLandToSeaEdge(Data));
            var covered = coastEdges.ToHashSet();
            var curr = coastEdges.ToHashSet();
            var nodes = curr.ToDictionary(e => e, e => new DrainGraphNode<MapPolygonEdge>(e));
            
            while (curr.Count > 0)
            {
                var adjs = curr.SelectMany(c => c.GetIncidentEdges(Data))
                    .Distinct()
                    .Where(e => covered.Contains(e) == false 
                                && e.HighPoly.Entity(Data).IsLand && e.LowPoly.Entity(Data).IsLand);
                curr = adjs.ToHashSet();
                if (adjs.Count() == 0) break;
                foreach (var adj in adjs)
                {
                    var coveredNeighborEdges = adj.GetIncidentEdges(Data).Where(covered.Contains);
                    if (coveredNeighborEdges.Count() == 0) continue;
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
                    if (node.DrainsTo != null)
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