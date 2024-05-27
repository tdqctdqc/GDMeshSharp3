
using System;

public class SendMessageCommand : Command
{
    public Message Message { get; private set; }
    public SendMessageCommand(
        Message message,
        Guid commandingPlayerGuid) 
        : base(commandingPlayerGuid)
    {
        Message = message;
    }

    public override void Enact(LogicWriteKey key)
    {
        key.SendMessage(Message);
    }

    public override bool Valid(Data data, out string error)
    {
        if (Message is Procedure p)
        {
            if (p.Valid(data, out var procError) == false)
            {
                error = procError;
                return false;
            }
        }
        

        error = "";
        return true;
    }
}