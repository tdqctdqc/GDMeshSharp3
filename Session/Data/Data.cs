using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Data
{
    private IdDispenser _idDispenser;
    public LogicRequests Requests { get; private set; }
    public ClientPlayerData ClientPlayerData { get; private set; }
    public HostLogicData HostLogicData { get; private set; }
    public DataNotices Notices { get; private set; }
    public DataHandles Handles { get; private set; }
    public Models Models { get; private set; }
    public RefFulfiller RefFulfiller { get; private set; }
    public Dictionary<Type, IEntityRegister> Registers { get; private set; }
    public Dictionary<int, Entity> Entities { get; private set; }
    public Entity this[int id] => Entities[id];
    public BaseDomain BaseDomain { get; private set; }
    public PlanetDomain Planet { get; private set; }
    public SocietyDomain Society { get; private set; }
    public InfrastructureDomain Infrastructure { get; private set; }
    public EntityTypeTree EntityTypeTree { get; private set; }
    public int Tick => BaseDomain.GameClock.Tick;

    public Data()
    {
        Requests = new LogicRequests();
        _idDispenser = new IdDispenser();
        EntityTypeTree = new EntityTypeTree(Game.I.Serializer.ConcreteEntityTypes);
        Init();
    }
    protected virtual void Init()
    {
        Registers = new Dictionary<Type, IEntityRegister>();
        Notices = new DataNotices();
        RefFulfiller = new RefFulfiller(this);
        Models = new Models();
        Entities = new Dictionary<int, Entity>();
        
        BaseDomain = new BaseDomain(this);
        Planet = new PlanetDomain(this);
        Society = new SocietyDomain(this);
        Infrastructure = new InfrastructureDomain(this);
        BaseDomain.Setup();
        Planet.Setup();
        Society.Setup();
        Infrastructure.Setup();
        
        ClientPlayerData = new ClientPlayerData(this);
        HostLogicData = new HostLogicData(this);
        Handles = new DataHandles();
        
        var s = Game.I.Serializer;
        var entityTypes = s.ConcreteEntityTypes;
        foreach (var entityType in entityTypes)
        {
            var register = EntityRegister<Entity>.ConstructFromType(entityType, this);
            Registers.Add(entityType, register);
        }
    }
    
    public void AddEntity(Entity e, StrongWriteKey key)
    {
        if (Entities.ContainsKey(e.Id))
        {
            GD.Print($"trying to overwrite {Entities[e.Id].GetType().ToString()} with {e.GetType().ToString()}");
        }
        Entities.Add(e.Id, e);
        e.GetEntityTypeTreeNode().PropagateCreation(e);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntityCreationUpdate.Create(e, hKey));
        }
    }
    public void AddEntities(IEnumerable<Entity> es, StrongWriteKey key) 
    {
        foreach (var e in es)
        {
            if (Entities.ContainsKey(e.Id))
            {
                throw new EntityTypeException($"trying to overwrite {Entities[e.Id].GetType().ToString()} " +
                                              $"with {e.GetType().ToString()}");
            }
            Entities.Add(e.Id, e);
        }
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntitiesCreationUpdate.Create(es.ToList(), hKey));
        }
        foreach (var e in es)
        {
            e.GetEntityTypeTreeNode().PropagateCreation(e);
        }
    }
    public void AddEntities<TEntity>(IReadOnlyList<TEntity> es, StrongWriteKey key) where TEntity : Entity
    {
        foreach (var e in es)
        {
            if (Entities.ContainsKey(e.Id))
            {
                throw new EntityTypeException($"trying to overwrite {Entities[e.Id].GetType().ToString()} " +
                                              $"with {e.GetType().ToString()}");
            }
            Entities.Add(e.Id, e);
        }

        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntitiesCreationUpdate.Create(es, hKey));
        }
        foreach (var entity in es)
        {
            entity.GetEntityTypeTreeNode().PropagateCreation(entity);
        }
    }
    public void RemoveEntities(int[] entityIds, StrongWriteKey key)
    {
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntitiesDeletionUpdate.Create(entityIds, hKey));
        }
        foreach (var eId in entityIds)
        {
            var e = Entities[eId];
            e.GetEntityTypeTreeNode().PropagateDestruction(e);
        }
        foreach (var eId in entityIds)
        {
            Entities.Remove(eId);
            RefFulfiller.EntityRemoved(eId);
        }
    }
    public void RemoveEntity(int eId, StrongWriteKey key)
    {
        var e = Entities[eId];
        e.GetEntityTypeTreeNode().PropagateDestruction(e);
        Entities.Remove(eId);
        RefFulfiller.EntityRemoved(eId);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntityDeletionUpdate.Create(eId, hKey));
        }
    }

    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        return (EntityRegister<T>)Registers[typeof(T)];
    }
    public void GetIdDispenser(CreateWriteKey key)
    {
        key.SetIdDispenser(_idDispenser);
    }
    public void SubscribeForCreation<TEntity>(Action<EntityCreatedNotice> callback) where TEntity : Entity
    {
        EntityTypeTree[typeof(TEntity)].Created.Subscribe(callback);
    }
    public void SubscribeForCreation<TEntity>(RefAction<EntityCreatedNotice> callback) where TEntity : Entity
    {
        EntityTypeTree[typeof(TEntity)].Created.Subscribe(callback);
    }
    public void SubscribeForDestruction<TEntity>(Action<EntityDestroyedNotice> callback) where TEntity : Entity
    {
        EntityTypeTree[typeof(TEntity)].Destroyed.Subscribe(callback);
    }
}
