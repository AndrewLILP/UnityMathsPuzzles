using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// FIXED: Puzzle 2 Tracker with proper starting position detection and state management
/// Ensures completely fresh start when transitioning from Puzzle 1
/// </summary>
public class Puzzle2Tracker : MonoBehaviour
{
    [Header("Puzzle 2 Configuration")]
    [SerializeField] private int requiredLandmasses = 4;
    // Note: Bridges can be reused in Puzzle 2 (they still block return paths)

    [Header("Starting Position Detection")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private bool autoDetectStartingPosition = true;

    [Header("Reset Settings")]
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private string resetMessage = "You returned to a previously visited area. Puzzle reset!";

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showProgressUpdates = true;

    // Tracking collections
    private HashSet<LandmassType> visitedLandmasses = new HashSet<LandmassType>();
    private List<LandmassType> visitOrder = new List<LandmassType>();
    private List<LandmassController> allLandmasses = new List<LandmassController>();

    // State tracking
    private bool puzzleComplete = false;
    private bool puzzleResetInProgress = false;
    private bool hasDetectedStartingPosition = false;
    private LandmassType startingLandmass;

    // FIXED: Add initialization state tracking
    private bool isInitialized = false;

    // Events for integration with LevelManager/UI
    public System.Action OnPuzzleCompleted;
    public System.Action OnPuzzleFailed;
    public System.Action OnPuzzleReset;
    public System.Action<int, int> OnLandmassProgressChanged;
    public System.Action<string> OnResetMessageTriggered;

    private void Start()
    {
        // FIXED: Only initialize if this tracker should be enabled at start
        // By default, Puzzle2Tracker should be DISABLED and only enabled during transition
        if (ShouldBeEnabledAtStart())
        {
            InitializePuzzle2();
        }
        else
        {
            // Disable this tracker - it will be enabled during Puzzle 1 → 2 transition
            this.enabled = false;
            if (showDebugInfo)
                Debug.Log("🔄 Puzzle2Tracker: Disabled at start (will be enabled during transition from Puzzle 1)");
        }
    }

    /// <summary>
    /// FIXED: Enhanced OnEnable with proper state management
    /// Ensures fresh start when transitioning from Puzzle 1
    /// </summary>
    private void OnEnable()
    {
        if (showDebugInfo)
            Debug.Log("🔄 Puzzle2Tracker enabled - starting fresh initialization");

        // FIXED: Complete fresh start when enabled (especially during transitions)
        StartFreshPuzzle2();
    }

    /// <summary>
    /// FIXED: Handle being disabled - stop all coroutines and clear state
    /// </summary>
    private void OnDisable()
    {
        if (showDebugInfo)
            Debug.Log("🔄 Puzzle2Tracker disabled - stopping all processes");

        // Stop any running coroutines
        StopAllCoroutines();

        // Mark as not initialized to prevent issues
        isInitialized = false;
    }

    /// <summary>
    /// FIXED: Determine if Puzzle2Tracker should be enabled at startup
    /// Default behavior: Puzzle2Tracker starts DISABLED and gets enabled during transition
    /// </summary>
    private bool ShouldBeEnabledAtStart()
    {
        // Check if PuzzleTracker (Puzzle 1) exists and is enabled
        PuzzleTracker puzzle1Tracker = FindFirstObjectByType<PuzzleTracker>();

        if (puzzle1Tracker != null && puzzle1Tracker.enabled)
        {
            // Puzzle 1 is active, so Puzzle 2 should be disabled
            if (showDebugInfo)
                Debug.Log("🔄 Puzzle2Tracker: Puzzle 1 is active, staying disabled");
            return false;
        }

        // If no Puzzle 1 tracker or it's disabled, then Puzzle 2 can be active
        if (showDebugInfo)
            Debug.Log("🔄 Puzzle2Tracker: No active Puzzle 1 found, enabling Puzzle 2");
        return true;
    }

    /// <summary>
    /// FIXED: Ensure completely fresh start for Puzzle 2
    /// Called when transitioning from Puzzle 1 or starting new game
    /// </summary>
    private void StartFreshPuzzle2()
    {
        // Clear all state
        visitedLandmasses.Clear();
        visitOrder.Clear();
        puzzleComplete = false;
        puzzleResetInProgress = false;
        hasDetectedStartingPosition = false;
        startingLandmass = LandmassType.LandmassA; // Default
        isInitialized = false;

        if (showDebugInfo)
            Debug.Log("✅ Puzzle2Tracker: Cleared all state for fresh start");

        // Initialize puzzle
        InitializePuzzle2();

        // Start starting position detection if auto-detection is enabled
        if (autoDetectStartingPosition)
        {
            StartCoroutine(DelayedStartingPositionDetection());
        }
        else
        {
            isInitialized = true;
            UpdatePuzzle2UIInitial();
        }
    }

    /// <summary>
    /// FIXED: Add small delay before starting position detection
    /// Ensures all systems are properly set up after transition
    /// </summary>
    private IEnumerator DelayedStartingPositionDetection()
    {
        // Wait a moment for everything to settle after transition
        yield return new WaitForSeconds(0.5f);

        // FIXED: Double-check that we're still enabled and should be running
        if (!enabled || !gameObject.activeInHierarchy)
        {
            if (showDebugInfo)
                Debug.Log("🔄 Puzzle2Tracker: Delayed detection cancelled - tracker was disabled");
            yield break;
        }

        if (showDebugInfo)
            Debug.Log("🎯 Starting position detection beginning...");

        StartCoroutine(DetectStartingLandmass());
    }

    private void InitializePuzzle2()
    {
        allLandmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None).ToList();

        if (showDebugInfo)
        {
            Debug.Log($"Puzzle 2 initialized: {allLandmasses.Count} landmasses found");
            Debug.Log($"Required: Visit all {requiredLandmasses} landmasses exactly once");
            Debug.Log($"Starting position detection: {(autoDetectStartingPosition ? "ENABLED" : "DISABLED")}");
        }

        if (allLandmasses.Count < requiredLandmasses)
        {
            Debug.LogWarning($"Not enough landmasses in scene! Found {allLandmasses.Count}, need {requiredLandmasses}");
        }

        isInitialized = true;
    }

    private void UpdatePuzzle2UIInitial()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.SetCurrentPuzzleNumber(2);
            uiManager.SetSubtitleText("Puzzle 2: Path");

            if (showDebugInfo)
                Debug.Log("Puzzle 2 UI initialized to Puzzle 2");
        }
    }

    /// <summary>
    /// FIXED: Enhanced starting position detection with better error handling
    /// </summary>
    private IEnumerator DetectStartingLandmass()
    {
        yield return null; // Wait a frame

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // FIXED: Better error handling - try alternative methods to find player
            Debug.LogWarning("⚠️ Player tag not found, trying alternative methods...");

            // Try finding player by name
            player = GameObject.Find("Player");
            if (player == null)
            {
                // Try finding any object with PlayerMovementBehaviour
                PlayerMovementBehaviour playerMovement = FindFirstObjectByType<PlayerMovementBehaviour>();
                if (playerMovement != null)
                {
                    player = playerMovement.gameObject;
                    Debug.Log($"✅ Found player via PlayerMovementBehaviour: {player.name}");
                }
            }
            else
            {
                Debug.Log($"✅ Found player by name: {player.name}");
            }
        }

        if (player == null)
        {
            Debug.LogError("❌ Player not found with any method! Cannot detect starting landmass.");
            Debug.LogError("💡 Make sure your player GameObject has the 'Player' tag or is named 'Player'");
            UpdateUIWithoutStartingPosition();
            yield break;
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"✅ Player found: {player.name} at position {player.transform.position}");
        }

        // Find closest landmass
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
            // FIXED: Mark starting landmass as visited
            startingLandmass = closestLandmass.LandmassType;
            visitedLandmasses.Add(startingLandmass);
            visitOrder.Add(startingLandmass);
            hasDetectedStartingPosition = true;

            if (showDebugInfo)
                Debug.Log($"🎯 Puzzle 2 starting position detected: {startingLandmass} (distance: {closestDistance:F1}m)");

            // Update progress (1/4 landmasses visited to start)
            OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);

            UpdateUIWithStartingPosition();
        }
        else
        {
            string warningMsg = closestLandmass != null
                ? $"Closest landmass ({closestLandmass.LandmassType}) is {closestDistance:F1}m away (max: {detectionRange}m)"
                : "No landmasses found in scene";

            Debug.LogWarning($"⚠️ Could not detect starting landmass: {warningMsg}");
            UpdateUIWithoutStartingPosition();
        }
    }

    private void UpdateUIWithStartingPosition()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            int remaining = requiredLandmasses - visitedLandmasses.Count;

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

    /// <summary>
    /// FIXED: Enhanced landmass visit handling with proper state validation
    /// </summary>
    public void OnLandmassVisited(LandmassType landmassType)
    {
        // FIXED: Early exit if not properly initialized or in reset
        if (!enabled || !isInitialized || puzzleComplete || puzzleResetInProgress)
        {
            if (showDebugInfo)
                Debug.Log($"⏸️ Puzzle2Tracker: Ignoring {landmassType} visit (enabled={enabled}, initialized={isInitialized}, complete={puzzleComplete}, resetting={puzzleResetInProgress})");
            return;
        }

        // FIXED: Special handling for starting position detection
        if (!hasDetectedStartingPosition)
        {
            if (showDebugInfo)
                Debug.Log($"🎯 Puzzle 2: First landmass visit detected during play: {landmassType}");

            // This is the starting position
            startingLandmass = landmassType;
            visitedLandmasses.Add(landmassType);
            visitOrder.Add(landmassType);
            hasDetectedStartingPosition = true;

            OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);
            UpdateUIWithStartingPosition();
            return;
        }

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

        OnLandmassProgressChanged?.Invoke(visitedLandmasses.Count, requiredLandmasses);
        UpdateProgressUI();
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

        OnResetMessageTriggered?.Invoke(message);
        OnPuzzleFailed?.Invoke();

        StartCoroutine(DelayedReset());
    }

    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetPuzzle2();
    }

    /// <summary>
    /// FIXED: Enhanced reset with complete state clearing
    /// </summary>
    public void ResetPuzzle2()
    {
        if (showDebugInfo)
            Debug.Log("Resetting Puzzle 2...");

        // Clear tracking state
        visitedLandmasses.Clear();
        visitOrder.Clear();
        puzzleComplete = false;
        puzzleResetInProgress = false;
        hasDetectedStartingPosition = false;
        startingLandmass = LandmassType.LandmassA;

        // Reset all landmasses
        foreach (var landmass in allLandmasses)
        {
            landmass.ResetLandmass();
        }

        // Re-detect starting position if auto-detection is enabled
        if (autoDetectStartingPosition)
        {
            StartCoroutine(DelayedStartingPositionDetection());
        }
        else
        {
            OnLandmassProgressChanged?.Invoke(0, requiredLandmasses);
        }

        OnPuzzleReset?.Invoke();

        if (showDebugInfo)
            Debug.Log("Puzzle 2 reset complete!");
    }

    // Public getters for UI/debugging
    public int LandmassesVisited => visitedLandmasses.Count;
    public int RequiredLandmasses => requiredLandmasses;
    public bool IsPuzzleComplete => puzzleComplete;
    public List<LandmassType> VisitOrder => new List<LandmassType>(visitOrder);
    public bool IsResetInProgress => puzzleResetInProgress;
    public bool HasDetectedStartingPosition => hasDetectedStartingPosition;
    public LandmassType StartingLandmass => startingLandmass;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintPuzzle2Status()
    {
        Debug.Log($"=== PUZZLE 2 STATUS ===");
        Debug.Log($"Enabled: {enabled}");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Landmasses: {visitedLandmasses.Count}/{requiredLandmasses}");
        Debug.Log($"Starting landmass: {startingLandmass}");
        Debug.Log($"Starting position detected: {hasDetectedStartingPosition}");
        Debug.Log($"Visited landmasses: {string.Join(", ", visitedLandmasses)}");
        Debug.Log($"Visit order: {string.Join(" → ", visitOrder)}");
        Debug.Log($"Complete: {IsPuzzleComplete}");
        Debug.Log($"Reset in progress: {IsResetInProgress}");
    }

    // Debug methods
    [ContextMenu("Force Complete Puzzle 2")]
    public void DebugForcePuzzle2Complete()
    {
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

    [ContextMenu("Test Starting Position Detection")]
    public void DebugTestStartingPosition()
    {
        StartFreshPuzzle2();
    }
}