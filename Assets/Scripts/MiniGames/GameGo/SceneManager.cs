namespace Outcasts.Minigames.GameGo
{
    using Common.Cutscenes;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Controls the entirety of the GameGoScene. Now it's just a test class, later it will coordinate the minigame with dialogues, cutscenes, etc
    /// </summary>
    public sealed class SceneManager : MonoBehaviour
    {
        [SerializeField] GameManager m_Manager;

        [SerializeField] List<CutsceneBase> m_GameIntros;
        [SerializeField] List<StealingGameDefinition> m_GameDefinitions;

        [SerializeField] CutsceneBase m_GetOut;

        [SerializeField] GameOverScreen m_GameOverScreen;

        bool m_RestartRequested = false;

        int m_GamesWon = 0;

        // On Start, start a minigame
        IEnumerator Start()
        {
            if (m_GameDefinitions.Count != m_GameIntros.Count) throw new System.Exception($"GameDefinitions and GameIntros lists must have the same length");
            m_Manager.GameLost += ShowLost;

            m_GamesWon = 0;
#if UNITY_EDITOR
            m_GamesWon = (int)m_StartPhase;
#endif
            int attempts = 0;
            while (m_GamesWon < m_GameDefinitions.Count)
            {
                if (attempts == 0)
                {
                    var currentIntro = m_GameIntros[m_GamesWon];
                    if (currentIntro != null) yield return currentIntro.PlayAwaitable();
                }
                else
                {
                    yield return new WaitUntil(() => m_RestartRequested);
                    m_RestartRequested = false;
                }

                var currentGameDefinition = m_GameDefinitions[m_GamesWon];
                m_Manager.StartGame(currentGameDefinition);
                while (m_Manager.GameState == GameState.Running) yield return null;

                if (m_Manager.GameState == GameState.Won)
                {
                    m_GamesWon++;
                    attempts = 0;
                }
                else attempts++;
            }

            yield return m_GetOut.PlayAwaitable();
            ShowWin();
        }

        void ShowWin() => m_GameOverScreen.Show(GameState.Won, "You Won!");
        void ShowLost(FailReason reason) => m_GameOverScreen.Show(GameState.Lost, GetFeedbackMessage(reason));

        // Returns a text message with the reason why you lost the game.
        // TODO: Replace with a string database lookup
        string GetFeedbackMessage(FailReason reason)
        {
            switch (reason)
            {
                case FailReason.CaughtByGuard: return "You got caught!";
                case FailReason.OutOfTime: return "Time's up";
                default: return "";
            }
        }

        // Called by inspector events
        public void Restart()
        {
            // If this came form the final GameOver screen after winning all games, reload the scene
            if (m_GameOverScreen.State == GameState.Won)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            // If this came from a GameOver screen for a failed game, request to restart the game
            else
            {
                m_GameOverScreen.Hide();
                m_RestartRequested = true;
            }
        }

#if UNITY_EDITOR
        enum Phase
        {
            Start,
            Game2,
            Game3,
            Outro,
        }
        [SerializeField] Phase m_StartPhase;
#endif
    }
}
