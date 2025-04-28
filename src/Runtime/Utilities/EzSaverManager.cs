using System.Collections.Generic;
using Racer.EzSaver.Core;
using UnityEngine;

// ReSharper disable MemberCanBeProtected.Global

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Singleton class for managing the save-files using multiple instances of <see cref="EzSaverCore"/>.
    /// </summary>
    /// <remarks>
    /// Attach this script to a GameObject in the scene
    /// or drag and drop the <see cref="EzSaverManager"/> prefab(available in this package's samples' demo folder) to your desired scene.
    /// </remarks>
    [DefaultExecutionOrder(-998)]
    public class EzSaverManager : SingletonPattern.SingletonPersistent<EzSaverManager>
    {
        /// <summary>
        /// Dictionary to store EzSaverCore instances by filename.
        /// </summary>
        private Dictionary<string, EzSaverCore> _ezSaverCoreStore = new();

        /// <summary>
        /// Gets the EzSaverCore instance associated with the specified filename.
        /// </summary>
        /// <param name="filename">The filename of the save file.</param>
        /// <returns>The EzSaverCore instance if found; otherwise, null.</returns>
        public EzSaverCore GetSaveFile(string filename)
        {
            if (_ezSaverCoreStore.TryGetValue(filename, out var ezSaverCore))
                return ezSaverCore;

            EzLogger.Error(
                $"No instance of {nameof(EzSaverCore)} was found for '{filename}'.\nEnsure to call {nameof(CreateSaveFile)} first.");

            return null;
        }

        /// <summary>
        /// Creates a new EzSaverCore instance for the specified filename.
        /// </summary>
        /// <param name="filename">The filename of the save file.</param>
        /// <param name="useSecurity">Indicates whether to use security features.</param>
        public EzSaverCore CreateSaveFile(string filename, bool useSecurity = false)
        {
            if (!_ezSaverCoreStore.ContainsKey(filename))
                return _ezSaverCoreStore[filename] = new EzSaverCore(filename, false, useSecurity);

            EzLogger.Warn(
                $"The save-file: '{FileHelper.AssignExtension(filename)}', has already been initialized. Use {nameof(GetSaveFile)} to access it instead.");

            return GetSaveFile(filename);
        }
    }
}