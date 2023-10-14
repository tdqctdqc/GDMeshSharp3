// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using Godot;
// using Google.OrTools.LinearSolver;
//
//
// public abstract class BudgetPriority
// {
//     
//     public BudgetPriority(string name, Func<Data, Regime, float> getWeight)
//     {
//         Name = name;
//         _getWeight = getWeight;
//         Account = new BudgetAccount();
//         Wishlist = new Dictionary<Item, int>();
//     }
//
//     public void SetWeight(Data data, Regime regime)
//     {
//         Weight = _getWeight(data, regime);
//     }
//
//     public abstract void Calculate(Regime regime, Data data,
//         MajorTurnOrders orders);
//
//     public abstract Dictionary<Item, int> CalculateWishlist(Regime regime, Data data,
//         BudgetPool pool, float proportion);
//
//     public void Wipe()
//     {
//         Account.Clear();
//         Wishlist.Clear();
//     }
//
//     public void SetWishlist(Regime r, Data d, BudgetPool pool, float proportion)
//     {
//         Wishlist = CalculateWishlist(r, d, pool, proportion);
//     }
//     public void FirstRound(MajorTurnOrders orders, Regime regime, float proportion, 
//         BudgetPool pool, Data data)
//     {
//         var taken = new BudgetAccount();
//         taken.TakeShare(proportion, pool, data);
//         Account.Add(taken);
//         Calculate(regime, data, orders);
//     }
//
//     public void SecondRound(MajorTurnOrders orders, Regime regime, float proportion, 
//         BudgetPool pool, Data data, float multiplier)
//     {
//         proportion = Mathf.Min(1f, multiplier * proportion);
//         FirstRound(orders, regime, proportion, pool, data);
//         ReturnUnused(pool, data);
//     }
//
//     private void ReturnUnused(BudgetPool pool, Data data)
//     {
//         foreach (var kvp in Account.Items.Contents)
//         {
//             var item = data.Models.GetModel<Item>(kvp.Key);
//             var q = kvp.Value;
//             if (Account.UsedItem.Contains(item) == false 
//                 && Wishlist.ContainsKey(item) == false)
//             {
//                 Account.Items.Remove(item, q);
//                 pool.AvailItems.Add(item, q);
//             }
//         }
//         
//         foreach (var kvp in Account.Flows.Contents)
//         {
//             var flow = data.Models.GetModel<Flow>(kvp.Key);
//             var q = kvp.Value;
//             if (Account.UsedFlow.Contains(flow) == false)
//             {
//                 Account.Flows.Remove(flow, q);
//                 pool.AvailFlows.Add(flow, q);
//             }
//         }
//
//         if (Account.UsedLabor == false)
//         {
//             var labor = Account.Labor;
//             Account.UseLabor(labor);
//             pool.AvailLabor += labor;
//         }
//     }
//     protected Solver MakeSolver()
//     {
//         var solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
//         // var solver = Solver.CreateSolver("GLOP");
//         if (solver is null)
//         {
//             throw new Exception("solver null");
//         }
//
//         return solver;
//     }
// }
