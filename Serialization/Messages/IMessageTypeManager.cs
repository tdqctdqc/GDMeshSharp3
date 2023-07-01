
using System;

public interface IMessageTypeManager
{
    Type GetMessageTypeFromMarker(byte marker);
    byte GetMarkerFromMessageType(Type type);
}
