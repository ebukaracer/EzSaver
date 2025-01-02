using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global

namespace Racer.EzSaver.Demo
{
    public class DefaultConverters
    {
        /// <summary>
        /// Retrieves a list of all default converters.
        /// </summary>
        /// <returns>List of JsonConverter objects.</returns>
        public static List<JsonConverter> AvailableConverters()
        {
            return new List<JsonConverter>
            {
                new ColorConverter(),
                new QuaternionConverter(),
                new Matrix4X4Converter(),
                new Texture2DConverter(),
                new SpriteConverter(),
                new Vector2Converter(),
                new Vector3Converter(),
                new Vector4Converter()
            };
        }

        /// <summary>
        /// Registers all default converters with the EzSaverSerializer.
        /// </summary>
        public static void RegisterAll()
        {
            EzSaverSerializer.RegisterConverters(AvailableConverters());
        }
    }
}