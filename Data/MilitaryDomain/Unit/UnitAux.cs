
using System.Diagnostics;
using System.Linq;
using Godot;

public class UnitAux
{
    public ERefColIndexer<Army, Unit> UnitByGroup { get; private set; }
    public OneToManyIndexer<Regime, UnitTemplate> UnitTemplates { get; private set; }
    public ManyToManyIndexer<Army, Cell> ArmiesByOccupancy { get; private set; }
    public OneToManyIndexer<Cell, Army> ArmiesByHomeCell { get; private set; }
    private Data _data;
    public UnitAux(Data d)
    {
        _data = d;
        
        UnitTemplates = OneToManyIndexer.MakeForEntity<Regime, UnitTemplate>(
            t => t.Regime.Get(d), d);
        
        UnitByGroup = new ERefColIndexer<Army, Unit>(
            g => g.Units.Items(d),  
            d.GetEntityMeta<Army>().GetRefColMeta<Unit>(nameof(Army.Units)),
            d);
        
        ArmiesByOccupancy = ManyToManyIndexer.MakeForEntity<Cell, Army>(
            a => a.Cells.Select(c => PlanetDomainExt.GetPolyCell(c, d)),
            d);

        ArmiesByHomeCell = OneToManyIndexer.MakeForEntity<Cell, Army>(
            a => a.GetHomeCell(d), d);
       
        d.Notices.FinishedStateSync.Subscribe(MakeUnitGrid);
        d.Notices.Ticked.Blank.Subscribe(MakeUnitGrid);
        d.Notices.Gen.ExitedGen.Subscribe(MakeUnitGrid);
        d.Notices.Gen.FinishedGen.Subscribe(MakeUnitGrid);
    }

    private void MakeUnitGrid()
    {
        var sw = new Stopwatch();
        sw.Start();
        var dim = new Vector2(_data.Planet.Width, _data.Planet.Height);
        ArmiesByOccupancy.ReCalc();
        sw.Stop();
        _data.Logger.Log("Make army grid time " + sw.Elapsed.TotalMilliseconds,
            LogType.Logic);
    }
}