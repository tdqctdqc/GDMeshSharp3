
using System;
using System.Collections.Generic;using Godot;

public class EntityGraphicReservoir<TEntity, TGraphic> 
    where TEntity : Entity 
    where TGraphic : Node2D
{
    public Dictionary<TEntity, TGraphic> Graphics { get; private set; }

    public EntityGraphicReservoir(
        Func<TEntity, TGraphic> makeGraphic,
        Data d)
    {
        Graphics = new Dictionary<TEntity, TGraphic>();
        foreach (var entity in d.GetAll<TEntity>())
        {
            var graphic = makeGraphic(entity);
            Graphics.Add(entity, graphic);
        }
        d.SubscribeForCreation<TEntity>(n =>
        {
            Game.I.Client.QueuedUpdates
                .Enqueue(() =>
                {
                    var entity = (TEntity)n.Entity;
                    var graphic = makeGraphic(entity);
                    Graphics.Add(entity, graphic);
                });
        });
        d.SubscribeForDestruction<TEntity>(n =>
        {
            Game.I.Client.QueuedUpdates
                .Enqueue(() =>
                {
                    var entity = (TEntity)n.Entity;
                    var graphic = Graphics[entity];
                    graphic.QueueFree();
                    Graphics.Remove(entity);
                });
        });
    }
    
    
}