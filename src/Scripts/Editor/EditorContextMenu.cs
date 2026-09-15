using System.IO;
using Racer.EzSaver.Scripts.Runtime.Core;
using Racer.EzUtilities.Common.Runtime;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Racer.EzSaver.Scripts.Editor
{
    /// <summary>
    /// Main editor window for EzSaver.
    /// </summary>
    internal static class EditorContextMenu
    {
        private static RemoveRequest _removeRequest;
        private static bool _isElementsImported;
        private const string PkgId = "com.racer.ezsaver";
        private const string AssetPkgId = "EzSaver.unitypackage";

        private const string RootPath = "Assets/EzSaver";
        private const string SamplesPath = "Assets/Samples/EzSaver";
        private const string PrefabsPath = RootPath + "/Elements/Prefabs/EzSaverManager.prefab";

        internal const string ContextMenuSetupPath = "Racer/EzSaver/Setup/";
        private const string ContextMenuPath = "Racer/EzSaver/";
        private const string AddPrefabToScenePath = ContextMenuSetupPath + "Add EzSaverManager GameObject to Scene";

        private const string ImportElementsContextMenuPath = ContextMenuPath + "Import Elements";
        private const string ForceImportElementsContextMenuPath = ContextMenuPath + "Import Elements (force)";


        [MenuItem(ImportElementsContextMenuPath, false, priority = 0)]
        private static void ImportElements()
        {
            var packagePath = $"Packages/{PkgId}/Dependencies~/Package/{AssetPkgId}";

            if (File.Exists(packagePath))
                AssetDatabase.ImportPackage(packagePath, true);
            else
                EditorUtility.DisplayDialog("Missing Package File", $"{AssetPkgId} not found in the package.", "OK");
        }

        [MenuItem(ImportElementsContextMenuPath, true)]
        private static bool ValidateImportElements()
        {
            _isElementsImported = AssetDatabase.IsValidFolder($"{RootPath}/Elements");
            return !_isElementsImported;
        }

        [MenuItem(ForceImportElementsContextMenuPath, false, priority = 1)]
        private static void ForceImportElements()
        {
            ImportElements();
        }

        [MenuItem(ForceImportElementsContextMenuPath, true)]
        private static bool ValidateForceImportElements()
        {
            return _isElementsImported;
        }

        [MenuItem(AddPrefabToScenePath, false, 3)]
        private static void AddPrefabToScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath);

            if (prefab)
            {
                var go = Object.Instantiate(prefab);
                go.name = prefab.name;
                Undo.RegisterCreatedObjectUndo(go, $"Add {nameof(EzSaverManager)} prefab instance to Scene");
            }
            else
                EditorUtility.DisplayDialog("Missing Prefab",
                    $"{nameof(EzSaverManager)} prefab not found in the package.\n\nImport this package's elements and try again",
                    "OK");
        }

        [MenuItem(AddPrefabToScenePath, true)]
        private static bool ValidateAddPrefabToScene()
        {
            // Validate that the prefab exists and is not already in the scene
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsPath);
            return prefab && !GameObject.Find(prefab.name);
        }

        [MenuItem(ContextMenuPath + "Remove Package (recommended)", priority = 10)]
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
                    CommonUtils.DeleteAssets(new[] { SamplesPath, RootPath, EzSaverConfig.ConfigAssetPath });
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
    }
}