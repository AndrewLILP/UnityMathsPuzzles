using UnityEngine;
using System;

public class BlueKeyCard : MonoBehaviour
{
    [SerializeField] private int requiredHits = 10;
    [SerializeField] private GameObject blueKeycardPrefab;
    [SerializeField] private Transform spawnPoint;
    //[SerializeField] private GameObject[] level2Gates; // Handle gates directly here

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
            SpawnKeycard();
            //OpenGates();
            OnTargetComplete?.Invoke();
        }
    }

    private void SpawnKeycard()
    {
        if (blueKeycardPrefab != null && spawnPoint != null)
        {
            GameObject keycard = Instantiate(blueKeycardPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("BlueKeyCard: Blue keycard spawned at " + spawnPoint.position);
        }
        else
        {
            Debug.LogWarning("BlueKeyCard: Cannot spawn keycard - missing prefab or spawn point!");
        }
    }

}