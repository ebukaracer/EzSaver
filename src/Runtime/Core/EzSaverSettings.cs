namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Represents the settings for <see cref="EzSaverCore"/>.
    /// </summary>
    public class EzSaverSettings
    {
        /// <summary>
        /// Gets the encryptor used for encryption.
        /// </summary>
        public IEncryptor Encryptor { get; }

        /// <summary>
        /// Gets the reader used for reading data.
        /// </summary>
        public IReader Reader { get; }

        /// <summary>
        /// Gets a value indicating whether security is used.
        /// </summary>
        public bool UseSecurity { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EzSaverSettings"/> class.
        /// </summary>
        /// <param name="reader">The reader used for reading data.</param>
        /// <param name="encryptor">The encryptor used for encryption.</param>
        /// <param name="useSecurity">A value indicating whether security is used.</param>
        /// <remarks>
        /// Security here is optional, if enabled, the content will be encrypted/decrypted.
        /// </remarks>
        public EzSaverSettings(IReader reader, IEncryptor encryptor, bool useSecurity = false)
        {
            Encryptor = encryptor;
            Reader = reader;
            UseSecurity = useSecurity;
        }
    }
}