using Racer.EzSaver.Utilities;
using UnityEngine;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Provides methods for reading and writing strings to a file.
    /// </summary>
    internal class FileReader : IReader
    {
        private readonly string _path;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileReader"/> class.
        /// </summary>
        /// <param name="path">Path to the filename.</param>
        public FileReader(string path) => _path = path;

        public bool Exists(string filename, bool assignExtension = false) =>
            FileHelper.Exists(filename, assignExtension);

        public void CreateString(string filename, string content)
        {
            if (!FileHelper.CreateString(filename, content))
                Debug.LogError(
                    $"Failed to write the contents to '{FileHelper.AssignExtension(filename)}'.\nThe file was not found or properly initialized.");
        }

        public string LoadString()
        {
            FileHelper.CreateString(_path);
            return FileHelper.LoadString(_path);
        }

        public void SaveString(string content)
        {
            if (!FileHelper.UpdateString(_path, content))
                Debug.LogError(
                    $"Failed to save the contents to '{FileHelper.AssignExtension(_path)}'.\nThe file was not found or properly initialized.");
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
        public bool Delete()
        {
            return FileHelper.Delete(_path);
        }
    }

    /// <summary>
    /// Defines methods for reading and writing strings.
    /// </summary>
    internal interface IReader
    {
        bool Exists(string filename, bool assignExtension = false);
        string LoadString();
        void CreateString(string filename, string content);
        void SaveString(string content);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
        bool Delete();
    }
}