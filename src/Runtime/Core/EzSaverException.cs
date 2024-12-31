using System;

namespace Racer.EzSaver
{
    /// <summary>
    /// Represents an exception thrown by EzSaver.
    /// </summary>
    public class EzSaverException : Exception
    {
        internal EzSaverException(string message)
            : base(message)
        {
        }

        internal EzSaverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}