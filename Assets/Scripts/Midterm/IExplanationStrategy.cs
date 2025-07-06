using UnityEngine;

// Strategy Pattern for different explanation types
public interface IExplanationStrategy
{
    void ShowExplanation(string content);
    void HideExplanation();
}