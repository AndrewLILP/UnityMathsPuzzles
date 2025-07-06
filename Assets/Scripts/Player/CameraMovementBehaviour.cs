using UnityEngine;

[RequireComponent(typeof(Camera))] // Ensures that the GameObject has a Camera component

public class CameraMovementBehaviour : MonoBehaviour
{
    private PlayerInput input; // Reference to the PlayerInput script

    [Header("Player Turn")]
    [SerializeField] private float turnSpeed;
    [SerializeField] private bool invertMouse;

    private float cameraXRotation;








    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = PlayerInput.GetInstance(); 
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen
        Cursor.visible = false; // Hides the cursor
        
    }

    // Update is called once per frame
    void Update()
    {
        RotateCamera();

    }

    void RotateCamera()
    {
        cameraXRotation += Time.deltaTime * input.mouseY * turnSpeed * (invertMouse ? -1 : 1);
        cameraXRotation = Mathf.Clamp(cameraXRotation, -85f, 85f); // Clamps the rotation to prevent flipping

        transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0); // Applies the rotation to the camera
    }
}
