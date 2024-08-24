//
// public class ChangeStockProcedure : Procedure
// {
//     public ERef<Regime> Regime { get; private set; }
//     public ModelRef<IModel> Model { get; private set; }
//     public float Amount { get; private set; }
//     public override void Enact(ProcedureKey key)
//     {
//         var stock = Regime.Get(key.Data).Stock;
//         var model = Model.Get(key.Data);
//         if (Amount > 0f)
//         {
//             stock.Stock.Add(model, Amount);
//         }
//         else
//         {
//             stock.Stock.Remove(model, -Amount);
//         }
//     }
//
//     public override bool Valid(Data data, out string error)
//     {
//         if (data.HasEntity(Regime.RefId) == false)
//         {
//             error = "Regime not found";
//             return false;
//         }
//
//         error = "";
//         return true;
//     }
// }