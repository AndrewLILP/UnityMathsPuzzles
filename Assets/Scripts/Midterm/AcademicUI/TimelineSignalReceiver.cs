using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

/// <summary>
/// Timeline Signal Receiver for educational puzzle cutscenes
/// Handles Timeline Signal Emitters to trigger events during playback
/// 
/// NOTE: Updated for current Unity API - uses FindFirstObjectByType
/// </summary>
public class TimelineSignalReceiver : MonoBehaviour, INotificationReceiver
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Reference to cutscene manager
    private TimelineCutsceneManager cutsceneManager;

    private void Start()
    {
        // Get reference to cutscene manager
        cutsceneManager = TimelineCutsceneManager.Instance;

        if (cutsceneManager == null)
        {
            cutsceneManager = FindFirstObjectByType<TimelineCutsceneManager>();
        }

        if (cutsceneManager == null)
        {
            Debug.LogError("TimelineSignalReceiver: Could not find TimelineCutsceneManager!");
        }

        if (showDebugLogs)
            Debug.Log("Timeline Signal Receiver initialized");
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // Handle Timeline Signal Emitters
        if (notification is SignalEmitter signalEmitter)
        {
            ProcessSignal(signalEmitter.asset.name, context);
        }
        // Handle custom markers if using them
        else if (notification is Marker marker)
        {
            ProcessMarker(marker, context);
        }
    }

    private void ProcessSignal(string signalName, object context)
    {
        if (showDebugLogs)
            Debug.Log($"📡 Timeline Signal Received: {signalName}");

        if (cutsceneManager == null)
        {
            Debug.LogWarning("Cannot process signal - TimelineCutsceneManager not found");
            return;
        }

        // Process different signal types
        switch (signalName.ToLower())
        {
            // UI Signals
            case "showimpossibleui":
            case "show_impossible_ui":
                cutsceneManager.OnTimelineSignal_ShowUI("impossible");
                break;

            case "hideimpossibleui":
            case "hide_impossible_ui":
                cutsceneManager.OnTimelineSignal_ShowUI("hide");
                break;

            case "showdiscoveryui":
            case "show_discovery_ui":
                cutsceneManager.OnTimelineSignal_ShowUI("discovery");
                break;

            // Level Transition Signals
            case "startnextlevel":
            case "start_next_level":
                OnSignal_StartNextLevel();
                break;

            case "endcutscene":
            case "end_cutscene":
                OnSignal_EndCutscene();
                break;

            // Audio Signals
            case "playsound":
            case "play_sound":
                OnSignal_PlaySound("discovery");
                break;

            case "playmusic":
            case "play_music":
                OnSignal_PlayMusic("contemplative");
                break;

            case "stopmusic":
            case "stop_music":
                OnSignal_StopMusic();
                break;

            // Camera Signals
            case "focusplayer":
            case "focus_player":
                OnSignal_FocusPlayer();
                break;

            case "showoverview":
            case "show_overview":
                OnSignal_ShowOverview();
                break;

            // Educational Content Signals
            case "shownewconcept":
            case "show_new_concept":
                OnSignal_ShowNewConcept();
                break;

            case "explainmathematics":
            case "explain_mathematics":
                OnSignal_ExplainMathematics();
                break;

            // Puzzle State Signals
            case "resetpuzzle":
            case "reset_puzzle":
                OnSignal_ResetPuzzle();
                break;

            case "completepuzzle":
            case "complete_puzzle":
                OnSignal_CompletePuzzle();
                break;

            default:
                if (showDebugLogs)
                    Debug.LogWarning($"Unknown timeline signal: {signalName}");
                break;
        }
    }

    private void ProcessMarker(Marker marker, object context)
    {
        if (showDebugLogs)
            Debug.Log($"📍 Timeline Marker: {marker.GetType().Name}");

        // Handle different marker types if you're using Timeline Markers
        // This is for more advanced Timeline usage
    }

    // Signal Handler Methods
    private void OnSignal_StartNextLevel()
    {
        if (showDebugLogs)
            Debug.Log("🚀 Signal: Starting next level");

        cutsceneManager?.OnTimelineSignal_NextLevel();
    }

    private void OnSignal_EndCutscene()
    {
        if (showDebugLogs)
            Debug.Log("🎬 Signal: Ending cutscene");

        // Force end current cutscene
        if (cutsceneManager != null && cutsceneManager.IsCutscenePlaying)
        {
            cutsceneManager.SkipCurrentCutscene();
        }
    }

    private void OnSignal_PlaySound(string soundName)
    {
        if (showDebugLogs)
            Debug.Log($"🔊 Signal: Playing sound - {soundName}");

        cutsceneManager?.OnTimelineSignal_PlaySound(soundName);
    }

    private void OnSignal_PlayMusic(string musicName)
    {
        if (showDebugLogs)
            Debug.Log($"🎵 Signal: Playing music - {musicName} (Audio disabled for now)");

        // Audio functionality removed for initial implementation  
        // TODO: Add audio support later when needed
    }

    private void OnSignal_StopMusic()
    {
        if (showDebugLogs)
            Debug.Log("🔇 Signal: Stopping music (Audio disabled for now)");

        // Audio functionality removed for initial implementation
        // TODO: Add audio support later when needed
    }

    private void OnSignal_FocusPlayer()
    {
        if (showDebugLogs)
            Debug.Log("📷 Signal: Focus on player");

        // This could trigger camera behavior if needed
        // For now, just log - the camera animation is handled by Timeline
    }

    private void OnSignal_ShowOverview()
    {
        if (showDebugLogs)
            Debug.Log("📷 Signal: Show overview");

        // Camera overview behavior
    }

    private void OnSignal_ShowNewConcept()
    {
        if (showDebugLogs)
            Debug.Log("💡 Signal: Show new concept");

        // Trigger educational UI for new concepts
        cutsceneManager?.OnTimelineSignal_ShowUI("newconcept");
    }

    private void OnSignal_ExplainMathematics()
    {
        if (showDebugLogs)
            Debug.Log("📚 Signal: Explain mathematics");

        // Show mathematical explanation UI
        cutsceneManager?.OnTimelineSignal_ShowUI("mathematics");
    }

    private void OnSignal_ResetPuzzle()
    {
        if (showDebugLogs)
            Debug.Log("🔄 Signal: Reset puzzle");

        // Reset current puzzle state
        PuzzleTracker puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        if (puzzleTracker != null)
        {
            puzzleTracker.ResetPuzzle();
        }
    }

    private void OnSignal_CompletePuzzle()
    {
        if (showDebugLogs)
            Debug.Log("✅ Signal: Complete puzzle");

        // Force complete current puzzle
        // This might be used for tutorial or testing scenarios
    }

    // Public methods for manual testing
    [ContextMenu("Test Show UI Signal")]
    public void TestShowUISignal()
    {
        ProcessSignal("ShowImpossibleUI", null);
    }

    [ContextMenu("Test Next Level Signal")]
    public void TestNextLevelSignal()
    {
        ProcessSignal("StartNextLevel", null);
    }

    [ContextMenu("Test Play Sound Signal")]
    public void TestPlaySoundSignal()
    {
        ProcessSignal("PlaySound", null);
    }
}