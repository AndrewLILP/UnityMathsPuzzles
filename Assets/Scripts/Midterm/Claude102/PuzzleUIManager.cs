using UnityEngine;
using TMPro;

/// <summary>
/// Complete PuzzleUIManager with direct transition support (no cutscene)
/// Handles bridge/landmass progress + puzzle transitions with simple text updates
/// </summary>
public class PuzzleUIManager : MonoBehaviour
{
    [Header("Original UI Elements")]
    [SerializeField] private TMP_Text bridgeProgressText;
    [SerializeField] private TMP_Text landmassProgressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private GameObject resetButton;

    [Header("Simple Text UI")]
    [SerializeField] private TMP_Text subtitleText; // Your existing subtitle textbox
    [SerializeField] private TMP_Text mainText; // Your existing main textbox

    [Header("Debug UI")]
    [SerializeField] private bool showDebugPanel = true;
    [SerializeField] private TMP_Text debugText;

    [Header("Puzzle State")]
    [SerializeField] private int currentPuzzleNumber = 1;

    private PuzzleTracker puzzleTracker;
    private Puzzle2Tracker puzzle2Tracker;

    private void Start()
    {
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        puzzle2Tracker = FindFirstObjectByType<Puzzle2Tracker>();

        if (puzzleTracker == null)
        {
            Debug.LogError("PuzzleTracker not found! UI cannot function properly.");
            return;
        }

        // Subscribe to Puzzle 1 events
        puzzleTracker.OnBridgeProgressChanged += UpdateBridgeProgress;
        puzzleTracker.OnLandmassProgressChanged += UpdateLandmassProgress;
        puzzleTracker.OnPuzzleCompleted += OnPuzzleCompleted;
        puzzleTracker.OnPuzzleFailed += OnPuzzleFailed;
        puzzleTracker.OnPuzzleImpossible += OnImpossibleDetected;
        puzzleTracker.OnPuzzleTransitionToPuzzle2 += OnTransitionToPuzzle2;

        // Subscribe to Puzzle 2 events if available
        if (puzzle2Tracker != null)
        {
            puzzle2Tracker.OnLandmassProgressChanged += UpdateLandmassProgress;
            puzzle2Tracker.OnPuzzleCompleted += OnPuzzle2Completed;
            puzzle2Tracker.OnPuzzleFailed += OnPuzzle2Failed;
        }

        // Initialize UI
        InitializeUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from Puzzle 1 events
        if (puzzleTracker != null)
        {
            puzzleTracker.OnBridgeProgressChanged -= UpdateBridgeProgress;
            puzzleTracker.OnLandmassProgressChanged -= UpdateLandmassProgress;
            puzzleTracker.OnPuzzleCompleted -= OnPuzzleCompleted;
            puzzleTracker.OnPuzzleFailed -= OnPuzzleFailed;
            puzzleTracker.OnPuzzleImpossible -= OnImpossibleDetected;
            puzzleTracker.OnPuzzleTransitionToPuzzle2 -= OnTransitionToPuzzle2;
        }

        // Unsubscribe from Puzzle 2 events
        if (puzzle2Tracker != null)
        {
            puzzle2Tracker.OnLandmassProgressChanged -= UpdateLandmassProgress;
            puzzle2Tracker.OnPuzzleCompleted -= OnPuzzle2Completed;
            puzzle2Tracker.OnPuzzleFailed -= OnPuzzle2Failed;
        }
    }

    private void InitializeUI()
    {
        // Original initialization
        UpdateBridgeProgress(0, puzzleTracker.RequiredBridges);
        UpdateLandmassProgress(0, puzzleTracker.RequiredLandmasses);
        UpdateStatusText("Start crossing bridges and visiting landmasses!");

        if (completionPanel != null)
            completionPanel.SetActive(false);

        // Initialize simple text UI
        UpdatePuzzleUI();
    }

    // === ORIGINAL METHODS ===

    private void UpdateBridgeProgress(int current, int total)
    {
        if (bridgeProgressText != null)
            bridgeProgressText.text = $"Bridges: {current}/{total}";
    }

    private void UpdateLandmassProgress(int current, int total)
    {
        if (landmassProgressText != null)
            landmassProgressText.text = $"Landmasses: {current}/{total}";
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }


    private void OnPuzzleCompleted()
    {
        UpdateStatusText("🎉 PUZZLE COMPLETED!");

        if (completionPanel != null)
            completionPanel.SetActive(true);
    }

    private void OnPuzzleFailed()
    {
        UpdateStatusText("❌ Puzzle Failed - Reset to try again");
    }

    public void OnResetButtonPressed()
    {
        if (puzzleTracker != null)
        {
            puzzleTracker.ResetPuzzle();
            UpdateStatusText("Puzzle Reset - Good luck!");

            if (completionPanel != null)
                completionPanel.SetActive(false);
        }
    }

    private void ClearPuzzle1UI()
    {
        // Hide bridge progress (Puzzle 2 doesn't use bridges)
        if (bridgeProgressText != null)
            bridgeProgressText.text = ""; // Clear bridge text

        // Update status for Puzzle 2
        UpdateStatusText("Puzzle 2: Visit all landmasses exactly once!");

    }

    private void Update()
    {
        // Update debug info if enabled
        if (showDebugPanel && debugText != null)
        {
            if (currentPuzzleNumber == 1 && puzzleTracker != null)
            {
                debugText.text = $"Puzzle 1 Debug:\n" +
                               $"Bridges: {puzzleTracker.BridgesCrossed}/{puzzleTracker.RequiredBridges}\n" +
                               $"Landmasses: {puzzleTracker.LandmassesVisited}/{puzzleTracker.RequiredLandmasses}\n" +
                               $"Complete: {puzzleTracker.IsPuzzleComplete}";
            }
            else if (currentPuzzleNumber == 2 && puzzle2Tracker != null)
            {
                debugText.text = $"Puzzle 2 Debug:\n" +
                               $"Landmasses: {puzzle2Tracker.LandmassesVisited}/{puzzle2Tracker.RequiredLandmasses}\n" +
                               $"Complete: {puzzle2Tracker.IsPuzzleComplete}";
            }
        }
    }

    // === PUZZLE UI METHODS ===
    private void UpdatePuzzleUI()
    {
        // Update subtitle based on puzzle number
        if (subtitleText != null)
        {
            switch (currentPuzzleNumber)
            {
                case 1:
                    subtitleText.text = "Puzzle 1: 7 Bridges of Königsberg";
                    break;
                case 2:
                    subtitleText.text = "Puzzle 2: Path";
                    break;
                default:
                    subtitleText.text = $"Puzzle {currentPuzzleNumber}";
                    break;
            }
        }

        // Update main instruction text
        if (mainText != null)
        {
            switch (currentPuzzleNumber)
            {
                case 1:
                    mainText.text = "Cross all 7 bridges exactly once.\n\nTry to visit every landmass and cross every bridge!";
                    break;
                case 2:
                    mainText.text = "🎯 Puzzle 2: Path Challenge\n\nVisit all 4 landmasses exactly once.\n\n⚠️ You cannot return to a landmass you've already visited!\n\nPlan your route carefully!";
                    break;
                default:
                    mainText.text = "Complete the challenge.";
                    break;
            }
        }

        Debug.Log($"Updated UI for Puzzle {currentPuzzleNumber}");
    }

    private void OnImpossibleDetected()
    {
        Debug.Log("Impossible puzzle detected - showing discovery message");

        ShowPuzzleTransitionMessage();
        UpdateStatusText("Mathematical discovery: The puzzle is impossible!");
    }

    public void ShowPuzzleTransitionMessage()
    {
        if (mainText != null)
        {
            // Get current progress
            int bridgesCrossed = puzzleTracker != null ? puzzleTracker.BridgesCrossed : 6;

            // Show discovery message
            mainText.text = $"🎉 Mathematical Discovery!\n\n" +
                           $"You crossed {bridgesCrossed} bridges and visited all landmasses.\n\n" +
                           "Mathematician Leonhard Euler proved in 1736 that crossing all 7 bridges exactly once is impossible.\n\n" +
                           "This problem founded the field of graph theory!\n\n" +
                           "Preparing next challenge...";
        }
    }

    private void OnTransitionToPuzzle2()
    {
        Debug.Log("Transitioning to Puzzle 2");
        StartNextPuzzle();
    }
    
    public void StartNextPuzzle()
    {
        currentPuzzleNumber++;

        // Clear Puzzle 1 specific UI
        ClearPuzzle1UI();

        // Update puzzle UI
        UpdatePuzzleUI();

        // Reset progress displays for new puzzle
        if (currentPuzzleNumber == 2)
        {
            UpdateBridgeProgress(0, 0); // Hide bridge progress for Puzzle 2
            UpdateLandmassProgress(0, 4); // Reset landmass count for Puzzle 2
        }

        Debug.Log($"Started Puzzle {currentPuzzleNumber}");
    }

    // === PUZZLE 2 EVENT HANDLERS ===

    private void OnPuzzle2Completed()
    {
        UpdateStatusText("🎉 PUZZLE 2 COMPLETED! You visited all landmasses without returning!");

        if (mainText != null)
        {
            mainText.text = "🎉 Excellent!\n\n" +
                           "You successfully completed the Path puzzle!\n\n" +
                           "You visited all landmasses exactly once without returning to any.\n\n" +
                           "This demonstrates a fundamental concept in graph theory!";
        }

        if (completionPanel != null)
            completionPanel.SetActive(true);
    }

    private void OnPuzzle2Failed()
    {
        UpdateStatusText("❌ Puzzle 2 Failed - You revisited a landmass!");

        if (mainText != null)
        {
            mainText.text = "⚠️ Puzzle Reset!\n\n" +
                           "You returned to a landmass you already visited.\n\n" +
                           "In a Path puzzle, you can only visit each landmass once.\n\n" +
                           "Try again!";
        }
    }

    // === PUBLIC METHODS FOR EXTERNAL SYSTEMS ===

    public void SetMainText(string text)
    {
        if (mainText != null)
            mainText.text = text;
    }

    public void SetSubtitleText(string text)
    {
        if (subtitleText != null)
            subtitleText.text = text;
    }

    public void SetCurrentPuzzleNumber(int puzzleNumber)
    {
        currentPuzzleNumber = puzzleNumber;
        UpdatePuzzleUI();
    }

    // Getters for other systems
    public int CurrentPuzzleNumber => currentPuzzleNumber;

    // === DEBUG METHODS ===

    [ContextMenu("Test Impossible Message")]
    public void DebugTestImpossibleMessage()
    {
        OnImpossibleDetected();
    }

    [ContextMenu("Test Puzzle 2 Transition")]
    public void DebugTestPuzzle2Transition()
    {
        StartNextPuzzle();
    }
}