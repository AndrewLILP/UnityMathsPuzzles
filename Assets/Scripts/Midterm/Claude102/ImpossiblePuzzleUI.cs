using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// UI Manager for handling the "impossible puzzle" revelation
/// Shows educational message about the Seven Bridges of Königsberg
/// </summary>
public class ImpossiblePuzzleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject impossiblePanel; // Main panel to show
    [SerializeField] private TMP_Text impossibleTitleText; // Title text
    [SerializeField] private TMP_Text impossibleMessageText; // Main message
    [SerializeField] private TMP_Text countdownText; // Countdown to cutscene (optional)

    [Header("Animation Settings")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool showCountdown = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip revelationSound;

    private PuzzleTracker puzzleTracker;
    private bool isShowing = false;

    // Pre-written educational message
    private readonly string impossibleTitle = "The Seven Bridges of Königsberg";
    private readonly string impossibleMessage =
        "Congratulations! You've discovered what mathematician Leonhard Euler proved in 1736.\n\n" +
        "The Seven Bridges of Königsberg cannot be crossed exactly once each in a single journey.\n\n" +
        "This problem led to the birth of graph theory and topology in mathematics.\n\n" +
        "In graph theory terms: A path that crosses every edge exactly once (an Eulerian path) " +
        "only exists if there are exactly 0 or 2 vertices with an odd number of connections.\n\n" +
        "Königsberg had 4 landmasses, each with an odd number of bridges - making the puzzle impossible to solve!";

    private void Start()
    {
        puzzleTracker = FindObjectOfType<PuzzleTracker>();

        if (puzzleTracker == null)
        {
            Debug.LogError("PuzzleTracker not found! ImpossiblePuzzleUI cannot function.");
            return;
        }

        // Subscribe to impossible puzzle events
        puzzleTracker.OnPuzzleImpossible += ShowImpossibleMessage;
        puzzleTracker.OnPuzzleImpossibleCutscene += OnCutsceneTrigger;

        // Hide panel initially
        if (impossiblePanel != null)
            impossiblePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (puzzleTracker != null)
        {
            puzzleTracker.OnPuzzleImpossible -= ShowImpossibleMessage;
            puzzleTracker.OnPuzzleImpossibleCutscene -= OnCutsceneTrigger;
        }
    }

    private void ShowImpossibleMessage()
    {
        if (isShowing) return;
        isShowing = true;

        Debug.Log("📝 Showing impossible puzzle revelation message");

        // Show panel
        if (impossiblePanel != null)
            impossiblePanel.SetActive(true);

        // Play revelation sound
        if (audioSource != null && revelationSound != null)
            audioSource.PlayOneShot(revelationSound);

        // Set up text content
        if (useTypewriterEffect)
        {
            StartCoroutine(TypewriterEffect());
        }
        else
        {
            SetTextImmediate();
        }

        // Start countdown if enabled
        if (showCountdown && countdownText != null)
        {
            StartCoroutine(ShowCountdown());
        }
    }

    private void SetTextImmediate()
    {
        if (impossibleTitleText != null)
            impossibleTitleText.text = impossibleTitle;

        if (impossibleMessageText != null)
            impossibleMessageText.text = impossibleMessage;
    }

    private IEnumerator TypewriterEffect()
    {
        // Set title immediately
        if (impossibleTitleText != null)
            impossibleTitleText.text = impossibleTitle;

        // Typewriter effect for message
        if (impossibleMessageText != null)
        {
            impossibleMessageText.text = "";

            foreach (char c in impossibleMessage)
            {
                impossibleMessageText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }
    }

    private IEnumerator ShowCountdown()
    {
        // Wait a bit before starting countdown
        yield return new WaitForSeconds(1f);

        if (countdownText != null)
        {
            for (int i = 5; i >= 1; i--)
            {
                countdownText.text = $"Continuing in {i}...";
                yield return new WaitForSeconds(1f);
            }
            countdownText.text = "Proceeding to next challenge...";
        }
    }

    private void OnCutsceneTrigger()
    {
        Debug.Log("🎬 Cutscene triggered - hiding impossible message UI");

        // Hide the panel
        if (impossiblePanel != null)
            impossiblePanel.SetActive(false);

        isShowing = false;

        // Here you can trigger your cutscene system
        TriggerCutscene();
    }

    private void TriggerCutscene()
    {
        // Integration point for your cutscene system
        Debug.Log("🎭 Starting cutscene for level transition...");

        // Example integration points:
        // CutsceneManager.Instance?.PlayCutscene("KonigsbergSolution");
        // LevelTransitionManager.Instance?.TransitionToNextLevel();
        // GameManager.Instance?.ChangeState(GameState.Cutscene);

        // For now, just log that we're ready for the next puzzle
        Debug.Log("✅ Ready to transition to next puzzle/level");
    }

    // Public method to hide panel (can be called from button or other systems)
    public void HideImpossibleMessage()
    {
        if (impossiblePanel != null)
            impossiblePanel.SetActive(false);
        isShowing = false;
    }

    // Public method for manual testing
    [ContextMenu("Test Impossible Message")]
    public void TestImpossibleMessage()
    {
        ShowImpossibleMessage();
    }
}