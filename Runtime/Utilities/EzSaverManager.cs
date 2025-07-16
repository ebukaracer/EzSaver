using System.Collections.Generic;
using Racer.EzSaver.Core;
using UnityEngine;

// ReSharper disable MemberCanBeProtected.Global

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Singleton persistent class for managing save-files via shared instances of <see cref="EzSaverCore"/>.
    /// </summary>
    /// <remarks>
    /// Attach this script to a GameObject in your scene or simply use the <see cref="EzSaverManager"/> prefab.
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    public class EzSaverManager : SingletonPattern.SingletonPersistent<EzSaverManager>
    {
        /// <summary>
        /// Dictionary to cache <see cref="EzSaverCore"/> instances by filename.
        /// </summary>
        private Dictionary<string, EzSaverCore> _ezSaverCoreStore = new();

        [SerializeField,
         Tooltip(
             "Whether or not to commit changes to save-files(only) automatically by this script, when the application is shutdown." +
             "\n\nNB:" +
             "\nIt is not applicable to WebGL builds." +
             "\nIf you're saving elsewhere, un-toggle this option.")]
        private bool autoSaveOnQuit = true;

        [SerializeField,
         Tooltip(
             "Whether or not to commit changes immediately to save-files(only) by this script, as soon as a write operation is made." +
             "\n\nNB:" +
             "\nIt is not applicable to WebGL builds." +
             "\nIf you're saving elsewhere, un-toggle this option.")]
        private bool saveOnModification;


        /// <summary>
        /// Creates or reuses an instance of <see cref="EzSaverCore"/> for the specified <c>contentSource</c>.
        /// </summary>
        /// <param name="contentSource">The save filename(Creates and uses one, if left empty) or a JSON string-literal.</param>
        /// <param name="isJsonStringLiteral">Indicates whether the content is a JSON string-literal(false) or a filename(true-default).</param>
        /// <param name="useSecurity">Indicates whether to use security features(encryption/decryption) on the save-file(false-default).</param>
        /// <remarks>
        /// <c>contentSource</c> can either be a filename or a JSON string-literal. eg. Data.json or "{ "Highscore": 4 }".
        /// </remarks>
        public EzSaverCore GetSave(string contentSource = "Data.json", bool isJsonStringLiteral = false,
            bool useSecurity = false)
        {
            EzSaverCore ezSaverCore;

            if (!_ezSaverCoreStore.ContainsKey(contentSource))
            {
                ezSaverCore = _ezSaverCoreStore[contentSource] =
                    new EzSaverCore(contentSource, isJsonStringLiteral, useSecurity);

                if (!isJsonStringLiteral)
                {
                    ezSaverCore.OnSaveDataModified += () =>
                    {
                        if (saveOnModification)
                            ezSaverCore.Save();
                    };
                }
            }
            else
                ezSaverCore = GetExistingInstance(contentSource);

            return ezSaverCore;
        }

        /// <summary>
        /// Removes the <see cref="EzSaverCore"/> instance associated with the specified <c>contentSource</c>.
        /// </summary>
        /// <param name="contentSource">The old save-file name or JSON string-literal</param>
        /// <param name="isJsonStringLiteral">False if it's a save-file otherwise true(for a JSON string-literal)</param>
        /// <remarks>
        /// Next time <see cref="GetSave(string, bool, bool)"/> is called with the same <c>contentSource</c>, a new instance will be created.
        /// </remarks>
        public void ClearCacheSave(string contentSource = "Data.json", bool isJsonStringLiteral = false)
        {
            if (_ezSaverCoreStore.ContainsKey(contentSource))
                _ezSaverCoreStore.Remove(contentSource);
            else
                Debug.LogWarning(
                    $"No {nameof(EzSaverCore)} instance found for content source: {(isJsonStringLiteral ? $"\n{contentSource}" : contentSource)}");
        }

        /// <summary>
        /// Gets an existing <see cref="EzSaverCore"/> instance associated with the specified <paramref name="src"/>.
        /// </summary>
        /// <remarks>
        /// <c>src</c> can either be a filename or a JSON string-literal. eg. Data.json or "{ Highscore: 4 }".
        /// </remarks>
        private EzSaverCore GetExistingInstance(string src)
        {
            return _ezSaverCoreStore.GetValueOrDefault(src);
        }

        private void TryAutoSave()
        {
            foreach (var kvp in _ezSaverCoreStore)
            {
                var ezSaverCore = kvp.Value;

                if (!ezSaverCore.IsJsonStringLiteral)
                    ezSaverCore.Save();
            }
        }

#if !UNITY_EDITOR // Mobile platforms tries to save, when user loses focus or when the app is paused internally
#if UNITY_IOS || UNITY_ANDROID
        private void OnApplicationFocus(bool onFocus)
        {
            if (!onFocus && autoSaveOnQuit)
                TryAutoSave();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused && autoSaveOnQuit)
                TryAutoSave();
        }
#endif
#endif
        private void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
                TryAutoSave();
        }
    }
}