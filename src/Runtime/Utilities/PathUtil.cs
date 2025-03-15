// ReSharper disable RedundantUsingDirective

using System.IO;
using Newtonsoft.Json;

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Utility class for managing file paths.
    /// </summary>
    public static class PathUtil
    {
        // The root path of this package.
#if !UNITY_EDITOR
        private static readonly string RootPath = $"{UnityEngine.Application.persistentDataPath}/EzSaver";
#else
        private static readonly string RootPath = "Assets/EzSaver";
#endif

        /// <summary>
        /// Path to the directory where the save files are stored.
        /// The default path is: Assets/EzSaver/Saves
        /// </summary>
        public static string SaveDirPath { get; set; } = $"{RootPath}/Saves";

        // Defaults
        internal static string DefaultFileName => "DefaultSave";
        internal static string DefaultFileExtension => ".json";
        internal static string FullFileName => DefaultFileName + DefaultFileExtension;
        internal static string FullFilePath => Path.Combine(SaveDirPath, FullFileName);
        internal static Formatting FileFormatting => Formatting.Indented;
    }
}