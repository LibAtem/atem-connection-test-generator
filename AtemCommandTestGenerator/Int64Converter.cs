using Newtonsoft.Json;
using System;

namespace AtemCommandTestGenerator
{
    public class Int64Converter : JsonConverter<long>
    {
        public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
        {
            if (writer.Path.EndsWith("SourceId") || writer.Path.Contains("LongData"))
                writer.WriteValue(value.ToString());
            else
                writer.WriteValue(value);
        }

        public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Note: we don't deserialize in a way that will require this to do any conversion
            return (long)reader.Value;
        }

    }

    // public class Int64ArrayConverter : JsonConverter<long[]>
    // {
    //     public override void WriteJson(JsonWriter writer, long[] value, JsonSerializer serializer)
    //     {
    //         writer.WriteValue(value.ToString());
    //     }

    //     public override long[] ReadJson(JsonReader reader, Type objectType, long[] existingValue, bool hasExistingValue, JsonSerializer serializer)
    //     {
    //         // Note: we don't deserialize in a way that will require this to do any conversion
    //         return (long[])reader.Value;
    //     }

    // }
}
