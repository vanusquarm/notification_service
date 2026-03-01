using System.Collections.Concurrent;
using System.Reflection;

public static class SqlLoader
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    private static readonly ConcurrentDictionary<string, string> _cache = new();

    public static string Load(string resourceName)
    {
        return _cache.GetOrAdd(resourceName, name =>
        {
            using var stream = _assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"SQL resource not found: {name}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
    }
}