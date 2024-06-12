
using System;

public class SendMessagesCommand : Command
{
    public Message[] Messages { get; private set; }
    public SendMessagesCommand(
        Message[] messages,
        Guid commandingPlayerGuid) 
        : base(commandingPlayerGuid)
    {
        Messages = messages;
    }

    public override void Enact(LogicWriteKey key)
    {
        for (var i = 0; i < Messages.Length; i++)
        {
            key.SendMessage(Messages[i]);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        for (var i = 0; i < Messages.Length; i++)
        {
            var m = Messages[i];
            if (m is Procedure p)
            {
                if (p.Valid(data, out var procError) == false)
                {
                    error = procError;
                    return false;
                }
            }
        }
        error = "";
        return true;
    }
}