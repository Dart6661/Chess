using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chess.Shared.DtoMapping
{
    public static class JsonHandler
    {
        private static readonly JsonSerializerOptions options = new() { Converters = { new JsonStringEnumConverter() } };

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options) ?? throw new DataTypeException("deserialization failed");
        }

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
