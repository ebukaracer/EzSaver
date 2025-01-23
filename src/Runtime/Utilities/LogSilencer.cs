using UnityEngine;

namespace Racer.EzSaver.Utilities
{
    /// <summary>
    /// Silences console log messages from EzSaver.
    /// </summary>
    /// <remarks>
    /// Attach this script to a GameObject in the scene
    /// or drag and drop the <see cref="LogSilencer"/> prefab(available in this package's samples' demo folder) to your desired scene.
    /// </remarks>
    [DefaultExecutionOrder(-999)]
    internal sealed class LogSilencer : MonoBehaviour
    {
        private static LogSilencer _instance;

        [SerializeField, Tooltip("Enables or disables console log messages from this package.")]
        private bool silenceLogs = true;


        private void Awake()
        {
            EzLogger.EnableLogs = !silenceLogs;

            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}