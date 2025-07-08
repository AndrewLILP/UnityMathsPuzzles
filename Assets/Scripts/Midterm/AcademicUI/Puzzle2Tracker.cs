using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// Puzzle 2: Path Puzzle - Visit all landmasses without revisiting any
/// HOUR 1.1 FIX: Fixed UI issues and obsolete warnings
/// Victory: Visit all 4 landmasses exactly once
/// Failure: Revisit a landmass (triggers reset)
/// </summary>
public class Puzzle2Tracker : MonoBehaviour
{
    [Header("Puzzle 2 Configuration")]
    [SerializeField] private int requiredLandmasses = 4;
    [SerializeField] private bool allowBridgeReuse = true; // Bridges can be crossed multiple times in Puzzle 2

    [Header("Starting Position Detection")]
    [SerializeField] private float detectionRange = 50f; // How close player must be to landmass to detect it
    [SerializeField] private bool autoDetectStartingPosition = true; // Enable/disable starting position detection

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
    private bool hasDetectedStartingPosition = false; // Track if we've set starting position
    private LandmassType startingLandmass; // Remember which landmass we started on

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

    private void OnEnable()
    {
        // When Puzzle 2 becomes active, detect starting position
        if (autoDetectStartingPosition && !hasDetectedStartingPosition)
        {
            StartCoroutine(DetectStartingLandmass());
        }
    }

    private void InitializePuzzle2()
    {
        // FIXED: Use new Unity method instead of obsolete one
        allLandmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"Puzzle 2 initialized: {allLandmasses.Count} landmasses found");
            Debug.Log($"Required: Visit all {requiredLandmasses} landmasses exactly once");
            Debug.Log($"Starting position detection: {(autoDetectStartingPosition ? "ENABLED" : "DISABLED")}");
        }

        // Validate setup
        if (allLandmasses.Count < requiredLandmasses)
        {
            Debug.LogWarning($"Not enough landmasses in scene! Found {allLandmasses.Count}, need {requiredLandmasses}");
        }

        // Update UI for Puzzle 2 - FIXED: Only call this once and set correct puzzle number
        UpdatePuzzle2UIInitial();
    }

    // FIXED: Separate method for initial UI setup
    private void UpdatePuzzle2UIInitial()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.SetCurrentPuzzleNumber(2); // FIXED: Explicitly set to 2
            uiManager.SetSubtitleText("Puzzle 2: Path");
            // Don't set main text here - let DetectStartingLandmass() handle it

            if (showDebugInfo)
                Debug.Log("Puzzle 2 UI initialized to Puzzle 2");
        }
    }

    // Auto-detect which landmass player is standing on when Puzzle 2 starts
    private IEnumerator DetectStartingLandmass()
    {
        // Wait a frame to ensure everything is initialized
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ Player not found! Cannot detect starting landmass.");
            yield break;
        }

        // Check which landmass player is closest to
        LandmassController closestLandmass = null;
        float closestDistance = float.MaxValue;

        foreach (var landmass in allLandmasses)
        {
            if (landmass == null) continue;

            float distance = Vector3.Distance(player.transform.position, landmass.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLandmass = landmass;
            }
        }

        if (closestLandmass != null && closestDistance < detectionRange)
        {
            // Mark this landmass as already visited (starting position)
            startingLandmass = closestLandmass.LandmassType;

            visitedLandmasses.Add(startingLandmass);
            visitOrder.Add(startingLandmass);
            hasDetectedStartingPosition = true;

            if (showDebugInfo)
                Debug.Log($"🎯 Puzzle 2 starting position detected: {startingLandmass} (distance: {closestDistance:F1}m)");

            // Update progress (1/4 landmasses visited to start)
            OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);

            // Update UI to show starting status
            UpdateUIWithStartingPosition();
        }
        else
        {
            string warningMsg = closestLandmass != null
                ? $"Closest landmass ({closestLandmass.LandmassType}) is {closestDistance:F1}m away (max: {detectionRange}m)"
                : "No landmasses found in scene";

            Debug.LogWarning($"⚠️ Could not detect starting landmass: {warningMsg}");

            // Still update UI with generic message
            UpdateUIWithoutStartingPosition();
        }
    }

    private void UpdateUIWithStartingPosition()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            int remaining = requiredLandmasses - visitedLandmasses.Count;

            // FIXED: Don't call SetCurrentPuzzleNumber here - it's already set
            uiManager.SetMainText($"🎯 Puzzle 2: Path Challenge\n\n" +
                                $"Starting at: {startingLandmass}\n\n" +
                                $"Visit the remaining {remaining} landmasses exactly once.\n\n" +
                                $"⚠️ You cannot return to any landmass you've visited!\n\n" +
                                $"Progress: {visitedLandmasses.Count}/{requiredLandmasses} landmasses");

            if (showDebugInfo)
                Debug.Log($"Updated UI with starting position: {startingLandmass}");
        }
    }

    private void UpdateUIWithoutStartingPosition()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.SetMainText($"🎯 Puzzle 2: Path Challenge\n\n" +
                                $"Visit all {requiredLandmasses} landmasses exactly once.\n\n" +
                                $"⚠️ You cannot return to any landmass you've visited!\n\n" +
                                $"Progress: {visitedLandmasses.Count}/{requiredLandmasses} landmasses");
        }
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

        // Update UI with current progress
        UpdateProgressUI();

        // Check for completion
        CheckPuzzle2State();
    }

    private void UpdateProgressUI()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            int remaining = requiredLandmasses - visitedLandmasses.Count;

            if (remaining > 0)
            {
                string visitedList = string.Join(" → ", visitOrder);
                uiManager.SetMainText($"🎯 Puzzle 2: Path Challenge\n\n" +
                                    $"Visited: {visitedList}\n\n" +
                                    $"Remaining: {remaining} landmasses\n\n" +
                                    $"⚠️ You cannot return to any visited landmass!\n\n" +
                                    $"Progress: {visitedLandmasses.Count}/{requiredLandmasses}");
            }
        }
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

        // Update UI with completion message
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            string visitedList = string.Join(" → ", visitOrder);
            uiManager.SetMainText($"🎉 Puzzle 2 COMPLETED!\n\n" +
                                $"Perfect path: {visitedList}\n\n" +
                                $"You visited all {requiredLandmasses} landmasses exactly once!\n\n" +
                                $"This demonstrates a fundamental concept in graph theory!");
        }

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

        // Clear tracking but preserve starting position detection capability
        visitedLandmasses.Clear();
        visitOrder.Clear();
        puzzleComplete = false;
        puzzleResetInProgress = false;
        hasDetectedStartingPosition = false; // Reset this so it detects again
        startingLandmass = LandmassType.LandmassA; // Reset to default

        // Reset all landmasses
        foreach (var landmass in allLandmasses)
        {
            landmass.ResetLandmass();
        }

        // Re-detect starting position if auto-detection is enabled
        if (autoDetectStartingPosition)
        {
            StartCoroutine(DetectStartingLandmass());
        }
        else
        {
            // If auto-detection disabled, just update progress
            OnLandmassProgressChanged?.Invoke(0, requiredLandmasses);
        }

        OnPuzzleReset?.Invoke();

        if (showDebugInfo)
            Debug.Log("Puzzle 2 reset complete!");
    }

    // REMOVED: UpdatePuzzle2UI() method that was causing double UI updates

    // Public getters for UI/debugging
    public int LandmassesVisited => visitedLandmasses.Count;
    public int RequiredLandmasses => requiredLandmasses;
    public bool IsPuzzleComplete => puzzleComplete;
    public List<LandmassType> VisitOrder => new List<LandmassType>(visitOrder); // Return copy
    public bool IsResetInProgress => puzzleResetInProgress;
    public bool HasDetectedStartingPosition => hasDetectedStartingPosition;
    public LandmassType StartingLandmass => startingLandmass;

    // Debug method to print current status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintPuzzle2Status()
    {
        Debug.Log($"=== PUZZLE 2 STATUS ===");
        Debug.Log($"Landmasses: {visitedLandmasses.Count}/{requiredLandmasses}");
        Debug.Log($"Starting landmass: {startingLandmass}");
        Debug.Log($"Starting position detected: {hasDetectedStartingPosition}");
        Debug.Log($"Visited landmasses: {string.Join(", ", visitedLandmasses)}");
        Debug.Log($"Visit order: {string.Join(" → ", visitOrder)}");
        Debug.Log($"Complete: {IsPuzzleComplete}");
        Debug.Log($"Reset in progress: {IsResetInProgress}");
    }

    // Manual trigger for testing - UPDATED
    [ContextMenu("Force Complete Puzzle 2")]
    public void DebugForcePuzzle2Complete()
    {
        // If starting position detected, don't re-add it
        if (!visitedLandmasses.Contains(LandmassType.LandmassA))
            OnLandmassVisited(LandmassType.LandmassA);
        if (!visitedLandmasses.Contains(LandmassType.LandmassB))
            OnLandmassVisited(LandmassType.LandmassB);
        if (!visitedLandmasses.Contains(LandmassType.LandmassC))
            OnLandmassVisited(LandmassType.LandmassC);
        if (!visitedLandmasses.Contains(LandmassType.LandmassD))
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

    // Debug method to test starting position detection
    [ContextMenu("Test Starting Position Detection")]
    public void DebugTestStartingPosition()
    {
        hasDetectedStartingPosition = false;
        visitedLandmasses.Clear();
        visitOrder.Clear();

        if (autoDetectStartingPosition)
        {
            StartCoroutine(DetectStartingLandmass());
        }
        else
        {
            Debug.Log("Auto-detection is disabled. Enable it in inspector to test.");
        }
    }

    // Debug method to disable auto-detection (for testing old behavior)
    [ContextMenu("Disable Auto-Detection")]
    public void DebugDisableAutoDetection()
    {
        autoDetectStartingPosition = false;
        Debug.Log("Auto-detection disabled. Puzzle 2 will work like before.");
    }

    // Debug method to enable auto-detection
    [ContextMenu("Enable Auto-Detection")]
    public void DebugEnableAutoDetection()
    {
        autoDetectStartingPosition = true;
        Debug.Log("Auto-detection enabled. Puzzle 2 will detect starting position.");
    }
}