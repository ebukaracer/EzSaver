using System;
using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Core class for EzSaver, providing methods for reading and writing data.
    /// </summary>
    public class EzSaverCore : EzSaverBase
    {
        #region Setup

        /// <summary>
        /// Initializes a new instance of the <see cref="EzSaverCore"/> class.
        /// </summary>
        /// <param name="contentSource">The source of the content(either a filename, eg. SaveFile.txt or a literal string, eg. "{ "Highscore": 4 }").</param>
        /// <param name="fromLiteral">Indicates whether the content is from a literal string(true) or a file(false-default).</param>
        /// <param name="settings">The settings for EzSaver.</param>
        /// <remarks>
        /// Initialize with <paramref name="settings"/> to use custom settings, such as a custom <see cref="IReader"/> or <see cref="IEncryptor"/>.
        /// </remarks>
        public EzSaverCore(string contentSource, bool fromLiteral, EzSaverSettings settings)
            : base(contentSource, fromLiteral, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EzSaverCore"/> class with default settings.
        /// </summary>
        /// <param name="contentSource">The source of the content(either a filename, eg. SaveFile.txt or a literal string, eg. "{ "Highscore": 4 }").</param>
        /// <param name="fromLiteral">Indicates whether the content is from a literal string(true) or a file(false-default).</param>
        /// <param name="useSecurity">Indicates whether to use security features(encryption/decryption).</param>
        /// <remarks>
        /// Content source is loaded automatically after initialization.
        /// </remarks>
        public EzSaverCore(string contentSource, bool fromLiteral = false, bool useSecurity = false)
            : this(contentSource, fromLiteral,
                new EzSaverSettings(new FileReader(contentSource), new AesEncryptor(), useSecurity))
        {
        }

        #endregion

        #region Read

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        /// <returns>The value associated with the specified key, or the default value if the key does not exist.</returns>
        public T Read<T>(string key, T defaultValue = default)
        {
            if (Exists(key)) return EzSaverSerializer.DeserializeKey<T>(key, SaveData);

            EzLogger.Warn($"'{key}' did not exist initially, default value was used.");

            return defaultValue;
        }

        /// <summary>
        /// Tries to read a value of type <typeparamref name="T"/> associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="result">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the result parameter.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="EzSaverException">Thrown when deserialization fails.</exception>
        public bool TryRead<T>(string key, out T result)
        {
            if (!Exists(key))
            {
                result = default;
                return false;
            }

            try
            {
                result = EzSaverSerializer.DeserializeKey<T>(key, SaveData);
            }
            catch (Exception ex)
            {
                throw new EzSaverException($"Deserialization failed. \n{ex.Message}", ex);
            }

            return true;
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="key">The key associated with the value.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>The current instance of <see cref="EzSaverCore"/>.</returns>
        /// <exception cref="EzSaverException">Thrown when the key or value is null, or when serialization fails.</exception>
        public EzSaverCore Write<T>(string key, T value)
        {
            if (value is null || key is null)
                throw new EzSaverException($"'{typeof(T).Name}' or '{key}' cannot be null.");

            try
            {
                if (Exists(key))
                    SaveData[key] = EzSaverSerializer.SerializeKey(value);
                else
                    SaveData.Add(key, EzSaverSerializer.SerializeKey(value));
            }
            catch (Exception ex)
            {
                throw new EzSaverException($"Serialization failed. \n{ex.Message}", ex);
            }

            return this;
        }

        #endregion
    }
}