using Racer.EzSaver.Core;
using UnityEngine;

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Utility class for enabling/disabling console messages.
    /// </summary>
    /// <remarks>
    /// Call <see cref="EnableLogs"/> to enable or disable logs.
    /// The logs are automatically disabled on builds.
    /// </remarks>
    public static class EzLogger
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable logs.
        /// Enabled by default when in the Unity editor.
        /// </summary>
        public static bool EnableLogs { get; set; } = true;

        // Disable log messages when not in the Unity editor.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DisableLogs()
        {
#if !UNITY_EDITOR
            EnableLogs = false;
#else
            EnableLogs = true;
#endif
        }

        internal static void Log(object message, Object context = null, string tag = nameof(EzSaverCore))
        {
            if (!EnableLogs) return;

            Debug.unityLogger.Log(tag, message, context);
        }

        internal static void Warn(object message, Object context = null, string tag = nameof(EzSaverCore))
        {
            if (!EnableLogs) return;

            Debug.unityLogger.LogWarning(tag, message, context);
        }

        internal static void Error(object message, Object context = null, string tag = nameof(EzSaverCore))
        {
            if (!EnableLogs) return;

            Debug.unityLogger.LogError(tag, message, context);
        }
    }
}