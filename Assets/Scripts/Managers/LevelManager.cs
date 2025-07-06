using UnityEngine;
using UnityEngine.Events;

// Updated for Lesson 9 + Bridge Puzzle Integration

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private bool isFinalLevel;

    [Header("Puzzle Integration")]
    [SerializeField] private bool useBridgePuzzle = true; // Toggle for bridge puzzle levels
    [SerializeField] private bool requireAllObjectives = true; // Require both bridges AND other objectives

    [Header("Events")]
    public UnityEvent onLevelStart;
    public UnityEvent onLevelEnd;
    public UnityEvent onPuzzleCompleted; // New event for puzzle completion
    public UnityEvent onPuzzleFailed; // New event for puzzle failure

    // Component references
    private PuzzleTracker puzzleTracker;
    private bool puzzleComplete = false;
    private bool levelComplete = false;

    private void Start()
    {
        InitializePuzzleIntegration();
    }

    private void InitializePuzzleIntegration()
    {
        if (useBridgePuzzle)
        {
            // Find PuzzleTracker in scene
            puzzleTracker = FindObjectOfType<PuzzleTracker>();

            if (puzzleTracker != null)
            {
                // Subscribe to puzzle events
                puzzleTracker.OnPuzzleCompleted += OnPuzzleCompleted;
                puzzleTracker.OnPuzzleFailed += OnPuzzleFailed;

                Debug.Log($"LevelManager: Bridge puzzle integration enabled for {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"LevelManager: Bridge puzzle enabled but PuzzleTracker not found in scene!");
                useBridgePuzzle = false; // Disable if not found
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (puzzleTracker != null)
        {
            puzzleTracker.OnPuzzleCompleted -= OnPuzzleCompleted;
            puzzleTracker.OnPuzzleFailed -= OnPuzzleFailed;
        }
    }

    public void StartLevel()
    {
        Debug.Log($"Starting level: {gameObject.name}");

        // Reset puzzle state
        puzzleComplete = false;
        levelComplete = false;

        // Reset puzzle if it exists
        if (useBridgePuzzle && puzzleTracker != null)
        {
            puzzleTracker.ResetPuzzle();
        }

        onLevelStart?.Invoke();
    }

    public void EndLevel()
    {
        if (levelComplete) return; // Prevent multiple calls

        // Check if we need to wait for puzzle completion
        if (useBridgePuzzle && requireAllObjectives && !puzzleComplete)
        {
            Debug.Log("LevelManager: Cannot end level - bridge puzzle not completed yet");
            return;
        }

        levelComplete = true;
        Debug.Log($"Ending level: {gameObject.name}");

        onLevelEnd?.Invoke();

        if (isFinalLevel)
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.GameEnd, this);
        }
        else
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.LevelEnd, this);
        }
    }

    // Called when puzzle system detects completion
    private void OnPuzzleCompleted()
    {
        puzzleComplete = true;
        Debug.Log("LevelManager: Bridge puzzle completed!");

        onPuzzleCompleted?.Invoke();

        if (!requireAllObjectives)
        {
            // If puzzle completion is the only requirement, end level immediately
            EndLevel();
        }
        else
        {
            Debug.Log("LevelManager: Puzzle complete, waiting for other objectives...");
        }
    }

    // Called when puzzle system detects failure
    private void OnPuzzleFailed()
    {
        Debug.Log("LevelManager: Bridge puzzle failed!");
        onPuzzleFailed?.Invoke();

        // Optionally trigger game over or reset
        // GameManager.GetInstance().ChangeState(GameManager.GameState.GameOver, this);
    }

    // Public method to force end level (for other triggers like LevelTrigger.cs)
    public void ForceEndLevel()
    {
        Debug.Log("LevelManager: Force ending level (bypassing puzzle requirements)");
        levelComplete = true;
        onLevelEnd?.Invoke();

        if (isFinalLevel)
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.GameEnd, this);
        }
        else
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.LevelEnd, this);
        }
    }

    // Method to check if level can be completed
    public bool CanCompleteLevel()
    {
        if (!useBridgePuzzle) return true;
        if (!requireAllObjectives) return true;
        return puzzleComplete;
    }

    // Public getters for other systems to check status
    public bool IsPuzzleComplete => puzzleComplete;
    public bool IsLevelComplete => levelComplete;
    public bool UsesBridgePuzzle => useBridgePuzzle;

    // Method to get puzzle progress (for UI)
    public (int bridgesCrossed, int totalBridges, int landmassesVisited, int totalLandmasses) GetPuzzleProgress()
    {
        if (puzzleTracker != null)
        {
            return (puzzleTracker.BridgesCrossed, puzzleTracker.RequiredBridges,
                   puzzleTracker.LandmassesVisited, puzzleTracker.RequiredLandmasses);
        }
        return (0, 0, 0, 0);
    }

    // Debug method to print level status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintLevelStatus()
    {
        Debug.Log($"=== LEVEL STATUS ({gameObject.name}) ===");
        Debug.Log($"Uses Bridge Puzzle: {useBridgePuzzle}");
        Debug.Log($"Require All Objectives: {requireAllObjectives}");
        Debug.Log($"Puzzle Complete: {puzzleComplete}");
        Debug.Log($"Level Complete: {levelComplete}");
        Debug.Log($"Can Complete Level: {CanCompleteLevel()}");

        if (puzzleTracker != null)
        {
            var progress = GetPuzzleProgress();
            Debug.Log($"Puzzle Progress: {progress.bridgesCrossed}/{progress.totalBridges} bridges, {progress.landmassesVisited}/{progress.totalLandmasses} landmasses");
        }
        Debug.Log("================================");
    }
}