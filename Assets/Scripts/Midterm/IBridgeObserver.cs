using UnityEngine;

// Observer Pattern - Bridge event notifications
public interface IBridgeObserver
{
    void OnBridgeCrossed(Bridge bridge);
    void OnBridgeStateChanged(Bridge bridge, BridgeState newState);
}

// State Pattern - Bridge state management
public enum BridgeState
{
    Locked,      // Requires keycard to access
    Available,   // Can be crossed
    Crossed,     // Has been crossed (for one-time bridges)
    Active,      // Currently being crossed
    Disabled     // Permanently disabled
}

// Data structure for bridge information
[System.Serializable]
public class BridgeData
{
    public string id;
    public string displayName;
    public string fromLandMass;
    public string toLandMass;
    public BridgeType type;
    public string requiredKeycardId;
    public bool isOneWay;
    public bool consumeKeycard;

    public BridgeData(string id, string displayName, string from, string to)
    {
        this.id = id;
        this.displayName = displayName;
        this.fromLandMass = from;
        this.toLandMass = to;
        this.type = BridgeType.Standard;
        this.isOneWay = false;
        this.consumeKeycard = false;
    }
}

public enum BridgeType
{
    Standard,
    OneTime,     // Can only be crossed once
    Timed,       // Available for limited time
    Sequential,  // Must be crossed in specific order
    Master       // Requires master keycard
}