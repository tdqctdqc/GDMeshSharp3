//
// using System;
// using System.Collections.Concurrent;
// using System.Linq;
// using MessagePack;
//
// public class TheaterGraphicLayer : GraphicLayer<TheaterBranch, TheaterGraphic>
// {
//     [SerializationConstructor] private TheaterGraphicLayer(Data data, GraphicsSegmenter segmenter) 
//         : base(LayerOrder.Theaters, "Theater", segmenter)
//     {
//     }
//
//     private void Draw(Data data)
//     {
//         foreach (var theater in Graphics.Keys.ToList())
//         {
//             Remove(theater, data);
//         }
//         foreach (var ai in data.HostLogicData
//                      .RegimeAis.Dic.Values)
//         {
//             var root = ai.Military.Deployment.GetRoot();
//             if (root != null)
//             {
//                 var theaters = root.SubBranches.OfType<TheaterBranch>();
//                 foreach (var theater in theaters)
//                 {   
//                     Add(theater, data);
//                 }
//             }
//         }
//         
//         
//     }
//     protected override TheaterGraphic GetGraphic(TheaterBranch key, Data d)
//     {
//         return new TheaterGraphic(key, _segmenter, d);
//     }
//
//     public static TheaterGraphicLayer GetLayer(
//         GraphicsSegmenter seg,
//         Client client)
//     {
//         var l = new TheaterGraphicLayer(client.Data, seg);
//         
//         
//         client.Data.Notices.FinishedAiCalc.Subscribe(() =>
//             {
//                 client.QueuedUpdates.Enqueue(() =>
//                 {
//                     l.Draw(client.Data);
//                     l.EnforceSettings();
//                 });
//             }
//         );
//         return l;
//     }
// }