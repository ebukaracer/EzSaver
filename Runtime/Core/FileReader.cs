using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Provides methods for reading and writing strings to a file.
    /// </summary>
    internal class FileReader : IReader
    {
        private readonly string _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReader"/> class.
        /// </summary>
        /// <param name="root">The root path of the file.</param>
        public FileReader(string root)
        {
            _root = root;
        }

        /// <summary>
        /// Loads the content of the file as a string.
        /// </summary>
        /// <returns>The content of the file as a string.</returns>
        public string LoadString()
        {
            if (FileHelper.CreateString(_root))
                EzLogger.Warn(
                    $"No existing {FileHelper.AssignExtension(_root)} was found, a new one was initialized.");

            return FileHelper.LoadString(_root);
        }

        /// <summary>
        /// Saves the specified content to the file.
        /// </summary>
        /// <param name="content">The content to save.</param>
        public void SaveString(string content)
        {
            if (!FileHelper.SaveString(_root, content))
                EzLogger.Warn(
                    $"Failed to save content to '{FileHelper.AssignExtension(_root)}'.\nThe file was not found or properly initialized.");
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
        public bool Delete()
        {
            return FileHelper.Delete(_root);
        }
    }

    /// <summary>
    /// Defines methods for reading and writing strings.
    /// </summary>
    public interface IReader
    {
        /// <summary>
        /// Loads the content of the file as a string.
        /// </summary>
        /// <returns>The content of the file as a string.</returns>
        string LoadString();

        /// <summary>
        /// Saves the specified content to the file.
        /// </summary>
        /// <param name="content">The content to save.</param>
        void SaveString(string content);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
        bool Delete();
    }
}