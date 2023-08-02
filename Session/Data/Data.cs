using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public class Data
{
    private IdDispenser _idDispenser;
    public LogicRequests Requests { get; private set; }
    public ClientPlayerData ClientPlayerData { get; private set; }
    public HostLogicData HostLogicData { get; private set; }
    public DataNotices Notices { get; private set; }
    public DataHandles Handles { get; private set; }
    public Models Models { get; private set; }
    // public RefFulfiller RefFulfiller { get; private set; }
    public Dictionary<int, Entity> EntitiesById { get; private set; }
    public Entity this[int id] => EntitiesById[id];
    public BaseDomain BaseDomain { get; private set; }
    public PlanetDomain Planet { get; private set; }
    public SocietyDomain Society { get; private set; }
    public InfrastructureDomain Infrastructure { get; private set; }
    private EntityTypeTree _entityTypeTree;
    public int Tick => BaseDomain.GameClock.Tick;
    public Serializer Serializer { get; private set; }

    public Data()
    {
        Serializer = new Serializer();
        Requests = new LogicRequests();
        _idDispenser = new IdDispenser();
        _entityTypeTree = new EntityTypeTree(this);
        Init();
    }
    protected virtual void Init()
    {
        GD.Print("doot");

        Notices = new DataNotices();
        // RefFulfiller = new RefFulfiller(this);
        Models = new Models(this);
        EntitiesById = new Dictionary<int, Entity>();
        
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
    }

    private void AddEntityType(Type t)
    {
        _entityTypeTree.Get(t);
    }
    public void AddEntity(Entity e, StrongWriteKey key)
    {
        var t = e.GetType();
        if (_entityTypeTree.Nodes.ContainsKey(t) == false)
        {
            AddEntityType(t);
        }
        if (e.Id == -1)
        {
            e.SetId(_idDispenser.GetID(), key);
        }
        _idDispenser.SetMin(e.Id);
        if (EntitiesById.ContainsKey(e.Id))
        {
            GD.Print($"trying to overwrite {EntitiesById[e.Id].GetType().ToString()} with {e.GetType().ToString()}");
        }
        EntitiesById.Add(e.Id, e);
        _entityTypeTree.Get(e.GetType()).Propagate(EntityCreatedNotice.Get(e));
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntityCreationUpdate.Create(e, hKey));
        }
    }
    public void AddEntities<TEntity>(IReadOnlyList<TEntity> es, StrongWriteKey key) where TEntity : Entity
    {
        foreach (var e in es)
        {
            AddEntity(e, key);
        }
    }

    private void SetupEntity(Entity e, StrongWriteKey key)
    {
        var t = e.GetType();
        if (_entityTypeTree.Nodes.ContainsKey(t) == false)
        {
            AddEntityType(t);
        }
        if (e.Id == -1)
        {
            e.SetId(_idDispenser.GetID(), key);
        }
        _idDispenser.SetMin(e.Id);
        if (EntitiesById.ContainsKey(e.Id))
        {
            throw new EntityTypeException($"trying to overwrite {EntitiesById[e.Id].GetType().ToString()} " +
                                          $"with {e.GetType().ToString()}");
        }
        EntitiesById.Add(e.Id, e);
    }
    public void RemoveEntities(int[] entityIds, StrongWriteKey key)
    {
        foreach (var entityId in entityIds)
        {
            RemoveEntity(entityId, key);
        }
    }
    public void RemoveEntity(int eId, StrongWriteKey key)
    {
        var e = EntitiesById[eId];
        key.Data._entityTypeTree.Get(e.GetType()).Propagate(EntityDestroyedNotice.Get(e));
        EntitiesById.Remove(eId);
        // RefFulfiller.EntityRemoved(eId);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntityDeletionUpdate.Create(eId, hKey));
        }
    }

    public void SubscribeForCreation<TEntity>(Action<EntityCreatedNotice> callback) where TEntity : Entity
    {
        _entityTypeTree.Get(typeof(TEntity)).Created.Subscribe(callback);
    }
    public void SubscribeForCreation<TEntity>(RefAction<EntityCreatedNotice> callback) where TEntity : Entity
    {
        _entityTypeTree.Get(typeof(TEntity)).Created.Subscribe(callback);
    }
    public void SubscribeForDestruction<TEntity>(Action<EntityDestroyedNotice> callback) where TEntity : Entity
    {
        _entityTypeTree.Get(typeof(TEntity)).Destroyed.Subscribe(callback);
    }

    public T Get<T>(int id) where T : Entity
    {
        return (T) EntitiesById[id];
    }

    public HashSet<T> GetAll<T>() where T : Entity
    {
        return _entityTypeTree.Get<T>().Entities;
    }

    public EntityMeta<T> GetEntityMeta<T>() where T : Entity
    {
        return (EntityMeta<T>)_entityTypeTree.Get(typeof(T)).Meta;
    }
    public IEntityMeta GetEntityMeta(Type entityType) 
    {
        return _entityTypeTree.Get(entityType).Meta;
    }

    public EntityTypeTreeNode<T> GetEntityTypeNode<T>() where T : Entity
    {
        return _entityTypeTree.Get<T>();
    }
    public IEnumerable<IEntityTypeTreeNode> GetAllEntityTypeNodes()
    {
        return _entityTypeTree.Nodes.Values;
    }
}
