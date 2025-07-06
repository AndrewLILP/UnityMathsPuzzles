using UnityEngine;

[RequireComponent(typeof(PlayerMovementBehaviour))]

public class PlayerJumpBehaviour : Interactor
{
    [SerializeField] private float jumpVelocity;

    private PlayerMovementBehaviour playerMovementBehaviour;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovementBehaviour = GetComponent<PlayerMovementBehaviour>();

    }


    public override void Interact()
    {
        if (input.jumpPressed && playerMovementBehaviour.isGrounded)
        {
            playerMovementBehaviour.SetYVelocity(jumpVelocity);
        }

    }
}
