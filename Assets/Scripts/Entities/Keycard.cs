using UnityEngine;

// created for lesson 9

public class Keycard : MonoBehaviour
{
    private KeycardManager keycardManager;
    private void Start()
    {
        keycardManager = FindAnyObjectByType<KeycardManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
              
        if (other.gameObject.CompareTag("Player"))
        {
            keycardManager.firstkeycard = true; // Set the first keycard to true
            Destroy(gameObject); // Destroy the keycard object
        }
    }
}

