
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ConquerCellProcedure : Procedure
{
    public CellRef Cell { get; private set; }
    public ERef<Regime> ConqueringRegime { get; private set; }
    public HashSet<int> ConqueringArmies { get; private set; }
    public static ConquerCellProcedure Construct(Cell cell, 
        Regime conqueringRegime,
        IEnumerable<Army> conqueringArmies)
    {
        return new ConquerCellProcedure(cell.MakeRef(), 
            conqueringRegime.MakeRef(),
            conqueringArmies.Select(a => a.Id).ToHashSet());
    }
    [SerializationConstructor] private ConquerCellProcedure(
        CellRef cell, 
        ERef<Regime> conqueringRegime,
        HashSet<int> conqueringArmies)
    {
        Cell = cell;
        ConqueringRegime = conqueringRegime;
        ConqueringArmies = conqueringArmies;
    }

    public override void Enact(ProcedureKey key)
    {
        var cell = Cell.Get(key.Data);
        var newController = ConqueringRegime.Get(key.Data);
        var oldController = cell.Controller.IsEmpty() ? null : cell.Controller.Get(key.Data);
        cell.SetController(newController, key);
        
        var conqueringArmies = ConqueringArmies.Select(i => key.Data.Get<Army>(i));
        foreach (var army in conqueringArmies)
        {
            army.Cells.Add(Cell, key);
        }
        key.Data.Notices.CellChangedController.Invoke((cell, oldController, newController));
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}