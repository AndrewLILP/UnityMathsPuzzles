using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// Puzzle 2: Path Puzzle - Visit all landmasses without revisiting any
/// Victory: Visit all 4 landmasses exactly once
/// Failure: Revisit a landmass (triggers reset)
/// </summary>
public class Puzzle2Tracker : MonoBehaviour
{
    [Header("Puzzle 2 Configuration")]
    [SerializeField] private int requiredLandmasses = 4;
    [SerializeField] private bool allowBridgeReuse = true; // Bridges can be crossed multiple times in Puzzle 2

    [Header("Reset Settings")]
    [SerializeField] private float resetDelay = 2f; // Time to show message before reset
    [SerializeField] private string resetMessage = "You returned to a previously visited area. Puzzle reset!";

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showProgressUpdates = true;

    // Tracking collections
    private HashSet<LandmassType> visitedLandmasses = new HashSet<LandmassType>();
    private List<LandmassType> visitOrder = new List<LandmassType>(); // Track visit order
    private List<LandmassController> allLandmasses = new List<LandmassController>();

    // State tracking
    private bool puzzleComplete = false;
    private bool puzzleResetInProgress = false;

    // Events for integration with LevelManager/UI
    public System.Action OnPuzzleCompleted;
    public System.Action OnPuzzleFailed;
    public System.Action OnPuzzleReset;
    public System.Action<int, int> OnLandmassProgressChanged; // current, total
    public System.Action<string> OnResetMessageTriggered; // For UI message display

    private void Start()
    {
        InitializePuzzle2();
    }

    private void InitializePuzzle2()
    {
        // Find all landmasses in the scene
        allLandmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"Puzzle 2 initialized: {allLandmasses.Count} landmasses found");
            Debug.Log($"Required: Visit all {requiredLandmasses} landmasses exactly once");
        }

        // Validate setup
        if (allLandmasses.Count < requiredLandmasses)
        {
            Debug.LogWarning($"Not enough landmasses in scene! Found {allLandmasses.Count}, need {requiredLandmasses}");
        }

        // Update UI for Puzzle 2
        UpdatePuzzle2UI();
    }

    public void OnLandmassVisited(LandmassType landmassType)
    {
        if (puzzleComplete || puzzleResetInProgress) return;

        // Check if this landmass was already visited
        if (visitedLandmasses.Contains(landmassType))
        {
            if (showDebugInfo)
                Debug.LogWarning($"🚫 PUZZLE 2 VIOLATION: Player revisited {landmassType}!");

            TriggerPuzzleReset($"You revisited {landmassType}! {resetMessage}");
            return;
        }

        // Valid visit - add to tracking
        visitedLandmasses.Add(landmassType);
        visitOrder.Add(landmassType);

        if (showProgressUpdates)
            Debug.Log($"Puzzle 2 - Landmass visited: {landmassType} ({visitedLandmasses.Count}/{requiredLandmasses})");

        // Trigger progress event
        OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);

        // Check for completion
        CheckPuzzle2State();
    }

    private void CheckPuzzle2State()
    {
        if (visitedLandmasses.Count == requiredLandmasses)
        {
            CompletePuzzle2();
        }
    }

    private void CompletePuzzle2()
    {
        puzzleComplete = true;

        if (showDebugInfo)
        {
            Debug.Log("🎉 PUZZLE 2 COMPLETED! All landmasses visited exactly once!");
            Debug.Log($"Visit order: {string.Join(" → ", visitOrder)}");
        }

        OnPuzzleCompleted?.Invoke();

        // Integrate with LevelManager
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.EndLevel();
        }
    }

    private void TriggerPuzzleReset(string message)
    {
        if (puzzleResetInProgress) return;
        puzzleResetInProgress = true;

        if (showDebugInfo)
            Debug.Log($"🔄 Puzzle 2 Reset Triggered: {message}");

        // Trigger UI message
        OnResetMessageTriggered?.Invoke(message);
        OnPuzzleFailed?.Invoke();

        // Start reset after delay
        StartCoroutine(DelayedReset());
    }

    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetPuzzle2();
    }

    public void ResetPuzzle2()
    {
        if (showDebugInfo)
            Debug.Log("Resetting Puzzle 2...");

        // Clear tracking
        visitedLandmasses.Clear();
        visitOrder.Clear();
        puzzleComplete = false;
        puzzleResetInProgress = false;

        // Reset all landmasses
        foreach (var landmass in allLandmasses)
        {
            landmass.ResetLandmass();
        }

        // Trigger progress events
        OnLandmassProgressChanged?.Invoke(0, requiredLandmasses);
        OnPuzzleReset?.Invoke();

        if (showDebugInfo)
            Debug.Log("Puzzle 2 reset complete!");
    }

    private void UpdatePuzzle2UI()
    {
        // Update UI to show Puzzle 2 instructions
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.SetCurrentPuzzleNumber(2);
            uiManager.SetSubtitleText("Puzzle 2: Path");
            uiManager.SetMainText("Visit all Land Masses: You can walk from one land area to another, but you can never return to a land area you've already visited");
        }
    }

    // Public getters for UI/debugging
    public int LandmassesVisited => visitedLandmasses.Count;
    public int RequiredLandmasses => requiredLandmasses;
    public bool IsPuzzleComplete => puzzleComplete;
    public List<LandmassType> VisitOrder => new List<LandmassType>(visitOrder); // Return copy
    public bool IsResetInProgress => puzzleResetInProgress;

    // Debug method to print current status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintPuzzle2Status()
    {
        Debug.Log($"=== PUZZLE 2 STATUS ===");
        Debug.Log($"Landmasses: {visitedLandmasses.Count}/{requiredLandmasses}");
        Debug.Log($"Visited landmasses: {string.Join(", ", visitedLandmasses)}");
        Debug.Log($"Visit order: {string.Join(" → ", visitOrder)}");
        Debug.Log($"Complete: {IsPuzzleComplete}");
        Debug.Log($"Reset in progress: {IsResetInProgress}");
    }

    // Manual trigger for testing
    [ContextMenu("Force Complete Puzzle 2")]
    public void DebugForcePuzzle2Complete()
    {
        // Simulate visiting all landmasses
        OnLandmassVisited(LandmassType.LandmassA);
        OnLandmassVisited(LandmassType.LandmassB);
        OnLandmassVisited(LandmassType.LandmassC);
        OnLandmassVisited(LandmassType.LandmassD);

        if (showDebugInfo)
            Debug.Log("🧪 Debug: Forced Puzzle 2 completion!");
    }

    [ContextMenu("Force Reset Puzzle 2")]
    public void DebugForceReset()
    {
        TriggerPuzzleReset("Debug reset triggered");
    }

    [ContextMenu("Test Revisit Violation")]
    public void DebugTestRevisit()
    {
        // Visit A, then visit A again (should trigger reset)
        OnLandmassVisited(LandmassType.LandmassA);
        OnLandmassVisited(LandmassType.LandmassA); // This should trigger reset
    }
}