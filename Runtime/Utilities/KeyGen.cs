using System;
using System.Security.Cryptography;

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
        /// Returns a generated random key(as string).
        /// </summary>
        public static string GetRandomBase64Str() => GenerateRndStr();

        /// <summary>
        /// Returns the byte representation of generated random key(string) 
        /// </summary>
        public static byte[] StrToBase64Bytes(string key) => Convert.FromBase64String(key);
    }
}