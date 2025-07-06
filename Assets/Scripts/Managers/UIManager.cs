using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// UIManager.cs made for Lesson 7 of the Game Architecture course (1h 10min)
/// <summary>
/// Observer pattern implementation for UI updates in Unity.
/// subscribing to the Health class
/// </summary>

public class UIManager : MonoBehaviour
{
    [SerializeField] private Health playerHealth; // Reference to the player's health component

    [Header("UI Elements")]
    public TMP_Text txtHealth; // Text component to display health
    public GameObject gameOverText; // GameObject to show when the game is over

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverText.SetActive(false); // Hide the game over text at the start
    }

    private void OnEnable()
    {
        playerHealth.OnHealthUpdate += OnHealthUpdate; // Subscribe to health updates
        playerHealth.OnDeath += OnDeath; // Subscribe to death event

    }

    private void OnDestroy()
    {
        playerHealth.OnHealthUpdate -= OnHealthUpdate; // Unsubscribe from health updates
        playerHealth.OnDeath -= OnDeath; // Unsubscribe from death event
    }

    private void OnHealthUpdate(float health)
    {
        txtHealth.text = "Health: " + Mathf.Floor(health).ToString(); // Update the health text
    }

    private void OnDeath()
    {
        gameOverText.SetActive(true); // Show the game over text when the player dies
        
    }
}
