
using System.Diagnostics;
using Godot;

public class UnitAux
{
    public ERefColIndexer<UnitGroup, Unit> UnitByGroup { get; private set; }
    public MultiIndexer<Regime, UnitTemplate> UnitTemplates { get; private set; }
    public MultiIndexer<Cell, Unit> UnitsByCell { get; private set; }
    
    private Data _data;
    public UnitAux(Data d)
    {
        _data = d;
        
        UnitTemplates = MultiIndexer.MakeForEntity<Regime, UnitTemplate>(
            t => t.Regime.Get(d), d);
        
        UnitByGroup = new ERefColIndexer<UnitGroup, Unit>(
            g => g.Units.Items(d),  
            d.GetEntityMeta<UnitGroup>().GetRefColMeta<Unit>(nameof(UnitGroup.Units)),
            d);
       
        var unitChangedCell = new ValChangeAction<Unit, Cell>();
        _data.Notices.Military.UnitChangedPos.Subscribe(n => unitChangedCell.Invoke(n.Owner, n.NewVal.GetCell(d), n.OldVal.GetCell(d)));
                
        UnitsByCell = MultiIndexer.MakeForEntity<Cell, Unit>(
            u => u.Position.GetCell(d), d);
        UnitsByCell.RegisterReCalc(d.Notices.Gen.FinishedGen);
        UnitsByCell.RegisterReCalc(d.Notices.FinishedStateSync);
        UnitsByCell.RegisterChanged(unitChangedCell);
        
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

        sw.Stop();
        _data.Logger.Log("Make unit grid time " + sw.Elapsed.TotalMilliseconds,
            LogType.Logic);
    }
}