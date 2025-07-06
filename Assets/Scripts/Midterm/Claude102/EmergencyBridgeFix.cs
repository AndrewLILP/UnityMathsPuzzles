using UnityEngine;

/// <summary>
/// Emergency fix for bridge trigger positions
/// Run this once to fix all bridge triggers manually
/// </summary>
public class EmergencyBridgeFix : MonoBehaviour
{
    [Header("Fix Settings")]
    [SerializeField] private float playerHeight = 2.5f; // Where player walks
    [SerializeField] private float triggerHeight = 1f;   // Height above player

    [ContextMenu("Emergency Fix All Bridges NOW")]
    public void EmergencyFixAllBridges()
    {
        // Find player to get correct height
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHeight = player.transform.position.y;
        }

        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int fixedCount = 0;

        Debug.Log($"🚨 EMERGENCY FIX: Found {bridges.Length} bridges to fix");
        Debug.Log($"Target trigger height: {playerHeight + triggerHeight}");

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();

            foreach (var collider in colliders)
            {
                if (collider.isTrigger && collider is BoxCollider boxCol)
                {
                    // Calculate world position for the trigger
                    float targetWorldY = playerHeight + triggerHeight;

                    // Convert to local space
                    Vector3 worldPos = new Vector3(
                        bridge.transform.position.x,
                        targetWorldY,
                        bridge.transform.position.z
                    );

                    Vector3 localPos = bridge.transform.InverseTransformPoint(worldPos);

                    // Set the new center
                    Vector3 oldCenter = boxCol.center;
                    boxCol.center = new Vector3(localPos.x, localPos.y, localPos.z);

                    Debug.Log($"✅ FIXED {bridge.name}: Trigger moved from Y={oldCenter.y:F2} to Y={boxCol.center.y:F2}");
                    fixedCount++;
                }
            }
        }

        Debug.Log($"🎯 EMERGENCY FIX COMPLETE: Fixed {fixedCount} triggers!");

        // Test immediately
        TestBridgesAfterFix();
    }

    private void TestBridgesAfterFix()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int workingBridges = 0;

        Debug.Log("=== TESTING AFTER EMERGENCY FIX ===");

        foreach (var bridge in bridges)
        {
            Collider[] colliders = bridge.GetComponents<Collider>();

            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    float distance = Vector3.Distance(player.transform.position, collider.bounds.center);
                    float heightDiff = Mathf.Abs(collider.bounds.center.y - player.transform.position.y);

                    bool shouldWork = distance < 20f && heightDiff < 3f;

                    Debug.Log($"🌉 {bridge.name}: Distance={distance:F1}m, HeightDiff={heightDiff:F1}m - {(shouldWork ? "✅ SHOULD WORK" : "❌ STILL BROKEN")}");

                    if (shouldWork) workingBridges++;
                }
            }
        }

        Debug.Log($"🎯 Result: {workingBridges} bridges should now work!");
        Debug.Log("===================================");
    }
}