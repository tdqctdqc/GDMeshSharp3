using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public class Data
{
    public IdDispenser IdDispenser => BaseDomain.IdDispenser;
    public LogicRequests Requests { get; private set; }
    public ClientPlayerData ClientPlayerData { get; private set; }
    public HostLogicData HostLogicData { get; private set; }
    public DataNotices Notices { get; private set; }
    public Models Models { get; private set; }
    public Dictionary<int, Entity> EntitiesById { get; private set; }
    public Entity this[int id] => EntitiesById[id];
    public BaseDomain BaseDomain { get; private set; }
    public PlanetDomain Planet { get; private set; }
    public SocietyDomain Society { get; private set; }
    public MilitaryDomain Military { get; private set; }
    public InfrastructureDomain Infrastructure { get; private set; }
    private EntityTypeTree _entityTypeTree;
    public int Tick => BaseDomain.GameClock.Tick;
    public Serializer Serializer { get; private set; }

    public Data()
    {
        Serializer = new Serializer();
        Requests = new LogicRequests();
        _entityTypeTree = new EntityTypeTree(this);
        Init();
    }
    protected virtual void Init()
    {
        Notices = new DataNotices();
        Models = new Models(this);
        EntitiesById = new Dictionary<int, Entity>();
        
        BaseDomain = new BaseDomain();
        Planet = new PlanetDomain(this);
        Society = new SocietyDomain();
        Infrastructure = new InfrastructureDomain();
        Military = new MilitaryDomain();
        BaseDomain.Setup(this);
        Planet.Setup();
        Society.Setup(this);
        Infrastructure.Setup(this);
        Military.Setup(this);
        
        ClientPlayerData = new ClientPlayerData(this);
        HostLogicData = new HostLogicData(this);
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
            if (e is IdDispenser id)
            {
                e.SetId(id.TakeId(), key);
            }
            else
            {
                e.SetId(IdDispenser.TakeId(), key);
            }
        }
        
        if (EntitiesById.ContainsKey(e.Id))
        {
            GD.Print($"trying to overwrite id {e.Id} " +
                     $"{EntitiesById[e.Id].GetType().ToString()} " +
                     $"with {e.GetType().ToString()}");
        }
        EntitiesById.Add(e.Id, e);
        _entityTypeTree.Get(e.GetType()).Propagate(EntityCreatedNotice.Get(e));
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueMessage(EntityCreationUpdate.Create(e, hKey));
        }
        
        IdDispenser.SetMin(e.Id);
    }
    public void AddEntities<TEntity>(IReadOnlyList<TEntity> es, StrongWriteKey key) where TEntity : Entity
    {
        foreach (var e in es)
        {
            AddEntity(e, key);
        }
    }
    public void LoadEntities(IReadOnlyList<Entity> es, StrongWriteKey key) 
    {

        foreach (var e in es)
        {
            var t = e.GetType();
            if (e.GetType().ToString() == "Player")
            {
                GD.Print("loading player");
            }
            if (_entityTypeTree.Nodes.ContainsKey(t) == false)
            {
                AddEntityType(t);
            }
            if (EntitiesById.ContainsKey(e.Id))
            {
                continue;
                GD.Print($"{e.Id} trying to overwrite " +
                         $"{EntitiesById[e.Id].GetType()} with {e.GetType()}");
            }
            EntitiesById.Add(e.Id, e);
        }
        foreach (var e in es)
        {
            _entityTypeTree.Get(e.GetType()).Propagate(EntityCreatedNotice.Get(e));
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
            e.SetId(IdDispenser.TakeId(), key);
        }
        IdDispenser.SetMin(e.Id);
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
