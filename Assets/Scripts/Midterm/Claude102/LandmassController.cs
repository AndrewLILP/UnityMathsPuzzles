using UnityEngine;

/// <summary>
/// FIXED: LandmassController with proper Strategy pattern state management
/// Only notifies the currently ACTIVE puzzle system to prevent cross-puzzle interference
/// </summary>
public class LandmassController : MonoBehaviour
{
    [Header("Landmass Configuration")]
    [SerializeField] private LandmassType landmassType;
    [SerializeField] private string landmassName;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visitedIndicator;
    [SerializeField] private Material visitedMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    [Header("Puzzle Mode")]
    [SerializeField] private int puzzleMode = 1; // 1 = Königsberg, 2 = Path

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private bool hasBeenVisited = false;
    private Material originalMaterial;

    // References to both puzzle systems
    private PuzzleTracker puzzleTracker;
    private Puzzle2Tracker puzzle2Tracker;

    // FIXED: Clear strategy - determine active puzzle at visit time, not continuously
    private bool isCurrentlyUsingPuzzle2 = false;

    private void Start()
    {
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();

        if (puzzleTracker == null)
        {
            Debug.LogError($"PuzzleTracker not found! Landmass {landmassType} cannot function properly.");
        }

        if (meshRenderer != null)
            originalMaterial = meshRenderer.material;

        if (visitedIndicator != null)
            visitedIndicator.SetActive(false);

        if (string.IsNullOrEmpty(landmassName))
            landmassName = landmassType.ToString();

        // Set initial active puzzle system
        DetermineActivePuzzleSystem();
    }

    /// <summary>
    /// FIXED: Determine which puzzle system is currently active
    /// Strategy Pattern - choose the right strategy based on current state
    /// </summary>
    private void DetermineActivePuzzleSystem()
    {
        bool shouldUsePuzzle2 = false;

        // Strategy Selection Logic
        if (puzzleTracker != null && puzzle2Tracker != null)
        {
            // If Puzzle 1 is disabled and Puzzle 2 is enabled
            shouldUsePuzzle2 = !puzzleTracker.enabled && puzzle2Tracker.enabled;
        }
        else if (puzzleTracker == null && puzzle2Tracker != null)
        {
            // If only Puzzle 2 exists
            shouldUsePuzzle2 = true;
        }
        else if (puzzle2Tracker != null && puzzle2Tracker.enabled)
        {
            // If Puzzle 2 is explicitly enabled
            shouldUsePuzzle2 = true;
        }

        // Also check puzzle mode setting
        if (puzzleMode == 2)
        {
            shouldUsePuzzle2 = true;
        }

        // Update strategy if changed
        if (shouldUsePuzzle2 != isCurrentlyUsingPuzzle2)
        {
            isCurrentlyUsingPuzzle2 = shouldUsePuzzle2;
            if (showDebugInfo)
            {
                Debug.Log($"🔄 {landmassName}: Switched to {(isCurrentlyUsingPuzzle2 ? "Puzzle 2" : "Puzzle 1")} system");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // FIXED: Determine active puzzle system at visit time (not continuously)
            DetermineActivePuzzleSystem();

            if (isCurrentlyUsingPuzzle2)
            {
                HandlePuzzle2Visit();
            }
            else
            {
                HandlePuzzle1Visit();
            }
        }
    }

    /// <summary>
    /// FIXED: Handle visit for Puzzle 1 - only notify if Puzzle 1 is enabled
    /// </summary>
    private void HandlePuzzle1Visit()
    {
        // Double-check that Puzzle 1 is actually enabled
        if (puzzleTracker == null || !puzzleTracker.enabled)
        {
            if (showDebugInfo)
                Debug.LogWarning($"⚠️ {landmassName}: Puzzle 1 visit attempted but PuzzleTracker is disabled!");
            return;
        }

        if (!hasBeenVisited)
        {
            VisitLandmass("Puzzle 1");

            // Notify Puzzle 1 tracker
            puzzleTracker.OnLandmassVisited(landmassType);
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"{landmassName}: Already visited in Puzzle 1");
        }
    }

    /// <summary>
    /// FIXED: Handle visit for Puzzle 2 - only notify if Puzzle 2 is enabled
    /// </summary>
    private void HandlePuzzle2Visit()
    {
        // Double-check that Puzzle 2 is actually enabled
        if (puzzle2Tracker == null || !puzzle2Tracker.enabled)
        {
            if (showDebugInfo)
                Debug.LogWarning($"⚠️ {landmassName}: Puzzle 2 visit attempted but Puzzle2Tracker is disabled!");
            return;
        }

        // ALWAYS notify Puzzle2Tracker - it will handle revisit logic internally
        puzzle2Tracker.OnLandmassVisited(landmassType);

        // Update visual state for Puzzle 2 (only if not already visited)
        if (!hasBeenVisited)
        {
            VisitLandmass("Puzzle 2");
        }
    }

    /// <summary>
    /// FIXED: Mark landmass as visited with proper puzzle context
    /// </summary>
    private void VisitLandmass(string puzzleContext)
    {
        hasBeenVisited = true;

        if (showDebugInfo)
        {
            Debug.Log($"Player visited {landmassName} ({landmassType}) - {puzzleContext}");
        }

        // Visual feedback
        ShowVisitedState();
    }

    private void ShowVisitedState()
    {
        if (visitedIndicator != null)
            visitedIndicator.SetActive(true);

        if (meshRenderer != null && visitedMaterial != null)
            meshRenderer.material = visitedMaterial;
    }

    /// <summary>
    /// FIXED: Reset landmass with proper puzzle context tracking
    /// </summary>
    public void ResetLandmass()
    {
        hasBeenVisited = false;

        if (visitedIndicator != null)
            visitedIndicator.SetActive(false);

        if (meshRenderer != null && originalMaterial != null)
            meshRenderer.material = originalMaterial;

        // Re-determine active puzzle system after reset
        DetermineActivePuzzleSystem();

        if (showDebugInfo)
        {
            string puzzleSystem = isCurrentlyUsingPuzzle2 ? "Puzzle 2" : "Puzzle 1";
            Debug.Log($"Landmass {landmassName} has been reset - {puzzleSystem}");
        }
    }

    /// <summary>
    /// FIXED: Force switch to specific puzzle system
    /// Used during puzzle transitions
    /// </summary>
    public void SetPuzzleMode(int puzzleNumber)
    {
        puzzleMode = puzzleNumber;

        if (showDebugInfo)
            Debug.Log($"🔄 {landmassName}: Set to Puzzle {puzzleNumber} mode");

        // Force re-determination of active system
        DetermineActivePuzzleSystem();
    }

    // Public getters
    public bool HasBeenVisited => hasBeenVisited;
    public LandmassType LandmassType => landmassType;
    public string LandmassName => landmassName;
    public bool IsUsingPuzzle2 => isCurrentlyUsingPuzzle2;
    public int PuzzleMode => puzzleMode;

    // Context menu for debugging
    [ContextMenu("Switch to Puzzle 2")]
    private void DebugSwitchToPuzzle2()
    {
        SetPuzzleMode(2);
        ResetLandmass();
    }

    [ContextMenu("Switch to Puzzle 1")]
    private void DebugSwitchToPuzzle1()
    {
        SetPuzzleMode(1);
        ResetLandmass();
    }

    [ContextMenu("Debug Current State")]
    private void DebugCurrentState()
    {
        Debug.Log($"=== {landmassName} Debug State ===");
        Debug.Log($"Landmass Type: {landmassType}");
        Debug.Log($"Has Been Visited: {hasBeenVisited}");
        Debug.Log($"Puzzle Mode: {puzzleMode}");
        Debug.Log($"Currently Using Puzzle 2: {isCurrentlyUsingPuzzle2}");
        Debug.Log($"PuzzleTracker Found: {puzzleTracker != null}");
        Debug.Log($"Puzzle2Tracker Found: {puzzle2Tracker != null}");

        if (puzzleTracker != null)
            Debug.Log($"PuzzleTracker Enabled: {puzzleTracker.enabled}");
        if (puzzle2Tracker != null)
            Debug.Log($"Puzzle2Tracker Enabled: {puzzle2Tracker.enabled}");

        Debug.Log("================================");
    }
}