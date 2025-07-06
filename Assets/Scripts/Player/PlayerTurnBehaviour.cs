using UnityEngine;

public class PlayerTurnBehaviour : MonoBehaviour
{
    private PlayerInput input;

    [Header("Player Turn Settings")]
    [SerializeField] private float turnSpeed; // Speed at which the player turns

    private void Start()
    {
        input = PlayerInput.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        RotatePlayer();
    }

    void RotatePlayer()
    {
        transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime * input.mouseX);
    }
}
