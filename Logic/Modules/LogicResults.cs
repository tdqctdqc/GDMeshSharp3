
using System;
using System.Collections.Generic;
using System.Linq;

public class LogicResults
{
    public List<Message> Messages { get; private set; }

    public LogicResults()
    {
        Messages = new List<Message>();
    }
    public LogicResults(IEnumerable<Message> messages, IEnumerable<Func<HostWriteKey, Entity>> createEntities)
    {
        Messages = messages.ToList();
    }
}
