using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class pauseMenuManager : MonoBehaviour
{
    public GameObject buttonPlayer;
    public Transform buttonParent;
    public string filePath = "Assets/playersList.txt";

    public GameObject pauseMenuPrefab;
    public static bool isPaused;
    public bool canPause;

    public TextMeshProUGUI displayText;


    void OnEnable()
    {
        retrievePlayers();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
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
    }

    public void retrievePlayers()
    {
        /* string[] names = File.ReadAllLines(filePath);
        foreach (string name in names)
        {
            GameObject button = Instantiate(buttonPlayer, buttonParent);
            button.GetComponentInChildren<UnityEngine.UI.Text>().text = name;
        } */
    }

    public void PauseGame()
    {
        pauseMenuPrefab.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPrefab.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("Exit to main Menu");
        //Application.Quit();
    }

    public void PrintOnSelected(string buttonText)
    {
        if (displayText.gameObject.activeSelf)
        {
            displayText.text = "";
        }

        displayText.text = buttonText;

        displayText.gameObject.SetActive(true);
    }
}
