using UnityEngine;

/// <summary>
/// Auto-fix script to setup proper trigger colliders and blocking cube setup for bridges
/// </summary>
public class BridgeSetupFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool createDefaultBlockingCube = true;
    [SerializeField] private bool createSpawnPoints = true;

    [Header("Default Blocking Cube")]
    [SerializeField] private Material blockingCubeMaterial;
    [SerializeField] private Vector3 blockingCubeSize = new Vector3(2f, 3f, 1f);

    private GameObject defaultBlockingCubePrefab;

    private void Start()
    {
        if (autoFixOnStart)
        {
            CreateDefaultBlockingCubePrefab();
            FixAllBridges();
        }
    }

    [ContextMenu("Fix All Bridges")]
    public void FixAllBridges()
    {
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();
        int fixedCount = 0;

        foreach (var bridge in bridges)
        {
            if (FixBridgeSetup(bridge.gameObject))
            {
                fixedCount++;
            }
        }

        Debug.Log($"✅ Fixed setup for {fixedCount} bridges!");
    }

    public bool FixBridgeSetup(GameObject bridgeObject)
    {
        Debug.Log($"🔧 Fixing bridge: {bridgeObject.name}");

        // Step 1: Ensure bridge has trigger collider
        EnsureTriggerCollider(bridgeObject);

        // Step 2: Create spawn point if missing
        if (createSpawnPoints)
        {
            EnsureSpawnPoint(bridgeObject);
        }

        // Step 3: Assign blocking cube prefab if missing
        if (createDefaultBlockingCube)
        {
            EnsureBlockingCubePrefab(bridgeObject);
        }

        return true;
    }

    private void EnsureTriggerCollider(GameObject bridgeObject)
    {
        Collider[] colliders = bridgeObject.GetComponents<Collider>();
        bool hasTrigger = false;

        foreach (var collider in colliders)
        {
            if (collider.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }

        if (!hasTrigger)
        {
            // Add a trigger collider
            BoxCollider triggerCollider = bridgeObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;

            // Try to size it based on the bridge's renderer bounds
            Renderer renderer = bridgeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                triggerCollider.size = renderer.bounds.size;
                triggerCollider.center = Vector3.zero;
            }
            else
            {
                // Default size for bridge trigger
                triggerCollider.size = new Vector3(10f, 2f, 3f);
            }

            Debug.Log($"✅ {bridgeObject.name}: Added trigger collider");
        }
        else
        {
            Debug.Log($"✅ {bridgeObject.name}: Trigger collider already exists");
        }
    }

    private void EnsureSpawnPoint(GameObject bridgeObject)
    {
        BridgeController bridge = bridgeObject.GetComponent<BridgeController>();
        if (bridge == null) return;

        // Check if spawn point is already assigned (this would require making the field public or using reflection)
        // For now, we'll create a spawn point as a child object

        Transform existingSpawnPoint = bridgeObject.transform.Find("PlayerBlockSpawnPoint");
        if (existingSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("PlayerBlockSpawnPoint");
            spawnPoint.transform.SetParent(bridgeObject.transform);

            // Position it at the back of the bridge (assuming bridge faces forward)
            Vector3 bridgeSize = Vector3.one;
            Renderer renderer = bridgeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                bridgeSize = renderer.bounds.size;
            }

            spawnPoint.transform.localPosition = new Vector3(0, 0, -bridgeSize.z * 0.6f);

            Debug.Log($"✅ {bridgeObject.name}: Created spawn point");
        }
        else
        {
            Debug.Log($"✅ {bridgeObject.name}: Spawn point already exists");
        }
    }

    private void EnsureBlockingCubePrefab(GameObject bridgeObject)
    {
        if (defaultBlockingCubePrefab == null)
        {
            CreateDefaultBlockingCubePrefab();
        }

        // Note: We can't directly assign the prefab to the BridgeController field without reflection
        // The user will need to manually drag the created prefab to the inspector
        Debug.Log($"💡 {bridgeObject.name}: Default blocking cube prefab created. Drag 'DefaultBlockingCube' prefab to BridgeController inspector.");
    }

    private void CreateDefaultBlockingCubePrefab()
    {
        if (defaultBlockingCubePrefab != null) return;

        // Create the blocking cube prefab
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "DefaultBlockingCube";
        cube.transform.localScale = blockingCubeSize;

        // Make it red so it's obvious
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        if (blockingCubeMaterial != null)
        {
            cubeRenderer.material = blockingCubeMaterial;
        }
        else
        {
            // Create a red material
            Material redMaterial = new Material(Shader.Find("Standard"));
            redMaterial.color = Color.red;
            cubeRenderer.material = redMaterial;
        }

        // Disable it initially
        cube.SetActive(false);

        // Store reference
        defaultBlockingCubePrefab = cube;

        Debug.Log($"✅ Created DefaultBlockingCube prefab");
    }

    [ContextMenu("Create Blocking Cube Prefab")]
    public void CreateBlockingCubePrefabOnly()
    {
        CreateDefaultBlockingCubePrefab();

        if (defaultBlockingCubePrefab != null)
        {
            Debug.Log($"🎯 DefaultBlockingCube prefab created! You can now drag this to your BridgeController's 'Blocking Cube Prefab' field in the inspector.");
        }
    }

    [ContextMenu("Show Bridge Info")]
    public void ShowBridgeInfo()
    {
        BridgeController[] bridges = FindObjectsOfType<BridgeController>();

        Debug.Log("=== BRIDGE SETUP INFO ===");
        foreach (var bridge in bridges)
        {
            Debug.Log($"Bridge: {bridge.name}");
            Debug.Log($"  - ID: {bridge.BridgeID}");
            Debug.Log($"  - From: {bridge.FromLandmass} → To: {bridge.ToLandmass}");

            // Check colliders
            Collider[] colliders = bridge.GetComponents<Collider>();
            int triggerCount = 0;
            int solidCount = 0;

            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                    triggerCount++;
                else
                    solidCount++;
            }

            Debug.Log($"  - Colliders: {triggerCount} triggers, {solidCount} solid");

            // Check spawn point
            Transform spawnPoint = bridge.transform.Find("PlayerBlockSpawnPoint");
            Debug.Log($"  - Has spawn point: {spawnPoint != null}");

            Debug.Log($"  - Has been crossed: {bridge.HasBeenCrossed}");
        }
        Debug.Log("========================");
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
        Debug.Log($"🧪 Testing bridge detection for {bridges.Length} bridges...");

        foreach (var bridge in bridges)
        {
            Collider[] triggers = bridge.GetComponents<Collider>();
            foreach (var trigger in triggers)
            {
                if (trigger.isTrigger)
                {
                    float distance = Vector3.Distance(player.transform.position, trigger.bounds.center);
                    Debug.Log($"  - {bridge.name}: Distance = {distance:F2}m, Bounds = {trigger.bounds.size}");
                }
            }
        }
    }
}