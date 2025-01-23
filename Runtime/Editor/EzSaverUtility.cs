using System;
using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Editor
{
    /// <summary>
    /// Handy utility class reserved for <see cref="EzSaverEditor"/>.
    /// </summary>
    internal static class EzSaverUtility
    {
        private static AesEncryptor _aesEncryptor = new();


        public static string ReadFileContent(string fileName)
        {
            return FileHelper.LoadString(fileName);
        }

        public static void DecryptFile(string fileName)
        {
            var original = ReadFileContent(fileName);

            if (string.IsNullOrEmpty(original))
            {
                EzLogger.Warn("File is empty.");
                return;
            }

            string roundTrip;

            if (original.TrimStart().StartsWith('{'))
            {
                EzLogger.Warn("Save-data was already decrypted.");
                return;
            }

            try
            {
                roundTrip = _aesEncryptor.Decrypt(original);
            }
            catch (Exception e)
            {
                roundTrip = string.Empty;
                EzLogger.Error($"Operation failed.\n{e}");
            }

            if (!FileHelper.SaveString(fileName, string.IsNullOrEmpty(roundTrip) ? "{}" : roundTrip))
                EzLogger.Warn($"Failed to save decrypted-data to '{fileName}'.");
            else
                EzLogger.Log("Operation successful.");
        }

        public static void EncryptFile(string fileName)
        {
            var original = FileHelper.LoadString(fileName);

            if (string.IsNullOrEmpty(original))
            {
                EzLogger.Warn("File is empty.");
                return;
            }

            var encrypted = string.Empty;

            try
            {
                if (original.TrimStart().StartsWith('{'))
                    encrypted = _aesEncryptor.Encrypt(original);
                else
                {
                    EzLogger.Warn("Save-data was already encrypted.");
                    return;
                }
            }
            catch (Exception e)
            {
                EzLogger.Error(e);
            }

            if (!FileHelper.SaveString(fileName, encrypted))
                EzLogger.Warn($"Failed to save encrypted-data to '{fileName}'.");
            else
                EzLogger.Log("Operation successful.");
        }

        public static bool CreateFile()
        {
            var fileName = PathUtil.FullFileName;

            if (FileHelper.Exists(fileName))
            {
                EzLogger.Warn($"'{fileName}' already exists!");
                return false;
            }

            FileHelper.CreateString(PathUtil.DefaultFileName);
            return true;
        }

        public static bool DeleteFile(string fileName)
        {
            if (!FileHelper.Delete(fileName)) return false;

            EzLogger.Log("Operation successful.");
            return true;
        }

        public static void LoadAllFiles()
        {
            foreach (var file in FileHelper.LoadFiles(true)) EzLogger.Log(file);
        }

        public static void DeleteAllFiles()
        {
            foreach (var file in FileHelper.LoadFiles(true)) FileHelper.Delete(file);
        }
    }
}