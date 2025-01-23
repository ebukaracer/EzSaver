using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Racer.EzSaver.Converters
{
    public class SpriteConverter : JsonConverter<Sprite>
    {
        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
        {
            JArray array = new(value.texture, value.rect, value.pivot, value.pixelsPerUnit, value.border);
            array.WriteTo(writer);
        }

        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return Sprite.Create(array[0].ToObject<Texture2D>(serializer),
                array[1].ToObject<Rect>(serializer),
                array[2].ToObject<Vector2>(serializer),
                array[3].ToObject<float>(),
                0, SpriteMeshType.Tight,
                array[4].ToObject<Vector4>(serializer)
            );
        }
    }
}