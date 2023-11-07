using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class LevelConfig
{
    public List<GuardGameGoScriptable> guardPatterns;
    public List<ButtonsGameGoScriptable> buttonPatterns;
}

public class GameGoLevelManager : MonoBehaviour
{
    #region Configuration

    [Header("Configuration Panel")] [Space(10)] [Header("Level Configuration Parameters")]
    public SDictionary<int, LevelConfig> levelConfiguration;

    #endregion

    #region DevConfiguration

    [Header("Dev Panel")] [Space(10)] static GameGoLevelManager instance;

    public SpawnerManager spawnerManager;

    public Guard guard;

    public bool inCheck;

    public int levelTest = 0;

    private GuardGameGoScriptable patternToGo;
    private ButtonsGameGoScriptable buttonToGo;

    public float timer;

    public enum GameState
    {
        Begin,
        Start,
        Stop,
        End
    }

    public bool newButton;

    public KeyCode keyToPress;

    public GameState gameState;

    public SpriteRenderer checkTest;
    public GameObject retryButton;

    #endregion

    public static GameGoLevelManager Instance
    {
        get => instance;
        private set => instance = value;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.Begin;
        StartCoroutine(StartinRoutine());
    }

    private void Update()
    {
        checkTest.color = inCheck ? Color.green : Color.red;
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("GameGo");
        }

        if (gameState == GameState.Start)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                gameState = GameState.Stop;
                guard.StopGuard();
                inCheck = false;
                retryButton.SetActive(true);
                return;
            }

            if (!newButton) return;
            if (inCheck && Input.anyKeyDown)
            {
                gameState = GameState.End;
                guard.StopGuard();
                retryButton.SetActive(true);
                
            }

            if (Input.GetKeyDown(keyToPress))
            {
                newButton = false;
                GetComponent<GameGoUIManager>().TargetFunction(GameGoUIManager.TargetState.right);
            }
            else if (Input.anyKeyDown)
            {
                GetComponent<GameGoUIManager>().TargetFunction(GameGoUIManager.TargetState.wrong);
            }
        }
    }

    IEnumerator StartinRoutine()
    {
        RetrieveParametersForGame();
        yield return new WaitForSeconds(1);
        gameState = GameState.Start;
        spawnerManager.GameInit(buttonToGo);
        guard.StartGuard(patternToGo);
        spawnerManager.StartGame();
    }

    private void RetrieveParametersForGame()
    {
        patternToGo = levelConfiguration[levelTest]
            .guardPatterns[Random.Range(0, levelConfiguration[levelTest].guardPatterns.Count)];
        buttonToGo = levelConfiguration[levelTest]
            .buttonPatterns[Random.Range(0, levelConfiguration[levelTest].buttonPatterns.Count)];
        timer = patternToGo.timer;
        
    }

    public void SetUpButton(KeyCode keyCode)
    {
        keyToPress = keyCode;
        newButton = true;
    }

    public void NextPattern()
    {
        guard.StopGuard();
        levelTest++;
        if (levelTest > levelConfiguration.Count)
        {
            gameState = GameState.Stop;
            retryButton.SetActive(true);
            return;
        }
        StartCoroutine(StartinRoutine());
    }
}