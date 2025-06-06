using System;
using System.IO;
using System.Security.Cryptography;
using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Provides AES encryption and decryption methods.
    /// </summary>
    public class AesEncryptor : IEncryptor
    {
        private EzSaverConfig _ezSaverConfig;

        /// <summary>
        /// Encrypts the specified string using AES encryption.
        /// </summary>
        /// <param name="original">The string to encrypt.</param>
        /// <returns>The encrypted string, encoded in Base64.</returns>
        public string Encrypt(string original)
        {
            _ezSaverConfig = EzSaverConfig.Load;

            using var myAes = Aes.Create();
            myAes.Key = KeyGen.StrToBase64Bytes(_ezSaverConfig.ActiveKey);
            myAes.IV = KeyGen.StrToBase64Bytes(_ezSaverConfig.ActiveIv);

            return Convert.ToBase64String(EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV));
        }

        /// <summary>
        /// Decrypts the specified encrypted string using AES decryption.
        /// </summary>
        /// <param name="encrypted">The encrypted string, encoded in Base64.</param>
        /// <returns>The decrypted string.</returns>
        public string Decrypt(string encrypted)
        {
            _ezSaverConfig = EzSaverConfig.Load;

            using var myAes = Aes.Create();
            myAes.Key = KeyGen.StrToBase64Bytes(_ezSaverConfig.ActiveKey);
            myAes.IV = KeyGen.StrToBase64Bytes(_ezSaverConfig.ActiveIv);

            return DecryptStringFromBytes_Aes(Convert.FromBase64String(encrypted), myAes.Key, myAes.IV);
        }

        /// <summary>
        /// Encrypts a string to a byte array using AES encryption.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>The encrypted byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when plainText, key, or iv is null or empty.</exception>
        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText is not { Length: > 0 })
                throw new ArgumentNullException(nameof(plainText));
            if (key is not { Length: > 0 })
                throw new ArgumentNullException(nameof(key));
            if (iv is not { Length: > 0 })
                throw new ArgumentNullException(nameof(iv));

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var encrypted = msEncrypt.ToArray();

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Decrypts a byte array to a string using AES decryption.
        /// </summary>
        /// <param name="cipherText">The byte array to decrypt.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>The decrypted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when cipherText, key, or iv is null or empty.</exception>
        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText is not { Length: > 0 })
                throw new ArgumentNullException(nameof(cipherText));
            if (key is not { Length: > 0 })
                throw new ArgumentNullException(nameof(key));
            if (iv is not { Length: > 0 })
                throw new ArgumentNullException(nameof(iv));

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using var msDecrypt = new MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            var plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }
    }

    /// <summary>
    /// Defines methods for encryption and decryption.
    /// </summary>
    public interface IEncryptor
    {
        /// <summary>
        /// Encrypts the specified content.
        /// </summary>
        /// <param name="content">The content to encrypt.</param>
        /// <returns>The encrypted content.</returns>
        string Encrypt(string content);

        /// <summary>
        /// Decrypts the specified content.
        /// </summary>
        /// <param name="content">The content to decrypt.</param>
        /// <returns>The decrypted content.</returns>
        string Decrypt(string content);
    }
}