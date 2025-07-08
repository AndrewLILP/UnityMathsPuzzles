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
    [SerializeField] private float transitionDelay = 4f; // Time to wait before transitioning to Puzzle 2

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
    public System.Action OnPuzzleImpossible; // Event for impossible detection
    public System.Action OnPuzzleTransitionToPuzzle2; // NEW: Direct transition event
    public System.Action<int, int> OnBridgeProgressChanged; // current, total
    public System.Action<int, int> OnLandmassProgressChanged; // current, total

    private void Start()
    {
        InitializePuzzle();
    }


    // ALSO REPLACE the InitializePuzzle() method to fix obsolete warning:
    private void InitializePuzzle()
    {
        // FIXED: Use new Unity method instead of obsolete one
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
        yield return new WaitForSeconds(transitionDelay);

        if (showDebugInfo)
            Debug.Log("🔄 Starting direct transition to Puzzle 2...");

        // SIMPLE DIRECT TRANSITION (No cutscene!)
        StartDirectTransitionToPuzzle2();
    }


    // REPLACE the StartDirectTransitionToPuzzle2() method in your PuzzleTracker.cs:
    private void StartDirectTransitionToPuzzle2()
    {
        Debug.Log("🔄 Direct Transition: Königsberg → Path puzzle");

        // Step 1: Disable Puzzle 1 tracker
        this.enabled = false;
        Debug.Log("✅ Disabled PuzzleTracker (Puzzle 1)");

        // Step 2: Enable Puzzle 2 tracker
        Puzzle2Tracker puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();
        if (puzzle2Tracker != null)
        {
            puzzle2Tracker.enabled = true;
            Debug.Log("✅ Enabled Puzzle2Tracker");
        }
        else
        {
            Debug.LogWarning("⚠️ Puzzle2Tracker not found!");
        }

        // Step 3: Reset landmasses for Puzzle 2
        SwitchLandmassesToPuzzle2();

        // Step 4: Update UI for Puzzle 2 - FIXED: Don't call StartNextPuzzle, just set UI directly
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            // FIXED: Set puzzle number to 2 explicitly, don't increment
            uiManager.SetCurrentPuzzleNumber(2);
            uiManager.SetSubtitleText("Puzzle 2: Path");
            uiManager.SetMainText("🎯 Puzzle 2: Path Challenge\n\nTransitioning from Königsberg puzzle...\n\nDetecting your starting position...");
            Debug.Log("✅ Set UI to Puzzle 2 directly");
        }

        // Step 5: Trigger transition event - FIXED: Don't call StartNextPuzzle here
        OnPuzzleTransitionToPuzzle2?.Invoke();

        Debug.Log("✅ Direct transition to Puzzle 2 complete!");
    }

    private void SwitchLandmassesToPuzzle2()
    {
        // Reset landmasses
        LandmassController[] landmasses = FindObjectsOfType<LandmassController>();
        foreach (var landmass in landmasses)
        {
            landmass.ResetLandmass();
            landmass.SetPuzzleMode(2); // Switch to Puzzle 2 mode
        }
        Debug.Log($"✅ Switched {landmasses.Length} landmasses to Puzzle 2 mode");

        // NEW: Reset bridge cubes to invisible state
        ResetBridgeCubesForPuzzle2();
    }

    private void ResetBridgeCubesForPuzzle2()
    {
        // Find all BridgeCrossingDetector components and reset them
        BridgeCrossingDetector[] bridgeDetectors = FindObjectsOfType<BridgeCrossingDetector>();

        foreach (var detector in bridgeDetectors)
        {
            detector.ResetBridge(); // This should make them invisible again
        }

        Debug.Log($"✅ Reset {bridgeDetectors.Length} bridge crossing detectors to invisible state");
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

    // Manual trigger for testing
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

    [ContextMenu("Force Transition to Puzzle 2")]
    public void DebugForceTransition()
    {
        StartDirectTransitionToPuzzle2();
    }
}