#if UNITY_EDITOR
using UnityEngine;

namespace Racer.EzSaver
{
    /// <summary>
    /// Configuration class for EzSaver, used to store settings in the Unity Editor.
    /// </summary>
    internal class EzSaverConfig : ScriptableObject
    {
        public static readonly string DefaultFileName = PathUtil.FullFileName;

        public string fileName = DefaultFileName;
    }
}
#endif