using UnityEngine;

// updated for lesson 9
public class Trigger : MonoBehaviour
{
    [SerializeField] private Door door; // Reference to the Door component
    [SerializeField] private KeycardManager keycardManager; // Reference to the KeycardManager component
    [SerializeField] private GameObject UIShowCardMissing; // Reference to the UI element that shows the card is missing

    private void Start()
    {
        UIShowCardMissing.SetActive(false); // Ensure the UI element is inactive at the start
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player") && keycardManager.firstkeycard == true) // Check if the player has the first keycard
        {
            door.ObjectEntered(true); // Call the method to open the door

        }

        if (other.gameObject.CompareTag("Player") && keycardManager.firstkeycard == false) // Check if the player has the first keycard
        {
            UIShowCardMissing.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        {
            if (other.gameObject.CompareTag("Player") && keycardManager.firstkeycard == false) // Check if the player has the first keycard
            {
                //UIShowCardMissing.SetActive(false);
            }
        }

    }

}

