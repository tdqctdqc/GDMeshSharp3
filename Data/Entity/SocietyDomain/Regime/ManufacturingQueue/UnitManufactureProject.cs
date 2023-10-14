//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public class UnitManufactureProject : ManufactureProject
// {
//     public EntityRef<UnitTemplate> Template { get; private set; }
//     public UnitManufactureProject(int id, float ipProgress, float amount, 
//         EntityRef<UnitTemplate> template) : base(id, ipProgress, amount)
//     {
//         Template = template;
//     }
//     public override float IndustrialCost(Data d)
//     {
//         return Template.Entity(d).Makeable.IndustrialCost * Amount;
//     }
//     protected override Icon GetIcon(Data d)
//     {
//         return new Icon();
//     }
//     public override IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d)
//     {
//         return Template.Entity(d).Makeable.ItemCosts.GetEnumerableModel(d)
//             .Select(kvp => new KeyValuePair<Item, int>(kvp.Key, 
//                 Mathf.FloorToInt(kvp.Value * Amount)));
//     }
//     public override void Work(Regime r, ProcedureWriteKey key, float ip)
//     {
//         if (ip < 0) throw new Exception();
//         IpProgress += ip;
//         if (IpProgress >= IndustrialCost(key.Data))
//         {
//             // Unit.Create(Template, r.Capital.Entity(key.Data).Center, key);
//         }
//     }
// }