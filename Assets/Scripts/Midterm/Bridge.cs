using UnityEngine;
using System.Collections.Generic;

// Bridge implementation that can be attached to GameObjects
public class Bridge : MonoBehaviour
{
    [Header("Bridge Configuration")]
    [SerializeField] private string bridgeId = "bridge_01";
    [SerializeField] private string requiredKeycardId = "";
    [SerializeField] private bool isOneTimeCrossing = false;

    [Header("Connected Land Masses")]
    [SerializeField] private string fromLandMass = "LandMassA";
    [SerializeField] private string toLandMass = "LandMassB";

    [Header("Visual Components")]
    [SerializeField] private MeshRenderer bridgeRenderer;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private Material availableMaterial;
    [SerializeField] private Material crossedMaterial;
    [SerializeField] private Material activeMaterial;

    // State Pattern - Current bridge state
    private BridgeState currentState = BridgeState.Available;
    private List<IBridgeObserver> observers = new List<IBridgeObserver>();
    private IKeycardService keycardService;
    private bool hasCrossed = false;
    private BridgeData bridgeData;

    // Public properties
    public string BridgeId => bridgeId;
    public BridgeState CurrentState => currentState;
    public BridgeData Data => bridgeData;

    private void Start()
    {
        InitializeBridge();
        RegisterWithService();
    }

    private void Update()
    {
        CheckKeycardStatus();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AttemptCrossing(other.gameObject);
        }
    }

    #region Initialization
    private void InitializeBridge()
    {
        // Get keycard service using Service Locator pattern
        keycardService = KeycardServiceManager.GetService();

        // Initialize bridge data
        bridgeData = new BridgeData(bridgeId, $"Bridge {bridgeId}", fromLandMass, toLandMass);
        bridgeData.requiredKeycardId = requiredKeycardId;
        bridgeData.type = isOneTimeCrossing ? BridgeType.OneTime : BridgeType.Standard;

        // Set initial state based on keycard requirement
        if (!string.IsNullOrEmpty(requiredKeycardId))
        {
            SetState(BridgeState.Locked);
        }
        else
        {
            SetState(BridgeState.Available);
        }

        UpdateVisualState();
        Debug.Log($"Bridge initialized: {bridgeData.displayName}");
    }

    private void RegisterWithService()
    {
        // Future: Register with bridge tracking service when implemented
        Debug.Log($"Bridge {bridgeId} ready for service registration");
    }
    #endregion

    #region Bridge Logic
    public bool CanCross(GameObject crosser)
    {
        // Check current state
        if (currentState == BridgeState.Disabled || currentState == BridgeState.Active)
        {
            return false;
        }

        // Check if already crossed for one-time bridges
        if (isOneTimeCrossing && hasCrossed)
        {
            return false;
        }

        // Check keycard requirement
        if (!string.IsNullOrEmpty(requiredKeycardId))
        {
            return keycardService?.HasKeycard(requiredKeycardId) ?? false;
        }

        return currentState == BridgeState.Available;
    }

    public bool AttemptCrossing(GameObject crosser)
    {
        if (!CanCross(crosser))
        {
            string reason = GetCrossingFailureReason();
            Debug.Log($"Cannot cross bridge {bridgeId}: {reason}");
            return false;
        }

        return ExecuteCrossing(crosser);
    }

    private bool ExecuteCrossing(GameObject crosser)
    {
        // Use keycard if required
        if (!string.IsNullOrEmpty(requiredKeycardId))
        {
            bool keycardUsed = keycardService.UseKeycard(requiredKeycardId);
            if (!keycardUsed)
            {
                Debug.Log($"Failed to use keycard for bridge {bridgeId}");
                return false;
            }
        }

        // Set state to active during crossing
        SetState(BridgeState.Active);

        // Mark as crossed
        hasCrossed = true;

        Debug.Log($"Player successfully crossed bridge: {bridgeId}");

        // Notify observers
        NotifyBridgeCrossed();

        // Update state after crossing
        if (isOneTimeCrossing)
        {
            SetState(BridgeState.Crossed);
        }
        else
        {
            SetState(BridgeState.Available);
        }

        return true;
    }

    public void SetState(BridgeState newState)
    {
        if (currentState != newState)
        {
            BridgeState oldState = currentState;
            currentState = newState;

            UpdateVisualState();
            NotifyStateChanged(newState);

            Debug.Log($"Bridge {bridgeId} state changed: {oldState} -> {newState}");
        }
    }

    private string GetCrossingFailureReason()
    {
        if (currentState == BridgeState.Disabled)
            return "Bridge is disabled";

        if (currentState == BridgeState.Active)
            return "Bridge is currently being used";

        if (isOneTimeCrossing && hasCrossed)
            return "Bridge can only be crossed once";

        if (!string.IsNullOrEmpty(requiredKeycardId))
        {
            if (!keycardService.HasKeycard(requiredKeycardId))
                return $"Requires keycard: {requiredKeycardId}";
        }

        return "Unknown reason";
    }
    #endregion

    #region State Management
    private void CheckKeycardStatus()
    {
        // Check if keycard requirement is met for locked bridges
        if (currentState == BridgeState.Locked && !string.IsNullOrEmpty(requiredKeycardId))
        {
            if (keycardService != null && keycardService.HasKeycard(requiredKeycardId))
            {
                SetState(BridgeState.Available);
            }
        }
    }

    private void UpdateVisualState()
    {
        if (bridgeRenderer == null) return;

        switch (currentState)
        {
            case BridgeState.Locked:
                ApplyMaterial(lockedMaterial, Color.red);
                break;
            case BridgeState.Available:
                ApplyMaterial(availableMaterial, Color.green);
                break;
            case BridgeState.Crossed:
                ApplyMaterial(crossedMaterial, Color.blue);
                break;
            case BridgeState.Active:
                ApplyMaterial(activeMaterial, Color.yellow);
                break;
            case BridgeState.Disabled:
                ApplyMaterial(null, Color.gray);
                break;
        }
    }

    private void ApplyMaterial(Material material, Color fallbackColor)
    {
        if (material != null)
        {
            bridgeRenderer.material = material;
        }
        else
        {
            bridgeRenderer.material.color = fallbackColor;
        }
    }
    #endregion

    #region Observer Pattern
    public void RegisterObserver(IBridgeObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
            Debug.Log($"Registered bridge observer: {observer.GetType().Name}");
        }
    }

    public void UnregisterObserver(IBridgeObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
            Debug.Log($"Unregistered bridge observer: {observer.GetType().Name}");
        }
    }

    private void NotifyBridgeCrossed()
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.OnBridgeCrossed(this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Observer notification error: {ex.Message}");
            }
        }
    }

    private void NotifyStateChanged(BridgeState newState)
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.OnBridgeStateChanged(this, newState);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Observer notification error: {ex.Message}");
            }
        }
    }
    #endregion

    #region Context Menu Testing
    [ContextMenu("Test Bridge Crossing")]
    private void TestCrossing()
    {
        if (Application.isPlaying)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                AttemptCrossing(player);
            }
            else
            {
                Debug.LogWarning("No player found for test crossing");
            }
        }
    }

    [ContextMenu("Force Available State")]
    private void ForceAvailableState()
    {
        if (Application.isPlaying)
        {
            SetState(BridgeState.Available);
        }
    }

    [ContextMenu("Debug Bridge State")]
    private void DebugBridgeState()
    {
        Debug.Log($"=== Bridge {bridgeId} State ===");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Can Cross: {CanCross(null)}");
        Debug.Log($"Has Crossed: {hasCrossed}");
        Debug.Log($"Required Keycard: {requiredKeycardId}");
    }
    #endregion
}