using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Racer.EzSaver.Converters
{
    public class Texture2DConverter : JsonConverter<Texture2D>
    {
        public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
        {
            JArray array = new(value.GetRawTextureData(), value.width, value.height, value.format, value.mipmapCount);
            array.WriteTo(writer);
        }

        public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            var texture2D = new Texture2D((int)array[1], (int)array[2], array[3].ToObject<TextureFormat>(),
                (int)array[4] > 0);
            texture2D.LoadRawTextureData((byte[])array[0]);
            texture2D.Apply();
            return texture2D;
        }
    }
}