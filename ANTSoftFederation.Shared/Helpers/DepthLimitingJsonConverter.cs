using System.Text.Json;
using System.Text.Json.Serialization;

namespace SorteFederation.Shared.Helpers;

public class DepthLimitingJsonConverter<T> : JsonConverter<T> where T : class
{
    private readonly int _maxDepth;
    private readonly ThreadSafeCounter _depthCounter = new();

    public DepthLimitingJsonConverter(int maxDepth = 10)
    {
        _maxDepth = maxDepth;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization, use default behavior
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(this); // Avoid infinite recursion
        return JsonSerializer.Deserialize<T>(ref reader, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var currentDepth = _depthCounter.Increment();

        try
        {
            if (currentDepth > _maxDepth)
            {
                // Write a simplified representation or null
                writer.WriteStringValue($"[Max depth {_maxDepth} exceeded for type {typeof(T).Name}]");
                return;
            }

            // Create new options without this converter to avoid infinite recursion
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);

            JsonSerializer.Serialize(writer, value, newOptions);
        }
        finally
        {
            _depthCounter.Decrement();
        }
    }
}

internal class ThreadSafeCounter
{
    [ThreadStatic]
    private static int _counter;

    public int Increment() => ++_counter;
    public void Decrement() => _counter--;
    public int Current => _counter;
}

