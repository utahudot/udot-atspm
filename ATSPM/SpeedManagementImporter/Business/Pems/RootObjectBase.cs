public class RootObjectBase
{
    public Status status { get; set; }
    public int count { get; set; }
}

public class Status
{
    public int error { get; set; }
    public string description { get; set; }
}