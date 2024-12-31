namespace Racer.EzSaver.Demo
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaverCore"/> to save and load data by initializing a one-time instance.
    /// </summary>
    internal class BlueSquare : RedSquare
    {
        protected override void InitializeEzSaver()
        {
            // Initialize the variable once, using 'new()'
            EzSaverCore = new EzSaverCore(gameObject.name + "_Save");
        }

        protected override void OnDestroy()
        {
            // Use the initialized variable across
            EzSaverCore.Write("Highscore", CurrentHighscore);
            EzSaverCore.Save();
        }
    }
}