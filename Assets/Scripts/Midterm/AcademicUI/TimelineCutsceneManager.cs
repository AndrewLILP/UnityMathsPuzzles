using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

/// <summary>
/// Timeline-based cutscene manager with CAMERA CONTROL FIX
/// Properly handles camera restoration after Timeline cutscenes
/// </summary>
public class TimelineCutsceneManager : MonoBehaviour
{
    [Header("Timeline Assets")]
    [SerializeField] private TimelineAsset konigsbergSolutionTimeline;
    [SerializeField] private TimelineAsset nextPuzzleIntroTimeline;
    [SerializeField] private TimelineAsset puzzleTransitionTimeline;

    [Header("Playable Directors")]
    [SerializeField] private PlayableDirector mainDirector;
    [SerializeField] private PlayableDirector uiDirector; // For UI-only sequences

    [Header("Integration Settings")]
    [SerializeField] private bool allowSkipping = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float skipHoldTime = 1f;
    [SerializeField] private float timelineTimeout = 10f;

    [Header("Camera Control - NEW")]
    [SerializeField] private Camera mainCamera; // Assign your main camera
    [SerializeField] private Camera playerCamera; // If different from main camera
    [SerializeField] private bool forceResetCameraTransform = true;
    [SerializeField] private bool debugCameraIssues = true;

    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;

    // Singleton pattern for easy access
    public static TimelineCutsceneManager Instance { get; private set; }

    // Events
    public System.Action<string> OnCutsceneStarted;
    public System.Action<string> OnCutsceneEnded;
    public System.Action OnCutsceneSkipped;

    // Internal state
    private bool isCutscenePlaying = false;
    private float skipTimer = 0f;
    private float timelineTimer = 0f;
    private string currentCutsceneName = "";
    private bool inputWasDisabled = false;

    // NEW: Camera state tracking
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;
    private bool cameraStateStored = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeTimelineSystem();
        SubscribeToPuzzleEvents();
        StoreCameraOriginalState(); // NEW: Store original camera state
    }

    // NEW: Store the original camera state before any Timeline manipulation
    private void StoreCameraOriginalState()
    {
        Camera targetCamera = GetMainCamera();
        if (targetCamera != null)
        {
            originalCameraPosition = targetCamera.transform.position;
            originalCameraRotation = targetCamera.transform.rotation;
            originalCameraParent = targetCamera.transform.parent;
            cameraStateStored = true;

            if (debugCameraIssues)
                Debug.Log($"📷 Stored original camera state: {targetCamera.name} at {originalCameraPosition}");
        }
        else
        {
            Debug.LogWarning("No main camera found to store original state!");
        }
    }

    // NEW: Get the main camera (try multiple methods)
    private Camera GetMainCamera()
    {
        // Try assigned cameras first
        if (mainCamera != null) return mainCamera;
        if (playerCamera != null) return playerCamera;

        // Try to find main camera by tag
        Camera foundCamera = Camera.main;
        if (foundCamera != null) return foundCamera;

        // Try to find any active camera
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in allCameras)
        {
            if (cam.isActiveAndEnabled && cam.gameObject.activeInHierarchy)
                return cam;
        }

        return null;
    }

    private void InitializeTimelineSystem()
    {
        // Get PlayableDirector if not assigned
        if (mainDirector == null)
            mainDirector = GetComponent<PlayableDirector>();

        if (mainDirector == null)
        {
            Debug.LogError("TimelineCutsceneManager: No PlayableDirector found! Please assign one.");
            return;
        }

        // Subscribe to director events
        mainDirector.stopped += OnTimelineFinished;

        if (uiDirector != null)
            uiDirector.stopped += OnUITimelineFinished;

        Debug.Log("Timeline cutscene system initialized");
    }

    private void SubscribeToPuzzleEvents()
    {
        // Subscribe to puzzle tracker events
        PuzzleTracker puzzleTracker = FindFirstObjectByType<PuzzleTracker>();
        if (puzzleTracker != null)
        {
            puzzleTracker.OnPuzzleImpossibleCutscene += PlayKonigsbergSolutionCutscene;
            Debug.Log("Timeline manager subscribed to puzzle events");
        }
    }

    private void Update()
    {
        HandleSkipInput();

        // Handle timeline timeout
        if (isCutscenePlaying)
        {
            timelineTimer += Time.unscaledDeltaTime;

            if (timelineTimer >= timelineTimeout)
            {
                Debug.LogWarning($"Timeline timeout reached for {currentCutsceneName}! Force finishing...");
                ForceFinishTimeline();
            }
        }
    }

    private void HandleSkipInput()
    {
        if (!isCutscenePlaying || !allowSkipping) return;

        if (Input.GetKey(skipKey))
        {
            skipTimer += Time.unscaledDeltaTime;

            if (skipTimer >= skipHoldTime)
            {
                SkipCurrentCutscene();
            }
        }
        else
        {
            skipTimer = 0f;
        }
    }

    public void PlayKonigsbergSolutionCutscene()
    {
        if (konigsbergSolutionTimeline != null)
        {
            PlayCutscene(konigsbergSolutionTimeline, "KonigsbergSolution");
        }
        else
        {
            Debug.LogWarning("Königsberg solution timeline not assigned! Using direct transition.");
            HandleKonigsbergToPath();
        }
    }

    public void PlayCutscene(TimelineAsset timeline, string cutsceneName)
    {
        if (timeline == null || mainDirector == null)
        {
            Debug.LogError($"Cannot play cutscene {cutsceneName}: Missing timeline or director");
            return;
        }

        if (isCutscenePlaying)
        {
            Debug.LogWarning($"Cutscene {cutsceneName} requested while {currentCutsceneName} is already playing. Stopping current cutscene.");
            ForceFinishTimeline();
        }

        isCutscenePlaying = true;
        currentCutsceneName = cutsceneName;
        skipTimer = 0f;
        timelineTimer = 0f;

        Debug.Log($"🎬 Starting Timeline cutscene: {cutsceneName}");

        // Store current camera state before cutscene
        StoreCameraState(); // NEW

        // Disable player input during cutscene
        SetPlayerInputEnabled(false);
        inputWasDisabled = true;

        // Set timeline and play
        mainDirector.playableAsset = timeline;
        mainDirector.Play();

        OnCutsceneStarted?.Invoke(cutsceneName);
    }

    // NEW: Store camera state before cutscene starts
    private void StoreCameraState()
    {
        Camera targetCamera = GetMainCamera();
        if (targetCamera != null && !cameraStateStored)
        {
            originalCameraPosition = targetCamera.transform.position;
            originalCameraRotation = targetCamera.transform.rotation;
            originalCameraParent = targetCamera.transform.parent;
            cameraStateStored = true;

            if (debugCameraIssues)
                Debug.Log($"📷 Stored camera state before cutscene: {targetCamera.name}");
        }
    }

    public void SkipCurrentCutscene()
    {
        if (!isCutscenePlaying) return;

        Debug.Log($"⏭️ Skipping cutscene: {currentCutsceneName}");
        ForceFinishTimeline();
    }

    private void ForceFinishTimeline()
    {
        if (mainDirector != null && mainDirector.playableGraph.IsValid())
        {
            mainDirector.Stop();
        }

        OnCutsceneSkipped?.Invoke();
        OnCutsceneCompleted(currentCutsceneName);
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        if (director == mainDirector && isCutscenePlaying)
        {
            Debug.Log($"🎬 Timeline cutscene finished: {currentCutsceneName}");
            OnCutsceneCompleted(currentCutsceneName);
        }
    }

    private void OnUITimelineFinished(PlayableDirector director)
    {
        if (director == uiDirector)
        {
            Debug.Log("UI Timeline sequence finished");
        }
    }

    private void OnCutsceneCompleted(string cutsceneName)
    {
        isCutscenePlaying = false;
        skipTimer = 0f;
        timelineTimer = 0f;

        // NEW: Restore camera control FIRST, before re-enabling input
        RestoreCameraControl();

        // Re-enable player input if we disabled it
        if (inputWasDisabled)
        {
            SetPlayerInputEnabled(true);
            inputWasDisabled = false;
        }

        // NEW: Additional camera cleanup
        ForceCameraCleanup();

        OnCutsceneEnded?.Invoke(cutsceneName);

        // Handle specific cutscene completions
        HandleCutsceneCompletion(cutsceneName);

        currentCutsceneName = "";
    }

    // NEW: Restore camera control after cutscene
    private void RestoreCameraControl()
    {
        if (debugCameraIssues)
            Debug.Log("📷 Restoring camera control...");

        // STEP 1: Find and disable cutscene cameras
        DeactivateCutsceneCameras();

        // STEP 2: Find and activate main/player camera
        Camera playerCamera = FindPlayerCamera();
        if (playerCamera == null)
        {
            Debug.LogError("❌ Cannot find player camera to restore!");
            return;
        }

        // STEP 3: Activate the player camera
        ActivatePlayerCamera(playerCamera);

        // STEP 4: Clear Timeline bindings
        ClearTimelineBindings();

        // STEP 5: Reset camera transform if needed
        if (forceResetCameraTransform && cameraStateStored)
        {
            playerCamera.transform.position = originalCameraPosition;
            playerCamera.transform.rotation = originalCameraRotation;
            playerCamera.transform.SetParent(originalCameraParent);

            if (debugCameraIssues)
                Debug.Log($"📷 Reset camera transform to original state: {originalCameraPosition}");
        }

        if (debugCameraIssues)
            Debug.Log("✅ Camera control restoration complete");
    }

    // NEW: Find and disable all cutscene cameras
    private void DeactivateCutsceneCameras()
    {
        // List of common cutscene camera names
        string[] cutsceneCameraNames = {
            "Cam_EndofPuzzle",
            "CutsceneCamera",
            "TimelineCamera",
            "CinematicCamera",
            "EndCamera"
        };

        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in allCameras)
        {
            // Check if this is a cutscene camera by name
            foreach (string cutsceneName in cutsceneCameraNames)
            {
                if (cam.name.Contains(cutsceneName) && cam.gameObject.activeInHierarchy)
                {
                    cam.gameObject.SetActive(false);
                    if (debugCameraIssues)
                        Debug.Log($"📷 Deactivated cutscene camera: {cam.name}");
                }
            }

            // Also check if it's NOT a child of player (likely a cutscene camera)
            if (cam.gameObject.activeInHierarchy && cam.enabled &&
                cam.transform.GetComponentInParent<PlayerMovementBehaviour>() == null &&
                cam.name != "Main Camera")
            {
                // This might be an active cutscene camera
                if (debugCameraIssues)
                    Debug.Log($"📷 Found potentially active cutscene camera: {cam.name}");

                // Disable it to be safe
                cam.gameObject.SetActive(false);
                if (debugCameraIssues)
                    Debug.Log($"📷 Deactivated potential cutscene camera: {cam.name}");
            }
        }
    }

    // NEW: Find the player camera (child of player)
    private Camera FindPlayerCamera()
    {
        // Method 1: Look for camera that's a child of an object with player components
        PlayerMovementBehaviour playerMovement = FindFirstObjectByType<PlayerMovementBehaviour>();
        if (playerMovement != null)
        {
            Camera playerCam = playerMovement.GetComponentInChildren<Camera>();
            if (playerCam != null)
            {
                if (debugCameraIssues)
                    Debug.Log($"📷 Found player camera: {playerCam.name} (child of {playerMovement.name})");
                return playerCam;
            }
        }

        // Method 2: Look for camera with MainCamera tag
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform.GetComponentInParent<PlayerMovementBehaviour>() != null)
        {
            if (debugCameraIssues)
                Debug.Log($"📷 Found main camera under player: {mainCam.name}");
            return mainCam;
        }

        // Method 3: Look for any camera named "Main Camera" that's a child of something
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            if (cam.name.Contains("Main") && cam.transform.parent != null)
            {
                if (debugCameraIssues)
                    Debug.Log($"📷 Found potential player camera: {cam.name}");
                return cam;
            }
        }

        // Method 4: Use the assigned main camera
        if (mainCamera != null)
        {
            if (debugCameraIssues)
                Debug.Log($"📷 Using assigned main camera: {mainCamera.name}");
            return mainCamera;
        }

        Debug.LogError("❌ Could not find player camera!");
        return null;
    }

    // NEW: Activate the player camera
    private void ActivatePlayerCamera(Camera playerCamera)
    {
        // Activate the camera GameObject
        if (!playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera.gameObject.SetActive(true);
            if (debugCameraIssues)
                Debug.Log($"📷 Activated player camera GameObject: {playerCamera.name}");
        }

        // Enable the camera component
        if (!playerCamera.enabled)
        {
            playerCamera.enabled = true;
            if (debugCameraIssues)
                Debug.Log($"📷 Enabled player camera component: {playerCamera.name}");
        }

        // Set as main camera
        if (playerCamera.tag != "MainCamera")
        {
            playerCamera.tag = "MainCamera";
            if (debugCameraIssues)
                Debug.Log($"📷 Set player camera tag to MainCamera: {playerCamera.name}");
        }

        // Activate parent objects if needed (in case player GameObject was disabled)
        Transform parent = playerCamera.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeInHierarchy)
            {
                parent.gameObject.SetActive(true);
                if (debugCameraIssues)
                    Debug.Log($"📷 Activated parent GameObject: {parent.name}");
            }
            parent = parent.parent;
        }
    }

    // NEW: Clear Timeline bindings
    private void ClearTimelineBindings()
    {
        if (mainDirector != null)
        {
            var playableAsset = mainDirector.playableAsset as TimelineAsset;
            if (playableAsset != null)
            {
                foreach (var track in playableAsset.GetOutputTracks())
                {
                    mainDirector.SetGenericBinding(track, null);
                }
                if (debugCameraIssues)
                    Debug.Log("📷 Cleared Timeline track bindings");
            }
        }
    }

    // NEW: Force camera cleanup - additional safety measures
    private void ForceCameraCleanup()
    {
        // Wait one frame, then do additional cleanup
        StartCoroutine(DelayedCameraCleanup());
    }

    private System.Collections.IEnumerator DelayedCameraCleanup()
    {
        yield return null; // Wait one frame

        // Find player camera again
        Camera playerCamera = FindPlayerCamera();
        if (playerCamera != null)
        {
            // Double-check player camera is working
            if (!playerCamera.enabled || !playerCamera.gameObject.activeInHierarchy)
            {
                ActivatePlayerCamera(playerCamera);
                Debug.Log("📷 Delayed cleanup: Re-activated player camera");
            }

            // Ensure CameraMovementBehaviour is enabled
            CameraMovementBehaviour cameraMovement = playerCamera.GetComponent<CameraMovementBehaviour>();
            if (cameraMovement != null && !cameraMovement.enabled)
            {
                cameraMovement.enabled = true;
                Debug.Log("📷 Delayed cleanup: Re-enabled CameraMovementBehaviour");
            }
        }

        // Double-check no cutscene cameras are still active
        DeactivateCutsceneCameras();

        if (debugCameraIssues)
            Debug.Log("✅ Delayed camera cleanup complete");
    }

    private void HandleCutsceneCompletion(string cutsceneName)
    {
        switch (cutsceneName)
        {
            case "KonigsbergSolution":
                HandleKonigsbergToPath();
                break;

            case "Puzzle1to2Transition":
            case "GraphTheoryTerminology":
                HandleGenericTransition();
                break;

            default:
                Debug.Log($"Cutscene completed: {cutsceneName}");
                break;
        }
    }

    private void HandleKonigsbergToPath()
    {
        Debug.Log("🔄 Transitioning from Königsberg to Path puzzle...");

        // STEP 1: Disable Puzzle 1 system
        PuzzleTracker puzzle1 = FindFirstObjectByType<PuzzleTracker>();
        if (puzzle1 != null)
        {
            puzzle1.enabled = false;
            Debug.Log("✅ Disabled PuzzleTracker (Puzzle 1)");
        }

        // STEP 2: Enable Puzzle 2 system
        Puzzle2Tracker puzzle2 = FindFirstObjectByType<Puzzle2Tracker>();
        if (puzzle2 == null)
        {
            GameObject puzzle2Object = new GameObject("Puzzle2Tracker");
            puzzle2 = puzzle2Object.AddComponent<Puzzle2Tracker>();
            Debug.Log("✅ Created new Puzzle2Tracker");
        }
        else
        {
            puzzle2.enabled = true;
            Debug.Log("✅ Enabled existing Puzzle2Tracker");
        }

        // STEP 3: Switch LandmassControllers to use Puzzle 2
        SwitchLandmassesToPuzzle2();

        // STEP 4: Update UI for Puzzle 2
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.StartNextPuzzle();
        }

        // STEP 5: Force end current level to trigger GameManager level progression
        if (levelManager != null)
        {
            levelManager.ForceEndLevel();
        }

        Debug.Log("✅ Puzzle 1 → 2 transition complete");
    }

    private void SwitchLandmassesToPuzzle2()
    {
        LandmassController[] landmasses = FindObjectsByType<LandmassController>(FindObjectsSortMode.None);
        foreach (var landmass in landmasses)
        {
            landmass.ResetLandmass();
        }
        Debug.Log($"✅ Switched {landmasses.Length} landmasses to Puzzle 2 mode");
    }

    private void HandleGenericTransition()
    {
        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.StartNextPuzzle();
        }

        if (levelManager != null)
        {
            levelManager.ForceEndLevel();
        }
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        if (debugCameraIssues)
            Debug.Log($"🎮 Setting player input enabled: {enabled}");

        // Disable/enable player input during cutscenes
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = enabled;
        }

        // Also disable/enable player movement
        PlayerMovementBehaviour playerMovement = FindFirstObjectByType<PlayerMovementBehaviour>();
        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        // Disable camera movement
        CameraMovementBehaviour cameraMovement = FindFirstObjectByType<CameraMovementBehaviour>();
        if (cameraMovement != null)
        {
            cameraMovement.enabled = enabled;
            if (debugCameraIssues)
                Debug.Log($"📷 CameraMovementBehaviour enabled: {enabled}");
        }

        Debug.Log($"Player input {(enabled ? "enabled" : "disabled")} for cutscene");
    }

    // Timeline Signal methods (kept for backward compatibility)
    public void OnTimelineSignal_ShowUI(string uiName)
    {
        Debug.Log($"Timeline Signal: Show UI - {uiName}");

        PuzzleUIManager uiManager = FindFirstObjectByType<PuzzleUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("PuzzleUIManager not found in scene");
            return;
        }

        switch (uiName.ToLower())
        {
            case "impossible":
                uiManager.SetMainText("You crossed 6 bridges and visited all landmasses.\n\n" +
                                     "Mathematician Leonhard Euler proved in 1736 that crossing all 7 bridges exactly once is impossible.\n\n" +
                                     "This problem founded the field of graph theory!");
                break;

            case "discovery":
                uiManager.SetMainText("Mathematical discovery complete! Preparing next challenge...");
                break;

            case "newconcept":
                uiManager.SetMainText("Ready to learn new graph theory concepts!");
                break;

            case "mathematics":
                uiManager.SetMainText("Graph theory: The study of vertices, edges, and mathematical relationships.");
                break;

            default:
                Debug.LogWarning($"Unknown UI type: {uiName}");
                break;
        }
    }

    public void OnTimelineSignal_PlaySound(string soundName)
    {
        Debug.Log($"Timeline Signal: Play Sound - {soundName} (Audio disabled)");
    }

    public void OnTimelineSignal_NextLevel()
    {
        Debug.Log("Timeline Signal: Start Next Level");
        HandleGenericTransition();
    }

    public void TriggerCutsceneByName(string cutsceneName)
    {
        switch (cutsceneName.ToLower())
        {
            case "konigsberg":
            case "konigsbergsolution":
                PlayKonigsbergSolutionCutscene();
                break;

            default:
                Debug.LogWarning($"Unknown cutscene name: {cutsceneName}");
                break;
        }
    }

    // Getters for external systems
    public bool IsCutscenePlaying => isCutscenePlaying;
    public string CurrentCutsceneName => currentCutsceneName;
    public float SkipProgress => skipTimer / skipHoldTime;

    private void OnDestroy()
    {
        if (mainDirector != null)
            mainDirector.stopped -= OnTimelineFinished;

        if (uiDirector != null)
            uiDirector.stopped -= OnUITimelineFinished;
    }

    // Debug methods
    [ContextMenu("Test Königsberg Cutscene")]
    public void DebugPlayKonigsbergCutscene()
    {
        PlayKonigsbergSolutionCutscene();
    }

    [ContextMenu("Force Puzzle 1 to 2 Transition")]
    public void DebugForcePuzzle1to2Transition()
    {
        HandleKonigsbergToPath();
    }

    [ContextMenu("Force Finish Current Timeline")]
    public void DebugForceFinishTimeline()
    {
        if (isCutscenePlaying)
        {
            ForceFinishTimeline();
        }
    }

    [ContextMenu("Force Enable Player Input")]
    public void DebugForceEnableInput()
    {
        SetPlayerInputEnabled(true);
        inputWasDisabled = false;
    }

    // NEW: Camera debug methods
    [ContextMenu("Force Restore Camera Control")]
    public void DebugForceRestoreCamera()
    {
        RestoreCameraControl();
        ForceCameraCleanup();
    }

    [ContextMenu("Debug Camera State")]
    public void DebugCameraState()
    {
        Debug.Log("=== CAMERA DEBUG STATE ===");

        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"Total cameras in scene: {allCameras.Length}");

        foreach (Camera cam in allCameras)
        {
            bool isPlayer = cam.transform.GetComponentInParent<PlayerMovementBehaviour>() != null;
            bool isCutscene = cam.name.Contains("Cam_EndofPuzzle") || cam.name.Contains("Cutscene");

            Debug.Log($"📷 {cam.name}:");
            Debug.Log($"  - Active: {cam.gameObject.activeInHierarchy}");
            Debug.Log($"  - Enabled: {cam.enabled}");
            Debug.Log($"  - Position: {cam.transform.position}");
            Debug.Log($"  - Parent: {(cam.transform.parent != null ? cam.transform.parent.name : "None")}");
            Debug.Log($"  - Is Player Camera: {isPlayer}");
            Debug.Log($"  - Is Cutscene Camera: {isCutscene}");
            Debug.Log($"  - Tag: {cam.tag}");

            if (isPlayer)
            {
                CameraMovementBehaviour cameraMovement = cam.GetComponent<CameraMovementBehaviour>();
                Debug.Log($"  - CameraMovementBehaviour: {(cameraMovement != null ? cameraMovement.enabled : false)}");
            }
            Debug.Log("");
        }

        Camera mainCam = Camera.main;
        Debug.Log($"Camera.main points to: {(mainCam != null ? mainCam.name : "NULL")}");
        Debug.Log("==========================");
    }

    // NEW: Manual camera switching for testing
    [ContextMenu("Force Switch to Player Camera")]
    public void DebugSwitchToPlayerCamera()
    {
        DeactivateCutsceneCameras();
        Camera playerCamera = FindPlayerCamera();
        if (playerCamera != null)
        {
            ActivatePlayerCamera(playerCamera);
            Debug.Log($"✅ Manually switched to player camera: {playerCamera.name}");
        }
        else
        {
            Debug.LogError("❌ Could not find player camera!");
        }
    }

    [ContextMenu("List All Cameras")]
    public void DebugListAllCameras()
    {
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log("=== ALL CAMERAS IN SCENE ===");

        for (int i = 0; i < allCameras.Length; i++)
        {
            Camera cam = allCameras[i];
            string status = cam.gameObject.activeInHierarchy && cam.enabled ? "🟢 ACTIVE" : "🔴 INACTIVE";
            Debug.Log($"{i + 1}. {cam.name} - {status}");
        }
        Debug.Log("============================");
    }
}