namespace OnlyHumans;

public interface IPlugin
{
    Dictionary<string, Dictionary<string, object>> SharedState { get; set; }
}
