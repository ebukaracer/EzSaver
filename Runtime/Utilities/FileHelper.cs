using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Racer.EzSaver.Utilities
{
    internal static class FileHelper
    {
        private static string[] FilteredExtensions { get; } = { ".meta" };
        private static string SaveDirPath => PathUtil.SaveDirPath;


        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <returns>True if the file was created, otherwise false.</returns>
        public static bool CreateString(string filename, string content = "")
        {
            filename = AssignExtension(filename);

            // Do not create a new file if one already exists, hence return false.
            if (Exists(filename))
                return false;

            CreateOrWriteToFile(filename, content);

            return true;
        }

        public static bool SaveString(string filename, string content)
        {
            filename = AssignExtension(filename);

            if (!Exists(filename))
                return false;

            CreateOrWriteToFile(filename, content);

            return true;
        }

        private static void CreateOrWriteToFile(string filename, string content)
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
                EzLogger.Error(e);
            }
        }

        public static string LoadString(string filename)
        {
            filename = AssignExtension(filename);

            try
            {
                if (Exists(filename, out var location))
                {
                    return File.ReadAllText(location);
                }
            }
            catch (Exception e)
            {
                EzLogger.Error(e);
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
                EzLogger.Error(e);
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
                EzLogger.Error(e);
            }

            return false;
        }

        public static bool Exists(string filename)
        {
            return Exists(filename, out _);
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
                    : filename + PathUtil.DefaultFileExtension;
            }
            catch (Exception e)
            {
                EzLogger.Error(e);
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