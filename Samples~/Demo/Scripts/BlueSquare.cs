using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;

namespace Racer.EzSaver.Samples
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaverCore"/> to save and load data by initializing and reusing an instance, using <see cref="EzSaverManager"/>.
    /// <remarks>
    /// Ensure <see cref="EzSaverManager"/> prefab or a gameobject containing it, is present in the scene.
    /// </remarks>
    /// </summary>
    internal class BlueSquare : RedSquare
    {
        protected override void InitializeEzSaver()
        {
            // Initialized to a file with extension
            EzSaverCore = EzSaverManager.Instance.GetSave(gameObject.name + "_Save.ini", useSecurity: encrypt);
        }

        protected override void WriteChanges()
        {
            EzSaverCore.Write("Highscore", CurrentHighscore);
        }

        protected override void OnDestroy()
        {
            // Serialized content saved to the initialized file
            EzSaverCore.Save();
        }
    }
}