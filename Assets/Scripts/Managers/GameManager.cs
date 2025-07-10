using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR.Haptics;

// created for Lesson 9

public class GameManager : MonoBehaviour
{
    [SerializeField] private LevelManager[] levels;

    public static GameManager instance;

    private GameState currentState;
    private LevelManager currentLevel;
    private int currentLevelIndex = 0;
    private bool isInputActive = true;

    // SINGLETON PATTERN
    private void Awake()
    {
        SingletonInstance();
    }

    private void SingletonInstance()
    {
        // Ensure that there is only one instance of GameManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levels.Length > 0)
        {
            ChangeState(GameState.Briefing, levels[currentLevelIndex]);
        }
    }

    public void ChangeState(GameState state, LevelManager level)
    {
        currentState = state;
        currentLevel = level;

        switch (currentState)
        {
            case GameState.Briefing:
                StartBriefing();
                break;

            case GameState.LevelStart:
                InitiateLevel();
                break;

            case GameState.LevelIn:
                RunLevel();
                break;

            case GameState.LevelEnd:
                CompleteLevel();
                break;

            case GameState.GameOver:
                GameOver();
                break;

            case GameState.GameEnd:
                GameEnd();

                break;

        }
    }

    private void StartBriefing()
    {
        Debug.Log("Game State: Briefing");
        isInputActive = false;
        ChangeState(GameState.LevelStart, currentLevel);
    }

    private void InitiateLevel()
    {
        Debug.Log("Game State: Level Start");
        isInputActive = true;
        currentLevel.StartLevel();
        ChangeState(GameState.LevelIn, currentLevel);
    }

    private void RunLevel()
    {
        Debug.Log("Game State: Level In");
    }

    private void CompleteLevel()
    {
        Debug.Log("Game State: Level End");
        ChangeState(GameState.LevelStart, levels[++currentLevelIndex]);
    }

    private void GameEnd()
    {
        Debug.Log("Game State: Game End");
    }

    private void GameOver()
    {
        Debug.Log("Game State: Game Over");
    }

    private void OnCanvasGroupChanged()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public enum GameState
    {
        Briefing, // option 0
        LevelStart,// option 1
        LevelIn,// option 2
        LevelEnd,// option 3
        GameOver,// option 4
        GameEnd// option 5
    }
}
