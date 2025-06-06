using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEngine;

namespace Racer.EzSaver.Samples
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaverCore"/> to save and load data by initializing and reusing an instance, using <see cref="EzSaverManager"/>.
    /// <remarks>
    /// Ensure <see cref="EzSaverManager"/> prefab or a gameobject containing it, is present in the scene.
    /// </remarks>
    /// </summary>
    internal class YellowSquare : BlueSquare
    {
        // JSON string-literal to be used.
        private string _contentSrc = @"{""Highscore"": 1}";

        protected override void InitializeEzSaver()
        {
            _contentSrc = PlayerPrefs.GetString($"{nameof(YellowSquare)}", _contentSrc);

            // Initialized to a JSON string-literal defined above
            EzSaverCore = EzSaverManager.Instance.GetSave(_contentSrc, isJsonStringLiteral: true, useSecurity: encrypt);
        }

        protected override void WriteChanges()
        {
            EzSaverCore.Write("Highscore", CurrentHighscore);
        }

        protected override void OnDestroy()
        {
            // Serialize the final save-data to a JSON string-literal and save it to PlayerPrefs.
            PlayerPrefs.SetString($"{nameof(YellowSquare)}", EzSaverCore.Save());
        }
    }
}