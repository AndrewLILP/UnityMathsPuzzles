using UnityEngine;

public class BridgeController : MonoBehaviour
{
    [Header("Bridge Configuration")]
    [SerializeField] private string bridgeID; // Unique identifier for this bridge
    [SerializeField] private LandmassType fromLandmass; // Starting landmass
    [SerializeField] private LandmassType toLandmass; // Destination landmass
    [SerializeField] private Transform playerBlockSpawnPoint; // Where to spawn the blocking cube
    [SerializeField] private GameObject blockingCubePrefab; // Cube prefab to block return path

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private bool hasBeenCrossed = false;
    private GameObject spawnedBlockingCube;
    private PuzzleTracker puzzleTracker;

    private void Start()
    {
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        if (puzzleTracker == null)
        {
            Debug.LogError($"PuzzleTracker not found! Bridge {bridgeID} cannot function properly.");
        }

        // Auto-generate bridge ID if not set
        if (string.IsNullOrEmpty(bridgeID))
        {
            bridgeID = $"Bridge_{fromLandmass}_to_{toLandmass}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenCrossed)
        {
            CrossBridge(other.transform);
        }
        else if (other.CompareTag("Player") && hasBeenCrossed)
        {
            if (showDebugInfo)
                Debug.Log($"Player attempted to cross {bridgeID} again - blocked!");
        }
    }

    private void CrossBridge(Transform player)
    {
        hasBeenCrossed = true;

        if (showDebugInfo)
            Debug.Log($"Player crossed {bridgeID} from {fromLandmass} to {toLandmass}");

        // Notify the puzzle tracker
        puzzleTracker?.OnBridgeCrossed(bridgeID, fromLandmass, toLandmass);

        // Spawn blocking cube to prevent return
        SpawnBlockingCube(player);
    }

    private void SpawnBlockingCube(Transform player)
    {
        if (blockingCubePrefab != null && playerBlockSpawnPoint != null)
        {
            spawnedBlockingCube = Instantiate(blockingCubePrefab, playerBlockSpawnPoint.position, playerBlockSpawnPoint.rotation);

            if (showDebugInfo)
                Debug.Log($"Blocking cube spawned for {bridgeID}");
        }
        else
        {
            Debug.LogWarning($"Missing blocking cube prefab or spawn point for {bridgeID}");
        }
    }

    public void ResetBridge()
    {
        hasBeenCrossed = false;

        if (spawnedBlockingCube != null)
        {
            Destroy(spawnedBlockingCube);
            spawnedBlockingCube = null;
        }

        if (showDebugInfo)
            Debug.Log($"Bridge {bridgeID} has been reset");
    }

    public bool HasBeenCrossed => hasBeenCrossed;
    public string BridgeID => bridgeID;
    public LandmassType FromLandmass => fromLandmass;
    public LandmassType ToLandmass => toLandmass;
}