using UnityEngine;

/// <summary>
/// SOLUTION: Puzzle System Manager
/// Ensures only one puzzle system is active at a time
/// Prevents the startup conflict between PuzzleTracker and Puzzle2Tracker
/// </summary>
public class PuzzleSystemManager : MonoBehaviour
{
    [Header("Puzzle System Configuration")]
    [SerializeField] private PuzzleSystemType startingPuzzle = PuzzleSystemType.Puzzle1;
    [SerializeField] private bool autoManagePuzzleSystems = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private PuzzleTracker puzzleTracker;
    private Puzzle2Tracker puzzle2Tracker;

    public enum PuzzleSystemType
    {
        Puzzle1,
        Puzzle2
    }

    private void Awake()
    {
        if (autoManagePuzzleSystems)
        {
            InitializePuzzleSystems();
        }
    }

    private void InitializePuzzleSystems()
    {
        // Find puzzle trackers
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();

        if (showDebugInfo)
        {
            Debug.Log($"🎮 PuzzleSystemManager: Found PuzzleTracker={puzzleTracker != null}, Puzzle2Tracker={puzzle2Tracker != null}");
        }

        // Set initial state based on configuration
        switch (startingPuzzle)
        {
            case PuzzleSystemType.Puzzle1:
                EnablePuzzle1();
                break;
            case PuzzleSystemType.Puzzle2:
                EnablePuzzle2();
                break;
        }
    }

    /// <summary>
    /// Enable Puzzle 1 and disable Puzzle 2
    /// </summary>
    public void EnablePuzzle1()
    {
        if (puzzleTracker != null)
        {
            puzzleTracker.enabled = true;
            if (showDebugInfo)
                Debug.Log("✅ PuzzleSystemManager: Enabled Puzzle 1 (Königsberg)");
        }

        if (puzzle2Tracker != null)
        {
            puzzle2Tracker.enabled = false;
            if (showDebugInfo)
                Debug.Log("🔄 PuzzleSystemManager: Disabled Puzzle 2 (Path)");
        }
    }

    /// <summary>
    /// Enable Puzzle 2 and disable Puzzle 1
    /// </summary>
    public void EnablePuzzle2()
    {
        if (puzzleTracker != null)
        {
            puzzleTracker.enabled = false;
            if (showDebugInfo)
                Debug.Log("🔄 PuzzleSystemManager: Disabled Puzzle 1 (Königsberg)");
        }

        if (puzzle2Tracker != null)
        {
            puzzle2Tracker.enabled = true;
            if (showDebugInfo)
                Debug.Log("✅ PuzzleSystemManager: Enabled Puzzle 2 (Path)");
        }
    }

    /// <summary>
    /// Get currently active puzzle system
    /// </summary>
    public PuzzleSystemType GetActivePuzzleSystem()
    {
        if (puzzleTracker != null && puzzleTracker.enabled)
            return PuzzleSystemType.Puzzle1;

        if (puzzle2Tracker != null && puzzle2Tracker.enabled)
            return PuzzleSystemType.Puzzle2;

        // Default to Puzzle1 if unclear
        return PuzzleSystemType.Puzzle1;
    }

    /// <summary>
    /// Check if a specific puzzle system is active
    /// </summary>
    public bool IsPuzzleSystemActive(PuzzleSystemType puzzleType)
    {
        return GetActivePuzzleSystem() == puzzleType;
    }

    /// <summary>
    /// Transition from Puzzle 1 to Puzzle 2
    /// This is called by PuzzleTracker during the impossible scenario
    /// </summary>
    public void TransitionToPuzzle2()
    {
        if (showDebugInfo)
            Debug.Log("🔄 PuzzleSystemManager: Handling transition from Puzzle 1 to Puzzle 2");

        EnablePuzzle2();
    }

    /// <summary>
    /// Force refresh - re-find puzzle trackers if needed
    /// </summary>
    public void RefreshPuzzleReferences()
    {
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();

        if (showDebugInfo)
        {
            Debug.Log($"🔄 PuzzleSystemManager: Refreshed references - PuzzleTracker={puzzleTracker != null}, Puzzle2Tracker={puzzle2Tracker != null}");
        }
    }

    // Context menu methods for debugging
    [ContextMenu("Enable Puzzle 1")]
    private void DebugEnablePuzzle1()
    {
        EnablePuzzle1();
    }

    [ContextMenu("Enable Puzzle 2")]
    private void DebugEnablePuzzle2()
    {
        EnablePuzzle2();
    }

    [ContextMenu("Show Current State")]
    private void DebugShowCurrentState()
    {
        Debug.Log($"=== PUZZLE SYSTEM STATE ===");
        Debug.Log($"Starting Puzzle: {startingPuzzle}");
        Debug.Log($"Auto Manage: {autoManagePuzzleSystems}");
        Debug.Log($"Active System: {GetActivePuzzleSystem()}");

        if (puzzleTracker != null)
            Debug.Log($"PuzzleTracker: Enabled={puzzleTracker.enabled}");
        else
            Debug.Log($"PuzzleTracker: NOT FOUND");

        if (puzzle2Tracker != null)
            Debug.Log($"Puzzle2Tracker: Enabled={puzzle2Tracker.enabled}");
        else
            Debug.Log($"Puzzle2Tracker: NOT FOUND");

        Debug.Log("===========================");
    }
}