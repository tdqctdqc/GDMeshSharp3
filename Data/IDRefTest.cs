
public class IDRefTest
{
    
}

public class Test
{
    public IdRef TestRef { get; private set; }

    public Test(IdRef testRef)
    {
        TestRef = testRef;
    }
}

public class TestRef<T> : ITestRef
{
    public int RefId { get; }

    public TestRef(int refId)
    {
        RefId = refId;
    }
}

public interface ITestRef
{
    int RefId { get; }
        
}