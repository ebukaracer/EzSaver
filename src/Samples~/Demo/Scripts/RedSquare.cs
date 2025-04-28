using Racer.EzSaver.Core;
using Racer.EzSaver.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Racer.EzSaver.Samples
{
    /// <summary>
    /// Demonstrates how to use <see cref="Core.EzSaverCore"/> to save and load data by using <see cref="EzSaverManager"/> singleton.
    /// <remarks>
    /// Ensure <see cref="EzSaverManager"/> prefab or a gameobject containing <see cref="EzSaverManager"/> is present in the scene.
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
            // One time initialization using the singleton instance
            EzSaverCore = EzSaverManager.Instance.CreateSaveFile(_saveFileName, encrypt);
        }

        private void InitializeHighscore()
        {
            // Reused here
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
        }

        public void ClearData()
        {
            // Reused here
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

        /// <summary>
        /// Writing and Saving changes here may not be advisable.
        /// Instead you can Write() on the go, and find a more suitable trigger point to Save() all at once.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Reused here
            EzSaverCore.Write("Highscore", CurrentHighscore);

            // Alternatively, using the singleton instance (unsimplified)
            EzSaverManager.Instance.GetSaveFile(_saveFileName).Save();
        }
    }
}