using UnityEngine;

/// <summary>
/// State pattern interface for puzzle states
/// Each state handles puzzle events differently
/// </summary>
public interface IPuzzleState
{
    void OnEnter();
    void OnExit();
    void OnUpdate();

    // Puzzle-specific events
    void OnBridgeCrossed(string bridgeId);
    void OnLandmassVisited(LandmassType landmass);
    void OnPuzzleReset();

    // State identification
    string StateName { get; }
}