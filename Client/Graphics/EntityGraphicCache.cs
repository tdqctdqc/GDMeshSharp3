
using System;
using System.Collections.Generic;using Godot;

public class EntityGraphicCache<TEntity, TGraphic> 
    where TEntity : Entity 
    where TGraphic : Node2D
{
    public Dictionary<TEntity, TGraphic> Graphics { get; private set; }
    public EntityGraphicCache(
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
            var graphic = makeGraphic(n);
            Graphics.Add(n, graphic);
        });
        d.SubscribeForDestruction<TEntity>(n =>
        {
            var graphic = Graphics[n];
            Graphics.Remove(n);
            graphic.QueueFree();
        });
    }
    
    
}