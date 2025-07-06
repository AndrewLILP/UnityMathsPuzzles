using UnityEngine;

// Service Locator Pattern - Main service interface
public interface IKeycardService
{
    bool HasKeycard(string keycardId);
    void CollectKeycard(string keycardId);
    bool UseKeycard(string keycardId);
    void RegisterObserver(IKeycardObserver observer);
    void UnregisterObserver(IKeycardObserver observer);
    KeycardData GetKeycardData(string keycardId);
}

// Observer Pattern - Event notifications
public interface IKeycardObserver
{
    void OnKeycardCollected(string keycardId);
    void OnKeycardUsed(string keycardId);
}

// Data structure for keycard information
[System.Serializable]
public class KeycardData
{
    public string id;
    public string displayName;
    public KeycardType type;
    public bool isConsumable;

    public KeycardData(string id, string displayName, KeycardType type = KeycardType.Standard)
    {
        this.id = id;
        this.displayName = displayName;
        this.type = type;
        this.isConsumable = false;
    }
}

public enum KeycardType
{
    Standard,
    Bridge,
    Level,
    Master
}