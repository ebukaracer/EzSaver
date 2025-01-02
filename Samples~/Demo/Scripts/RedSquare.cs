using UnityEngine;
using UnityEngine.UI;

namespace Racer.EzSaver.Demo
{
    /// <summary>
    /// Demonstrates how to use <see cref="EzSaver.EzSaverCore"/> to save and load data via the use of <see cref="EzSaverManager"/> singleton.
    /// <remarks>
    /// Ensure <see cref="EzSaverManager"/> prefab or a gameobject containing <see cref="EzSaverManager"/> is present in the scene.
    /// </remarks>
    /// </summary>
    public class RedSquare : MonoBehaviour
    {
        private string _saveFileName;
        private int _currentScore;
        private protected int CurrentHighscore;

        // Simplify by creating a one-time variable(shared)
        private protected EzSaverCore EzSaverCore;

        private Button _button;
        private Text _scoreText;
        private Text _highscoreText;

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
            // Initialize the variable once using the singleton instance
            EzSaverCore = EzSaverManager.Instance.CreateSaveFile(_saveFileName);
        }

        private void InitializeHighscore()
        {
            // Use the initialized variable across
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
            EzSaverCore.Write("Highscore", CurrentHighscore);

            // Alternatively, by using the full (unsimplified) version.
            EzSaverManager.Instance.GetSaveFile(_saveFileName).Save();
        }
    }
}