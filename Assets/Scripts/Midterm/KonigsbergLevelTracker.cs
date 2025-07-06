using UnityEngine;
using System.Collections.Generic;

// Implementation that tracks Königsberg puzzle progress
public class KonigsbergLevelTracker : MonoBehaviour, ILevelProgressObserver, IBridgeObserver, IKeycardObserver
{
    [Header("Level Configuration")]
    [SerializeField] private int totalBridgesRequired = 7;
    [SerializeField] private float timeLimit = 300f; // 5 minutes
    [SerializeField] private bool enableHints = true;

    [Header("Progress Tracking")]
    [SerializeField] private int bridgesCrossed = 0;
    [SerializeField] private float currentProgress = 0f;
    [SerializeField] private bool levelCompleted = false;

    private List<ILevelProgressObserver> progressObservers = new List<ILevelProgressObserver>();
    private List<string> crossedBridgeIds = new List<string>();
    private float levelStartTime;
    private bool timerStarted = false;

    // Services
    private IKeycardService keycardService;

    private void Start()
    {
        InitializeLevelTracker();
    }

    private void Update()
    {
        if (timerStarted && !levelCompleted)
        {
            CheckTimeLimit();
        }
    }

    private void OnDestroy()
    {
        UnregisterFromServices();
    }

    #region Initialization
    private void InitializeLevelTracker()
    {
        // Get keycard service using Service Locator pattern
        keycardService = KeycardServiceManager.GetService();
        if (keycardService != null)
        {
            keycardService.RegisterObserver(this);
        }

        // Find and register with all bridges
        RegisterWithBridges();

        levelStartTime = Time.time;
        Debug.Log($"Königsberg Level Tracker initialized. Target: {totalBridgesRequired} bridges");

        // Show initial hint
        if (enableHints)
        {
            OnPuzzleHintNeeded("Welcome to the Seven Bridges of Königsberg! Collect keycards to unlock bridges and try to cross them all.");
        }
    }

    private void RegisterWithBridges()
    {
        Bridge[] bridges = FindObjectsOfType<Bridge>();
        foreach (var bridge in bridges)
        {
            bridge.RegisterObserver(this);
        }
        Debug.Log($"Registered with {bridges.Length} bridges");
    }

    private void UnregisterFromServices()
    {
        // Unregister observers to prevent memory leaks
        if (keycardService != null)
        {
            keycardService.UnregisterObserver(this);
        }

        // Unregister from bridges
        Bridge[] bridges = FindObjectsOfType<Bridge>();
        foreach (var bridge in bridges)
        {
            bridge.UnregisterObserver(this);
        }
    }
    #endregion

    #region ILevelProgressObserver Implementation
    public void OnLevelProgressChanged(float progress)
    {
        currentProgress = progress;
        Debug.Log($"Level progress changed: {progress:P1}");

        // Notify other observers
        foreach (var observer in progressObservers)
        {
            observer.OnLevelProgressChanged(progress);
        }
    }

    public void OnLevelCompleted(bool success)
    {
        if (levelCompleted) return;

        levelCompleted = true;
        float completionTime = Time.time - levelStartTime;

        Debug.Log($"Level completed! Success: {success}, Time: {completionTime:F1}s");

        // Notify observers
        foreach (var observer in progressObservers)
        {
            observer.OnLevelCompleted(success);
        }

        // Show completion message
        if (success)
        {
            OnPuzzleHintNeeded("Incredible! You've achieved the impossible! You've crossed all bridges, defying Euler's mathematical proof.");
        }
        else
        {
            OnPuzzleHintNeeded("Time's up! As Euler proved, it's mathematically impossible to cross all bridges exactly once. You've experienced the power of mathematical proof!");
        }
    }

    public void OnPuzzleHintNeeded(string hintText)
    {
        Debug.Log($"Puzzle hint: {hintText}");

        // Notify observers (like UI managers)
        foreach (var observer in progressObservers)
        {
            observer.OnPuzzleHintNeeded(hintText);
        }

        // Direct UI integration if needed
        MathConceptStrategy mathUI = FindObjectOfType<MathConceptStrategy>();
        if (mathUI != null && hintText.Contains("mathematical"))
        {
            // Could trigger math explanation for math-related hints
        }
    }
    #endregion

    #region IBridgeObserver Implementation
    public void OnBridgeCrossed(Bridge bridge)
    {
        if (!timerStarted)
        {
            timerStarted = true;
            levelStartTime = Time.time;
            Debug.Log("Timer started - first bridge crossed!");
        }

        // Only count unique bridges
        if (!crossedBridgeIds.Contains(bridge.BridgeId))
        {
            bridgesCrossed++;
            crossedBridgeIds.Add(bridge.BridgeId);

            Debug.Log($"Bridge crossed: {bridge.BridgeId} ({bridgesCrossed}/{totalBridgesRequired})");

            // Update progress
            float newProgress = (float)bridgesCrossed / totalBridgesRequired;
            OnLevelProgressChanged(newProgress);

            // Check for completion
            if (bridgesCrossed >= totalBridgesRequired)
            {
                OnLevelCompleted(true);
            }
            else if (enableHints)
            {
                ProvideContextualHints();
            }
        }
    }

    public void OnBridgeStateChanged(Bridge bridge, BridgeState newState)
    {
        Debug.Log($"Bridge {bridge.BridgeId} state changed to {newState}");

        if (newState == BridgeState.Available && enableHints)
        {
            OnPuzzleHintNeeded($"Bridge {bridge.BridgeId} is now available! You can cross it.");
        }
    }
    #endregion

    #region IKeycardObserver Implementation
    public void OnKeycardCollected(string keycardId)
    {
        Debug.Log($"Keycard collected: {keycardId}");

        if (enableHints)
        {
            OnPuzzleHintNeeded("Keycard collected! This should unlock some bridges. Look for bridges that change from red to green.");
        }
    }

    public void OnKeycardUsed(string keycardId)
    {
        Debug.Log($"Keycard used: {keycardId}");
    }
    #endregion

    #region Progress Management
    private void ProvideContextualHints()
    {
        if (bridgesCrossed == 1)
        {
            OnPuzzleHintNeeded("Great start! Remember, you need to cross ALL bridges exactly once. This is the famous Königsberg problem!");
        }
        else if (bridgesCrossed == totalBridgesRequired / 2)
        {
            OnPuzzleHintNeeded("You're halfway there! But according to Euler's theorem, what you're attempting is mathematically impossible...");
        }
        else if (bridgesCrossed == totalBridgesRequired - 1)
        {
            OnPuzzleHintNeeded("One bridge left! If you succeed, you'll have done the impossible!");
        }
    }

    private void CheckTimeLimit()
    {
        float elapsedTime = Time.time - levelStartTime;

        if (elapsedTime >= timeLimit)
        {
            OnLevelCompleted(false); // Time's up - failure
        }
        else if (enableHints)
        {
            CheckTimeWarnings(timeLimit - elapsedTime);
        }
    }

    private void CheckTimeWarnings(float remainingTime)
    {
        // Time-based hints (only show once per threshold)
        if (remainingTime <= 60f && remainingTime > 59f)
        {
            OnPuzzleHintNeeded("One minute remaining! Remember, Euler proved this puzzle is impossible - but try your best!");
        }
        else if (remainingTime <= 30f && remainingTime > 29f)
        {
            OnPuzzleHintNeeded("30 seconds left! The mathematics says you can't succeed, but maybe you'll prove Euler wrong?");
        }
    }
    #endregion

    #region Public API
    public void RegisterProgressObserver(ILevelProgressObserver observer)
    {
        if (!progressObservers.Contains(observer))
        {
            progressObservers.Add(observer);
        }
    }

    public void UnregisterProgressObserver(ILevelProgressObserver observer)
    {
        if (progressObservers.Contains(observer))
        {
            progressObservers.Remove(observer);
        }
    }

    public float GetCurrentProgress()
    {
        return currentProgress;
    }

    public int GetBridgesCrossed()
    {
        return bridgesCrossed;
    }

    public float GetRemainingTime()
    {
        if (!timerStarted) return timeLimit;
        return Mathf.Max(0, timeLimit - (Time.time - levelStartTime));
    }

    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }
    #endregion

    #region Context Menu Testing
    [ContextMenu("Test Level Completion")]
    private void TestLevelCompletion()
    {
        if (Application.isPlaying)
        {
            OnLevelCompleted(true);
        }
    }

    [ContextMenu("Test Hint")]
    private void TestHint()
    {
        if (Application.isPlaying)
        {
            OnPuzzleHintNeeded("This is a test hint!");
        }
    }

    [ContextMenu("Debug Level State")]
    private void DebugLevelState()
    {
        Debug.Log($"=== Level State ===");
        Debug.Log($"Bridges Crossed: {bridgesCrossed}/{totalBridgesRequired}");
        Debug.Log($"Progress: {currentProgress:P1}");
        Debug.Log($"Remaining Time: {GetRemainingTime():F1}s");
        Debug.Log($"Level Completed: {levelCompleted}");
        Debug.Log($"Crossed Bridge IDs: [{string.Join(", ", crossedBridgeIds)}]");
    }
    #endregion
}