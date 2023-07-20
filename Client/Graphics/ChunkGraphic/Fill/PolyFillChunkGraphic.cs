// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public partial class PolyFillChunkGraphic : MapChunkGraphicModule
// {
//     public PolyFillChunkGraphic(string name, MapChunk chunk, Data data, Func<MapPolygon, Color> getPolyColor,
//         float transparency = 1f) : base(name)
//     {
//         var polyLayer = new PolyFillLayer(name, chunk, data, getPolyColor, new Vector2(0f, 1f), 
//             transparency);
//         AddLayer(polyLayer);
//     }
//
//     private PolyFillChunkGraphic()
//     {
//         
//     }
// }