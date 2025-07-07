using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Strategy pattern interface for puzzle validation logic
/// Each puzzle type has different rules for what's valid
/// </summary>
public interface IPuzzleValidator
{
    bool ValidateBridgeCrossing(string bridgeId, PuzzleContext context);
    bool ValidateLandmassVisit(LandmassType landmass, PuzzleContext context);
    bool IsPuzzleComplete(PuzzleContext context);
    bool IsPuzzleFailed(PuzzleContext context);
    string GetFailureReason();
    string GetCompletionMessage();
}

/// <summary>
/// Context object that holds current puzzle state
/// Passed to validators for decision making
/// </summary>
[System.Serializable]
public class PuzzleContext
{
    [Header("Progress Tracking")]
    public HashSet<string> bridgesCrossed = new HashSet<string>();
    public HashSet<LandmassType> landmassesVisited = new HashSet<LandmassType>();
    public List<LandmassType> visitOrder = new List<LandmassType>();
    public List<string> bridgeOrder = new List<string>();

    [Header("Puzzle Configuration")]
    public int requiredBridges = 7;
    public int requiredLandmasses = 4;
    public PuzzleType puzzleType;

    [Header("State")]
    public bool isActive = false;
    public bool isCompleted = false;
    public bool hasFailed = false;
    public string lastFailureReason = "";

    // Helper methods
    public void Reset()
    {
        bridgesCrossed.Clear();
        landmassesVisited.Clear();
        visitOrder.Clear();
        bridgeOrder.Clear();
        isActive = false;
        isCompleted = false;
        hasFailed = false;
        lastFailureReason = "";
    }

    public void AddBridgeCrossing(string bridgeId)
    {
        bridgesCrossed.Add(bridgeId);
        bridgeOrder.Add(bridgeId);
    }

    public void AddLandmassVisit(LandmassType landmass)
    {
        landmassesVisited.Add(landmass);
        visitOrder.Add(landmass);
    }
}