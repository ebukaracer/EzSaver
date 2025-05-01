using Racer.EzSaver.Core;

namespace Racer.EzSaver.Samples
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaverCore"/> to save and load data by initializing and reusing an instance.
    /// </summary>
    internal class BlueSquare : RedSquare
    {
        protected override void InitializeEzSaver()
        {
            // One time initialization, using 'new()'
            EzSaverCore = new EzSaverCore(gameObject.name + "_Save", encrypt);
        }

        protected override void OnDestroy()
        {
            // Reused here
            EzSaverCore.Write("Highscore", CurrentHighscore);
            EzSaverCore.Save();
        }
    }
}