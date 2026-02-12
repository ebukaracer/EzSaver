using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Racer.EzSaver.Samples
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaverCore"/> to save and load data by initializing and reusing an instance, using <see cref="EzSaverManager"/>.
    /// <remarks>
    /// Ensure <see cref="EzSaverManager"/> prefab or a gameobject containing it, is present in the scene.
    /// </remarks>
    /// </summary>
    internal class RedSquare : MonoBehaviour
    {
        private string _saveFileName;
        private int _currentScore;
        private protected int CurrentHighscore;

        // Cached variable to be initialized once and used across
        private protected EzSaverCore EzSaverCore;

        private Button _button;
        private Text _scoreText;
        private Text _highscoreText;

        [SerializeField] protected bool encrypt;


        private void Awake()
        {
            _button = GetComponent<Button>();
            _scoreText = transform.GetChild(0).GetComponent<Text>();
            _highscoreText = transform.GetChild(1).GetComponent<Text>();
            _saveFileName = gameObject.name + "_Save";

            InitializeEzSaver();
            InitializeHighscore();

            _button.onClick.AddListener(AddScore);
        }

        protected virtual void InitializeEzSaver()
        {
            // Initialized to a file without extension, default extension set in the config, will be used
            EzSaverCore = EzSaverManager.Instance.GetSave(_saveFileName, useSecurity: encrypt);
        }

        private void InitializeHighscore()
        {
            // Initialized variable reused here
            CurrentHighscore = EzSaverCore.Read("Highscore", 0);

            SetHighscoreText();
        }

        private void AddScore()
        {
            _currentScore++;

            SetScore();

            if (CurrentHighscore >= _currentScore) return;

            CurrentHighscore = _currentScore;

            SetHighscoreText();
            WriteChanges();
        }

        public void ClearData()
        {
            // ...reused here too
            EzSaverCore.ClearAll();

            _currentScore = CurrentHighscore = 0;

            SetScore();
            SetHighscoreText();
        }

        private void SetScore()
        {
            _scoreText.text = "Score: " + _currentScore;
        }

        private void SetHighscoreText()
        {
            _highscoreText.text = "Highscore: " + CurrentHighscore;
        }

        protected virtual void WriteChanges()
        {
            // ...reused here too
            EzSaverCore.Write("Highscore", CurrentHighscore);
        }

        /// <summary>
        /// Writing and Saving changes here is not recommended.
        /// Instead, you can Write() on the go, and find a more suitable trigger point to Save() all at once.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Serialized content saved to the initialized file using the full singleton instance (unsimplified)
            EzSaverManager.Instance.GetSave(_saveFileName).Save();

            // Or by simply calling this below, using the initialized variable 
            // EzSaverCore.Save();
        }
    }
}