using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Racer.EzSaver.Core;
using UnityEngine;

namespace Racer.EzSaver.Utilities
{
    internal static class FileHelper
    {
        private static string[] FilteredExtensions { get; } = { ".meta" };
        public static string SaveDirPath => EzSaverConfig.Load.FileRootPath;


        /// <summary>
        /// Creates a new file with the given content, if it does not exist initially.
        /// </summary>
        /// <returns><c>true</c> if the file never existed and was created, otherwise <c>false</c>.</returns>
        public static bool CreateString(string filename, string content = "")
        {
            filename = AssignExtension(filename);

            if (Exists(filename))
                return false;

            CreateOrUpdateFile(filename, content);
            return true;
        }

        /// <summary>
        /// Updates the content of an existing file with the new content.
        /// </summary>
        /// <returns><c>true</c> if the file existed and was updated, otherwise <c>false</c></returns>
        public static bool UpdateString(string filename, string content)
        {
            filename = AssignExtension(filename);

            if (!Exists(filename))
                return false;

            CreateOrUpdateFile(filename, content);
            return true;
        }

        private static void CreateOrUpdateFile(string filename, string content)
        {
            try
            {
                CreateDir(SaveDirPath);
                File.WriteAllText(Path.Combine(SaveDirPath, filename), content);

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        /// <summary>
        /// Loads the content of a file as a string.
        /// </summary>
        /// <returns>String contents of the file(if it exists, otherwise an empty string)</returns>
        public static string LoadString(string filename)
        {
            try
            {
                if (Exists(AssignExtension(filename), out var location))
                    return File.ReadAllText(location);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a list of all save-files available, with or without extension. 
        /// </summary>
        public static IEnumerable<string> LoadFiles(bool includeExtension)
        {
            try
            {
                var filteredFiles = Directory.EnumerateFiles(SaveDirPath)
                    .Where(file => !FilteredExtensions.Any(file.ToLower().EndsWith));

                return includeExtension
                    ? filteredFiles.Select(Path.GetFileName)
                    : filteredFiles.Select(Path.GetFileNameWithoutExtension);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return new List<string>();
        }

        public static bool Delete(string filename)
        {
            filename = AssignExtension(filename);

            try
            {
                if (Exists(filename, out var location))
                {
                    File.Delete(location);

                    if (Exists(filename + ".meta", out location))
                        File.Delete(location);

#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif


                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return false;
        }

        public static bool Exists(string filename, bool assignExtension = false)
        {
            return Exists(AssignExtension(filename), out _);
        }

        private static bool Exists(string filename, out string fileLocation)
        {
            fileLocation = Path.Combine(SaveDirPath, filename);
            return File.Exists(fileLocation);
        }


        /// <summary>
        /// Auto assigns an extension, if not included in the filename.
        /// </summary>
        public static string AssignExtension(string filename)
        {
            try
            {
                return Path.HasExtension(filename)
                    ? filename
                    : filename + EzSaverConfig.Load.SaveFileExtension;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return filename;
            }
        }

        private static void CreateDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}