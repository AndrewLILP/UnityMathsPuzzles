using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovementBehaviour : MonoBehaviour
{
    private PlayerInput input;

    [Header("Player Movement Settings")]
    [SerializeField] private float moveSpeed; // Speed at which the player moves
    [SerializeField] private float gravity = -9.81f; // Gravity applied to the player
    [SerializeField] private float sprintMultiplier = 1.5f; // Multiplier for sprinting speed

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck; // Transform used to check if the player is grounded
    [SerializeField] private LayerMask groundMask; // Layer mask to identify ground layers
    [SerializeField] private float groundCheckDistance; // Distance to check for ground

    private CharacterController characterController; // Reference to the CharacterController component

    private Vector3 playerVelocity;
    public bool isGrounded { get; private set; } // Property to check if the player is grounded

    private float moveMultiplier; // Multiplier for movement speed based on whether the player is sprinting

// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        input = PlayerInput.GetInstance();
    

}

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        MovePlayer();
        
    }
    private void MovePlayer()
    {
        moveMultiplier = input.sprintHeld ? sprintMultiplier : 1f; // Set move multiplier based on sprinting state
        characterController.Move((transform.forward * input.vertical + transform.right * input.horizontal) * moveSpeed * Time.deltaTime * moveMultiplier);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // v = u + at
        playerVelocity.y += gravity * Time.deltaTime;
        // V = (1/2) * a * t^2
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
        Debug.Log("Grounded: " + isGrounded); // debug log to check if grounded
    }

    public void SetYVelocity(float value)
    {
        playerVelocity.y = value; // Set the vertical velocity of the player


    }

    public float GetForwardSpeed()
    {
        return input.vertical * moveSpeed * moveMultiplier; // Calculate and return the forward speed based on input and movement settings
    }
}
