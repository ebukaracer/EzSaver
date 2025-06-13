using System;

namespace Racer.EzSaver.Core
{
    /// <summary>
    /// Represents an exception thrown by EzSaver.
    /// </summary>
    internal class EzSaverException : Exception
    {
        public EzSaverException(string message)
            : base(message)
        {
        }

        public EzSaverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}