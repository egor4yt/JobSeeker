using System.Text.Json;

namespace JobSeeker.Deduplication.Shared;

public static class SerializationHelper
{
    public static string SerializeJson(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public static T? DeserializeJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
}