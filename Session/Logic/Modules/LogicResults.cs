
using System;
using System.Collections.Generic;
using System.Linq;

public class LogicResults
{
    public List<Procedure> Procedures { get; private set; }
    public List<Update> Updates { get; private set; }
    public List<Func<HostWriteKey, Entity>> CreateEntities { get; private set; }

    public LogicResults()
    {
        Updates = new List<Update>();
        Procedures = new List<Procedure>();
        CreateEntities = new List<Func<HostWriteKey, Entity>>();
    }
    public LogicResults(IEnumerable<Message> messages, IEnumerable<Func<HostWriteKey, Entity>> createEntities)
    {
        Updates = messages.SelectWhereOfType<Message, Update>().ToList();
        Procedures = messages.SelectWhereOfType<Message, Procedure>().ToList();
        CreateEntities = createEntities.ToList();
    }
}
