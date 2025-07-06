using UnityEngine;

// Observer Pattern for level events
public interface ILevelProgressObserver
{
    void OnLevelProgressChanged(float progress);
    void OnLevelCompleted(bool success);
    void OnPuzzleHintNeeded(string hintText);
}

// Command Pattern for level actions
public interface ILevelCommand
{
    void Execute();
    bool CanExecute();
    void Undo();
}