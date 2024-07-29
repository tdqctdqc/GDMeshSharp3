using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VoronoiSandbox;

public class GeologyGenerator : Generator
{
    public GenData Data { get; private set; }
    private GenKey _key;
    public GeologyGenerator()
    {
        
    }
    public override GenReport Generate(GenKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
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
        Data.Notices.Gen.SetLandAndSea.Invoke();
        report.StopSection("SetLandmasses");
        
        return report;
    }

    private void HandleIsthmusAndInlandSeas()
    {
        
    }
    
    private void BuildCells()
    {
        var polysPerCell = 3;
        var polys = Data.GetAll<MapPolygon>();
        var numCells = polys.Count / polysPerCell;
        var polyCellDic = Data.GenAuxData.PolyGenCells;
        var cellSeeds = Picker.PickSeeds(polys, new int[] {numCells})[0];

        var cells = cellSeeds.Select(p => new GenCell(p, _key, polyCellDic, Data)).ToList();
        Data.GenAuxData.Cells.AddRange(cells);
        var polysNotTaken =
            polys.Except(cellSeeds);

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
        var id = Data.IdDispenser;
        var cellsPerPlate = 3;
        var numPlates 
            = Data.GenAuxData.Cells.Count / cellsPerPlate;
        var plateSeeds 
            = Picker.PickSeeds(Data.GenAuxData.Cells, 
                new[] {numPlates})[0];
        var plates = plateSeeds.Select(s => new GenPlate(s, id.TakeId(), _key)).ToList();
        
        Data.GenAuxData.Plates.AddRange(plates);
        var cellsNotTaken = Data.GenAuxData.Cells.Except(plateSeeds);
        var remainder = Picker.PickInTurnHeuristic(cellsNotTaken, plates, 
            plate => plate.NeighboringCells,
            (plate, cell) => plate.AddCell(cell, _key),
            (cell, plate) => 
                plate.Center.Offset(cell.Center, Data).Length()
                // plate.NeighboringCellsAdjCount[cell]
            
            );
        if (remainder.Count > 0) throw new Exception();
        plates.ForEach(p =>
        {
            p.SetNeighbors();
        });
        foreach (var poly in Data.GetAll<MapPolygon>())
        {
            var cell = Data.GenAuxData.PolyGenCells[poly];
            var plate = cell.Plate;
        }
    }

    private void BuildMasses()
    {
        var id = Data.IdDispenser;

        var platesPerMass = 3;
        var numMasses = Data.GenAuxData.Plates.Count / 3;
        var massSeeds = Picker.PickSeeds(Data.GenAuxData.Plates, new int[] {numMasses})[0];
        var masses = massSeeds.Select(s => new GenMass(s, id.TakeId())).ToList();

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
        var id = Data.IdDispenser;

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
            .Select(s => new GenContinent(s, 
                id.TakeId(), 
            Game.I.Random.RandfRange(landMinAlt, landMaxAlt), true))
            .ToList();
        //todo make delaunay graph for landConts and put a sea on each edge
        var seaConts = waterSeeds
            .Select(s => new GenContinent(s, id.TakeId(), 
                Game.I.Random.RandfRange(seaMinAlt, seaMaxAlt), false))
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
                var cont = new GenContinent(u.First(), 
                    id.TakeId(), 
                    Game.I.Random.RandfRange(seaMinAlt, seaMaxAlt),
                    false);
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
                .SelectMany(c => c.Polys);
            foreach (var poly in polys)
            {
                var altNoise = Data.GenAuxData.GetAltPerlin(poly.Center);
                var altValue = cont.Altitude + .2f * altNoise;
                poly.SetAltitude(altValue, _key);
            }
        });
    }

    private void DoContinentFriction()
    {
        var gSettings = Data.GenMultiSettings.GeologySettings;
        var roughnessScale = gSettings.RoughnessScale.Value;
        var altScale = gSettings.FaultLineAltitudeScale.Value;
        
        var faultRangeSetting = gSettings.FaultLineRange.Value;
        var frictionAltEffect = gSettings.FrictionAltEffect.Value * altScale;
        var roughnessErosionMult = gSettings.RoughnessErosionMult.Value * roughnessScale;
        var seaLevel = gSettings.SeaLevel.Value;
        var frictionRoughnessEffectSetting = gSettings.FrictionRoughnessEffect.Value * roughnessScale;
        ConcurrentBag<FaultLine> faults = new ConcurrentBag<FaultLine>();
        MakeFaults(faults);

        var mtnPassNoise = new FastNoiseLite();
        mtnPassNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        mtnPassNoise.Frequency = 1 / 100f;
        
        foreach (var f in Data.GenAuxData.FaultLines.FaultLines)
        {
            f.DoEffect(Data, mtnPassNoise);
        }

        MakeIslands(seaLevel);
        MakeHills();
        
        foreach (var mapPolygon in Data.GetAll<MapPolygon>())
        {
            var pres = Data.GenAuxData.PreCellPolys[mapPolygon];
            var avg = pres.Average(p => p.Roughness);
            mapPolygon.SetRoughness(avg, _key);
        }
    }

    private void MakeHills()
    {
        foreach (var plate in Data.GenAuxData.Plates)
        {
            if (plate.Mass.GenContinent.IsLand == false) continue;
            if (Game.I.Random.Randf() < .5f) continue;
            var noise = new FastNoiseLite();
            noise.Frequency = 1f / Game.I.Random.RandfRange(50f, 300f);
            noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
            noise.FractalOctaves = 1;

            var mult = Game.I.Random.RandfRange(.5f, .75f);
            foreach (var cell in plate.Cells)
            {
                foreach (var poly in cell.Polys)
                {
                    foreach (var c in Data.GenAuxData.PreCellPolys[poly])
                    {
                        var sample = noise.GetNoise2D(c.RelTo.X, c.RelTo.Y);
                        sample += 1f;
                        sample /= 2f;
                        sample *= mult;
                        c.SetRoughness(Mathf.Max(c.Roughness, sample));
                    }
                }
            }
        }
    }

    private HashSet<MapPolygon> MakeIslands(float seaLevel)
    {
        var polys = Data.GetAll<MapPolygon>();
        foreach (var poly in polys)
        {
            poly.SetIsLand(poly.Altitude > seaLevel, _key);
            if (poly.IsLand == false)
            {
                var avgRough = Data.GenAuxData.PreCellPolys[poly]
                    .Average(c => c.Roughness);
                if (avgRough >= .9f)
                {
                    poly.SetIsLand(true, _key);
                    foreach (var c in Data.GenAuxData.PreCellPolys[poly])
                    {
                        c.SetRoughness(c.Roughness / 2f);
                    }

                    poly.SetAltitude(seaLevel + .1f, _key);
                }
            }
        }

        return polys;
    }

    private void MakeFaults(ConcurrentBag<FaultLine> faults)
    {
        Parallel.ForEach(Data.GenAuxData.Plates, setFriction);
        foreach (var f in faults)
        {
            Data.GenAuxData.FaultLines.AddFault(f);
        }

        void setFriction(GenPlate hiPlate)
        {
            var neighbors = hiPlate.Neighbors.ToList();
            var count = neighbors.Count;
            var driftStr = 0f;
            for (var j = 0; j < count; j++)
            {
                var loPlate = neighbors[j];
                if (loPlate.Id > hiPlate.Id) continue;
                if (loPlate.Id < hiPlate.Id
                    && loPlate.Mass.GenContinent != hiPlate.Mass.GenContinent)
                {
                    var drift1 = hiPlate.Mass.GenContinent.Drift;
                    var drift2 = loPlate.Mass.GenContinent.Drift;
                    
                    var axis = loPlate.Center - hiPlate.Center;
                    driftStr = (drift1 - drift2).Length() / 2f;
                }
                else if(loPlate.Id < hiPlate.Id
                        && loPlate.Mass != hiPlate.Mass)
                {
                    var drift1 = hiPlate.Mass.Drift;
                    var drift2 = loPlate.Mass.Drift;
                    
                    var axis = loPlate.Center - hiPlate.Center;
                    driftStr = (drift1 - drift2).Length() / 2f;
                }
                if (driftStr > .25f)
                {
                    var friction = driftStr.ProjectToRange(1f, .5f, .5f);
                    if (hiPlate.Mass.GenContinent.IsLand != loPlate.Mass.GenContinent.IsLand)
                    {
                        friction /= 2f;
                    }
                    var fault = new FaultLine(driftStr,
                        hiPlate, loPlate, Data);
                    faults.Add(fault);
                }
            }
        }
    }
}