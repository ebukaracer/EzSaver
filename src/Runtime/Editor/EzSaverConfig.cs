#if UNITY_EDITOR
using Racer.EzSaver.Utilities;
using UnityEngine;

namespace Racer.EzSaver.Editor
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