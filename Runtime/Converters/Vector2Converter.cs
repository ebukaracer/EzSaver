using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Racer.EzSaver.Converters
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            JArray array = new(value.x, value.y);
            array.WriteTo(writer);
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return new Vector2((float)array[0], (float)array[1]);
        }
    }
}