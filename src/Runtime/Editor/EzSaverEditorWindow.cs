#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Racer.EzSaver
{
    /// <summary>
    /// Main editor window for EzSaver.
    /// </summary>
    internal class EzSaverEditorWindow : EditorWindow
    {
        private const string ContextMenuPath = "Racer/EzSaver/";
        private const string RootPath = "Assets/EzSaver";
        private const string ConfigPath = RootPath + "/EzSaverConfig.asset";
        private const string SamplesPath = "Assets/Samples/EzSaver";
        private const string PkgId = "com.racer.ezsaver";

        private const int Width = 400;
        private const int Height = Width + 80;

        private static string _activeKey;
        private static string _id;
        private static bool _isKeyVisible;

        private static RemoveRequest _removeRequest;
        private static EzSaverConfig _ezSaverConfig;
        private static SerializedObject _config;
        private static SerializedProperty _fileName;


        private static void InitMenuData()
        {
            if (!_ezSaverConfig) return;

            _activeKey = KeyGen.GetKeyStr();

            _config = new SerializedObject(_ezSaverConfig);
            _fileName = _config.FindProperty("fileName");
        }

        private static void RefreshMenu()
        {
            Startup();
            InitMenuData();
        }

        [MenuItem(ContextMenuPath + "Menu")]
        private static void DisplayWindow()
        {
            var window = GetWindow<EzSaverEditorWindow>("EzSaver Menu");

            // Limit size of the window to non re-sizable.
            window.maxSize = window.minSize = new Vector2(Width, Height);

            InitMenuData();
        }

        [InitializeOnLoadMethod]
        private static void AutoRefresh()
        {
            Startup();

            if (HasOpenInstances<EzSaverEditorWindow>()) InitMenuData();
        }

        private static void Startup()
        {
            // if no data exists yet create and reference a new instance
            if (_ezSaverConfig) return;

            _ezSaverConfig = AssetDatabase.LoadAssetAtPath<EzSaverConfig>(ConfigPath);

            // if a previous data exists and is loaded successfully, we use it.
            if (_ezSaverConfig) return;

            // otherwise create and reference a new instance.
            _ezSaverConfig = CreateInstance<EzSaverConfig>();

            DirUtils.CreateDirectory(RootPath);
            AssetDatabase.CreateAsset(_ezSaverConfig, ConfigPath);
            AssetDatabase.Refresh();
        }

        [MenuItem(ContextMenuPath + "Remove Package(recommended)")]
        private static void RemovePackage()
        {
            _removeRequest = Client.Remove(PkgId);
            EditorApplication.update += RemoveProgress;
        }

        private static void RemoveProgress()
        {
            if (!_removeRequest.IsCompleted) return;

            switch (_removeRequest.Status)
            {
                case StatusCode.Success:
                {
                    DirUtils.DeleteDirectory(RootPath);
                    DirUtils.DeleteDirectory(SamplesPath);
                    KeyGen.ClearPrefs();
                    break;
                }
                case >= StatusCode.Failure:
                    EzLogger.Warn($"Failed to remove package: '{PkgId}'\n{_removeRequest.Error.message}");
                    break;
            }

            EditorApplication.update -= RemoveProgress;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            if (!_ezSaverConfig)
            {
                if (GUILayout.Button(Styles.RefreshBtn))
                    RefreshMenu();

                return;
            }

            _config.Update();
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionOne, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("Save-File Path:", PathUtil.SaveDirPath);
            EditorGUILayout.LabelField("Save-File Name:", PathUtil.DefaultFileName);
            EditorGUILayout.LabelField("Save-File Extension:", PathUtil.DefaultFileExtension);
            EditorGUILayout.LabelField("Save-File Formatting:", $"{PathUtil.FileFormatting}");

            GUILayout.Space(5);
            if (GUILayout.Button(Styles.PingBtn))
                PingLocation();

            if (GUILayout.Button(Styles.CreateBtn))
            {
                if (EzSaverUtility.CreateFile())
                {
                    _fileName.stringValue = PathUtil.FullFileName;
                    AssetDatabase.Refresh();
                    PingLocation();
                }
            }

            if (GUILayout.Button(Styles.LoadBtn))
                EzSaverUtility.LoadAllFiles();

            if (GUILayout.Button(Styles.DeleteAllBtn))
            {
                EzSaverUtility.DeleteAllFiles();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionTwo, EditorStyles.boldLabel);
            _fileName.stringValue = EditorGUILayout.TextField(Styles.SaveFile, _fileName.stringValue);

            if (string.IsNullOrEmpty(_fileName.stringValue))
            {
                var pos = new Rect(GUILayoutUtility.GetLastRect());
                EditorGUI.LabelField(pos, "Enter an existing [Save-File]",
                    Styles.PlaceHolderStyle(new RectOffset(170, 0, 0, 0)));
            }

            GUILayout.Space(5);
            if (GUILayout.Button(Styles.PrintContentBtn))
                if (IsFileAvailable)
                    EzLogger.Log(EzSaverUtility.ReadFileContent(_id));

            if (GUILayout.Button(Styles.EncryptBtn))
                if (IsFileAvailable)
                    EzSaverUtility.EncryptFile(_id);

            if (GUILayout.Button(Styles.DecryptBtn))
                if (IsFileAvailable)
                    EzSaverUtility.DecryptFile(_id);

            if (GUILayout.Button(Styles.DeleteBtn))
                if (IsFileAvailable)
                {
                    if (EzSaverUtility.DeleteFile(_id))
                    {
                        _fileName.stringValue = string.Empty;
                        AssetDatabase.Refresh();
                    }
                }

            if (GUILayout.Button(Styles.ResetFieldToDefaultBtn))
                _fileName.stringValue = EzSaverConfig.DefaultFileName;
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionThree, EditorStyles.boldLabel);

            _isKeyVisible = EditorGUILayout.BeginToggleGroup(Styles.KeyVisibilityToggle, _isKeyVisible);

            if (_isKeyVisible)
                EditorGUILayout.TextField(Styles.ExistingKeyField, _activeKey);
            else
                EditorGUILayout.PasswordField(Styles.ExistingKeyField, _activeKey);

            if (GUILayout.Button(Styles.NewKeyBtn))
            {
                KeyGen.SetRandomKey(out _activeKey);
                EzLogger.Log($"New current key: {_activeKey}");
            }

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            // Writes back all modified values into the real instance.
            _config?.ApplyModifiedProperties();

            // Detect mouse click outside controls and remove focus
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                GUI.FocusControl(null);
        }

        private static void PingLocation()
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(PathUtil.FullFilePath);
            EditorGUIUtility.PingObject(obj);
        }

        private static bool IsFileAvailable
        {
            get
            {
                _id = _fileName.stringValue;

                if (string.IsNullOrEmpty(_id))
                {
                    EzLogger.Warn("Field cannot be empty.");
                    return false;
                }

                if (FileHelper.Exists(_id))
                    return true;

                EzLogger.Warn($"'{_id}' does not exist.");
                return false;
            }
        }
    }

    internal static class DirUtils
    {
        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            Directory.Delete(path, true);
            DeleteEmptyMetaFiles(path);
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void DeleteEmptyMetaFiles(string directory)
        {
            if (Directory.Exists(directory)) return;

            var metaFile = directory + ".meta";

            if (File.Exists(metaFile))
                File.Delete(metaFile);
        }
    }

    internal static class Styles
    {
        #region SectionOne

        public static readonly GUIContent SectionOne = new("ReadOnly (Defaults)", "Default save settings.");

        public static readonly GUIContent PingBtn = new("Ping [Location]", "Pings to the save-file's location.");

        public static readonly GUIContent CreateBtn = new($"Create [{PathUtil.FullFileName}]",
            "Creates a default save-file at the default location.");

        public static readonly GUIContent LoadBtn = new("Load All [Save-Files]",
            "Prints all save-files, present at the default location, to the console.");

        public static readonly GUIContent DeleteAllBtn = new("Delete All [Save-Files]",
            "Deletes all save-files present at the default location.");

        #endregion

        #region SectionTwo

        public static readonly GUIContent
            SectionTwo = new("Operations", "Operations to be performed on the inputted save-file.");

        public static readonly GUIContent PrintContentBtn =
            new("Print [Content]", "Prints out the content of the save-file to the console.");

        public static readonly GUIContent EncryptBtn = new("Encrypt [Content]",
            "Encrypts and overwrites(Cipher) the content of the save-file using the current 'Active key'.");

        public static readonly GUIContent DecryptBtn = new("Decrypt [Content]",
            "Decrypts and overwrites(Plain) the content of the save-file using the 'Active key' used during encryption.");

        public static readonly GUIContent DeleteBtn = new("Delete [File]", "Deletes the save-file");

        public static readonly GUIContent RefreshBtn = new("Refresh",
            "Refreshes the menu-window if it is not properly initialized.");

        public static readonly GUIContent SaveFile = new("Save-File:",
            "Input an existing save-file here. Include its extension as well.");

        public static readonly GUIContent
            ResetFieldToDefaultBtn = new("Reset [Field]",
                "Resets the field to its default value.");

        #endregion

        #region SectionThree

        public static readonly GUIContent
            SectionThree = new("KeyGen", "Key generation for encryption and decryption.");

        public static readonly GUIContent ExistingKeyField = new("Active Key(ReadOnly)",
            "Current key being used for encrypt/decrypt operations. Each save file is encrypted and decrypted using a unique key generated for it. " +
            "\nIf a different key is used for an existing save file, its content will be overwritten!");

        public static readonly GUIContent NewKeyBtn = new("Set [New Key]",
            "Generates and sets a new key for encryption and decryption.");

        public static readonly GUIContent KeyVisibilityToggle =
            new("Show key",
                "Sets the current key's visibility to true or false.");

        #endregion

        public static GUIStyle PlaceHolderStyle(RectOffset offset)
        {
            return new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                padding = offset,
                fontStyle = FontStyle.Italic,
                normal = { textColor = Color.grey }
            };
        }
    }
}
#endif