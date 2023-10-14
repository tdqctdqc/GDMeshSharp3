
using System;

public class MessageWrapper : PolymorphMessage<Message>
{
    public MessageWrapper(Message message, Data data) 
        : base(message.GetType(),
            data.Serializer.MP.Serialize(message, message.GetType()))
    {
    }
}
