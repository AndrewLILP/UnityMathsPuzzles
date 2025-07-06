using UnityEngine;
using TMPro;

/// <summary>
/// Complete PuzzleUIManager with existing functionality + simple impossible puzzle handling
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

    [Header("Simple Text UI - NEW")]
    [SerializeField] private TMP_Text subtitleText; // Your existing subtitle textbox
    [SerializeField] private TMP_Text mainText; // Your existing main textbox

    [Header("Debug UI")]
    [SerializeField] private bool showDebugPanel = true;
    [SerializeField] private TMP_Text debugText;

    [Header("Puzzle State - NEW")]
    [SerializeField] private int currentPuzzleNumber = 1;

    private PuzzleTracker puzzleTracker;

    private void Start()
    {
        puzzleTracker = FindFirstObjectByType<PuzzleTracker>();

        if (puzzleTracker == null)
        {
            Debug.LogError("PuzzleTracker not found! UI cannot function properly.");
            return;
        }

        // Subscribe to original puzzle events
        puzzleTracker.OnBridgeProgressChanged += UpdateBridgeProgress;
        puzzleTracker.OnLandmassProgressChanged += UpdateLandmassProgress;
        puzzleTracker.OnPuzzleCompleted += OnPuzzleCompleted;
        puzzleTracker.OnPuzzleFailed += OnPuzzleFailed;

        // Subscribe to impossible puzzle events - NEW
        puzzleTracker.OnPuzzleImpossible += OnImpossibleDetected;
        puzzleTracker.OnPuzzleImpossibleCutscene += OnCutsceneStart;

        // Initialize UI
        InitializeUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from original events
        if (puzzleTracker != null)
        {
            puzzleTracker.OnBridgeProgressChanged -= UpdateBridgeProgress;
            puzzleTracker.OnLandmassProgressChanged -= UpdateLandmassProgress;
            puzzleTracker.OnPuzzleCompleted -= OnPuzzleCompleted;
            puzzleTracker.OnPuzzleFailed -= OnPuzzleFailed;

            // Unsubscribe from impossible puzzle events - NEW
            puzzleTracker.OnPuzzleImpossible -= OnImpossibleDetected;
            puzzleTracker.OnPuzzleImpossibleCutscene -= OnCutsceneStart;
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

        // NEW: Initialize simple text UI
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

    private void Update()
    {
        // Update debug info if enabled
        if (showDebugPanel && debugText != null && puzzleTracker != null)
        {
            debugText.text = $"Debug Info:\n" +
                           $"Bridges: {puzzleTracker.BridgesCrossed}/{puzzleTracker.RequiredBridges}\n" +
                           $"Landmasses: {puzzleTracker.LandmassesVisited}/{puzzleTracker.RequiredLandmasses}\n" +
                           $"Complete: {puzzleTracker.IsPuzzleComplete}";
        }
    }

    // === NEW SIMPLE METHODS ===

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
                    subtitleText.text = "Puzzle 2: Graph Theory Basics";
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
                    mainText.text = "Cross as many bridges as possible. Try to cross all 7 bridges exactly once.";
                    break;
                case 2:
                    mainText.text = "Welcome to Puzzle 2! Apply what you learned about graph theory.";
                    break;
                default:
                    mainText.text = "Complete the challenge.";
                    break;
            }
        }
    }

    private void OnImpossibleDetected()
    {
        Debug.Log("Impossible puzzle detected - updating UI");

        if (mainText != null)
        {
            // Get current progress
            int bridgesCrossed = puzzleTracker != null ? puzzleTracker.BridgesCrossed : 6;

            // Show discovery message
            mainText.text = $"You crossed {bridgesCrossed} bridges and visited all landmasses.\n\n" +
                           "Mathematician Leonhard Euler proved in 1736 that crossing all 7 bridges exactly once is impossible.\n\n" +
                           "This problem founded the field of graph theory!";
        }

        // Also update status text
        UpdateStatusText("Mathematical discovery: The puzzle is impossible!");
    }

    private void OnCutsceneStart()
    {
        Debug.Log("Starting transition to next puzzle");

        if (mainText != null)
        {
            mainText.text = "Preparing next challenge...";
        }

        UpdateStatusText("Transitioning to next puzzle...");
    }

    public void StartNextPuzzle()
    {
        currentPuzzleNumber++;
        UpdatePuzzleUI();
        UpdateStatusText("New puzzle ready!");
    }

    // Public methods for Timeline or other systems to call
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
}