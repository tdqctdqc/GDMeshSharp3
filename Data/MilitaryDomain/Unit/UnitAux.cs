
using System.Diagnostics;
using Godot;

public class UnitAux
{
    public EntityMultiIndexer<Regime, Unit> UnitByRegime { get; private set; }
    public EntityRefColIndexer<UnitGroup, Unit> UnitByGroup { get; private set; }
    public EntityMultiIndexer<Regime, UnitGroup> UnitGroupByRegime { get; private set; }
    public EntityMultiIndexer<Regime, UnitTemplate> UnitTemplates { get; private set; }
    public ValChangeAction<Unit, UnitGroup> UnitChangedGroup { get; private set; }
    public ValChangeAction<Unit, MapPos> UnitChangedPos { get; private set; }

    public PropertyMultiIndexer<Cell, Unit> UnitsByCell { get; private set; }
    
    private Data _data;
    public UnitAux(Data d)
    {
        _data = d;
        UnitByRegime = new EntityMultiIndexer<Regime, Unit>(d,
            u => u.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<Unit, Regime>[] { });
        UnitGroupByRegime = new EntityMultiIndexer<Regime, UnitGroup>(d,
            g => g.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<UnitGroup, Regime>[] { });
        UnitTemplates = new EntityMultiIndexer<Regime, UnitTemplate>(
            d, t => t.Regime.Entity(d), new RefAction[] { },
            new ValChangeAction<UnitTemplate, Regime>[] { });
        UnitByGroup = new EntityRefColIndexer<UnitGroup, Unit>(
            g => g.Units.Items(d),  
            d.GetEntityMeta<UnitGroup>().GetRefColMeta<Unit>(nameof(UnitGroup.Units)),
            d);
        UnitChangedPos = new ValChangeAction<Unit, MapPos>();
        UnitChangedGroup = new ValChangeAction<Unit, UnitGroup>();
        
       
       var unitChangedCell = new ValChangeAction<Unit, Cell>();
       UnitChangedPos.Subscribe(n => unitChangedCell.Invoke(n.Entity, n.NewVal.GetCell(d), n.OldVal.GetCell(d)));

        UnitsByCell = new PropertyMultiIndexer<Cell, Unit>(
            d, u => u.Position.GetCell(d),
            new RefAction[]
            {
                d.Notices.FinishedGen,
                d.Notices.FinishedStateSync
            }, 
            unitChangedCell
        );
        
        
        d.Notices.FinishedStateSync.Subscribe(MakeUnitGrid);
        d.Notices.Ticked.Blank.Subscribe(MakeUnitGrid);
        d.Notices.ExitedGen.Subscribe(MakeUnitGrid);
        d.Notices.FinishedGen.Subscribe(MakeUnitGrid);
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