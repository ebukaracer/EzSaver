#if UNITY_EDITOR
using System;
using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEngine;

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
                Debug.Log("File is empty.");
                return;
            }

            string roundTrip;

            if (original.TrimStart().StartsWith('{'))
            {
                Debug.Log("Save-file data was already decrypted.");
                return;
            }

            try
            {
                roundTrip = DecryptString(original);
            }
            catch (Exception e)
            {
                roundTrip = original;
                Debug.LogError($"Operation failed.\n{e}");
            }

            if (roundTrip != original && !FileHelper.UpdateString(fileName, roundTrip))
                Debug.LogWarning($"Failed to save decrypted-data to '{fileName}'.");
        }

        public static void EncryptFile(string fileName)
        {
            var original = FileHelper.LoadString(fileName);

            if (string.IsNullOrEmpty(original))
            {
                Debug.Log("File is empty.");
                return;
            }

            var encrypted = string.Empty;

            try
            {
                if (original.Length <= 2)
                {
                    Debug.Log("Save-file contains empty JSON data.");
                    return;
                }

                if (original.TrimStart().StartsWith('{'))
                    encrypted = EncryptString(original);
                else
                {
                    Debug.Log("Save-file data was already encrypted.");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Operation failed.\n{e}");
            }

            if (!FileHelper.UpdateString(fileName, encrypted))
                Debug.LogWarning($"Failed to save encrypted-data to '{fileName}'.");
        }

        public static string EncryptString(string content) => _aesEncryptor.Encrypt(content);
        public static string DecryptString(string content) => _aesEncryptor.Decrypt(content);

        public static bool CreateFile(string filename)
        {
            if (FileHelper.Exists(filename))
            {
                Debug.LogWarning($"The file '{filename}' already exists in that location!");
                return false;
            }

            FileHelper.CreateString(filename);
            return true;
        }

        public static bool DeleteFile(string fileName)
        {
            return FileHelper.Delete(fileName);
        }

        public static void LoadAllFiles()
        {
            foreach (var file in FileHelper.LoadFiles(true)) Debug.Log(file);
        }

        public static void DeleteAllFiles()
        {
            foreach (var file in FileHelper.LoadFiles(true)) FileHelper.Delete(file);
        }
    }
}
#endif