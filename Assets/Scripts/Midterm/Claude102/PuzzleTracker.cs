using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// FIXED: PuzzleTracker with proper state management during puzzle transitions
/// Clears state when disabled to prevent interference with Puzzle 2
/// </summary>
public class PuzzleTracker : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private int requiredBridges = 7;
    [SerializeField] private int requiredLandmasses = 4;

    [Header("Impossible Puzzle Detection")]
    [SerializeField] private bool enableImpossibleDetection = true;
    [SerializeField] private float impossibleDetectionDelay = 3f;
    [SerializeField] private float transitionDelay = 4f;

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

    // FIXED: Add state tracking for proper lifecycle management
    private bool isTransitioning = false;

    // Events for integration with GameManager/LevelManager
    public System.Action OnPuzzleCompleted;
    public System.Action OnPuzzleFailed;
    public System.Action OnPuzzleImpossible;
    public System.Action OnPuzzleTransitionToPuzzle2;
    public System.Action<int, int> OnBridgeProgressChanged;
    public System.Action<int, int> OnLandmassProgressChanged;

    private void Start()
    {
        InitializePuzzle();
    }

    /// <summary>
    /// FIXED: Handle component being disabled - clear state to prevent interference
    /// </summary>
    private void OnDisable()
    {
        if (showDebugInfo)
            Debug.Log("🔄 PuzzleTracker disabled - clearing state to prevent interference");

        ClearInternalState();
    }

    /// <summary>
    /// FIXED: Handle component being enabled - reinitialize if needed
    /// </summary>
    private void OnEnable()
    {
        if (showDebugInfo)
            Debug.Log("🔄 PuzzleTracker enabled - checking initialization");

        // Don't reinitialize during transitions
        if (!isTransitioning)
        {
            // Reinitialize if we have no bridges/landmasses (could happen after scene changes)
            if (allBridges.Count == 0 || allLandmasses.Count == 0)
            {
                InitializePuzzle();
            }
        }
    }

    /// <summary>
    /// FIXED: Clear internal state without affecting scene objects
    /// Used when transitioning to prevent cross-puzzle interference
    /// </summary>
    private void ClearInternalState()
    {
        crossedBridges.Clear();
        visitedLandmasses.Clear();
        puzzleIsImpossible = false;
        impossibleMessageShown = false;

        // Stop any running coroutines
        StopAllCoroutines();

        if (showDebugInfo)
            Debug.Log("✅ PuzzleTracker internal state cleared");
    }

    private void InitializePuzzle()
    {
        allBridges = FindObjectsByType<BridgeController>(FindObjectsSortMode.None).ToList();
        allLandmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"Puzzle initialized: {allBridges.Count} bridges found, {allLandmasses.Count} landmasses found");
            Debug.Log($"Required: {requiredBridges} bridges, {requiredLandmasses} landmasses");
        }

        if (allBridges.Count < requiredBridges)
        {
            Debug.LogWarning($"Not enough bridges in scene! Found {allBridges.Count}, need {requiredBridges}");
        }

        if (allLandmasses.Count < requiredLandmasses)
        {
            Debug.LogWarning($"Not enough landmasses in scene! Found {allLandmasses.Count}, need {requiredLandmasses}");
        }
    }

    /// <summary>
    /// FIXED: Only process bridge crossings if this tracker is enabled
    /// Prevents disabled tracker from interfering with Puzzle 2
    /// </summary>
    public void OnBridgeCrossed(string bridgeID, LandmassType from, LandmassType to)
    {
        // FIXED: Early exit if disabled or transitioning
        if (!enabled || isTransitioning)
        {
            if (showDebugInfo)
                Debug.Log($"⏸️ PuzzleTracker: Ignoring bridge {bridgeID} (disabled={!enabled}, transitioning={isTransitioning})");
            return;
        }

        if (crossedBridges.Contains(bridgeID))
        {
            if (showDebugInfo)
                Debug.LogWarning($"Bridge {bridgeID} was already crossed!");
            return;
        }

        crossedBridges.Add(bridgeID);

        if (showProgressUpdates)
            Debug.Log($"Bridge crossed: {bridgeID} ({crossedBridges.Count}/{requiredBridges})");

        OnBridgeProgressChanged?.Invoke(crossedBridges.Count, requiredBridges);
        CheckPuzzleState();
    }

    /// <summary>
    /// FIXED: Only process landmass visits if this tracker is enabled
    /// Prevents disabled tracker from interfering with Puzzle 2
    /// </summary>
    public void OnLandmassVisited(LandmassType landmassType)
    {
        // FIXED: Early exit if disabled or transitioning
        if (!enabled || isTransitioning)
        {
            if (showDebugInfo)
                Debug.Log($"⏸️ PuzzleTracker: Ignoring landmass {landmassType} (disabled={!enabled}, transitioning={isTransitioning})");
            return;
        }

        if (visitedLandmasses.Contains(landmassType))
        {
            if (showDebugInfo)
                Debug.Log($"Landmass {landmassType} already visited");
            return;
        }

        visitedLandmasses.Add(landmassType);

        if (showProgressUpdates)
            Debug.Log($"Landmass visited: {landmassType} ({visitedLandmasses.Count}/{requiredLandmasses})");

        OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);
        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        bool bridgesComplete = crossedBridges.Count == requiredBridges;
        bool landmassesComplete = visitedLandmasses.Count == requiredLandmasses;

        if (bridgesComplete && landmassesComplete)
        {
            CompletePuzzle();
            return;
        }

        if (crossedBridges.Count > requiredBridges)
        {
            FailPuzzle("Too many bridges crossed!");
            return;
        }

        if (enableImpossibleDetection && !puzzleIsImpossible)
        {
            CheckForImpossibleScenario();
        }
    }

    private void CheckForImpossibleScenario()
    {
        bool allLandmassesVisited = visitedLandmasses.Count == requiredLandmasses;
        bool nearlyAllBridgesCrossed = crossedBridges.Count == (requiredBridges - 1);

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
        yield return new WaitForSeconds(impossibleDetectionDelay);

        if (!impossibleMessageShown)
        {
            impossibleMessageShown = true;

            if (showDebugInfo)
                Debug.Log("🚫 PUZZLE IMPOSSIBLE: The Seven Bridges of Königsberg cannot be solved!");

            OnPuzzleImpossible?.Invoke();
        }

        yield return new WaitForSeconds(transitionDelay);

        if (showDebugInfo)
            Debug.Log("🔄 Starting direct transition to Puzzle 2...");

        StartDirectTransitionToPuzzle2();
    }

    /// <summary>
    /// FIXED: Proper transition to Puzzle 2 with complete state management
    /// </summary>
    private void StartDirectTransitionToPuzzle2()
    {
        Debug.Log("🔄 Direct Transition: Königsberg → Path puzzle");

        // FIXED: Set transitioning flag to prevent event processing
        isTransitioning = true;

        // Step 1: Clear this tracker's state BEFORE disabling
        ClearInternalState();

        // Step 2: Try to use PuzzleSystemManager if available
        PuzzleSystemManager puzzleManager = FindFirstObjectByType<PuzzleSystemManager>();
        if (puzzleManager != null)
        {
            puzzleManager.TransitionToPuzzle2();
            Debug.Log("✅ Used PuzzleSystemManager for transition");
        }
        else
        {
            // Fallback: Manual transition
            Debug.Log("⚠️ No PuzzleSystemManager found, using manual transition");

            // Step 2: Disable Puzzle 1 tracker
            this.enabled = false;
            Debug.Log("✅ Disabled PuzzleTracker (Puzzle 1)");

            // Step 3: Enable Puzzle 2 tracker
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
        }

        // Step 4: Switch landmasses to Puzzle 2 mode
        SwitchLandmassesToPuzzle2();

        // Step 5: Update UI for Puzzle 2
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.SetCurrentPuzzleNumber(2);
            uiManager.SetSubtitleText("Puzzle 2: Path");
            uiManager.SetMainText("🎯 Puzzle 2: Path Challenge\n\nTransitioning from Königsberg puzzle...\n\nDetecting your starting position...");
            Debug.Log("✅ Set UI to Puzzle 2 directly");
        }

        // Step 6: Trigger transition event
        OnPuzzleTransitionToPuzzle2?.Invoke();

        Debug.Log("✅ Direct transition to Puzzle 2 complete!");
    }

    private void SwitchLandmassesToPuzzle2()
    {
        LandmassController[] landmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None);
        foreach (var landmass in landmasses)
        {
            landmass.ResetLandmass();
            landmass.SetPuzzleMode(2);
        }
        Debug.Log($"✅ Switched {landmasses.Length} landmasses to Puzzle 2 mode");

        ResetBridgeCubesForPuzzle2();
    }

    private void ResetBridgeCubesForPuzzle2()
    {
        BridgeCrossingDetector[] bridgeDetectors = FindObjectsByType<BridgeCrossingDetector>(FindObjectsSortMode.None);

        foreach (var detector in bridgeDetectors)
        {
            detector.ResetBridge();
        }

        Debug.Log($"✅ Reset {bridgeDetectors.Length} bridge crossing detectors to invisible state");
    }

    private void CompletePuzzle()
    {
        if (showDebugInfo)
            Debug.Log("🎉 PUZZLE COMPLETED! All bridges crossed exactly once and all landmasses visited!");

        OnPuzzleCompleted?.Invoke();

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.EndLevel();
        }
    }

    private void FailPuzzle(string reason)
    {
        if (showDebugInfo)
            Debug.Log($"❌ PUZZLE FAILED: {reason}");

        OnPuzzleFailed?.Invoke();
    }

    /// <summary>
    /// FIXED: Enhanced reset with proper state management
    /// </summary>
    public void ResetPuzzle()
    {
        if (showDebugInfo)
            Debug.Log("Resetting puzzle...");

        // Clear transitioning flag if set
        isTransitioning = false;

        // Clear internal state
        ClearInternalState();

        // Reset all scene objects
        foreach (var bridge in allBridges)
        {
            bridge.ResetBridge();
        }

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

    // FIXED: Add state checking methods
    public bool IsTransitioning => isTransitioning;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintPuzzleStatus()
    {
        Debug.Log($"=== PUZZLE STATUS ===");
        Debug.Log($"Enabled: {enabled}");
        Debug.Log($"Transitioning: {isTransitioning}");
        Debug.Log($"Bridges: {crossedBridges.Count}/{requiredBridges}");
        Debug.Log($"Landmasses: {visitedLandmasses.Count}/{requiredLandmasses}");
        Debug.Log($"Crossed bridges: {string.Join(", ", crossedBridges)}");
        Debug.Log($"Visited landmasses: {string.Join(", ", visitedLandmasses)}");
        Debug.Log($"Complete: {IsPuzzleComplete}");
        Debug.Log($"Impossible: {IsPuzzleImpossible}");
    }

    [ContextMenu("Trigger Impossible Scenario")]
    public void DebugTriggerImpossible()
    {
        if (!puzzleIsImpossible)
        {
            puzzleIsImpossible = true;
            StartCoroutine(HandleImpossibleScenario());
        }
    }

    [ContextMenu("Force Test Impossible (Instant)")]
    public void ForceTestImpossible()
    {
        for (int i = 0; i < 6; i++)
        {
            OnBridgeCrossed($"TestBridge{i}", LandmassType.LandmassA, LandmassType.LandmassB);
        }

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