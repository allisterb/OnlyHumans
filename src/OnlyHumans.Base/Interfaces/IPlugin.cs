namespace OnlyHumans;

public interface IPlugin
{
    string Name { get; }    

    Dictionary<string, Dictionary<string, object>> SharedState { get; set; }
}
