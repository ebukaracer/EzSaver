#if UNITY_EDITOR
using System;
using System.IO;
using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Racer.EzSaver.Editor
{
    /// <summary>
    /// Main editor window for EzSaver.
    /// </summary>
    internal class EzSaverEditor : EditorWindow
    {
        private const string ContextMenuPath = "Racer/EzSaver/";
        private const string RootPath = "Assets/EzSaver";
        private const string SamplesPath = "Assets/Samples/EzSaver";

        private const string PkgId = "com.racer.ezsaver";
        private const string AssetPkgId = "EzSaver.unitypackage";
        private static RemoveRequest _removeRequest;

        private static bool _isElementsImported;
        private const string ImportElementsContextMenuPath = ContextMenuPath + "Import Elements";
        private const string ForceImportElementsContextMenuPath = ContextMenuPath + "Import Elements(Force)";

        private static bool _isConfigAssetCreated;
        private const string CreateEzSaverConfigContextMenuPath = ContextMenuPath + "Create EzSaverConfig?";

        private const int Width = 400;
        private const int Height = Width;

        private static EzSaverConfig _ezSaverConfig;
        private static string _saveFileName;

        [InitializeOnLoadMethod]
        private static void RefreshConfig()
        {
            if (!_ezSaverConfig)
                _ezSaverConfig = EzSaverConfig.Load;
        }

        [MenuItem(ContextMenuPath + "Menu", priority = 0)]
        private static void DisplayWindow()
        {
            _ezSaverConfig = EzSaverConfig.Load;

            if (_ezSaverConfig == null) return;

            var window = GetWindow<EzSaverEditor>("EzSaver Menu");

            // Limit size of the window to non re-sizable.
            window.maxSize = window.minSize = new Vector2(Width, Height);
        }

        [MenuItem(CreateEzSaverConfigContextMenuPath, false, priority = 1)]
        public static void CreateEzSaverConfig()
        {
            Debug.Log($"{nameof(EzSaverCore)} was created successfully. Ensure to backup your credentials!");

            var folderPath = "Assets/Resources";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var assetPath = $"{folderPath}/{nameof(EzSaverConfig)}.asset";

            var ezSaverConfig = CreateInstance<EzSaverConfig>();

            ezSaverConfig.ActiveKey = KeyGen.GetRandomBase64Str();
            ezSaverConfig.ActiveIv = KeyGen.GetRandomBase64Str();

            AssetDatabase.CreateAsset(ezSaverConfig, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Ping to the created config asset in next update
            EditorApplication.delayCall += () =>
            {
                var configAsset = AssetDatabase.LoadAssetAtPath<EzSaverConfig>(assetPath);

                if (configAsset)
                    EditorGUIUtility.PingObject(configAsset);
                else
                    Debug.LogError($"Failed to create {nameof(EzSaverConfig)} at: {assetPath}");
            };
        }

        [MenuItem(CreateEzSaverConfigContextMenuPath, true, priority = 1)]
        private static bool ValidateCreateEzSaverConfig()
        {
            try
            {
                _isConfigAssetCreated = EzSaverConfig.Load;
            }
            catch (Exception)
            {
                _isConfigAssetCreated = false;
            }

            return !_isConfigAssetCreated;
        }

        [MenuItem(ImportElementsContextMenuPath, false, priority = 1)]
        private static void ImportElements()
        {
            var packagePath = $"Packages/{PkgId}/Elements/{AssetPkgId}";

            if (File.Exists(packagePath))
                AssetDatabase.ImportPackage(packagePath, true);
            else
                EditorUtility.DisplayDialog("Missing Package File", $"{AssetPkgId} not found in the package.", "OK");
        }

        [MenuItem(ForceImportElementsContextMenuPath, false, priority = 1)]
        private static void ForceImportElements()
        {
            ImportElements();
        }

        [MenuItem(ImportElementsContextMenuPath, true, priority = 1)]
        private static bool ValidateImportElements()
        {
            _isElementsImported = AssetDatabase.IsValidFolder($"{RootPath}/Elements");
            return !_isElementsImported;
        }

        [MenuItem(ForceImportElementsContextMenuPath, true, priority = 1)]
        private static bool ValidateForceImportElements()
        {
            return _isElementsImported;
        }

        [MenuItem(ContextMenuPath + "Remove Package(recommended)", priority = 2)]
        private static void RemovePackage()
        {
            _removeRequest = Client.Remove(PkgId);
            EditorApplication.update += RemoveRequest;
        }

        private static void RemoveRequest()
        {
            if (!_removeRequest.IsCompleted) return;

            switch (_removeRequest.Status)
            {
                case StatusCode.Success:
                {
                    DirUtils.DeleteDirectory(RootPath);
                    DirUtils.DeleteDirectory(SamplesPath);
                    AssetDatabase.DeleteAsset("Assets/Resources/EzSaverConfig.asset");
                    PlayerPrefs.DeleteKey("YellowSquare");
                    AssetDatabase.Refresh();

                    break;
                }
                case >= StatusCode.Failure:
                    Debug.LogError($"Failed to remove package: '{PkgId}'\n{_removeRequest.Error.message}");
                    break;
            }

            EditorApplication.update -= RemoveRequest;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionOne, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("Save-File Name:", _ezSaverConfig.SaveFileName);
            EditorGUILayout.LabelField("Save-File Path:", _ezSaverConfig.FileRootPath);
            EditorGUILayout.LabelField("Save-File Extension:", _ezSaverConfig.SaveFileExtension);
            EditorGUILayout.LabelField("Save-File Formatting:", _ezSaverConfig.FileFormatting.ToString());

            GUILayout.Space(5);

            if (GUILayout.Button(Styles.CreateBtn))
            {
                if (EzSaverUtility.CreateFile(_ezSaverConfig.FileFullName))
                {
                    _saveFileName = _ezSaverConfig.FileFullName;
                    AssetDatabase.Refresh();
                    PingLocation();
                }
            }

            if (GUILayout.Button(Styles.PingBtn))
                PingLocation();

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button(Styles.LoadBtn))
                EzSaverUtility.LoadAllFiles();

            if (GUILayout.Button(Styles.DeleteAllBtn))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete All",
                        $"All save-files at: {_ezSaverConfig.FileRootPath}, will be permanently deleted?",
                        "Sure",
                        "Cancel"))
                {
                    EzSaverUtility.DeleteAllFiles();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionTwo, EditorStyles.boldLabel);
            _saveFileName = EditorGUILayout.TextField(Styles.SaveFile, _saveFileName);

            if (string.IsNullOrEmpty(_saveFileName))
            {
                var pos = new Rect(GUILayoutUtility.GetLastRect());
                EditorGUI.LabelField(pos, "e.g. Data.json",
                    Styles.PlaceHolderStyle(new RectOffset(90, 0, 0, 0)));
            }

            GUILayout.Space(5);

            if (GUILayout.Button(Styles.PrintContentBtn))
                if (FileExists)
                    Debug.Log(EzSaverUtility.ReadFileContent(_saveFileName));

            if (GUILayout.Button(Styles.EncryptBtn))
                if (FileExists)
                    EzSaverUtility.EncryptFile(_saveFileName);

            if (GUILayout.Button(Styles.DecryptBtn))
                if (FileExists)
                    EzSaverUtility.DecryptFile(_saveFileName);

            if (GUILayout.Button(Styles.DeleteBtn))
                if (FileExists)
                {
                    if (EditorUtility.DisplayDialog("Delete Save-File", $"{_saveFileName} will be permanently deleted?",
                            "OK"))
                    {
                        if (EzSaverUtility.DeleteFile(_saveFileName))
                        {
                            _saveFileName = string.Empty;
                            AssetDatabase.Refresh();
                        }
                    }
                }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (EditorGUILayout.LinkButton("Config Asset?"))
                EditorGUIUtility.PingObject(EzSaverConfig.Load);

            // Detect mouse click outside controls and remove focus
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                GUI.FocusControl(null);
        }

        private void PingLocation()
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(_ezSaverConfig.FileFullPath);

            if (obj)
                EditorGUIUtility.PingObject(obj);
            else
                Debug.LogError(
                    $"'{_ezSaverConfig.FileFullName}' was not found at: {_ezSaverConfig.FileRootPath}, consider creating it first.");
        }

        private bool FileExists
        {
            get
            {
                if (string.IsNullOrEmpty(_saveFileName))
                {
                    Debug.LogError("Field cannot be empty.");
                    return false;
                }

                if (FileHelper.Exists(_saveFileName))
                    return true;

                Debug.LogError(
                    $"'{_saveFileName}' was not found at: {_ezSaverConfig.FileRootPath}\nYou may need to create it first or include the extension.");
                return false;
            }
        }

        private static class Styles
        {
            #region SectionOne

            public static readonly GUIContent SectionOne = new("Save-File (Defaults)", "Default save settings.");

            public static readonly GUIContent PingBtn = new("Ping [Location]",
                "Highlights the default save-file's location(if available).");

            public static readonly GUIContent CreateBtn = new("Create [Save-File]",
                "Creates a default save-file at the default location.");

            public static readonly GUIContent LoadBtn = new("Load All [Save-Files]",
                "Prints all save-files, present at the default location, to the console.");

            public static readonly GUIContent DeleteAllBtn = new("Delete All [Save-Files]",
                "Deletes all save-files present at the default location.");

            #endregion

            #region SectionTwo

            public static readonly GUIContent
                SectionTwo = new("Operations", "Operations performed on the inputted save-file.");

            public static readonly GUIContent SaveFile = new("Existing Save-File:",
                "Input an existing save-file here. Include its extension as well.");

            public static readonly GUIContent PrintContentBtn =
                new("Print [Content]", "Prints out the content of the save-file to the console.");

            public static readonly GUIContent EncryptBtn = new("Encrypt [Content]",
                "Encrypts and overwrites(Cipher) the content of the save-file.");

            public static readonly GUIContent DecryptBtn = new("Decrypt [Content]",
                "Decrypts and overwrites(Plain) the content of the save-file.");

            public static readonly GUIContent DeleteBtn = new("Delete [File]", "Deletes the save-file");

            #endregion

            #region Helper

            public static GUIStyle PlaceHolderStyle(RectOffset offset)
            {
                return new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    padding = offset,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = Color.grey }
                };
            }

            #endregion
        }

        private static class DirUtils
        {
            public static void DeleteDirectory(string path)
            {
                if (!Directory.Exists(path)) return;

                Directory.Delete(path, true);
                DeleteEmptyMetaFiles(path);
            }

            private static void DeleteEmptyMetaFiles(string directory)
            {
                if (Directory.Exists(directory)) return;

                var metaFile = directory + ".meta";

                if (File.Exists(metaFile))
                    File.Delete(metaFile);
            }
        }
    }
}
#endif