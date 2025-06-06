using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Racer.EzSaver.Core
{
    [System.Serializable]
    internal class EzSaverKeyBackup
    {
        public string key;
        public string iv;
        public string testCipherText;
    }

    internal enum SaveFileExtension
    {
        Json,
        XML,
        Txt,
    }

    internal class EzSaverConfig : ScriptableObject
    {
        private static string SaveFileRootPath =>
            Application.isEditor ? "Assets/" : $"{Application.persistentDataPath}/";

        private static EzSaverConfig _instance;

        public const string SaveFileDefaultName = "Data";
        public const string SaveFileDefaultRootPath = "EzSaver/Saves";

        // Save-file section
        [SerializeField, Tooltip("Name of the current save-file(without extension)")]
        private string saveFileName = SaveFileDefaultName;

        [SerializeField,
         Tooltip(
             "Location to store the current save-file(editor and build).\nIt defaults to the 'Assets/' folder in the editor(if left empty), then, uses persistent data-path on build(if 'distinctSavePaths' is false).")]
        private string saveFilePath = SaveFileDefaultRootPath;

        [SerializeField,
         Tooltip(
             "Location to store the current save-file(build only), defaults to 'persistent data-path' on build(if left empty)")]
        private string saveFileBuildPath;

        [SerializeField,
         Tooltip(
             "Extension to use for the current save-file, other extensions can also be used when initializing a new save-file using EzSaverManager")]
        private SaveFileExtension saveFileExtension = Core.SaveFileExtension.Json;

        [SerializeField, Tooltip("Formatting style to use in the current save-file's content")]
        private Formatting saveFileFormatting = Formatting.Indented;

        [SerializeField, Tooltip("Whether or not to use separate save-paths for editor and build")]
        private bool distinctSavePaths;

        // Keygen section
        [field: SerializeField] public string ActiveKey { get; internal set; }
        [field: SerializeField] public string ActiveIv { get; internal set; }

        public string SaveFileExtension
        {
            get
            {
                return saveFileExtension switch
                {
                    Core.SaveFileExtension.Json => ".json",
                    Core.SaveFileExtension.XML => ".xml",
                    Core.SaveFileExtension.Txt => ".txt",
                    _ => null
                };
            }
        }

        public string SaveFileName => saveFileName;

        public string FileRootPath
        {
            get
            {
#if !UNITY_EDITOR
                if (distinctSavePaths) return SaveFileRootPath + saveFileBuildPath;
#endif
                return SaveFileRootPath + saveFilePath;
            }
        }

        public string FileFullName => saveFileName + SaveFileExtension;
        public string FileFullPath => Path.Combine(FileRootPath, FileFullName);
        public Formatting FileFormatting => saveFileFormatting;

        public static EzSaverConfig Load
        {
            get
            {
                if (_instance) return _instance;

                _instance = Resources.Load<EzSaverConfig>(nameof(EzSaverConfig));

                if (!_instance)
                    throw new FileNotFoundException(
                        $"{nameof(EzSaverConfig)} asset not found, consider creating one first.\nUse the context-menu: Racer > EzSaver > Create EzSaverConfig?\n");

                return _instance;
            }
        }
    }
}