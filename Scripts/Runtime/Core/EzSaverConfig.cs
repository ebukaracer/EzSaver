using System.IO;
using Newtonsoft.Json;
using Racer.EzSaver.Scripts.Runtime.Utils;
using UnityEngine;

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Racer.EzSaver.Editor")]
#endif

namespace Racer.EzSaver.Scripts.Runtime.Core
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

    internal class EzSaverConfig : ScriptableObject, IEzSaverConfig
    {
        private static string SaveFileRootPath =>
            Application.isEditor ? "Assets/" : $"{Application.persistentDataPath}/";

        private const string ResourcesPath = "Assets/Resources";
        public const string ConfigAssetPath = ResourcesPath + "/EzSaverConfig.asset";
        public const string SaveFileDefaultName = "Data";
        public const string SaveFileDefaultRootPath = "EzSaver/Saves";

        private static EzSaverConfig _instance;

        public static EzSaverConfig Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = Resources.Load<EzSaverConfig>(nameof(EzSaverConfig));

                if (!_instance)
                    CreateInstance();

                return _instance;
            }
        }


        private static void CreateInstance()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(ResourcesPath))
                Directory.CreateDirectory(ResourcesPath);

            var ezSaverConfig = CreateInstance<EzSaverConfig>();
            ezSaverConfig.ActiveKey = KeyGen.GetRandomBase64Str();
            ezSaverConfig.ActiveIv = KeyGen.GetRandomBase64Str();

            UnityEditor.AssetDatabase.CreateAsset(ezSaverConfig, ConfigAssetPath);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"[{nameof(EzSaverConfig)}] created successfully. Remember to backup your new credentials!\n",
                ezSaverConfig);

            _instance = Resources.Load<EzSaverConfig>(nameof(EzSaverConfig));
#else
            if (!_instance)
                throw new FileNotFoundException(
                    $"[{nameof(EzSaverConfig)}] asset not found in Resources directory. Make sure an instance exists in the editor before building the project.");
#endif
        }

        // Save-file section
        [SerializeField, Tooltip("Name of the current save-file (without extension).\nExample: 'Data'")]
        private string saveFileName = SaveFileDefaultName;

        [SerializeField,
         Tooltip(
             "Location to store the current save-file (editor and build).\nDefaults to 'Assets/' in the editor if left empty.\nUses persistent data-path on build if 'distinctSavePaths' is false.")]
        private string saveFilePath = SaveFileDefaultRootPath;

        [SerializeField,
         Tooltip(
             "Location to store the current save-file (build only).\nDefaults to 'persistent data-path' on build if left empty.")]
        private string saveFileBuildPath;

        [SerializeField,
         Tooltip(
             "Extension to use for the current save-file.\nOther extensions can also be used when initializing a new save-file using EzSaverManager.")]
        private SaveFileExtension saveFileExtension = Core.SaveFileExtension.Json;

        [SerializeField, Tooltip("Formatting style to use in the current save-file's content.")]
        private Formatting saveFileFormatting = Formatting.Indented;

        [SerializeField,
         Tooltip(
             "Retain a backup of the save-file when overwriting it.\nUseful for preserving previous contents when decrypting with different credentials.")]
        private bool retainBackupFile = true;

        [SerializeField,
         Tooltip(
             "Use separate save-paths for editor and build.\nIf enabled, the save-file path will differ between editor and build environments.")]
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
        public bool RetainBackupFile => retainBackupFile;
    }

    internal interface IEzSaverConfig
    {
        bool RetainBackupFile { get; }
    }
}