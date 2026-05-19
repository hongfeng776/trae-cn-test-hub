using UnityEngine;
using UnityEngine.SceneManagement;
using ForestMessenger.Core;

namespace ForestMessenger.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("游戏状态")]
        [SerializeField] private GameState currentGameState;

        public delegate void GameStateChanged(GameState newState);
        public event GameStateChanged OnGameStateChanged;

        public GameState CurrentGameState => currentGameState;

        protected override void Awake()
        {
            base.Awake();
            InitializeGame();
        }

        private void Start()
        {
            ChangeGameState(GameState.Playing);
        }

        private void InitializeGame()
        {
            Application.targetFrameRate = 60;
            Physics2D.gravity = new Vector2(0f, -9.81f * 3f);
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            currentGameState = newState;
            OnGameStateChanged?.Invoke(newState);

            HandleGameStateChange(newState);
        }

        private void HandleGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    break;
            }
        }

        public void PauseGame()
        {
            ChangeGameState(GameState.Paused);
        }

        public void ResumeGame()
        {
            ChangeGameState(GameState.Playing);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentGameState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (currentGameState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
}
