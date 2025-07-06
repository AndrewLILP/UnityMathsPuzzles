using UnityEngine;

public class Door : MonoBehaviour
{
    private bool isOpen; // Track if the door is open or closed
    private Animator animator; // Reference to the Animator component
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>(); // Get the Animator component attached to the door

    }

    private void Update()
    {
        DoorState(); // Check the state of the door and update the animation
    }

    public bool ObjectEntered(bool state)
    {
        if (state)
        {
            isOpen = true; // Set the door to open
        }
        else
        {
            isOpen = false; // Set the door to close
        }

        return isOpen;
    }

    private void DoorState()
    {
        if (isOpen)
        {
            animator.SetBool("DoorOpen", true); // Trigger the open animation
            animator.SetBool("DoorClose", false); // Ensure the close animation is not triggered

        }
        else
        {
            animator.SetBool("DoorOpen", false); // Trigger the close animation
            animator.SetBool("DoorClose", true); // Trigger the close animation
        }
    }
}
