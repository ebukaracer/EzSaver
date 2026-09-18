using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Racer.EzSaver.Converters
{
    public class Matrix4X4Converter : JsonConverter<Matrix4x4>
    {
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            JArray array = new(value.m00, value.m01, value.m02, value.m03,
                value.m10, value.m11, value.m12, value.m13,
                value.m20, value.m21, value.m22, value.m23,
                value.m30, value.m31, value.m32, value.m33);
            array.WriteTo(writer);
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return new Matrix4x4
            {
                m00 = (float)array[0],
                m01 = (float)array[1],
                m02 = (float)array[2],
                m03 = (float)array[3],
                m10 = (float)array[4],
                m11 = (float)array[5],
                m12 = (float)array[6],
                m13 = (float)array[7],
                m20 = (float)array[8],
                m21 = (float)array[9],
                m22 = (float)array[10],
                m23 = (float)array[11],
                m30 = (float)array[12],
                m31 = (float)array[13],
                m32 = (float)array[14],
                m33 = (float)array[15]
            };
        }
    }
}