using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class GeologyGenerator : Generator
{
    public GenData Data { get; private set; }
    private IdDispenser _id;
    private GenWriteKey _key;
    public GeologyGenerator()
    {
        
    }
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        _id = key.IdDispenser;
        Data = key.GenData;
        
        report.StartSection(); 
        BuildCells();
        report.StopSection("BuildCells");
        
        report.StartSection(); 
        BuildPlates();
        report.StopSection("BuildPlates");
        
        report.StartSection(); 
        BuildMasses();
        report.StopSection("BuildMasses");
        
        report.StartSection(); 
        BuildContinents();
        report.StopSection("BuildContinents");
        
        report.StartSection(); 
        DoContinentFriction();
        report.StopSection("DoContinentFriction");
        
        report.StartSection(); 
        HandleIsthmusAndInlandSeas();
        report.StopSection("HandleIsthmusAndInlandSeas");
        
        report.StartSection(); 
        Data.Notices.SetLandAndSea.Invoke();
        report.StopSection("SetLandmasses");
        
        return report;
    }

    private void HandleIsthmusAndInlandSeas()
    {
        
    }
    
    private void BuildCells()
    {
        var polysPerCell = 3;
        var numCells = Data.Planet.Polygons.Entities.Count / polysPerCell;
        var polyCellDic = Data.GenAuxData.PolyCells;
        var cellSeeds = Picker.PickSeeds(Data.Planet.Polygons.Entities, new int[] {numCells})[0];

        var cells = cellSeeds.Select(p => new GenCell(p, _key, polyCellDic, Data)).ToList();
        Data.GenAuxData.Cells.AddRange(cells);
        var polysNotTaken =
            Data.Planet.Polygons.Entities.Except(cellSeeds);

        var remainder = Picker.PickInTurn(polysNotTaken, 
            cells, 
            cell => cell.NeighboringPolyGeos, 
            (cell, poly) => cell.AddPolygon(poly, _key)
        );
        if (remainder.Count > 0)
        {
            throw new Exception();
        }
        Parallel.ForEach(cells, c => c.SetNeighbors(_key));
    }

    private void BuildPlates()
    {
        var cellsPerPlate = 3;
        var numPlates = Data.GenAuxData.Cells.Count / cellsPerPlate;
        var plateSeeds = Picker.PickSeeds(Data.GenAuxData.Cells, new[] {numPlates})[0];
        var plates = plateSeeds.Select(s => new GenPlate(s, _id.GetID(), _key)).ToList();
        
        Data.GenAuxData.Plates.AddRange(plates);
        var cellsNotTaken = Data.GenAuxData.Cells.Except(plateSeeds);
        var remainder = Picker.PickInTurnHeuristic(cellsNotTaken, plates, 
            plate => plate.NeighboringCells,
            (plate, cell) => plate.AddCell(cell, _key),
            ((cell, plate) => plate.NeighboringCellsAdjCount[cell]));
        if (remainder.Count > 0) throw new Exception();
        plates.ForEach(p =>
        {
            p.SetNeighbors();
        });
        foreach (var poly in Data.Planet.Polygons.Entities)
        {
            var cell = Data.GenAuxData.PolyCells[poly];
            var plate = cell.Plate;
        }
    }

    private void BuildMasses()
    {
        var platesPerMass = 3;
        var numMasses = Data.GenAuxData.Plates.Count / 3;
        var massSeeds = Picker.PickSeeds(Data.GenAuxData.Plates, new int[] {numMasses})[0];
        var masses = massSeeds.Select(s => new GenMass(s, _id.GetID())).ToList();

        var platesNotTaken = Data.GenAuxData.Plates.Except(massSeeds);
        var remainder = Picker.PickInTurnHeuristic(platesNotTaken, masses,
            mass => mass.NeighboringPlates,
            (mass, plate) => mass.AddPlate(plate),
            (plate, mass) => mass.NeighboringPlatesAdjCount[plate]);
        if (remainder.Count > 0) throw new Exception();
        
        Data.GenAuxData.Masses.AddRange(masses);
        masses.ForEach(m => m.SetNeighbors());
    }

    private void BuildContinents()
    {
        var numMasses = Data.GenAuxData.Masses.Count;
        var numLandConts = (int) Data.GenMultiSettings.GeologySettings.NumContinents.Value;
        var numSeas = (int) Data.GenMultiSettings.GeologySettings.NumSeas.Value;
        if (numLandConts + numSeas > Data.GenAuxData.Masses.Count) throw new Exception();

        var landMinAlt = .5f;
        var landMaxAlt = .9f;
        var seaMinAlt = .1f;
        var seaMaxAlt = .45f;
        
        var landRatio = Data.GenMultiSettings.GeologySettings.LandRatio.Value;
        var numSeaMasses = Mathf.FloorToInt(numMasses * (1f - landRatio));

        var seeds = Picker.PickSeeds(Data.GenAuxData.Masses, new int[] {numLandConts, numSeas});
        var landSeeds = seeds[0].ToHashSet();
        var waterSeeds = seeds[1].ToHashSet();
        var allSeeds = landSeeds.Union(waterSeeds);
        var landConts = landSeeds
            .Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(landMinAlt, landMaxAlt)))
            .ToList();
        //todo make delaunay graph for landConts and put a sea on each edge
        var seaConts = waterSeeds
            .Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(seaMinAlt, seaMaxAlt)))
            .ToList();
        var width = Data.GenMultiSettings.Dimensions.X;
        var landRemainder = Picker.PickInTurnToLimitHeuristic(
            Data.GenAuxData.Masses.Except(allSeeds), 
            landConts,
            cont => cont.NeighboringMasses,
            (cont, mass) => cont.AddMass(mass),
            (m, c) => width
                      + m.Center.DistanceTo(c.Center) / 20f
                      + Game.I.Random.RandfRange(0f, width / 5f), //todo use cylinder pos
            numSeaMasses);
        
        var seaRemainder = Picker.PickInTurn(landRemainder, seaConts,
            cont => cont.NeighboringMasses,
            (cont, mass) => cont.AddMass(mass));

        if (seaRemainder.Count > 0)
        {
            var unions = UnionFind.Find(seaRemainder, (g, h) => true, m => m.Neighbors);
            foreach (var u in unions)
            {
                var cont = new GenContinent(u.First(), _id.GetID(), Game.I.Random.RandfRange(seaMinAlt, seaMaxAlt));
                for (var i = 1; i < u.Count; i++)
                {
                    cont.AddMass(u[i]);
                }
                seaConts.Add(cont);
            }
        }
        Data.GenAuxData.Continents.AddRange(landConts);
        Data.GenAuxData.Continents.AddRange(seaConts);
        Data.GenAuxData.Continents.ForEach(c => c.SetNeighbors());
        Data.GenAuxData.Continents.ForEach(cont =>
        {
            var isLand = landSeeds.Contains(cont.Seed);
            var polys = cont.Masses
                .SelectMany(m => m.Plates)
                .SelectMany(p => p.Cells)
                .SelectMany(c => c.PolyGeos);
            foreach (var poly in polys)
            {
                var altNoise = Data.GenAuxData.GetAltPerlin(poly.Center);
                var altValue = cont.Altitude + .2f * altNoise;
                poly.SetAltitude(altValue, _key);
                // poly.Set<float>(nameof(MapPolygon.Altitude), altValue, _key);
            }
        });
    }

    private void DoContinentFriction()
    {
        var faultRangeSetting = Data.GenMultiSettings.GeologySettings.FaultLineRange.Value;
        var frictionAltEffectSetting = Data.GenMultiSettings.GeologySettings.FrictionAltEffect.Value;
        var roughnessErosionMult = Data.GenMultiSettings.GeologySettings.RoughnessErosionMult.Value;
        var oscilMetric = new OscillatingDownFunction(50f, 1f, 0f, 100f);
        var passMetric = new OscillatingFunction(50f, 1f, 0f);
        var seaLevelSetting = Data.GenMultiSettings.GeologySettings.SeaLevel.Value;
        var frictionRoughnessEffectSetting = Data.GenMultiSettings.GeologySettings.FrictionRoughnessEffect.Value;
        ConcurrentBag<FaultLine> faults = new ConcurrentBag<FaultLine>();
        Parallel.ForEach(Data.GenAuxData.Plates, setFriction);
        foreach (var f in faults)
        {
            Data.GenAuxData.FaultLines.AddFault(f);
        }
        Parallel.ForEach(Data.GenAuxData.FaultLines.FaultLines, f =>
        {
            var inRange = getPolysInRangeOfFault(f);
            foreach (var mapPolygon in inRange)
            {
                doFaultLineEffect(mapPolygon, f);
            }
            f.PolyFootprint.AddRange(inRange);   
        });
        foreach (var poly in Data.Planet.Polygons.Entities)
        {
            poly.SetIsLand(poly.Altitude > seaLevelSetting, _key);
        }
        
        
        
        void setFriction(GenPlate hiPlate)
        {
            var neighbors = hiPlate.Neighbors.ToList();
            var count = neighbors.Count;
            for (var j = 0; j < count; j++)
            {
                var loPlate = neighbors[j];
                if (loPlate.Id < hiPlate.Id 
                    && loPlate.Mass.GenContinent != hiPlate.Mass.GenContinent)
                {
                    var drift1 = hiPlate.Mass.GenContinent.Drift;
                    var drift2 = loPlate.Mass.GenContinent.Drift;

                    var axis = loPlate.Center - hiPlate.Center;
                    var driftStr = (drift1 - drift2).Length() / 2f;
                    if (driftStr > .5f)
                    {
                        var borders = Data.Planet.PolygonAux.BorderGraph
                            .GetBorderEdges(hiPlate.Cells.SelectMany(c => c.PolyGeos))
                            .ToList();
                        var friction = driftStr.ProjectToRange(1f, .5f, .5f);
                        var fault = new FaultLine(driftStr, hiPlate, loPlate, borders, Data);
                        faults.Add(fault);
                    }
                }
            }
        }
        

        IEnumerable<MapPolygon> getPolysInRangeOfFault(FaultLine fault)
        {
            var faultRange = fault.Friction * faultRangeSetting;
            var polys = fault.HighId.Cells.SelectMany(c => c.PolyGeos)
                .Union(fault.LowId.Cells.SelectMany(c => c.PolyGeos));
            
            var polysInRange = new List<MapPolygon>();
            foreach (var poly in polys)
            {
                var dist = fault.GetDist(poly, Data);
                if (dist < faultRange)
                {
                    polysInRange.Add(poly);
                }
            }
            return polysInRange;
        }
        
        void doFaultLineEffect(MapPolygon poly, FaultLine fault)
        {
            var close = fault.GetClosestSeg(poly, Data);
            var dist = close.DistanceTo(fault.Origin.GetOffsetTo(poly, Data));
            var faultRange = fault.Friction * faultRangeSetting;
            var distRatio = (faultRange - dist) / faultRange;
            var spineOsc =
                // 1f;
                oscilMetric.Calc(dist);
            
            var distFactor = distRatio * spineOsc;
            var altEffect = fault.Friction * frictionAltEffectSetting * distFactor;
            poly.Set<float>(nameof(poly.Altitude), Mathf.Min(1f, poly.Altitude + altEffect), _key);
            float roughnessErosion = 0f;
            if (poly.Altitude < seaLevelSetting) roughnessErosion 
                = poly.Altitude * roughnessErosionMult;
            
            var frictionEffect = fault.Friction * frictionRoughnessEffectSetting * distFactor;
            var rand = Game.I.Random.RandfRange(-.2f, .2f);
            var newRoughness = Mathf.Clamp(frictionEffect - roughnessErosion + rand, 0f, 1f);
            poly.Set(nameof(poly.Roughness), newRoughness, _key);
        }
    }
}