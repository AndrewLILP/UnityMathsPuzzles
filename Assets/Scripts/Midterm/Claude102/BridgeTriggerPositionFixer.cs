using UnityEngine;

/// <summary>
/// Fix bridge trigger positions - they're too low!
/// </summary>
public class BridgeTriggerPositionFixer : MonoBehaviour
{
    [Header("Position Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float targetPlayerHeight = 2.5f; // Where player walks
    [SerializeField] private float triggerHeightOffset = 1f; // Extra height for trigger

    private void Start()
    {
        if (autoFixOnStart)
        {
            FixAllBridgeTriggerPositions();
        }
    }

    [ContextMenu("Fix All Bridge Trigger Positions")]
    public void FixAllBridgeTriggerPositions()
    {
        BridgeController[] bridges = FindObjectsByType<BridgeController>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (var bridge in bridges)
        {
            if (FixBridgeTriggerPosition(bridge))
            {
                fixedCount++;
            }
        }

        Debug.Log($"✅ Fixed trigger positions for {fixedCount} bridges!");
    }

    public bool FixBridgeTriggerPosition(BridgeController bridge)
    {
        Collider[] colliders = bridge.GetComponents<Collider>();
        bool fixedAny = false;

        foreach (var collider in colliders)
        {
            if (collider.isTrigger && collider is BoxCollider boxCol)
            {
                // Get current world position
                Vector3 worldCenter = bridge.transform.TransformPoint(boxCol.center);
                float currentY = worldCenter.y;

                // Calculate new Y position (where player walks + offset)
                float newY = targetPlayerHeight + triggerHeightOffset;

                // Convert back to local space
                Vector3 newLocalCenter = bridge.transform.InverseTransformPoint(new Vector3(worldCenter.x, newY, worldCenter.z));

                // Apply the fix
                boxCol.center = newLocalCenter;

                Debug.Log($"✅ {bridge.name}: Moved trigger from Y={currentY:F2} to Y={newY:F2}");
                fixedAny = true;
            }
        }

        return fixedAny;
    }

    [ContextMenu("Test Bridge Detection After Fix")]
    public void TestBridgeDetectionAfterFix()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found!");
            return;
        }

        BridgeController[] bridges = FindObjectsByType<BridgeController>(FindObjectsSortMode.None);
        Debug.Log("=== TESTING AFTER POSITION FIX ===");

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    float distance = Vector3.Distance(player.transform.position, collider.bounds.center);
                    bool inRange = distance < 15f;

                    Debug.Log($"🌉 {bridge.name}:");
                    Debug.Log($"  - Player Y: {player.transform.position.y:F2}");
                    Debug.Log($"  - Trigger Y: {collider.bounds.center.y:F2}");
                    Debug.Log($"  - Distance: {distance:F2}m");
                    Debug.Log($"  - In Range: {(inRange ? "✅ YES" : "❌ NO")}");

                    if (inRange)
                    {
                        Debug.Log($"  🎯 {bridge.name} should now detect player!");
                    }
                }
            }
        }
    }

    [ContextMenu("Show Player vs Bridge Heights")]
    public void ShowPlayerVsBridgeHeights()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found!");
            return;
        }

        BridgeController[] bridges = FindObjectsByType<BridgeController>(FindObjectsSortMode.None);

        Debug.Log("=== PLAYER VS BRIDGE HEIGHTS ===");
        Debug.Log($"Player Height: {player.transform.position.y:F2}");
        Debug.Log($"Target Height: {targetPlayerHeight:F2}");

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    float heightDiff = collider.bounds.center.y - player.transform.position.y;
                    Debug.Log($"{bridge.name}: Trigger at Y={collider.bounds.center.y:F2} (diff: {heightDiff:F2})");
                }
            }
        }
        Debug.Log("=================================");
    }
}