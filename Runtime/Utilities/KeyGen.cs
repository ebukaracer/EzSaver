using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Utility class for generating and storing unique random key and initialization vector (IV).
    /// </summary>
    /// <remarks>
    /// Valid Key sizes:
    /// 16 bytes * 8 bits/byte = 128 bits[16 byte values - fastest(default)].
    /// 24 bytes * 8 bits/byte = 192 bits[24 byte values - faster].
    /// 32 bytes * 8 bits/byte = 256 bits[32 byte values - fast].
    /// </remarks>
    internal static class KeyGen
    {
        private const int KeySize = 16;
        private const string ID = "_id";
        private const string Iv = "_iv";

        /// <summary>
        /// Generates a random string using a cryptographic random number generator.
        /// </summary>
        /// <returns>A Base64 encoded random string.</returns>
        private static string GenerateRndStr()
        {
            using var random = RandomNumberGenerator.Create();
            var key = new byte[KeySize];
            random.GetBytes(key);
            
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Sets a random key and stores it in PlayerPrefs.
        /// </summary>
        /// <param name="rndKey">The generated random key.</param>
        public static void SetRandomKey(out string rndKey)
        {
            rndKey = GenerateRndStr();
            PlayerPrefs.SetString(ID, rndKey);
        }

        /// <summary>
        /// Retrieves the stored key string from PlayerPrefs, generating a new one if it does not exist.
        /// </summary>
        /// <returns>The stored key string.</returns>
        public static string GetKeyStr()
        {
            if (!PlayerPrefs.HasKey(ID))
                PlayerPrefs.SetString(ID, GenerateRndStr());

            return PlayerPrefs.GetString(ID);
        }

        /// <summary>
        /// Retrieves the stored key as a byte array.
        /// </summary>
        /// <returns>The stored key as a byte array.</returns>
        public static byte[] GetKeyBytes()
        {
            return Convert.FromBase64String(GetKeyStr());
        }

        /// <summary>
        /// Retrieves the stored initialization vector (IV) as a byte array, generating a new one if it does not exist.
        /// </summary>
        /// <returns>The stored IV as a byte array.</returns>
        public static byte[] GetIvBytes()
        {
            if (!PlayerPrefs.HasKey(Iv))
                PlayerPrefs.SetString(Iv, GenerateRndStr());

            return Convert.FromBase64String(PlayerPrefs.GetString(Iv));
        }

        /// <summary>
        /// Clears the stored key and IV from PlayerPrefs.
        /// </summary>
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteKey(ID);
            PlayerPrefs.DeleteKey(Iv);
        }
    }
}