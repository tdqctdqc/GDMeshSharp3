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
        var genCellMoistures = new ConcurrentDictionary<GenCell, float>();
        var equatorDistMultWeight = Data.GenMultiSettings
            .MoistureSettings.EquatorDistMoistureMultWeight.Value;
        var frictionCostMult = Data.GenMultiSettings
            .MoistureSettings.MoistureFlowRoughnessCostMult.Value;
        Parallel.ForEach(Data.GenAuxData.Cells, p =>
        {
            var distFromEquator = Mathf.Abs(Data.Planet.Height / 2f - p.Center.Y);
            var altMult = (1f - equatorDistMultWeight) 
                          + equatorDistMultWeight * (1f - distFromEquator / (Data.Planet.Height / 2f));
            var polyGeos = p.Polys;
            var count = polyGeos.Count;
            var waterCount = polyGeos.Where(g => g.IsWater()).Count();
            var score = scale * altMult * waterCount / count;
            genCellMoistures.TryAdd(p, score);
        });


        float maxFriction = 0f;
        float averageFriction = 0f;
        int iter = 0;
        for (int i = 0; i < 3; i++)
        {
            diffuse();
        }
        Parallel.ForEach(Data.GenAuxData.Cells, setPlateMoistures);
        void setPlateMoistures(GenCell cell)
        {
            foreach (var poly in cell.Polys)
            {
                if (poly.IsWater()) poly.SetMoisture(1f, _key);
                else
                {
                    var moisture = genCellMoistures[cell] + Game.I.Random.RandfRange(-.1f, .1f);
                    
                    poly.SetMoisture(Mathf.Clamp(moisture, 0f, 1f), _key);
                }
            }
        }
        
        
        void diffuse()
        {
            Data.GenAuxData.Cells.ForEach(c =>
            {
                var oldScore = genCellMoistures[c];
                var plate = c.Plate;

                var newScore = c.Neighbors.Select(n =>
                {
                    var mult = 1f;
                    var nPlate = n.Plate;
                    if (plate != nPlate 
                        && Data.GenAuxData.FaultLines.TryGetFault(plate, nPlate, out var fault))
                    {
                        mult = 1f - fault.Friction * frictionCostMult;
                        maxFriction = Mathf.Max(maxFriction, fault.Friction);
                        averageFriction += fault.Friction;
                        iter++;
                    }
                    return mult * genCellMoistures[n];
                }).Average();

                if (newScore > oldScore)
                {
                    genCellMoistures[c] = newScore;
                }
            });
        }
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
                .SelectMany(p => p.Neighbors.Items(Data).Select(n => p.GetEdge(n, Data)))
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