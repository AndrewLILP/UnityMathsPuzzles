using UnityEngine;

/// <summary>
/// Base interface for all puzzle implementations
/// Represents a single puzzle challenge (Konigsberg, Path, etc.)
/// </summary>
public interface IPuzzle
{
    string PuzzleName { get; }
    PuzzleType PuzzleType { get; }
    bool IsActive { get; }
    bool IsCompleted { get; }
    bool HasFailed { get; }

    void StartPuzzle();
    void EndPuzzle();
    void ResetPuzzle();

    // Events for Observer pattern
    System.Action<IPuzzle> OnPuzzleStarted { get; set; }
    System.Action<IPuzzle> OnPuzzleCompleted { get; set; }
    System.Action<IPuzzle, string> OnPuzzleFailed { get; set; }
}

/// <summary>
/// Enum for different puzzle types - easy to extend
/// </summary>
public enum PuzzleType
{
    Konigsberg,
    Path,
    Trail,      // Future
    Circuit,    // Future
    Cycle       // Future
}