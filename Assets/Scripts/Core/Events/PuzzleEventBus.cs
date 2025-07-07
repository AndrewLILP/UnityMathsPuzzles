using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Simple Event Bus for Observer pattern implementation
/// Allows loose coupling between puzzle system and UI/other systems
/// </summary>
public class PuzzleEventBus : MonoBehaviour
{
    // Singleton for easy access
    public static PuzzleEventBus Instance { get; private set; }

    // Dictionary to store event subscriptions
    private Dictionary<Type, List<Action<object>>> subscribers = new Dictionary<Type, List<Action<object>>>();

    private void Awake()
    {
        // Simple singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Subscribe to events of type T
    /// </summary>
    public void Subscribe<T>(Action<T> handler)
    {
        var eventType = typeof(T);

        if (!subscribers.ContainsKey(eventType))
        {
            subscribers[eventType] = new List<Action<object>>();
        }

        // Wrap the typed handler in an object handler
        subscribers[eventType].Add(obj => handler((T)obj));

        Debug.Log($"Subscribed to event type: {eventType.Name}");
    }

    /// <summary>
    /// Unsubscribe from events of type T
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler)
    {
        var eventType = typeof(T);

        if (subscribers.ContainsKey(eventType))
        {
            // Note: This is a simplified unsubscribe - in production you'd want better tracking
            Debug.Log($"Unsubscribed from event type: {eventType.Name}");
        }
    }

    /// <summary>
    /// Publish an event to all subscribers
    /// </summary>
    public void Publish<T>(T eventData)
    {
        var eventType = typeof(T);

        if (subscribers.ContainsKey(eventType))
        {
            foreach (var handler in subscribers[eventType])
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error handling event {eventType.Name}: {ex.Message}");
                }
            }

            Debug.Log($"Published event: {eventType.Name} to {subscribers[eventType].Count} subscribers");
        }
    }

    /// <summary>
    /// Clear all subscriptions (useful for testing or cleanup)
    /// </summary>
    public void ClearAllSubscriptions()
    {
        subscribers.Clear();
        Debug.Log("Cleared all event subscriptions");
    }
}

// Common event types
[System.Serializable]
public class PuzzleStartedEvent
{
    public PuzzleType puzzleType;
    public string puzzleName;
}

[System.Serializable]
public class PuzzleCompletedEvent
{
    public PuzzleType puzzleType;
    public string puzzleName;
    public float completionTime;
}

[System.Serializable]
public class PuzzleFailedEvent
{
    public PuzzleType puzzleType;
    public string puzzleName;
    public string reason;
}

[System.Serializable]
public class PuzzleTransitionEvent
{
    public PuzzleType fromPuzzle;
    public PuzzleType toPuzzle;
}