using UnityEngine;
using System;

public class BlueTargetBox : MonoBehaviour, IDestroyable
{
    // Static event for Observer Pattern communication
    public static Action OnBlueBoxHit;

    [SerializeField] private Material[] damageMaterials;
    [SerializeField] private int maxHitsBeforeDestroy = 3;

    private int hitsOnThisBox = 0;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Log("BlueTargetBox: " + gameObject.name + " ready for hits");
    }

    public void OnCollided()
    {
        hitsOnThisBox++;

        Debug.Log("BlueTargetBox: " + gameObject.name + " hit! (" + hitsOnThisBox + "/" + maxHitsBeforeDestroy + ")");

        // Visual feedback
        UpdateVisualFeedback();

        // Notify BlueKeyCard system
        OnBlueBoxHit?.Invoke();

        // Optional: destroy after max hits
        if (hitsOnThisBox >= maxHitsBeforeDestroy)
        {
            Debug.Log("BlueTargetBox: " + gameObject.name + " destroyed after max hits");
            Destroy(gameObject);
        }
    }

    private void UpdateVisualFeedback()
    {
        Color originalColor = Color.blue;
        Color damageColor = Color.red;
        float damagePercent = (float)hitsOnThisBox / maxHitsBeforeDestroy;

        meshRenderer.material.color = Color.Lerp(originalColor, damageColor, damagePercent);

    }
}