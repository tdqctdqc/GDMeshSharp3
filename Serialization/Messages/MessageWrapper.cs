
public class MessageWrapper
{
    public byte Marker { get; set; }
    public byte SubMarker { get; set; }
    public byte[] Data { get; set; }

    public MessageWrapper(byte marker, byte subMarker, byte[] data)
    {
        Marker = marker;
        SubMarker = subMarker;
        Data = data;
    }
    
    public Message Unwrap()
    {
        var subType = Message.GetSubType(Marker, SubMarker);
        return (Message)Game.I.Serializer.MP.Deserialize(Data, subType);
    }
}
