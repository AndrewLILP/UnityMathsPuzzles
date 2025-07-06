using UnityEngine;

/// <summary>
/// Helper script for testing and debugging the puzzle system
/// Add this to a GameObject in your scene during development
/// </summary>
public class PuzzleTestHelper : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode statusKey = KeyCode.T;
    [SerializeField] private bool enableTestKeys = true;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoCreateBlockingCubes = true;
    [SerializeField] private GameObject defaultBlockingCubePrefab;

    private PuzzleTracker puzzleTracker;

    private void Start()
    {
        puzzleTracker = FindObjectOfType<PuzzleTracker>();

        if (autoCreateBlockingCubes)
        {
            SetupDefaultBlockingCubes();
        }

        Debug.Log("=== PUZZLE TEST HELPER ===");
        Debug.Log($"Press {resetKey} to reset puzzle");
        Debug.Log($"Press {statusKey} to print status");
        Debug.Log("==========================");
    }

    private void Update()
    {
        if (!enableTestKeys) return;

        if (Input.GetKeyDown(resetKey))
        {
            ResetPuzzle();
        }

        if (Input.GetKeyDown(statusKey))
        {
            PrintStatus();
        }
    }

    private void SetupDefaultBlockingCubes()
    {
        if (defaultBlockingCubePrefab == null)
        {
            // Create a simple cube prefab if none provided
            defaultBlockingCubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            defaultBlockingCubePrefab.name = "DefaultBlockingCube";
            defaultBlockingCubePrefab.SetActive(false);
        }

        // Find all bridges without blocking cube prefabs and assign the default
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int setupCount = 0;

        foreach (var bridge in bridges)
        {
            // This would need reflection or making the field public to work
            // For now, just log that we found bridges
            setupCount++;
        }

        Debug.Log($"Found {setupCount} bridges for blocking cube setup");
    }

    public void ResetPuzzle()
    {
        if (puzzleTracker != null)
        {
            puzzleTracker.ResetPuzzle();
            Debug.Log("🔄 Puzzle reset via test helper");
        }
        else
        {
            Debug.LogError("PuzzleTracker not found!");
        }
    }

    public void PrintStatus()
    {
        if (puzzleTracker != null)
        {
            puzzleTracker.PrintPuzzleStatus();
        }
        else
        {
            Debug.LogError("PuzzleTracker not found!");
        }
    }

    // Method to validate scene setup
    [ContextMenu("Validate Scene Setup")]
    public void ValidateSceneSetup()
    {
        Debug.Log("=== SCENE VALIDATION ===");

        // Check for required components
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        LandmassController[] landmasses = FindObjectsOfType<LandmassController>();
        PuzzleTracker tracker = FindObjectOfType<PuzzleTracker>();

        Debug.Log($"Bridges found: {bridges.Length}");
        Debug.Log($"Landmasses found: {landmasses.Length}");
        Debug.Log($"PuzzleTracker found: {tracker != null}");

        // Check bridge configuration
        foreach (var bridge in bridges)
        {
            if (string.IsNullOrEmpty(bridge.BridgeID))
                Debug.LogWarning($"Bridge {bridge.name} has no ID set");
        }

        // Check for player
        GameObject player = GameObject.FindWithTag("Player");
        Debug.Log($"Player found: {player != null}");

        Debug.Log("========================");
    }
}
