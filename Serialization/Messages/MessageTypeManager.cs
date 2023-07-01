
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;


public class MessageTypeManager<T> : IMessageTypeManager
    where T : Message
{
    private Dictionary<Type, byte> _markersBySubType;
    private Dictionary<byte, Type> _subTypesByMarker;

    public static IMessageTypeManager Construct(Type messageType)
    {
        var type = typeof(MessageTypeManager<>).MakeGenericType(messageType);
        var get = type.GetMethod(nameof(Get), BindingFlags.Static | BindingFlags.NonPublic);
        return (IMessageTypeManager) get.Invoke(null, null);
    }

    private static MessageTypeManager<T> Get()
    {
        return new MessageTypeManager<T>();
    }
    public MessageTypeManager()
    {
        _markersBySubType = new Dictionary<Type, byte>();
        _subTypesByMarker = new Dictionary<byte, Type>();
        var types = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<T>().OrderBy(t => t.Name).ToList();
        if (types.Count > 254) throw new Exception();
        for (var i = 0; i < types.Count; i++)
        {
            var subType = types[i];
            var marker = Convert.ToByte(i);
            _markersBySubType.Add(subType, marker);
            _subTypesByMarker.Add(marker, subType);
        }
    }
    public Type GetMessageTypeFromMarker(byte marker)
    {
        return _subTypesByMarker[marker];
    }
    public byte GetMarkerFromMessageType(Type type)
    {
        return _markersBySubType[type];
    }
}
