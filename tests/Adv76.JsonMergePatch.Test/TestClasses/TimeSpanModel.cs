using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class TimeSpanModel
{
    [JsonConverter(typeof(CustomTimeSpanJsonConverter))]
    public TimeSpan TimeSpan1 { get; set; }

    internal class CustomTimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            var p1 = reader.GetString();
            reader.Read();
            var v1 = reader.GetInt32();
            reader.Read();
            var p2 = reader.GetString();
            reader.Read();
            var v2 = reader.GetInt32();
            reader.Read();
            return new TimeSpan(p1 == "hr" ? v1 : v2, p1 == "min" ? v1 : v2, 0);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("hr");
            writer.WriteNumberValue(value.Hours);
            writer.WritePropertyName("min");
            writer.WriteNumberValue(value.Minutes);
            writer.WriteEndObject();
        }
    }
}
