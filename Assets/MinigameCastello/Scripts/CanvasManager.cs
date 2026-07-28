using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject GameOverMenu;

    public static bool isPaused;
    public bool canPause;
    public bool isCaught;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        GameOverMenu.SetActive(false);
        Time.timeScale = 1f;
        canPause = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (isCaught)
        {
            GameOver();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void GameOver()
    {
        GameOverMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = false;
        canPause = false;
    }

    public void RestartMinigame()
    {
        SceneManager.LoadScene("MinigameCastello");
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        //SceneManager.LoadScene("MainMenu");
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
