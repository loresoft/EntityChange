namespace EntityChange.Tests.Models;

public abstract class AbstractBase
{
    public int Id { get; set; }
}

public class ConcreteClass : AbstractBase
{
    public string SomeString { get; set; }
}

public class Consumer
{
    public AbstractBase SomeProperty { get; set; }
}
