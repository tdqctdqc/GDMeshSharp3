
using System;

public class MessageWrapper : PortablePolymorph<Message>
{
    public MessageWrapper(Message message, Data data) 
        : base(message.GetType(),
            data.Serializer.MP.Serialize(message, message.GetType()))
    {
    }
}
