using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Racer.EzSaver.Utilities;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Racer.EzSaverTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Encapsulating some operations that may be performed to a given save-file.
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
        private string _cacheEncryptedContent;
        private string _cacheSerializedJson;
        private string _contentSrc;
        private bool _hasSavedModifiedChanges;

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

        internal bool IsJsonStringLiteral { get; }

        /// <summary>
        /// Returns the full path to the save-file assuming a save-file was initialized otherwise an empty string.
        /// </summary>
        public string SaveFilePath
        {
            get
            {
                if (IsJsonStringLiteral) return string.Empty;
                return FileHelper.SaveDirPath +
                       Path.DirectorySeparatorChar +
                       FileHelper.AssignExtension(_contentSrc);
            }
        }

        private string LastSavePoint => $"Content saved to: {FileHelper.AssignExtension(_contentSrc)}";


        /// <summary>
        /// <seealso cref="EzSaverCore"/>
        /// </summary>
        private protected EzSaverBase(string contentSource, bool isJsonStringLiteral, EzSaverSettings ezSaverSettings)
        {
            _ezSaverSettings = ezSaverSettings;
            IsJsonStringLiteral = isJsonStringLiteral;
            _contentSrc = contentSource;
            LoadFromSource();

            SaveData.CollectionChanged += (_, _) =>
            {
                _hasSavedModifiedChanges = false;
                OnSaveDataModified?.Invoke();
            };
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
                    $"\n{_contentSrc}" +
                    "\nIf you intend to delete to a file, initialize with a filename instead.");

            if (_ezSaverSettings.Reader.Delete())
                OnSaveDataAction?.Invoke(SaveDataAction.DeleteFile);
            else
                Debug.LogError($"Failed to delete save file: '{FileHelper.AssignExtension(_contentSrc)}'.");
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
            var content = IsJsonStringLiteral ? _contentSrc : _ezSaverSettings.Reader.LoadString();

            try
            {
                var roundTrip = content;

                if (!string.IsNullOrEmpty(content))
                {
                    // It's encrypted, hence decrypt & continue
                    if (!content.TrimStart().StartsWith('{'))
                    {
                        try
                        {
                            _cacheEncryptedContent = content;
                            roundTrip = _ezSaverSettings.Encryptor.Decrypt(content);
                        }
                        catch // Failed, hence backup content(encrypted) to a new file
                        {
                            Debug.LogWarning(
                                $"Unable to decrypt the content of {(!IsJsonStringLiteral ? $"the file '{_contentSrc}'" : $"the encrypted JSON string-literal '{_contentSrc[..5]}...'\n{_contentSrc}")}\nThe new content will be initialized with new credentials and reset to empty JSON string.\n");

                            if (!IsJsonStringLiteral && _ezSaverSettings.EzSaverConfig.RetainBackupFile)
                            {
                                var index = 0;
                                string filename;

                                do
                                {
                                    filename = Path.HasExtension(_contentSrc)
                                        ? $"{_contentSrc[.._contentSrc.IndexOf('.')]}-backup{index}{Path.GetExtension(_contentSrc)}"
                                        : $"{_contentSrc}-backup{index}";

                                    index++;
                                } while (_ezSaverSettings.Reader.Exists(filename, true));

                                _ezSaverSettings.Reader.CreateString(filename, content);
                                Debug.Log($"'{filename}' file(encrypted) was saved at the set location");
                            }

                            _cacheEncryptedContent = roundTrip = string.Empty;
                        }
                    }
                }

                ToJObject(_cacheSerializedJson = roundTrip);
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
        /// Serializes the current save-data to as a JSON string.
        /// </summary>
        /// <returns>The serialized JSON string.</returns>
        private string ToSerializedJson()
        {
            var json = string.Empty;

            try
            {
                _cacheSerializedJson = string.IsNullOrEmpty(_cacheEncryptedContent)
                    ? _cacheSerializedJson
                    : _cacheEncryptedContent;

                json = EzSaverSerializer.Serialize(SaveData);

                if (_ezSaverSettings.UseSecurity && SaveData.HasValues)
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
        /// The <c>serialized-contents</c> if initialized as a <c>JSON string-literal</c> and serialization was successful</returns>
        /// </summary>
        public string Save()
        {
            try
            {
                if (_hasSavedModifiedChanges) return IsJsonStringLiteral ? _cacheSerializedJson : LastSavePoint;

                var serializedJson = ToSerializedJson();

                if (_cacheSerializedJson == serializedJson)
                {
                    _hasSavedModifiedChanges = true;
                    return IsJsonStringLiteral ? _cacheSerializedJson : LastSavePoint;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogError("This build platform does not support initialization from save-files.");
#else
                if (!IsJsonStringLiteral)
                    _ezSaverSettings.Reader.SaveString(serializedJson);
#endif

                _cacheSerializedJson = serializedJson;
                _cacheEncryptedContent = string.Empty;

                _hasSavedModifiedChanges = true;

                return IsJsonStringLiteral ? serializedJson : LastSavePoint;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception was thrown while attempting to save.\n{e.Message}\n{e.StackTrace}");
                return string.Empty;
            }
        }
    }
}