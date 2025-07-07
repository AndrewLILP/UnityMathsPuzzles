using UnityEngine;
using System.Collections;

/// <summary>
/// Bridge Crossing Detector - Place on invisible cubes in the middle of bridges
/// 
/// SETUP INSTRUCTIONS:
/// 1. Create a Cube GameObject positioned in the middle of your bridge
/// 2. Set Box Collider "Is Trigger" = TRUE
/// 3. DISABLE the MeshRenderer component (makes cube invisible initially)
/// 4. Add this BridgeCrossingDetector script to the cube
/// 5. Assign the cube GameObject itself to the "Bridge Cube" field in inspector
/// 6. Set "Block Delay" time (2f recommended - gives player time to pass through)
/// 7. Ensure your player GameObject has the "Player" tag
/// 
/// BEHAVIOR:
/// - Invisible cube detects when player crosses bridge
/// - After delay: cube becomes visible and solid (blocks return path)
/// - Notifies PuzzleTracker of bridge crossing immediately
/// - Uses the GameObject name as the bridge ID
/// </summary>

public class BridgeCrossingDetector : MonoBehaviour
{
    [Header("Bridge Configuration")]
    [SerializeField] private GameObject bridgeCube; // Reference to this cube GameObject
    [SerializeField] private float blockDelay = 2f; // Time to wait before blocking (allows player to pass through)

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private bool hasBeenCrossed = false;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;
    private PuzzleTracker puzzleTracker;
    private string bridgeID;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Get required components
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();

        // Validation
        if (puzzleTracker == null)
        {
            Debug.LogError($"PuzzleTracker not found! Bridge {name} cannot function properly.");
        }

        if (meshRenderer == null)
        {
            Debug.LogError($"MeshRenderer not found on {name}! Add a MeshRenderer component.");
        }

        if (boxCollider == null)
        {
            Debug.LogError($"BoxCollider not found on {name}! Add a BoxCollider component.");
        }

        // Set bridge ID from GameObject name
        bridgeID = gameObject.name;

        // Auto-assign bridge cube if not set
        if (bridgeCube == null)
        {
            bridgeCube = gameObject;
            if (showDebugInfo)
                Debug.Log($"Auto-assigned bridge cube for {bridgeID}");
        }

        // Ensure proper initial setup
        SetupInitialState();

        if (showDebugInfo)
            Debug.Log($"Bridge crossing detector initialized: {bridgeID}");
    }

    private void SetupInitialState()
    {
        // Make sure we start invisible and as trigger
        if (meshRenderer != null)
            meshRenderer.enabled = false; // Invisible initially

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true; // Trigger for detection
        }

        if (showDebugInfo)
            Debug.Log($"{bridgeID}: Set to invisible trigger state");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to player and only if not already crossed
        if (other.CompareTag("Player") && !hasBeenCrossed)
        {
            CrossBridge();
        }
        else if (other.CompareTag("Player") && hasBeenCrossed)
        {
            if (showDebugInfo)
                Debug.Log($"Player attempted to cross {bridgeID} again - blocked by visible cube!");
        }
    }

    private void CrossBridge()
    {
        hasBeenCrossed = true;

        if (showDebugInfo)
            Debug.Log($"🌉 Player crossed bridge: {bridgeID} - blocking in {blockDelay} seconds");

        // Notify puzzle tracker immediately
        if (puzzleTracker != null)
        {
            puzzleTracker.OnBridgeCrossed(bridgeID, LandmassType.LandmassA, LandmassType.LandmassB);
        }

        // Start delayed blocking coroutine
        StartCoroutine(DelayedBlockReturnPath());
    }

    private System.Collections.IEnumerator DelayedBlockReturnPath()
    {
        // Wait for specified delay to allow player to pass through
        yield return new WaitForSeconds(blockDelay);

        // Now block the return path
        BlockReturnPath();
    }

    private void BlockReturnPath()
    {
        // Enable visual (make cube visible)
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            if (showDebugInfo)
                Debug.Log($"🚫 {bridgeID}: Cube now visible after delay");
        }

        // Make collider solid (blocks movement)
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
            if (showDebugInfo)
                Debug.Log($"🚫 {bridgeID}: Cube now solid - return path blocked!");
        }
    }

    public void ResetBridge()
    {
        hasBeenCrossed = false;

        // Stop any running delayed blocking coroutines
        StopAllCoroutines();

        // Return to invisible trigger state
        if (meshRenderer != null)
            meshRenderer.enabled = false; // Make invisible

        if (boxCollider != null)
            boxCollider.isTrigger = true; // Make it a trigger again

        if (showDebugInfo)
            Debug.Log($"{bridgeID}: Bridge reset to invisible trigger state");
    }

    // Public properties for external access
    public bool HasBeenCrossed => hasBeenCrossed;
    public string BridgeID => bridgeID;

    // Validation method for setup checking
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log($"=== BRIDGE SETUP VALIDATION: {name} ===");

        // Check components
        MeshRenderer mr = GetComponent<MeshRenderer>();
        BoxCollider bc = GetComponent<BoxCollider>();

        Debug.Log($"MeshRenderer found: {mr != null}");
        Debug.Log($"BoxCollider found: {bc != null}");

        if (bc != null)
            Debug.Log($"Is Trigger: {bc.isTrigger}");

        if (mr != null)
            Debug.Log($"Mesh Renderer enabled: {mr.enabled}");

        Debug.Log($"Bridge Cube assigned: {bridgeCube != null}");
        Debug.Log($"Bridge ID: {bridgeID}");
        Debug.Log($"Block Delay: {blockDelay} seconds");

        // Check for player in scene
        GameObject player = GameObject.FindWithTag("Player");
        Debug.Log($"Player found in scene: {player != null}");

        // Check for PuzzleTracker
        PuzzleTracker tracker = FindFirstObjectByType<PuzzleTracker>();
        Debug.Log($"PuzzleTracker found: {tracker != null}");

        Debug.Log("===============================");
    }
}