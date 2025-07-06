using UnityEngine;

/// <summary>
/// Auto-fix script to setup proper colliders for landmasses
/// Run this once to fix all landmasses in your scene
/// </summary>
public class LandmassColliderFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float triggerColliderScale = 1.2f; // How much bigger the trigger should be
    [SerializeField] private Vector3 triggerHeightOffset = new Vector3(0, 2f, 0); // Move trigger up slightly

    private void Start()
    {
        if (autoFixOnStart)
        {
            FixAllLandmassColliders();
        }
    }

    [ContextMenu("Fix All Landmass Colliders")]
    public void FixAllLandmassColliders()
    {
        LandmassController[] landmasses = FindObjectsOfType<LandmassController>();
        int fixedCount = 0;

        foreach (var landmass in landmasses)
        {
            if (FixLandmassColliders(landmass.gameObject))
            {
                fixedCount++;
            }
        }

        Debug.Log($"✅ Fixed colliders for {fixedCount} landmasses!");
    }

    public bool FixLandmassColliders(GameObject landmassObject)
    {
        // Get existing collider
        Collider existingCollider = landmassObject.GetComponent<Collider>();
        if (existingCollider == null)
        {
            Debug.LogWarning($"No collider found on {landmassObject.name}");
            return false;
        }

        // Step 1: Make existing collider solid (for ground)
        existingCollider.isTrigger = false;
        Debug.Log($"✅ {landmassObject.name}: Made existing collider solid for ground");

        // Step 2: Create trigger detection collider
        CreateTriggerCollider(landmassObject, existingCollider);

        return true;
    }

    private void CreateTriggerCollider(GameObject landmassObject, Collider groundCollider)
    {
        // Check if we already have a trigger collider
        Collider[] allColliders = landmassObject.GetComponents<Collider>();
        foreach (var col in allColliders)
        {
            if (col.isTrigger && col != groundCollider)
            {
                Debug.Log($"✅ {landmassObject.name}: Trigger collider already exists");
                return;
            }
        }

        // Create new trigger collider based on existing collider type
        Collider triggerCollider = null;

        if (groundCollider is BoxCollider boxCol)
        {
            BoxCollider newTrigger = landmassObject.AddComponent<BoxCollider>();
            newTrigger.size = boxCol.size * triggerColliderScale;
            newTrigger.center = boxCol.center + triggerHeightOffset;
            triggerCollider = newTrigger;
        }
        else if (groundCollider is SphereCollider sphereCol)
        {
            SphereCollider newTrigger = landmassObject.AddComponent<SphereCollider>();
            newTrigger.radius = sphereCol.radius * triggerColliderScale;
            newTrigger.center = sphereCol.center + triggerHeightOffset;
            triggerCollider = newTrigger;
        }
        else if (groundCollider is CapsuleCollider capsuleCol)
        {
            CapsuleCollider newTrigger = landmassObject.AddComponent<CapsuleCollider>();
            newTrigger.radius = capsuleCol.radius * triggerColliderScale;
            newTrigger.height = capsuleCol.height * triggerColliderScale;
            newTrigger.center = capsuleCol.center + triggerHeightOffset;
            triggerCollider = newTrigger;
        }
        else
        {
            // Fallback: create a box collider
            BoxCollider newTrigger = landmassObject.AddComponent<BoxCollider>();
            newTrigger.size = groundCollider.bounds.size * triggerColliderScale;
            newTrigger.center = triggerHeightOffset;
            triggerCollider = newTrigger;
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            Debug.Log($"✅ {landmassObject.name}: Created trigger collider for detection");
        }
    }

    [ContextMenu("Remove All Trigger Colliders")]
    public void RemoveAllTriggerColliders()
    {
        LandmassController[] landmasses = FindObjectsOfType<LandmassController>();
        int removedCount = 0;

        foreach (var landmass in landmasses)
        {
            Collider[] colliders = landmass.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    DestroyImmediate(collider);
                    removedCount++;
                }
            }
        }

        Debug.Log($"🗑️ Removed {removedCount} trigger colliders");
    }

    [ContextMenu("Show Collider Info")]
    public void ShowColliderInfo()
    {
        LandmassController[] landmasses = FindObjectsOfType<LandmassController>();

        Debug.Log("=== LANDMASS COLLIDER INFO ===");
        foreach (var landmass in landmasses)
        {
            Collider[] colliders = landmass.GetComponents<Collider>();
            Debug.Log($"{landmass.name}: {colliders.Length} colliders");

            for (int i = 0; i < colliders.Length; i++)
            {
                Debug.Log($"  - Collider {i + 1}: {colliders[i].GetType().Name}, IsTrigger: {colliders[i].isTrigger}");
            }
        }
        Debug.Log("==============================");
    }
}