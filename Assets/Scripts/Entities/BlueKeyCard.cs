using UnityEngine;
using System;

public class BlueKeyCard : MonoBehaviour
{
    [SerializeField] private int requiredHits = 10;
       private int currentHits = 0;

    // Events for future UI integration
    public Action<int> OnHitCountUpdate;
    public Action OnTargetComplete;

    private void Start()
    {
        Debug.Log("BlueKeyCard: System initialized. Need " + requiredHits + " hits.");

        // Subscribe to blue target box hits
        BlueTargetBox.OnBlueBoxHit += OnBlueBoxHit;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        BlueTargetBox.OnBlueBoxHit -= OnBlueBoxHit;
    }

    private void OnBlueBoxHit()
    {
        currentHits++;

        // Debug logging for hit counter
        Debug.Log("BlueKeyCard: Hit " + currentHits + "/" + requiredHits + " blue boxes");

        OnHitCountUpdate?.Invoke(currentHits);

        if (currentHits >= requiredHits)
        {
            Debug.Log("BlueKeyCard: Target complete! Spawning keycard and opening gates.");
            OnTargetComplete?.Invoke();
        }
    }


}