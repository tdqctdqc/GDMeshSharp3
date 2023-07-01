// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public class RegimeFlagChunkLayer : MapChunkGraphicLayer<int>
// {
//     public RegimeFlagChunkLayer(MapChunk chunk, Data data, MapGraphics mg) 
//         : base(data, chunk, mg.ChunkChangedCache.PolyRegimeChanged)
//     {
//         Draw(data);
//     }
//     public override void Draw(Data data)
//     {
//         this.ClearChildren();
//         foreach (var p in Chunk.Polys)
//         {
//             var reg = data.Society.Regimes.Entities.FirstOrDefault(r => r.Capital.RefId == p.Id);
//             if (reg != null)
//             {
//                 var center = reg.Polygons.Select(po => Chunk.RelTo.GetOffsetTo(po.Center, data)).Avg();
//                 var flagDim = new Vector2(150f, 100f);
//                 var scale = 3f;
//                 var vbox = new VBoxContainer();
//                 vbox.RectScale = Vector2.One * scale;
//                 var backRect = new ColorRect();
//                 var margin = 10f;
//                 backRect.RectMinSize = flagDim + margin * Vector2.One;
//                 backRect.Color = Colors.Black;
//                 var flagRect = new TextureRect();
//                 flagRect.RectPosition = Vector2.One * margin / 2f;
//                 flagRect.RectMinSize = flagDim;
//                 flagRect.Expand = true;
//                 flagRect.Texture = reg.Template.Model().Flag;
//                 backRect.AddChild(flagRect);
//                 vbox.AddChild(backRect);
//                 var label = new Label();
//                 label.Autowrap = true;
//                 label.Theme = (Theme) GD.Load("res://Assets/Themes/DefaultTheme.tres");
//                 label.Text = reg.Name;
//                 vbox.AddChild(label);
//                 vbox.RectPosition = center - scale * flagDim / 2f;;
//                 AddChild(vbox);
//             }
//         }
//     }
//
//     protected override Node2D MakeGraphic(int key, Data data)
//     {
//         throw new NotImplementedException();
//     }
//
//     protected override IEnumerable<int> Init(Data data)
//     {
//         throw new NotImplementedException();
//     }
// }
