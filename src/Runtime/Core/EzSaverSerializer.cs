using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Provides serialization and deserialization for EzSaver using Newtonsoft.Json.
    /// </summary>
    public static class EzSaverSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = EzSaverConfig.Load.FileFormatting
        };

        private static JsonSerializer _serializer = JsonSerializer.Create(Settings);

        /// <summary>
        /// Dynamically registers all converters from a provided list of converters.
        /// </summary>
        /// <param name="converters">List of converters to register.</param>
        public static void RegisterConverters(IEnumerable<JsonConverter> converters)
        {
            foreach (var converter in converters)
                RegisterConverter(converter);
        }

        /// <summary>
        /// Registers a single converter.
        /// </summary>
        /// <param name="converter">The converter to register.</param>
        public static void RegisterConverter(JsonConverter converter)
        {
            var canRegister = Settings.Converters.All(x => x.GetType() != converter.GetType());

            if (!canRegister) return;

            Settings.Converters.Add(converter);
            _serializer = JsonSerializer.Create(Settings);
        }

        internal static T DeserializeKey<T>(string key, JObject data)
        {
            return data[key].ToObject<T>(_serializer);
        }

        internal static JToken SerializeKey<T>(T data)
        {
            return JToken.FromObject(data, _serializer);
        }

        internal static string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }
    }
}