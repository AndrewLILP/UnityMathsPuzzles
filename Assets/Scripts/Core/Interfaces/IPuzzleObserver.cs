using UnityEngine;

/// <summary>
/// Observer pattern interface for puzzle events
/// UI components and other systems can subscribe to puzzle changes
/// </summary>
public interface IPuzzleObserver
{
    void OnPuzzleStateChanged(IPuzzle puzzle, IPuzzleState newState);
    void OnPuzzleProgressChanged(IPuzzle puzzle, PuzzleContext context);
    void OnPuzzleCompleted(IPuzzle puzzle);
    void OnPuzzleFailed(IPuzzle puzzle, string reason);
}

/// <summary>
/// Event data structures for Observer pattern
/// </summary>
[System.Serializable]
public class PuzzleEvent
{
    public IPuzzle puzzle;
    public string eventType;
    public float timestamp;

    public PuzzleEvent(IPuzzle puzzle, string eventType)
    {
        this.puzzle = puzzle;
        this.eventType = eventType;
        this.timestamp = Time.time;
    }
}

[System.Serializable]
public class PuzzleProgressEvent : PuzzleEvent
{
    public int bridgesCrossed;
    public int landmassesVisited;
    public int totalBridges;
    public int totalLandmasses;

    public PuzzleProgressEvent(IPuzzle puzzle, PuzzleContext context) : base(puzzle, "Progress")
    {
        this.bridgesCrossed = context.bridgesCrossed.Count;
        this.landmassesVisited = context.landmassesVisited.Count;
        this.totalBridges = context.requiredBridges;
        this.totalLandmasses = context.requiredLandmasses;
    }
}