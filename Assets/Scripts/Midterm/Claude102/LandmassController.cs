using UnityEngine;

public class LandmassController : MonoBehaviour
{
    [Header("Landmass Configuration")]
    [SerializeField] private LandmassType landmassType;
    [SerializeField] private string landmassName; // Optional friendly name

    [Header("Visual Feedback")]
    [SerializeField] private GameObject visitedIndicator; // Optional visual indicator when visited
    [SerializeField] private Material visitedMaterial; // Optional material change when visited
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

    // Track which puzzle system is active
    private bool isUsingPuzzle2 = false;

    private void Start()
    {
        // Get references to both puzzle systems
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();

        if (puzzleTracker == null)
        {
            Debug.LogError($"PuzzleTracker not found! Landmass {landmassType} cannot function properly.");
        }

        // Store original material
        if (meshRenderer != null)
            originalMaterial = meshRenderer.material;

        // Hide visited indicator initially
        if (visitedIndicator != null)
            visitedIndicator.SetActive(false);

        // Auto-generate name if not set
        if (string.IsNullOrEmpty(landmassName))
            landmassName = landmassType.ToString();

        // Check which puzzle system should be active
        CheckActivePuzzleSystem();
    }

    private void Update()
    {
        // Continuously check which puzzle system is active
        CheckActivePuzzleSystem();
    }

    // Determine which puzzle system is currently active
    private void CheckActivePuzzleSystem()
    {
        bool shouldUsePuzzle2 = false;

        // Check if Puzzle1 is disabled and Puzzle2 is enabled
        if (puzzleTracker != null && puzzle2Tracker != null)
        {
            shouldUsePuzzle2 = !puzzleTracker.enabled && puzzle2Tracker.enabled;
        }
        else if (puzzleTracker == null && puzzle2Tracker != null)
        {
            // If only Puzzle2 exists
            shouldUsePuzzle2 = true;
        }
        else if (puzzle2Tracker != null && puzzle2Tracker.enabled)
        {
            // If Puzzle2 is explicitly enabled
            shouldUsePuzzle2 = true;
        }

        // Also check puzzle mode setting
        if (puzzleMode == 2)
        {
            shouldUsePuzzle2 = true;
        }

        // Switch puzzle systems if needed
        if (shouldUsePuzzle2 != isUsingPuzzle2)
        {
            isUsingPuzzle2 = shouldUsePuzzle2;
            if (showDebugInfo)
            {
                Debug.Log($"🔄 {landmassName}: Switched to {(isUsingPuzzle2 ? "Puzzle 2" : "Puzzle 1")} system");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isUsingPuzzle2)
            {
                // Puzzle 2 logic - check for revisit
                HandlePuzzle2Visit();
            }
            else
            {
                // Original Puzzle 1 logic
                HandlePuzzle1Visit();
            }
        }
    }

    // Handle visit for Puzzle 1 (original logic)
    private void HandlePuzzle1Visit()
    {
        if (!hasBeenVisited)
        {
            VisitLandmass();

            // Notify Puzzle 1 tracker
            if (puzzleTracker != null)
            {
                puzzleTracker.OnLandmassVisited(landmassType);
            }
        }
    }

    // Handle visit for Puzzle 2 (no revisit allowed)
    private void HandlePuzzle2Visit()
    {
        if (puzzle2Tracker != null)
        {
            // Always notify Puzzle2Tracker - it will handle revisit logic
            puzzle2Tracker.OnLandmassVisited(landmassType);
        }

        // Update visual state for Puzzle 2
        if (!hasBeenVisited)
        {
            VisitLandmass();
        }
    }

    private void VisitLandmass()
    {
        hasBeenVisited = true;

        if (showDebugInfo)
        {
            string puzzleSystem = isUsingPuzzle2 ? "Puzzle 2" : "Puzzle 1";
            Debug.Log($"Player visited {landmassName} ({landmassType}) - {puzzleSystem}");
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

    public void ResetLandmass()
    {
        hasBeenVisited = false;

        if (visitedIndicator != null)
            visitedIndicator.SetActive(false);

        if (meshRenderer != null && originalMaterial != null)
            meshRenderer.material = originalMaterial;

        if (showDebugInfo)
        {
            string puzzleSystem = isUsingPuzzle2 ? "Puzzle 2" : "Puzzle 1";
            Debug.Log($"Landmass {landmassName} has been reset - {puzzleSystem}");
        }
    }

    // Force switch to specific puzzle system (for debugging)
    public void ForceSwitchToPuzzle2()
    {
        isUsingPuzzle2 = true;
        puzzleMode = 2;
        ResetLandmass(); // Reset state when switching

        if (showDebugInfo)
            Debug.Log($"🔄 {landmassName}: Forced switch to Puzzle 2");
    }

    public void ForceSwitchToPuzzle1()
    {
        isUsingPuzzle2 = false;
        puzzleMode = 1;
        ResetLandmass(); // Reset state when switching

        if (showDebugInfo)
            Debug.Log($"🔄 {landmassName}: Forced switch to Puzzle 1");
    }

    public void SetPuzzleMode(int puzzleNumber)
    {
        puzzleMode = puzzleNumber;

        if (showDebugInfo)
            Debug.Log($"🔄 {landmassName}: Set to Puzzle {puzzleNumber} mode");

        // Force check which system to use
        CheckActivePuzzleSystem();
    }

    // Public getters
    public bool HasBeenVisited => hasBeenVisited;
    public LandmassType LandmassType => landmassType;
    public string LandmassName => landmassName;
    public bool IsUsingPuzzle2 => isUsingPuzzle2;
    public int PuzzleMode => puzzleMode;

    // Context menu for debugging
    [ContextMenu("Switch to Puzzle 2")]
    private void DebugSwitchToPuzzle2()
    {
        ForceSwitchToPuzzle2();
    }

    [ContextMenu("Switch to Puzzle 1")]
    private void DebugSwitchToPuzzle1()
    {
        ForceSwitchToPuzzle1();
    }

    [ContextMenu("Debug Current State")]
    private void DebugCurrentState()
    {
        Debug.Log($"=== {landmassName} Debug State ===");
        Debug.Log($"Landmass Type: {landmassType}");
        Debug.Log($"Has Been Visited: {hasBeenVisited}");
        Debug.Log($"Puzzle Mode: {puzzleMode}");
        Debug.Log($"Using Puzzle 2: {isUsingPuzzle2}");
        Debug.Log($"PuzzleTracker Found: {puzzleTracker != null}");
        Debug.Log($"Puzzle2Tracker Found: {puzzle2Tracker != null}");

        if (puzzleTracker != null)
            Debug.Log($"PuzzleTracker Enabled: {puzzleTracker.enabled}");
        if (puzzle2Tracker != null)
            Debug.Log($"Puzzle2Tracker Enabled: {puzzle2Tracker.enabled}");

        Debug.Log("================================");
    }
}