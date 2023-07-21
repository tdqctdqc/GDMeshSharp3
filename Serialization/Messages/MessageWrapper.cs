
using System;

public class MessageWrapper : PortablePolymorph<Message>
{
    public MessageWrapper(Message message) 
        : base(message.GetType(),
            Game.I.Serializer.MP.Serialize(message, message.GetType()))
    {
    }
}
