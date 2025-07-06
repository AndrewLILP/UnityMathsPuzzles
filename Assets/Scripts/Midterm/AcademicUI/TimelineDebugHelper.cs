using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

/// <summary>
/// Debug helper to identify and fix Timeline freeze issues
/// Add this to a GameObject in your scene during development
/// </summary>
public class TimelineDebugHelper : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode forceFinishKey = KeyCode.Escape;
    [SerializeField] private KeyCode testTransitionKey = KeyCode.F1;
    [SerializeField] private bool enableDebugKeys = true;

    [Header("Debug Info")]
    [SerializeField] private bool showTimelineStatus = true;
    [SerializeField] private float statusUpdateInterval = 1f;

    private TimelineCutsceneManager cutsceneManager;
    private PlayableDirector mainDirector;
    private float lastStatusUpdate;

    private void Start()
    {
        cutsceneManager = TimelineCutsceneManager.Instance;
        if (cutsceneManager == null)
        {
            cutsceneManager = FindFirstObjectByType<TimelineCutsceneManager>();
        }

        mainDirector = FindFirstObjectByType<PlayableDirector>();

        Debug.Log("=== TIMELINE DEBUG HELPER ===");
        Debug.Log($"Press {forceFinishKey} to force finish stuck timeline");
        Debug.Log($"Press {testTransitionKey} to test puzzle transition");
        Debug.Log("==============================");
    }

    private void Update()
    {
        if (!enableDebugKeys) return;

        // Force finish timeline if stuck
        if (Input.GetKeyDown(forceFinishKey))
        {
            ForceFinishTimeline();
        }

        // Test transition
        if (Input.GetKeyDown(testTransitionKey))
        {
            TestPuzzleTransition();
        }

        // Show timeline status
        if (showTimelineStatus && Time.time - lastStatusUpdate > statusUpdateInterval)
        {
            ShowTimelineStatus();
            lastStatusUpdate = Time.time;
        }
    }

    [ContextMenu("Force Finish Timeline")]
    public void ForceFinishTimeline()
    {
        Debug.Log("🚨 FORCE FINISHING TIMELINE");

        // Force finish through cutscene manager
        if (cutsceneManager != null)
        {
            if (cutsceneManager.IsCutscenePlaying)
            {
                cutsceneManager.DebugForceFinishTimeline();
                Debug.Log("✅ Forced timeline finish through TimelineCutsceneManager");
            }
            else
            {
                Debug.Log("⚠️ No cutscene currently playing in TimelineCutsceneManager");
            }
        }

        // Force finish PlayableDirector directly
        if (mainDirector != null && mainDirector.playableGraph.IsValid())
        {
            mainDirector.Stop();
            Debug.Log("✅ Stopped PlayableDirector directly");
        }

        // Force enable player input
        ForceEnablePlayerInput();
    }

    [ContextMenu("Test Puzzle Transition")]
    public void TestPuzzleTransition()
    {
        Debug.Log("🧪 TESTING PUZZLE TRANSITION");

        if (cutsceneManager != null)
        {
            cutsceneManager.DebugForcePuzzle1to2Transition();
        }
        else
        {
            Debug.LogError("TimelineCutsceneManager not found!");
        }
    }

    [ContextMenu("Force Enable Player Input")]
    public void ForceEnablePlayerInput()
    {
        Debug.Log("🎮 FORCE ENABLING PLAYER INPUT");

        // Enable PlayerInput
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
            Debug.Log("✅ Enabled PlayerInput");
        }

        // Enable PlayerMovementBehaviour
        PlayerMovementBehaviour playerMovement = FindFirstObjectByType<PlayerMovementBehaviour>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("✅ Enabled PlayerMovementBehaviour");
        }

        // Enable CameraMovementBehaviour
        CameraMovementBehaviour cameraMovement = FindFirstObjectByType<CameraMovementBehaviour>();
        if (cameraMovement != null)
        {
            cameraMovement.enabled = true;
            Debug.Log("✅ Enabled CameraMovementBehaviour");
        }

        // Also use TimelineCutsceneManager method if available
        if (cutsceneManager != null)
        {
            cutsceneManager.DebugForceEnableInput();
        }
    }

    [ContextMenu("Show Timeline Status")]
    public void ShowTimelineStatus()
    {
        Debug.Log("=== TIMELINE STATUS ===");

        // TimelineCutsceneManager status
        if (cutsceneManager != null)
        {
            Debug.Log($"Cutscene Playing: {cutsceneManager.IsCutscenePlaying}");
            Debug.Log($"Current Cutscene: {cutsceneManager.CurrentCutsceneName}");
            Debug.Log($"Skip Progress: {cutsceneManager.SkipProgress:P1}");
        }
        else
        {
            Debug.Log("❌ TimelineCutsceneManager not found");
        }

        // PlayableDirector status
        if (mainDirector != null)
        {
            Debug.Log($"Director State: {mainDirector.state}");
            Debug.Log($"Director Time: {mainDirector.time:F2}");
            Debug.Log($"Director Duration: {mainDirector.duration:F2}");
            Debug.Log($"Graph Valid: {mainDirector.playableGraph.IsValid()}");
            Debug.Log($"Timeline Asset: {(mainDirector.playableAsset != null ? mainDirector.playableAsset.name : "None")}");
        }
        else
        {
            Debug.Log("❌ PlayableDirector not found");
        }

        // Player input status
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        Debug.Log($"PlayerInput Enabled: {(playerInput != null ? playerInput.enabled : false)}");

        PlayerMovementBehaviour playerMovement = FindFirstObjectByType<PlayerMovementBehaviour>();
        Debug.Log($"PlayerMovement Enabled: {(playerMovement != null ? playerMovement.enabled : false)}");

        CameraMovementBehaviour cameraMovement = FindFirstObjectByType<CameraMovementBehaviour>();
        Debug.Log($"CameraMovement Enabled: {(cameraMovement != null ? cameraMovement.enabled : false)}");

        // Puzzle system status
        PuzzleTracker puzzle1 = FindFirstObjectByType<PuzzleTracker>();
        Puzzle2Tracker puzzle2 = FindFirstObjectByType<Puzzle2Tracker>();

        Debug.Log($"PuzzleTracker Enabled: {(puzzle1 != null ? puzzle1.enabled : false)}");
        Debug.Log($"Puzzle2Tracker Enabled: {(puzzle2 != null ? puzzle2.enabled : false)}");

        Debug.Log("======================");
    }

    [ContextMenu("Validate Timeline Setup")]
    public void ValidateTimelineSetup()
    {
        Debug.Log("=== TIMELINE SETUP VALIDATION ===");

        if (cutsceneManager == null)
        {
            Debug.LogError("❌ TimelineCutsceneManager not found in scene!");
            return;
        }

        // Check Timeline assets
        var konigsbergTimeline = cutsceneManager.GetType()
            .GetField("konigsbergSolutionTimeline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(cutsceneManager) as TimelineAsset;

        if (konigsbergTimeline == null)
        {
            Debug.LogError("❌ konigsbergSolutionTimeline not assigned in TimelineCutsceneManager!");
        }
        else
        {
            Debug.Log($"✅ konigsbergSolutionTimeline assigned: {konigsbergTimeline.name}");
            Debug.Log($"Timeline Duration: {konigsbergTimeline.duration:F2} seconds");
        }

        // Check PlayableDirector
        if (mainDirector == null)
        {
            Debug.LogError("❌ No PlayableDirector found in scene!");
        }
        else
        {
            Debug.Log($"✅ PlayableDirector found: {mainDirector.name}");
        }

        // Check required components
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogWarning("⚠️ PlayerInput not found - player input disable/enable won't work");
        }

        Debug.Log("=================================");
    }

    [ContextMenu("Reset All Systems")]
    public void ResetAllSystems()
    {
        Debug.Log("🔄 RESETTING ALL SYSTEMS");

        // Stop any playing timeline
        ForceFinishTimeline();

        // Reset puzzle systems
        PuzzleTracker puzzle1 = FindFirstObjectByType<PuzzleTracker>();
        if (puzzle1 != null)
        {
            puzzle1.ResetPuzzle();
            puzzle1.enabled = true; // Re-enable Puzzle 1
        }

        Puzzle2Tracker puzzle2 = FindFirstObjectByType<Puzzle2Tracker>();
        if (puzzle2 != null)
        {
            puzzle2.ResetPuzzle2();
            puzzle2.enabled = false; // Disable Puzzle 2
        }

        // Reset all landmasses
        LandmassController[] landmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None);
        foreach (var landmass in landmasses)
        {
            landmass.ResetLandmass();
        }

        Debug.Log("✅ All systems reset");
    }
}