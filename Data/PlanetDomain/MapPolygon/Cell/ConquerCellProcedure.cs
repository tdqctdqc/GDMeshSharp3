
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ConquerCellProcedure : Procedure
{
    public CellRef Cell { get; private set; }
    public ERef<Regime> ConqueringRegime { get; private set; }
    public HashSet<int> ConqueringArmies { get; private set; }
    
    [SerializationConstructor] public ConquerCellProcedure(
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
        var data = key.GetData();
        var cell = Cell.Get(key.Data);
        var conqueringRegime = ConqueringRegime.Get(key.Data);
        var conqueringArmies = ConqueringArmies.Select(i => key.Data.Get<Army>(i));
        
        var oldController = cell.Controller.IsEmpty() ? null : cell.Controller.Get(data);
        cell.SetController(conqueringRegime, key);
        
        foreach (var army in conqueringArmies)
        {
            army.Cells.Add(cell.MakeRef(), key);
        }
        data.Notices.CellChangedController.Invoke((cell, oldController, conqueringRegime));
    }


    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}