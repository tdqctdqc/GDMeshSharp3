// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using Godot;
//
// public partial class MapDisplayOptionsUi : VBoxContainer
// {
//     private ButtonToken _roads, _regimes, _landforms, _vegetation;
//     private Label _mousePos;
//     private Data _data;
//     public override void _Ready()
//     {
//         
//     }
//
//     public override void _Process(double delta)
//     {
//         _mousePos.Text = Game.I.Client?.Cam?.GetMousePosInMapSpace().ToString();
//     }
//
//     public void Setup(Data data, IClient client)
//     {
//         _data = data;
//         _mousePos = new Label();
//         AddChild(_mousePos);
//     }
//
//     public override void _ExitTree()
//     {
//     }
//
//     private void AddLayerOption(string name)
//     {
//         var btn = new Button();
//         btn.Text = name;
//         Action toggle = () =>
//         {
//             Game.I.Client.UiRequests.ToggleMapGraphicsLayer.Invoke(name);
//         };
//         
//         var token = ButtonToken.CreateToken(btn, toggle);
//         AddChild(btn);
//     }
// }