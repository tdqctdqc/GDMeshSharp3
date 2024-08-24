
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ConquerCellProcedure : Procedure
{
    public CellRef Cell { get; private set; }
    public ERef<Regime> ConqueringRegime { get; private set; }
    public HashSet<int> ConqueringArmies { get; private set; }
    
    [SerializationConstructor] private ConquerCellProcedure(
        CellRef cell, 
        ERef<Regime> conqueringRegime,
        HashSet<int> conqueringArmies)
    {
        Cell = cell;
        ConqueringRegime = conqueringRegime;
        ConqueringArmies = conqueringArmies;
    }

    public static void Enact(Cell cell, 
        Regime conqueringRegime,
        IEnumerable<Army> conqueringArmies,
        IWriteKey key)
    {
        var data = key.GetData();
        var oldController = cell.Controller.IsEmpty() ? null : cell.Controller.Get(data);
        cell.SetController(conqueringRegime, key);
        
        foreach (var army in conqueringArmies)
        {
            army.Cells.Add(cell.MakeRef(), key);
        }
        data.Notices.CellChangedController.Invoke((cell, oldController, conqueringRegime));

        if (key is LogicKey l && key.HasRemotes())
        {
            var proc = new ConquerCellProcedure(cell.MakeRef(), 
                conqueringRegime.MakeRef(),
                conqueringArmies.Select(a => a.Id).ToHashSet());
            l.SendMessage(proc);
        }
    }

    public override void Enact(ProcedureKey key)
    {
        Enact(Cell.Get(key.Data),
            ConqueringRegime.Get(key.Data),
            ConqueringArmies.Select(i => key.Data.Get<Army>(i)),
            key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}