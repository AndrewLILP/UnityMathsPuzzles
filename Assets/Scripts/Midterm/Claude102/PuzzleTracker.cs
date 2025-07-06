using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class PuzzleTracker : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private int requiredBridges = 7;
    [SerializeField] private int requiredLandmasses = 4;

    [Header("Impossible Puzzle Detection")]
    [SerializeField] private bool enableImpossibleDetection = true;
    [SerializeField] private float impossibleDetectionDelay = 3f; // Time to wait before showing "impossible" message
    [SerializeField] private float cutsceneDelay = 5f; // Additional time before triggering cutscene

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showProgressUpdates = true;

    // Tracking collections
    private HashSet<string> crossedBridges = new HashSet<string>();
    private HashSet<LandmassType> visitedLandmasses = new HashSet<LandmassType>();
    private List<BridgeController> allBridges = new List<BridgeController>();
    private List<LandmassController> allLandmasses = new List<LandmassController>();

    // Impossible puzzle state
    private bool puzzleIsImpossible = false;
    private bool impossibleMessageShown = false;

    // Events for integration with GameManager/LevelManager
    public System.Action OnPuzzleCompleted;
    public System.Action OnPuzzleFailed;
    public System.Action OnPuzzleImpossible; // New event for impossible detection
    public System.Action OnPuzzleImpossibleCutscene; // New event for cutscene trigger
    public System.Action<int, int> OnBridgeProgressChanged; // current, total
    public System.Action<int, int> OnLandmassProgressChanged; // current, total

    private void Start()
    {
        InitializePuzzle();
    }

    private void InitializePuzzle()
    {
        // Find all bridges and landmasses in the scene
        allBridges = FindObjectsByType<BridgeController>(FindObjectsSortMode.None).ToList();
        allLandmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"Puzzle initialized: {allBridges.Count} bridges found, {allLandmasses.Count} landmasses found");
            Debug.Log($"Required: {requiredBridges} bridges, {requiredLandmasses} landmasses");
        }

        // Validate setup
        if (allBridges.Count < requiredBridges)
        {
            Debug.LogWarning($"Not enough bridges in scene! Found {allBridges.Count}, need {requiredBridges}");
        }

        if (allLandmasses.Count < requiredLandmasses)
        {
            Debug.LogWarning($"Not enough landmasses in scene! Found {allLandmasses.Count}, need {requiredLandmasses}");
        }
    }

    public void OnBridgeCrossed(string bridgeID, LandmassType from, LandmassType to)
    {
        if (crossedBridges.Contains(bridgeID))
        {
            if (showDebugInfo)
                Debug.LogWarning($"Bridge {bridgeID} was already crossed!");
            return;
        }

        crossedBridges.Add(bridgeID);

        if (showProgressUpdates)
            Debug.Log($"Bridge crossed: {bridgeID} ({crossedBridges.Count}/{requiredBridges})");

        // Trigger progress event
        OnBridgeProgressChanged?.Invoke(crossedBridges.Count, requiredBridges);

        CheckPuzzleState();
    }

    public void OnLandmassVisited(LandmassType landmassType)
    {
        if (visitedLandmasses.Contains(landmassType))
        {
            if (showDebugInfo)
                Debug.Log($"Landmass {landmassType} already visited");
            return;
        }

        visitedLandmasses.Add(landmassType);

        if (showProgressUpdates)
            Debug.Log($"Landmass visited: {landmassType} ({visitedLandmasses.Count}/{requiredLandmasses})");

        // Trigger progress event
        OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);

        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        bool bridgesComplete = crossedBridges.Count == requiredBridges;
        bool landmassesComplete = visitedLandmasses.Count == requiredLandmasses;

        // Check for standard completion
        if (bridgesComplete && landmassesComplete)
        {
            CompletePuzzle();
            return;
        }

        // Check for failure (too many bridges)
        if (crossedBridges.Count > requiredBridges)
        {
            FailPuzzle("Too many bridges crossed!");
            return;
        }

        // Check for impossible scenario (Königsberg problem)
        if (enableImpossibleDetection && !puzzleIsImpossible)
        {
            CheckForImpossibleScenario();
        }
    }

    private void CheckForImpossibleScenario()
    {
        // The classic scenario: 6 bridges crossed, all landmasses visited, but 7th bridge is impossible
        bool allLandmassesVisited = visitedLandmasses.Count == requiredLandmasses;
        bool nearlyAllBridgesCrossed = crossedBridges.Count == (requiredBridges - 1); // 6 out of 7

        if (allLandmassesVisited && nearlyAllBridgesCrossed)
        {
            if (showDebugInfo)
                Debug.Log("🔍 Impossible scenario detected! Starting delayed response...");

            puzzleIsImpossible = true;
            StartCoroutine(HandleImpossibleScenario());
        }
    }
    private IEnumerator HandleImpossibleScenario()
    {
        // Wait for the detection delay
        yield return new WaitForSeconds(impossibleDetectionDelay);

        if (!impossibleMessageShown)
        {
            impossibleMessageShown = true;

            if (showDebugInfo)
                Debug.Log("🚫 PUZZLE IMPOSSIBLE: The Seven Bridges of Königsberg cannot be solved!");

            // Trigger UI message event
            OnPuzzleImpossible?.Invoke();
        }

        // Wait additional time for player to read message
        yield return new WaitForSeconds(cutsceneDelay);

        if (showDebugInfo)
            Debug.Log("🎬 Triggering cutscene and level progression...");

        // Trigger cutscene event - Timeline system will handle everything from here
        OnPuzzleImpossibleCutscene?.Invoke();

        // TimelineCutsceneManager will handle the rest:
        // - Play Königsberg timeline if available
        // - Fall back to simple transition if no timeline
        // - Call ForceEndLevel() when transition completes

        // Only use direct fallback if NO Timeline system exists at all
        TimelineCutsceneManager timeline = FindFirstObjectByType<TimelineCutsceneManager>();
        if (timeline == null)
        {
            if (showDebugInfo)
                Debug.Log("No TimelineCutsceneManager found - using direct level progression fallback");

            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.ForceEndLevel(); // Direct bypass when no cutscene system
            }
        }
    }
    private void CompletePuzzle()
    {
        if (showDebugInfo)
            Debug.Log("🎉 PUZZLE COMPLETED! All bridges crossed exactly once and all landmasses visited!");

        OnPuzzleCompleted?.Invoke();

        // Integrate with your existing systems
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.EndLevel(); // Use normal EndLevel for traditional completion
        }
    }

    private void FailPuzzle(string reason)
    {
        if (showDebugInfo)
            Debug.Log($"❌ PUZZLE FAILED: {reason}");

        OnPuzzleFailed?.Invoke();
    }

    public void ResetPuzzle()
    {
        if (showDebugInfo)
            Debug.Log("Resetting puzzle...");

        crossedBridges.Clear();
        visitedLandmasses.Clear();
        puzzleIsImpossible = false;
        impossibleMessageShown = false;

        // Stop any running impossible scenario coroutines
        StopAllCoroutines();

        // Reset all bridges
        foreach (var bridge in allBridges)
        {
            bridge.ResetBridge();
        }

        // Reset all landmasses
        foreach (var landmass in allLandmasses)
        {
            landmass.ResetLandmass();
        }

        // Trigger progress events
        OnBridgeProgressChanged?.Invoke(0, requiredBridges);
        OnLandmassProgressChanged?.Invoke(0, requiredLandmasses);

        if (showDebugInfo)
            Debug.Log("Puzzle reset complete!");
    }

    // Public getters for UI/debugging
    public int BridgesCrossed => crossedBridges.Count;
    public int LandmassesVisited => visitedLandmasses.Count;
    public int RequiredBridges => requiredBridges;
    public int RequiredLandmasses => requiredLandmasses;
    public bool IsPuzzleComplete => crossedBridges.Count == requiredBridges && visitedLandmasses.Count == requiredLandmasses;
    public bool IsPuzzleImpossible => puzzleIsImpossible;

    // Debug method to print current status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintPuzzleStatus()
    {
        Debug.Log($"=== PUZZLE STATUS ===");
        Debug.Log($"Bridges: {crossedBridges.Count}/{requiredBridges}");
        Debug.Log($"Landmasses: {visitedLandmasses.Count}/{requiredLandmasses}");
        Debug.Log($"Crossed bridges: {string.Join(", ", crossedBridges)}");
        Debug.Log($"Visited landmasses: {string.Join(", ", visitedLandmasses)}");
        Debug.Log($"Complete: {IsPuzzleComplete}");
        Debug.Log($"Impossible: {IsPuzzleImpossible}");
    }

    // Manual trigger for testing - UPDATED with ForceEndLevel for consistency
    [ContextMenu("Trigger Impossible Scenario")]
    public void DebugTriggerImpossible()
    {
        if (!puzzleIsImpossible)
        {
            puzzleIsImpossible = true;
            StartCoroutine(HandleImpossibleScenario());
        }
    }

    // NEW: Quick test method to simulate the impossible scenario immediately
    [ContextMenu("Force Test Impossible (Instant)")]
    public void ForceTestImpossible()
    {
        // Simulate crossing 6 bridges and visiting all landmasses
        for (int i = 0; i < 6; i++)
        {
            OnBridgeCrossed($"TestBridge{i}", LandmassType.LandmassA, LandmassType.LandmassB);
        }

        // Visit all required landmasses
        OnLandmassVisited(LandmassType.LandmassA);
        OnLandmassVisited(LandmassType.LandmassB);
        OnLandmassVisited(LandmassType.LandmassC);
        OnLandmassVisited(LandmassType.LandmassD);

        if (showDebugInfo)
            Debug.Log("🧪 Force tested impossible scenario - should trigger detection!");
    }
}