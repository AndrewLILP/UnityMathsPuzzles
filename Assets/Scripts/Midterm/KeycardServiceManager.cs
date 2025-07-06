using UnityEngine;
using System.Collections.Generic;

// Implementation of IKeycardService that goes on GameObjects
public class KeycardServiceManager : MonoBehaviour, IKeycardService
{
    [Header("Available Keycards")]
    [SerializeField] private List<KeycardData> availableKeycards = new List<KeycardData>();

    // Observer Pattern - List of observers
    private List<IKeycardObserver> observers = new List<IKeycardObserver>();

    // Service state
    private HashSet<string> collectedKeycards = new HashSet<string>();
    private Dictionary<string, KeycardData> keycardDatabase = new Dictionary<string, KeycardData>();

    // Service Locator Instance
    private static KeycardServiceManager instance;

    private void Awake()
    {
        // Singleton pattern for Service Locator
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeKeycards();
    }

    public static IKeycardService GetService()
    {
        if (instance == null)
        {
            // Create service if it doesn't exist
            GameObject serviceObject = new GameObject("KeycardServiceManager");
            instance = serviceObject.AddComponent<KeycardServiceManager>();
        }
        return instance;
    }

    private void InitializeKeycards()
    {
        // Initialize with default keycards if none configured
        if (availableKeycards.Count == 0)
        {
            availableKeycards.Add(new KeycardData("bridge_access", "Bridge Access Card", KeycardType.Bridge));
            availableKeycards.Add(new KeycardData("level0_master", "Level 0 Master Key", KeycardType.Master));
        }

        // Populate database
        keycardDatabase.Clear();
        foreach (var keycard in availableKeycards)
        {
            keycardDatabase[keycard.id] = keycard;
        }

        Debug.Log($"KeycardServiceManager initialized with {keycardDatabase.Count} keycard types");
    }

    #region IKeycardService Implementation
    public bool HasKeycard(string keycardId)
    {
        return collectedKeycards.Contains(keycardId);
    }

    public void CollectKeycard(string keycardId)
    {
        if (!keycardDatabase.ContainsKey(keycardId))
        {
            Debug.LogWarning($"Unknown keycard ID: {keycardId}");
            return;
        }

        if (HasKeycard(keycardId))
        {
            Debug.LogWarning($"Keycard {keycardId} already collected");
            return;
        }

        collectedKeycards.Add(keycardId);
        Debug.Log($"Collected keycard: {keycardDatabase[keycardId].displayName}");

        // Notify observers
        foreach (var observer in observers)
        {
            observer.OnKeycardCollected(keycardId);
        }
    }

    public bool UseKeycard(string keycardId)
    {
        if (!HasKeycard(keycardId))
        {
            Debug.LogWarning($"Cannot use keycard {keycardId} - not in inventory");
            return false;
        }

        var keycardData = keycardDatabase[keycardId];

        // Remove if consumable
        if (keycardData.isConsumable)
        {
            collectedKeycards.Remove(keycardId);
            Debug.Log($"Consumed keycard: {keycardData.displayName}");
        }
        else
        {
            Debug.Log($"Used keycard: {keycardData.displayName}");
        }

        // Notify observers
        foreach (var observer in observers)
        {
            observer.OnKeycardUsed(keycardId);
        }

        return true;
    }

    public void RegisterObserver(IKeycardObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
            Debug.Log($"Registered keycard observer: {observer.GetType().Name}");
        }
    }

    public void UnregisterObserver(IKeycardObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
            Debug.Log($"Unregistered keycard observer: {observer.GetType().Name}");
        }
    }

    public KeycardData GetKeycardData(string keycardId)
    {
        return keycardDatabase.ContainsKey(keycardId) ? keycardDatabase[keycardId] : null;
    }
    #endregion

    #region Debug Helpers
    [ContextMenu("Debug - List Collected Keycards")]
    private void DebugListKeycards()
    {
        Debug.Log($"Collected Keycards ({collectedKeycards.Count}):");
        foreach (var keycardId in collectedKeycards)
        {
            if (keycardDatabase.ContainsKey(keycardId))
            {
                Debug.Log($"  - {keycardDatabase[keycardId].displayName} ({keycardId})");
            }
        }
    }

    [ContextMenu("Debug - Collect Test Keycard")]
    private void DebugCollectTestKeycard()
    {
        CollectKeycard("bridge_access");
    }
    #endregion
}