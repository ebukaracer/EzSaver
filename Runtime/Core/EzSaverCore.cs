using System;

#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Racer.EzSaverTest")]
#endif

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Core class for EzSaver, providing methods for initialization, reading and writing of data.
    /// </summary>
    /// <remarks>
    /// This class is designed to be used with <see cref="Racer.EzSaver.Utilities.EzSaverManager"/> for managing save-files.
    /// </remarks>
    public class EzSaverCore : EzSaverBase
    {
        #region Setup

        /// <summary>
        /// Initializes a new instance of the <see cref="EzSaverCore"/> class with the specified content source.
        /// </summary>
        /// <param name="contentSource">The source of the content, typically a file path or JSON string-literal.</param>
        /// <param name="isJsonStringLiteral">Indicates whether the content source is a JSON string-literal.</param>
        /// <param name="useSecurity">Indicates whether security features (e.g., encryption) should be enabled.</param>
        internal EzSaverCore(string contentSource, bool isJsonStringLiteral = false, bool useSecurity = false)
            : this(contentSource, isJsonStringLiteral,
                new EzSaverSettings(new FileReader(contentSource), new AesEncryptor(), useSecurity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EzSaverCore"/> class with the specified content source and settings.
        /// </summary>
        /// <param name="contentSource">The source of the content, typically a file path or JSON string-literal.</param>
        /// <param name="isJsonStringLiteral">Indicates whether the content source is a JSON string-literal.</param>
        /// <param name="settings">The settings to use for the EzSaver instance.</param>
        /// <remarks>
        /// Best used with custom settings for file reading and encryption.
        /// </remarks>
        internal EzSaverCore(string contentSource, bool isJsonStringLiteral, EzSaverSettings settings)
            : base(contentSource, isJsonStringLiteral, settings)
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
            return Exists(key) ? EzSaverSerializer.DeserializeKey<T>(key, SaveData) : defaultValue;
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

                InvokeOnSaveDataModified();
                HasSavedNewChanges = false;
            }
            catch (Exception ex)
            {
                throw new EzSaverException($"Serialization failed for '{key}'. \n{ex.Message}", ex);
            }

            return this;
        }

        #endregion
    }
}