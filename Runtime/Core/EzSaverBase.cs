using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Encapsulating some operations that may be performed to the save-file.
    /// </summary>
    public enum SaveDataAction
    {
        ClearAll,
        DeleteFile
    }

    /// <summary>
    /// Base class for EzSaver, providing base functionality for <see cref="EzSaverCore"/>.
    /// </summary>
    public class EzSaverBase
    {
        private (string, bool) _literalInUse;

        private string _loadedJson;
        private bool _wasDecrypted;

        private readonly EzSaverSettings _ezSaverSettings;
        protected JObject SaveData { get; private set; } = new();

        public event Action<SaveDataAction> OnSaveDataAction;


        /// <summary>
        /// <seealso cref="EzSaverCore"/>
        /// </summary>
        protected EzSaverBase(string contentSource, bool fromLiteral, EzSaverSettings ezSaverSettings)
        {
            _ezSaverSettings = ezSaverSettings;
            _literalInUse = (contentSource, fromLiteral);

            LoadFromSource();
        }

        /// <summary>
        /// Gets all keys in the current save-data.
        /// </summary>
        /// <returns>A collection of all keys.</returns>
        public IEnumerable<string> GetAllKeys() => SaveData.Properties().Select(x => x.Name).ToList();

        /// <summary>
        /// Checks if a key exists in the current save-data.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool Exists(string key) => SaveData.ContainsKey(key);

        /// <summary>
        /// Clears the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to clear.</param>
        /// <returns><c>true</c> if the key was cleared; otherwise, <c>false</c>.</returns>
        public bool Clear(string key) => Exists(key) && SaveData.Remove(key);

        /// <summary>
        /// Clears all keys and values in the current save-data.
        /// </summary>
        public void ClearAll()
        {
            SaveData.RemoveAll();
            OnSaveDataAction?.Invoke(SaveDataAction.ClearAll);
        }

        /// <summary>
        /// Deletes the file associated with the current settings.
        /// </summary>
        /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
        /// <exception cref="EzSaverException">Thrown if the content source is a literal string.</exception>
        public void DeleteFile()
        {
            if (_literalInUse.Item2)
                throw new EzSaverException(
                    "Cannot delete a string-literal initialized in the constructor like a file." +
                    "\nIf you intend to delete to a file, initialize with a filename instead.");

            if (_ezSaverSettings.Reader.Delete())
                OnSaveDataAction?.Invoke(SaveDataAction.DeleteFile);
            else
                EzLogger.Warn($"Failed to delete save file: '{FileHelper.AssignExtension(_literalInUse.Item1)}'.");
        }

        /// <summary>
        /// Gets the count of data in the current save-data.
        /// </summary>
        /// <returns>The count of data</returns>
        public int ItemCount() => SaveData.Count;

        /// <summary>
        /// Loads the content from the source.
        /// </summary>
        private void LoadFromSource()
        {
            _wasDecrypted = false;
            var content = _literalInUse.Item2 ? _literalInUse.Item1 : _ezSaverSettings.Reader.LoadString();

            try
            {
                string roundTrip;

                if (!string.IsNullOrEmpty(content))
                {
                    // It's not encrypted, hence skip & continue
                    if (content.TrimStart().StartsWith('{'))
                    {
                        roundTrip = content;
                    }
                    // It's encrypted, hence decrypt & continue
                    else
                    {
                        try
                        {
                            roundTrip = _ezSaverSettings.Encryptor.Decrypt(content);
                            _wasDecrypted = true;
                        }
                        catch
                        {
                            _wasDecrypted = false;
                            roundTrip = string.Empty;
                        }
                    }
                }
                else
                {
                    roundTrip = content;
                }

                ToJObject(roundTrip);

                if (_ezSaverSettings.UseSecurity && _wasDecrypted)
                {
                    _loadedJson = content;
                    _wasDecrypted = false;
                }
                else
                    _loadedJson = string.IsNullOrEmpty(roundTrip) ? "{}" : roundTrip;
            }
            catch (Exception e)
            {
                EzLogger.Error($"{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Serializes the current save-data to a JSON string.
        /// Use this method for final serialization, only when you're initializing from a literal string.
        /// </summary>
        /// <returns>The serialized JSON string.</returns>
        public string ToSerializedJson()
        {
            var json = string.Empty;

            try
            {
                json = EzSaverSerializer.Serialize(SaveData);

                if (_ezSaverSettings.UseSecurity)
                    json = _ezSaverSettings.Encryptor.Encrypt(json);
            }
            catch (Exception e)
            {
                EzLogger.Error($"{e.Message}\n{e.StackTrace}");
            }

            return json;
        }

        /// <summary>
        /// Serializes and saves the current save-data to an initialized file source.
        /// Use this method without calling <see cref="ToSerializedJson"/>, only when you're initializing from a file source.
        /// <remarks>
        /// This method should only be called once for every instance created!
        /// </remarks>
        /// </summary>
        /// <exception cref="EzSaverException">Thrown if the content source is a literal string instead of file.</exception>
        public void Save()
        {
            if (_literalInUse.Item2)
                throw new EzSaverException(
                    "Cannot save a string-literal initialized from the constructor." +
                    $"\nIf you intend to save to a file, initialize with a filename instead, otherwise use {nameof(ToSerializedJson)}");

            var serializedJson = ToSerializedJson();

            if (!_wasDecrypted && _loadedJson == serializedJson)
                return;

            _ezSaverSettings.Reader.SaveString(serializedJson);
        }

        /// <summary>
        /// Converts a JSON string to a JObject.
        /// </summary>
        /// <param name="jsonStr">The JSON string to convert.</param>
        private void ToJObject(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return;

            try
            {
                SaveData = JObject.Parse(jsonStr);
            }
            catch
            {
                // ignored
            }
        }
    }
}