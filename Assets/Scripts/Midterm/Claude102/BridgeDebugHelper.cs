using UnityEngine;

/// <summary>
/// Debug helper to test and fix bridge detection issues
/// </summary>
public class BridgeDebugHelper : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode testKey = KeyCode.B;
    [SerializeField] private bool showTriggerGizmos = true;
    [SerializeField] private bool enlargeTriggerColliders = false;
    [SerializeField] private float enlargeMultiplier = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestBridgeDetection();
        }
    }

    [ContextMenu("Test Bridge Detection")]
    public void TestBridgeDetection()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found with 'Player' tag!");
            return;
        }

        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        Debug.Log($"🧪 Testing detection for {bridges.Length} bridges...");

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            Debug.Log($"\n🌉 Bridge: {bridge.name}");
            Debug.Log($"  - ID: {bridge.BridgeID}");
            Debug.Log($"  - From: {bridge.FromLandmass} → To: {bridge.ToLandmass}");
            Debug.Log($"  - Has been crossed: {bridge.HasBeenCrossed}");
            Debug.Log($"  - Total colliders: {colliders.Length}");

            int triggerCount = 0;
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    triggerCount++;
                    float distance = Vector3.Distance(player.transform.position, collider.bounds.center);
                    Debug.Log($"  - Trigger {triggerCount}: Size={collider.bounds.size}, Distance={distance:F2}m");
                    Debug.Log($"  - Trigger Center: {collider.bounds.center}");
                    Debug.Log($"  - Player Position: {player.transform.position}");

                    if (distance < 5f)
                    {
                        Debug.Log($"    🎯 Player is CLOSE to this trigger!");
                    }
                    else
                    {
                        Debug.Log($"    ⚠️ Player is FAR from this trigger");
                    }
                }
            }

            if (triggerCount == 0)
            {
                Debug.LogWarning($"  ❌ {bridge.name} has NO trigger colliders!");
            }
        }
    }

    [ContextMenu("Show Bridge Inspector Status")]
    public void ShowBridgeInspectorStatus()
    {
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        Debug.Log("=== BRIDGE INSPECTOR STATUS ===");

        foreach (var bridge in bridges)
        {
            Debug.Log($"\n🌉 Bridge: {bridge.name}");
            Debug.Log($"  - Bridge ID: {bridge.BridgeID}");
            Debug.Log($"  - From: {bridge.FromLandmass}");
            Debug.Log($"  - To: {bridge.ToLandmass}");

            // Check spawn point (using child finding since we can't access private field)
            Transform spawnPoint = bridge.transform.Find("PlayerBlockSpawnPoint");
            Debug.Log($"  - Has spawn point child: {spawnPoint != null}");

            // Note: Can't check blocking cube prefab without reflection
            Debug.Log($"  - ⚠️ Check inspector: Blocking Cube Prefab assigned?");
            Debug.Log($"  - ⚠️ Check inspector: Player Block Spawn Point assigned?");
        }
        Debug.Log("================================");
    }

    [ContextMenu("Enlarge All Bridge Triggers")]
    public void EnlargeAllBridgeTriggers()
    {
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int enlargedCount = 0;

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger && collider is BoxCollider boxCol)
                {
                    Vector3 originalSize = boxCol.size;
                    boxCol.size = originalSize * enlargeMultiplier;
                    enlargedCount++;
                    Debug.Log($"✅ Enlarged trigger on {bridge.name}: {originalSize} → {boxCol.size}");
                }
            }
        }

        Debug.Log($"✅ Enlarged {enlargedCount} bridge trigger colliders by {enlargeMultiplier}x");
    }

    [ContextMenu("Force Trigger Test")]
    public void ForceTriggerTest()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found!");
            return;
        }

        // Find closest bridge
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        BridgeController closestBridge = null;
        float closestDistance = float.MaxValue;

        foreach (var bridge in bridges)
        {
            float distance = Vector3.Distance(player.transform.position, bridge.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBridge = bridge;
            }
        }

        if (closestBridge != null)
        {
            Debug.Log($"🧪 Force testing closest bridge: {closestBridge.name} (distance: {closestDistance:F2}m)");

            // Manually call the trigger method if we can access it
            var colliders = closestBridge.GetComponents<Collider>();
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    Debug.Log($"  - Trigger bounds: {col.bounds}");
                    Debug.Log($"  - Player in bounds: {col.bounds.Contains(player.transform.position)}");
                }
            }
        }
    }

    [ContextMenu("Auto-Assign Missing References")]
    public void AutoAssignMissingReferences()
    {
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int fixedCount = 0;

        // Find the DefaultBlockingCube in scene
        GameObject blockingCubePrefab = GameObject.Find("DefaultBlockingCube");

        foreach (var bridge in bridges)
        {
            // Try to find spawn point child
            Transform spawnPoint = bridge.transform.Find("PlayerBlockSpawnPoint");

            if (spawnPoint != null && blockingCubePrefab != null)
            {
                Debug.Log($"💡 {bridge.name}: Found spawn point and blocking cube prefab");
                Debug.Log($"   → Manually drag 'PlayerBlockSpawnPoint' to 'Player Block Spawn Point' field");
                Debug.Log($"   → Manually drag 'DefaultBlockingCube' to 'Blocking Cube Prefab' field");
                fixedCount++;
            }
        }

        Debug.Log($"💡 Found references for {fixedCount} bridges. Manual assignment still needed in Inspector!");
    }

    private void OnDrawGizmos()
    {
        if (!showTriggerGizmos) return;

        BridgeController[] bridges = FindObjectsOfType<BridgeController>();

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    Gizmos.color = bridge.HasBeenCrossed ? Color.red : Color.green;
                    Gizmos.matrix = collider.transform.localToWorldMatrix;

                    if (collider is BoxCollider boxCol)
                    {
                        Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                    }
                }
            }
        }

        // Draw player position
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(player.transform.position, 0.5f);
        }
    }
}