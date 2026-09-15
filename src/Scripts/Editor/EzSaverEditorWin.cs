using Racer.EzSaver.Scripts.Runtime.Core;
using Racer.EzSaver.Scripts.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace Racer.EzSaver.Scripts.Editor
{
    internal class EzSaverEditorWin : EditorWindow
    {
        private const int ButtonHeight = 28;
        private static string _saveFileName;

        private Vector2 _scrollPosition;


        [MenuItem(EditorContextMenu.ContextMenuSetupPath + "Menu %&s", priority = 2)]
        private static void DisplayWindow()
        {
            if (!EzSaverConfig.Instance)
                EditorApplication.delayCall += DisplayWin;
            else
                DisplayWin();
        }

        private static void DisplayWin()
        {
            GetWindow<EzSaverEditorWin>("EzSaver Editor");
        }

        private void OnGUI()
        {
            var ezSaverConfig = EzSaverConfig.Instance;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);

            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.SectionOne, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("Save-File Name:", ezSaverConfig.SaveFileName);
            EditorGUILayout.LabelField("Save-File Path:", ezSaverConfig.FileRootPath);
            EditorGUILayout.LabelField("Save-File Extension:", ezSaverConfig.SaveFileExtension);
            EditorGUILayout.LabelField("Save-File Formatting:", ezSaverConfig.FileFormatting.ToString());

            GUILayout.Space(5);

            if (GUILayout.Button(Styles.CreateBtn, GUILayout.Height(ButtonHeight)))
            {
                if (EzSaverFileUtility.CreateFile(ezSaverConfig.FileFullName))
                {
                    _saveFileName = ezSaverConfig.FileFullName;
                    AssetDatabase.Refresh();
                    PingLocation();
                }
            }

            if (GUILayout.Button(Styles.FindBtn, GUILayout.Height(ButtonHeight)))
                PingLocation();

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button(Styles.LoadBtn, GUILayout.Height(ButtonHeight)))
                EzSaverFileUtility.LoadAllFiles();

            if (GUILayout.Button(Styles.DeleteAllBtn, GUILayout.Height(ButtonHeight)))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete All",
                        $"Permanently delete all save-files at: {ezSaverConfig.FileRootPath}?",
                        "Proceed",
                        "Cancel"))
                {
                    EzSaverFileUtility.DeleteAllFiles();
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

            if (GUILayout.Button(Styles.PrintContentBtn, GUILayout.Height(ButtonHeight)))
                if (FileExists)
                    Debug.Log(EzSaverFileUtility.ReadFileContent(_saveFileName));

            if (GUILayout.Button(Styles.EncryptBtn, GUILayout.Height(ButtonHeight)))
                if (FileExists)
                    EzSaverFileUtility.EncryptFile(_saveFileName);

            if (GUILayout.Button(Styles.DecryptBtn, GUILayout.Height(ButtonHeight)))
                if (FileExists)
                    EzSaverFileUtility.DecryptFile(_saveFileName);

            if (GUILayout.Button(Styles.DeleteBtn, GUILayout.Height(ButtonHeight)))
                if (FileExists)
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete",
                            $"Permanently delete {_saveFileName}?",
                            "Proceed"))
                    {
                        if (EzSaverFileUtility.DeleteFile(_saveFileName))
                        {
                            _saveFileName = string.Empty;
                            AssetDatabase.Refresh();
                        }
                    }
                }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (EditorGUILayout.LinkButton("Config Asset"))
                EditorGUIUtility.PingObject(EzSaverConfig.Instance);

            // Detect mouse click outside controls and remove focus
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                GUI.FocusControl(null);

            GUILayout.EndScrollView();
        }

        private static void PingLocation()
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(EzSaverConfig.Instance.FileFullPath);

            if (obj)
                EditorGUIUtility.PingObject(obj);
            else
                Debug.LogWarning("Default Save-File not found at default location.");
        }

        private static bool FileExists
        {
            get
            {
                if (string.IsNullOrEmpty(_saveFileName))
                {
                    Debug.LogWarning("Field cannot be empty.");
                    return false;
                }

                if (FileHelper.Exists(_saveFileName))
                    return true;

                Debug.LogWarning(
                    $"[{_saveFileName}] was not found. You may need to create it first or include the right extension.\n");
                return false;
            }
        }

        private static class Styles
        {
            #region SectionOne

            public static readonly GUIContent SectionOne = new("Save-File (Defaults)", "Default Save-File operations");

            public static readonly GUIContent FindBtn = new("Find Default [Save-File]",
                "Highlights the default save-file's location (if available).");

            public static readonly GUIContent CreateBtn = new("Create Default [Save-File]",
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
                "Encrypts and overwrites (cipher) the content of the save-file.");

            public static readonly GUIContent DecryptBtn = new("Decrypt [Content]",
                "Decrypts and overwrites (plain) the content of the save-file.");

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
    }
}