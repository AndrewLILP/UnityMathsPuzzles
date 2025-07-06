using UnityEngine;

/// <summary>
/// Simple, direct fix for bridge trigger heights
/// This manually sets all bridge trigger colliders to the correct height
/// </summary>
public class SimpleBridgeTriggerFix : MonoBehaviour
{
    [Header("Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float playerWalkHeight = 2.5f; // Where the player walks
    [SerializeField] private float triggerHeight = 1f; // Height above player walk level

    private void Start()
    {
        if (autoFixOnStart)
        {
            FixAllBridgeTriggers();
        }
    }

    [ContextMenu("Fix All Bridge Triggers Now")]
    public void FixAllBridgeTriggers()
    {
        // Use the old method for compatibility
        BridgeController[] bridges = GameObject.FindObjectsOfType<BridgeController>();
        int fixedCount = 0;

        Debug.Log($"Found {bridges.Length} bridges to fix...");

        foreach (var bridge in bridges)
        {
            bool bridgeFixed = false;
            Collider[] colliders = bridge.GetComponents<Collider>();

            Debug.Log($"Checking bridge {bridge.name} with {colliders.Length} colliders...");

            foreach (var collider in colliders)
            {
                if (collider.isTrigger && collider is BoxCollider boxCol)
                {
                    // Simple direct fix - set the center Y to the correct height
                    Vector3 currentCenter = boxCol.center;
                    float targetY = playerWalkHeight + triggerHeight;

                    // Set the new center directly
                    boxCol.center = new Vector3(currentCenter.x, targetY, currentCenter.z);

                    Debug.Log($"✅ Fixed {bridge.name}: Trigger moved from Y={currentCenter.y:F2} to Y={targetY:F2}");
                    bridgeFixed = true;
                }
            }

            if (bridgeFixed)
                fixedCount++;
        }

        Debug.Log($"🎯 Fixed trigger heights for {fixedCount} bridges!");

        // Test the fix immediately
        TestFixAfterApplying();
    }

    [ContextMenu("Test Fix Results")]
    public void TestFixAfterApplying()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found!");
            return;
        }

        BridgeController[] bridges = GameObject.FindObjectsOfType<BridgeController>();

        Debug.Log("=== TESTING BRIDGE FIX RESULTS ===");
        Debug.Log($"Player at Y: {player.transform.position.y:F2}");

        int workingBridges = 0;

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    float distance = Vector3.Distance(player.transform.position, collider.bounds.center);
                    float heightDiff = Mathf.Abs(collider.bounds.center.y - player.transform.position.y);

                    bool shouldWork = heightDiff < 5f; // If height difference is reasonable

                    Debug.Log($"🌉 {bridge.name}:");
                    Debug.Log($"  - Trigger Y: {collider.bounds.center.y:F2}");
                    Debug.Log($"  - Height diff: {heightDiff:F2}");
                    Debug.Log($"  - Distance: {distance:F2}m");
                    Debug.Log($"  - Should work: {(shouldWork ? "✅ YES" : "❌ NO")}");

                    if (shouldWork)
                        workingBridges++;
                }
            }
        }

        Debug.Log($"🎯 {workingBridges} bridges should now work!");
        Debug.Log("==================================");
    }

    [ContextMenu("Manual Emergency Fix")]
    public void ManualEmergencyFix()
    {
        // Find all GameObjects that might be bridges
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int fixedCount = 0;

        foreach (var obj in allObjects)
        {
            if (obj.name.ToLower().Contains("bridge") && obj.GetComponent<BridgeController>() != null)
            {
                Collider[] colliders = obj.GetComponents<Collider>();

                foreach (var collider in colliders)
                {
                    if (collider.isTrigger && collider is BoxCollider boxCol)
                    {
                        // Force set to a specific height
                        boxCol.center = new Vector3(boxCol.center.x, 3.5f, boxCol.center.z);
                        Debug.Log($"🚨 Emergency fix: Set {obj.name} trigger to Y=3.5");
                        fixedCount++;
                    }
                }
            }
        }

        Debug.Log($"🚨 Emergency fixed {fixedCount} bridge triggers!");
    }

    [ContextMenu("Show All Bridge Trigger Info")]
    public void ShowAllBridgeTriggerInfo()
    {
        BridgeController[] bridges = GameObject.FindObjectsOfType<BridgeController>();

        Debug.Log("=== ALL BRIDGE TRIGGER INFO ===");

        foreach (var bridge in bridges)
        {
            Debug.Log($"\n🌉 {bridge.name}:");

            Collider[] colliders = bridge.GetComponents<Collider>();
            Debug.Log($"  - Total colliders: {colliders.Length}");

            int triggerCount = 0;
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    triggerCount++;
                    Debug.Log($"  - Trigger {triggerCount}: Type={collider.GetType().Name}");
                    Debug.Log($"    Center: {collider.bounds.center}");
                    Debug.Log($"    Size: {collider.bounds.size}");
                }
            }

            if (triggerCount == 0)
            {
                Debug.LogWarning($"  ❌ {bridge.name} has NO trigger colliders!");
            }
        }
        Debug.Log("===============================");
    }
}