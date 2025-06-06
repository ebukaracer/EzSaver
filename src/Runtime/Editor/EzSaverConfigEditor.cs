#if UNITY_EDITOR
using System.IO;
using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEditor;
using UnityEngine;

namespace Racer.EzSaver.Editor
{
    [CustomEditor(typeof(EzSaverConfig))]
    internal class EzSaverConfigEditor : UnityEditor.Editor
    {
        private const string EncDecTestStr = "Test123";

        private SerializedProperty _saveFileExtension;
        private SerializedProperty _saveFilePath;
        private SerializedProperty _saveFileFormatting;
        private SerializedProperty _saveFileName;
        private SerializedProperty _saveFileBuildPath;
        private SerializedProperty _distinctSavePaths;

        private EzSaverConfig _ezSaverConfig;
        private bool _isKeyVisible;

        private EzSaverKeyBackup _loadedBackup;
        private string _path;
        private (string, MessageType) _status;
        private (string, string) _cacheKeyIv;


        private void OnEnable()
        {
            _ezSaverConfig = (EzSaverConfig)target;

            if (string.IsNullOrEmpty(_ezSaverConfig.ActiveKey))
                RegenerateCred(true);

            if (string.IsNullOrEmpty(_ezSaverConfig.ActiveIv))
                RegenerateCred(false);

            _status = (string.Empty, MessageType.None);
            _loadedBackup = null;

            _saveFileName = serializedObject.FindProperty("saveFileName");
            _saveFilePath = serializedObject.FindProperty("saveFilePath");
            _saveFileExtension = serializedObject.FindProperty("saveFileExtension");
            _saveFileFormatting = serializedObject.FindProperty("saveFileFormatting");
            _saveFileBuildPath = serializedObject.FindProperty("saveFileBuildPath");
            _distinctSavePaths = serializedObject.FindProperty("distinctSavePaths");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label(Styles.SaveFileSection, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_saveFileName);
            EditorGUILayout.PropertyField(_saveFilePath);

            if (_distinctSavePaths.boolValue)
                EditorGUILayout.PropertyField(_saveFileBuildPath);

            EditorGUILayout.PropertyField(_saveFileExtension);
            EditorGUILayout.PropertyField(_saveFileFormatting);


            EditorGUILayout.PropertyField(_distinctSavePaths);

            GUILayout.Space(5);

            if (GUILayout.Button(Styles.ResetToDefaultsBtn))
            {
                _saveFileExtension.enumValueIndex = 0;
                _saveFileFormatting.enumValueIndex = 1;
                _saveFilePath.stringValue = EzSaverConfig.SaveFileDefaultRootPath;
                _saveFileBuildPath.stringValue = string.Empty;
                _saveFileName.stringValue = EzSaverConfig.SaveFileDefaultName;
                _distinctSavePaths.boolValue = false;
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(15);
            GUILayout.Label(Styles.KeygenSection, EditorStyles.boldLabel);

            _isKeyVisible = EditorGUILayout.BeginToggleGroup(Styles.KeyVisibilityToggle, _isKeyVisible);

            if (_isKeyVisible)
            {
                EditorGUILayout.TextField(Styles.KeyField, _ezSaverConfig.ActiveKey);
                EditorGUILayout.TextField(Styles.IvField, _ezSaverConfig.ActiveIv);
            }
            else
            {
                EditorGUILayout.PasswordField(Styles.KeyField, _ezSaverConfig.ActiveKey);
                EditorGUILayout.PasswordField(Styles.IvField, _ezSaverConfig.ActiveIv);
            }

            EditorGUILayout.EndToggleGroup();
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Styles.NewKeysBtn))
            {
                RegenerateCred(true);
                RegenerateCred(false);
            }

            if (GUILayout.Button(Styles.BackupKeysBtn))
                BackupKeys();

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(Styles.RestoreBackupKeysBtn))
                RestoreBackupKeys();

            TryLoadBackupFile();

            if (GUI.changed)
                EditorUtility.SetDirty(_ezSaverConfig);
        }


        private void RestoreBackupKeys()
        {
            _path = EditorUtility.OpenFilePanel("Load Backup File", "", "json");

            if (string.IsNullOrEmpty(_path)) return;

            try
            {
                var json = File.ReadAllText(_path);
                _loadedBackup = JsonUtility.FromJson<EzSaverKeyBackup>(json);
                _status = ("Loaded backup file.", MessageType.Info);
            }
            catch
            {
                _status = ("Invalid file format.", MessageType.Error);
            }
        }

        private void TryLoadBackupFile()
        {
            if (_loadedBackup != null)
            {
                if (GUILayout.Button(Styles.ValidateFileBtn))
                {
                    if (TryRestoreBackup())
                    {
                        _status = ("Credentials applied successfully.", MessageType.Info);
                        AssetDatabase.SaveAssets();
                    }
                    else
                        _status = ("Invalid backup; failed validation.", MessageType.Error);

                    // Clear the loaded backup after applying
                    _loadedBackup = null;
                }
            }

            if (!string.IsNullOrEmpty(_status.Item1))
                EditorGUILayout.HelpBox(_status.Item1, _status.Item2);

            return;

            bool TryRestoreBackup()
            {
                if (_loadedBackup == null || string.IsNullOrEmpty(_loadedBackup.testCipherText))
                    return false;

                try
                {
                    _cacheKeyIv = (_ezSaverConfig.ActiveKey, _ezSaverConfig.ActiveIv);
                    _ezSaverConfig.ActiveKey = _loadedBackup.key;
                    _ezSaverConfig.ActiveIv = _loadedBackup.iv;

                    var decrypted = EzSaverUtility.DecryptString(_loadedBackup.testCipherText);
                    return decrypted == EncDecTestStr;
                }
                catch
                {
                    _ezSaverConfig.ActiveKey = _cacheKeyIv.Item1;
                    _ezSaverConfig.ActiveIv = _cacheKeyIv.Item2;
                    return false;
                }
            }
        }


        private void BackupKeys()
        {
            var path = EditorUtility.SaveFilePanel("Save Backup File", "", "ezsaver_backup.json", "json");

            if (string.IsNullOrEmpty(path)) return;

            var backup = new EzSaverKeyBackup
            {
                key = _ezSaverConfig.ActiveKey,
                iv = _ezSaverConfig.ActiveIv,
                testCipherText = EzSaverUtility.EncryptString(EncDecTestStr)
            };

            File.WriteAllText(path, JsonUtility.ToJson(backup, true));
            Debug.Log("Backup file saved to: " + path);
        }


        private void RegenerateCred(bool isKey)
        {
            if (isKey)
                _ezSaverConfig.ActiveKey = KeyGen.GetRandomBase64Str();
            else
                _ezSaverConfig.ActiveIv = KeyGen.GetRandomBase64Str();

            AssetDatabase.SaveAssets();
        }

        private static class Styles
        {
            public static readonly GUIContent
                ResetToDefaultsBtn = new("Reset to Defaults", "Resets the fields to their default value");

            public static readonly GUIContent
                SaveFileSection = new("Save-File", "Save-file path and other configurations.");

            public static readonly GUIContent
                KeygenSection = new("Key Store(Readonly)", "Active credentials for encryption/decryption.");

            public static readonly GUIContent KeyField = new("Active Key",
                "Current key used for all save-files' security.");

            public static readonly GUIContent IvField = new("Active IV",
                "Current IV associated with the current key.");

            public static readonly GUIContent NewKeysBtn = new("Set New Keys?",
                "Generates and sets new credentials.\nSave files contents secured with previous credentials will be overwritten and assigned to the new one!");

            public static readonly GUIContent BackupKeysBtn = new("Backup Keys!",
                "Saves the current credentials to a backup file.\nDo this before regenerating new keys to ensure you can restore them later for save-files using them.");

            public static readonly GUIContent RestoreBackupKeysBtn = new("Restore Backup File",
                "Restores the credentials from a backup file.");

            public static readonly GUIContent ValidateFileBtn = new("Validate and Apply",
                "Validates and applies the backup keys from the loaded file");

            public static readonly GUIContent KeyVisibilityToggle = new("Toggle Visibility");
        }
    }
}
#endif