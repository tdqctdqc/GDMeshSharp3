using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public abstract class Message
{
    protected static Dictionary<Type, byte> _markers;
    protected static Dictionary<Type, IMessageTypeManager> _typeManagers;
    protected static Dictionary<byte, Type> _typesByMarker;

    public static void Setup()
    {
        _typeManagers = new Dictionary<Type, IMessageTypeManager>();
        _markers = new Dictionary<Type, byte>();
        _typesByMarker = new Dictionary<byte, Type>();
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var msgTypes = typeof(Message).GetDirectlyDerivedTypes(types).OrderBy(t => t.FullName).ToList();
        
        if (msgTypes.Count > 254) throw new Exception();
        for (var i = 0; i < msgTypes.Count; i++)
        {
            var type = msgTypes[i];
            var marker = Convert.ToByte(i);
            _markers.Add(type, marker);
            _typesByMarker.Add(marker, type);
            var manag = MessageTypeManager<Message>.Construct(type);
            _typeManagers.Add(type, manag);
        }
    }

    public static Type GetTypeFromMarker(byte marker)
    {
        return _typesByMarker[marker];
    }

    public static Type GetSubType(byte marker, byte subMarker)
    {
        return _typeManagers[_typesByMarker[marker]].GetMessageTypeFromMarker(subMarker);
    }
    
    public byte[] Wrap()
    {
        var innerBytes = Game.I.Serializer.MP.Serialize(this, GetType());
        var wrapper = new MessageWrapper(GetMarker(), GetSubMarker(), innerBytes);
        return Game.I.Serializer.MP.Serialize(wrapper);
    }
    protected abstract byte GetMarker();
    protected abstract byte GetSubMarker();
    public abstract void HandleHost(HostLogic logic);
    public abstract void HandleRemote(RemoteLogic logic);
}
