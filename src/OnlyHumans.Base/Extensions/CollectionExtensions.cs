namespace OnlyHumans;

public static class CollectionExtensions
{
    public static string JoinWith(this IEnumerable<string> s, string j)
    {
        if (s.Count() == 0)
        {
            return "";
        }
        else if (s.Count() == 1)
        {
            return s.First();
        }
        else
        {
            return s.Aggregate((a, b) => a + j + b);
        }
    }

    public static string JoinWithSpaces(this IEnumerable<string> s) => s.JoinWith(" ");

    public static T FailIfKeyNotPresent<T>(this Dictionary<string, object> d, string k)
        => d.ContainsKey(k) && d[k] is T ? (T)d[k] : throw new KeyNotFoundException($"The required key {k} of type {typeof(T).Name} is not present.");

    public static T? TryGet<T>(this Dictionary<string, object> d, string k) => d.ContainsKey(k) ? (T)d[k] : default(T);

    public static T Get<T>(this Dictionary<string, object> d, string k) => d.ContainsKey(k) ? (T)d[k] : throw new KeyNotFoundException();

    public static T[] ConcatArrays<T>(this IEnumerable<T[]> arrays)
    {
        if (arrays == null || !arrays.Any())
        {
            return Array.Empty<T>();
        }
        int totalLength = arrays.Sum(arr => arr.Length);
        T[] result = new T[totalLength];
        int offset = 0;
        foreach (var array in arrays)
        {
            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }
        return result;
    }

    public static T? PeekIfNotEmpty<T>(this Stack<T> q) => q.Count > 0 ? q.Peek() : default(T);

    public static T? PopIfNotEmpty<T>(this Stack<T> q) => q.Count > 0 ? q.Pop() : default(T);

    public static void Pop<T>(this Stack<T> stack, int n)
    {
        for (int i = 1; i <= n; i++)
        {
            stack.Pop();
        }
    }   

    public static T LastItem<T>(this List<T> list) => list.Count > 0 ? list[list.Count - 1] : throw new InvalidOperationException("This list is empty.");
}