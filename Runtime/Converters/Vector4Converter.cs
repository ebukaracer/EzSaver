using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Racer.EzSaver.Converters
{
    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            JArray array = new(value.x, value.y, value.z, value.w);
            array.WriteTo(writer);            
        }

        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return new Vector4((float)array[0], (float)array[1], (float)array[2], (float)array[3]);
        }
    }
}