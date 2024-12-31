namespace Racer.EzSaver
{
    /// <summary>
    /// Factory class for creating instances of EzSaverCore.
    /// </summary>
    public static class EzSaverFactory
    {
        /// <summary>
        /// Gets the current instance of EzSaverCore.
        /// </summary>
        public static EzSaverCore EzSaverCore { get; private set; }

        /// <summary>
        /// Creates and initializes a new instance of EzSaverCore.
        /// </summary>
        /// <param name="contentSource">The source of the content(either a filename, eg. SaveFile.txt or a literal string, eg. "{ "Highscore": 4 }").</param>
        /// <param name="fromLiteral">Indicates whether the content is from a literal string(true) or a file(false-default).</param>
        /// <param name="useSecurity">Indicates whether to use security features(encryption/decryption).</param>
        /// <returns>A new instance of EzSaverCore.</returns>
        public static EzSaverCore Create(string contentSource, bool fromLiteral = false, bool useSecurity = false)
        {
            if (EzSaverCore == null) return EzSaverCore = new EzSaverCore(contentSource, fromLiteral, useSecurity);
            
            EzLogger.Warn("An instance of EzSaverCore already exists.");
            return EzSaverCore;

        }
    }
}