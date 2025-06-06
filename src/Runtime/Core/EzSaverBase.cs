using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Racer.EzSaver.Utilities;
using UnityEngine;

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
    /// Provides base functionality for <see cref="EzSaverCore"/>.
    /// </summary>
    public class EzSaverBase
    {
        private (string, bool) _literalInUse;

        private string _loadedEncryptedContent;
        private string _cachedContent;

        private readonly EzSaverSettings _ezSaverSettings;
        protected JObject SaveData { get; private set; } = new();

        /// <summary>
        /// Event raised when some operations are performed on the save-file.
        /// </summary>
        public event Action<SaveDataAction> OnSaveDataAction;

        /// <summary>
        /// Event raised when a Write operation is performed on the save-data, hence modifying it.
        /// </summary>
        internal event Action OnSaveDataModified;

        protected bool HasSavedNewChanges;
        internal bool IsJsonStringLiteral => _literalInUse.Item2;

        public string SavePath => FileHelper.SaveDirPath;


        protected void InvokeOnSaveDataModified()
        {
            OnSaveDataModified?.Invoke();
        }

        /// <summary>
        /// <seealso cref="EzSaverCore"/>
        /// </summary>
        protected EzSaverBase(string contentSource, bool isJsonStringLiteral, EzSaverSettings ezSaverSettings)
        {
            _ezSaverSettings = ezSaverSettings;
            _literalInUse = (contentSource, isJsonStringLiteral);

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
        /// <exception cref="EzSaverException">Thrown if the content source is a JSON string-literal instead of a filename.</exception>
        public void DeleteFile()
        {
            if (IsJsonStringLiteral)
                throw new EzSaverException(
                    "Cannot delete a JSON string-literal initialized in the constructor." +
                    $"\n{_literalInUse.Item1}" +
                    "\nIf you intend to delete to a file, initialize with a filename instead.");

            if (_ezSaverSettings.Reader.Delete())
                OnSaveDataAction?.Invoke(SaveDataAction.DeleteFile);
            else
                Debug.LogError($"Failed to delete save file: '{FileHelper.AssignExtension(_literalInUse.Item1)}'.");
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
            var content = IsJsonStringLiteral ? _literalInUse.Item1 : _ezSaverSettings.Reader.LoadString();

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
                            _loadedEncryptedContent = content;
                            roundTrip = _ezSaverSettings.Encryptor.Decrypt(content);
                        }
                        catch
                        {
                            _loadedEncryptedContent = roundTrip = string.Empty;
                        }
                    }
                }
                else
                {
                    roundTrip = content;
                }

                ToJObject(roundTrip);

                _cachedContent = string.IsNullOrEmpty(roundTrip) ? "{}" : roundTrip;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
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

        /// <summary>
        /// Serializes the current save-data to a JSON string.
        /// </summary>
        /// <returns>The serialized JSON string.</returns>
        private string ToSerializedJson()
        {
            var json = string.Empty;

            try
            {
                _cachedContent = string.IsNullOrEmpty(_loadedEncryptedContent)
                    ? _cachedContent
                    : _loadedEncryptedContent;

                json = EzSaverSerializer.Serialize(SaveData);

                if (_ezSaverSettings.UseSecurity)
                    json = _ezSaverSettings.Encryptor.Encrypt(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }

            return json;
        }

        /// <summary>
        /// Serializes and saves the current save-data to an initialized file or returns the contents as a serialized JSON string-literal.
        /// <returns>
        /// An <c>empty string</c> if initialized from a <c>file</c> and saved successfully.
        /// The <c>serialized-contents</c> if initialized from as a <c>JSON string-literal</c> and serialization was successful</returns>
        /// </summary>
        public string Save()
        {
            try
            {
                if (HasSavedNewChanges) return IsJsonStringLiteral ? _cachedContent : string.Empty;

                var serializedJson = ToSerializedJson();

                if (_cachedContent == serializedJson)
                {
                    HasSavedNewChanges = true;
                    return IsJsonStringLiteral ? serializedJson : string.Empty;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogWarning("This build platform does not support initialization from save-files.");
#else
                if (!IsJsonStringLiteral)
                    _ezSaverSettings.Reader.SaveString(serializedJson);
#endif

                _cachedContent = serializedJson;
                _loadedEncryptedContent = string.Empty;

                HasSavedNewChanges = true;

                return IsJsonStringLiteral ? serializedJson : string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception was thrown while attempting to save.\n{e.Message}\n{e.StackTrace}");
                return string.Empty;
            }
        }
    }
}