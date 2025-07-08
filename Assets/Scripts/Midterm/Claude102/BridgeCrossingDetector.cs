using UnityEngine;
using System.Collections;

/// <summary>
/// FIXED: Bridge Crossing Detector with proper Observer pattern state management
/// Only notifies ENABLED puzzle trackers to prevent cross-puzzle interference
/// </summary>
public class BridgeCrossingDetector : MonoBehaviour
{
    [Header("Bridge Configuration")]
    [SerializeField] private GameObject bridgeCube;
    [SerializeField] private float blockDelay = 2f;

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
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();

        if (meshRenderer == null)
        {
            Debug.LogError($"MeshRenderer not found on {name}! Add a MeshRenderer component.");
        }

        if (boxCollider == null)
        {
            Debug.LogError($"BoxCollider not found on {name}! Add a BoxCollider component.");
        }

        bridgeID = gameObject.name;

        if (bridgeCube == null)
        {
            bridgeCube = gameObject;
            if (showDebugInfo)
                Debug.Log($"Auto-assigned bridge cube for {bridgeID}");
        }

        SetupInitialState();

        if (showDebugInfo)
            Debug.Log($"Bridge crossing detector initialized: {bridgeID}");
    }

    private void SetupInitialState()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        if (showDebugInfo)
            Debug.Log($"{bridgeID}: Set to invisible trigger state");
    }

    private void OnTriggerEnter(Collider other)
    {
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

        // FIXED: Only notify ENABLED puzzle trackers (Observer Pattern with State Management)
        NotifyActivePuzzleTrackers();

        StartCoroutine(DelayedBlockReturnPath());
    }

    /// <summary>
    /// FIXED: Notify only the currently active puzzle tracker
    /// This prevents disabled trackers from receiving events (proper Observer pattern)
    /// </summary>
    private void NotifyActivePuzzleTrackers()
    {
        // Check Puzzle 1 tracker (original)
        if (puzzleTracker != null && puzzleTracker.enabled)
        {
            puzzleTracker.OnBridgeCrossed(bridgeID, LandmassType.LandmassA, LandmassType.LandmassB);
            if (showDebugInfo)
                Debug.Log($"✅ Notified Puzzle 1 tracker: {bridgeID}");
        }

        // Check Puzzle 2 tracker (if it needs bridge notifications)
        // Note: Puzzle 2 might not need bridge tracking, but bridges still block paths
        Puzzle2Tracker puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();
        if (puzzle2Tracker != null && puzzle2Tracker.enabled)
        {
            // Puzzle 2 doesn't track bridges for completion, but bridges still block
            if (showDebugInfo)
                Debug.Log($"✅ Puzzle 2 active - bridge {bridgeID} will still block return path");
        }

        // If no active trackers found
        if ((puzzleTracker == null || !puzzleTracker.enabled) &&
            (puzzle2Tracker == null || !puzzle2Tracker.enabled))
        {
            if (showDebugInfo)
                Debug.LogWarning($"⚠️ No active puzzle trackers found for bridge {bridgeID}");
        }
    }

    private System.Collections.IEnumerator DelayedBlockReturnPath()
    {
        yield return new WaitForSeconds(blockDelay);
        BlockReturnPath();
    }

    private void BlockReturnPath()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            if (showDebugInfo)
                Debug.Log($"🚫 {bridgeID}: Cube now visible after delay");
        }

        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
            if (showDebugInfo)
                Debug.Log($"🚫 {bridgeID}: Cube now solid - return path blocked!");
        }
    }

    /// <summary>
    /// FIXED: Reset bridge to initial state
    /// Used when transitioning between puzzles
    /// </summary>
    public void ResetBridge()
    {
        hasBeenCrossed = false;

        StopAllCoroutines();

        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (boxCollider != null)
            boxCollider.isTrigger = true;

        if (showDebugInfo)
            Debug.Log($"{bridgeID}: Bridge reset to invisible trigger state");
    }

    // Public properties
    public bool HasBeenCrossed => hasBeenCrossed;
    public string BridgeID => bridgeID;

    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log($"=== BRIDGE SETUP VALIDATION: {name} ===");

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

        GameObject player = GameObject.FindWithTag("Player");
        Debug.Log($"Player found in scene: {player != null}");

        // Check for active puzzle trackers
        PuzzleTracker tracker1 = FindFirstObjectByType<PuzzleTracker>();
        Puzzle2Tracker tracker2 = FindFirstObjectByType<Puzzle2Tracker>();
        Debug.Log($"Puzzle 1 Tracker found: {tracker1 != null}, Enabled: {(tracker1 != null ? tracker1.enabled : false)}");
        Debug.Log($"Puzzle 2 Tracker found: {tracker2 != null}, Enabled: {(tracker2 != null ? tracker2.enabled : false)}");

        Debug.Log("===============================");
    }
}