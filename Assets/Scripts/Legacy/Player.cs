using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float moveMultiplier;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float jumpVelocity;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance;


    [Header("Shoot")]
    [SerializeField] private Rigidbody bulletPrefab;
    [SerializeField] private Rigidbody rocketPrefab;
    [SerializeField] private float shootForce;
    [SerializeField] private Transform shootPoint;

    [Header("Picked & Dropped")]
    [SerializeField] private LayerMask pickupLayer; //1h52
    [SerializeField] private float pickupDistance;
    [SerializeField] private Transform attachTransform;

    [Header("Interact")]
    [SerializeField] private LayerMask interactLayer; //1h52
    [SerializeField] private float interactDistance;


    private CharacterController characterController;
    private Transform mainCamera;

    private bool isGrounded;
    private float horizontal, vertical;
    private float mouseX, mouseY;
    private float cameraRotation;
    private Vector3 playerVelocity;

    // Raycast variables - 1h54 lesson 3
    private RaycastHit raycastHit;
    // 2h34
    private ISelectable selectable; // interface for selectable objects

    // Pickup and Drop variables - 1h54 lesson 3
    private bool isPicked = false;
    private IPickable pickable;




    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Start()
    {
        characterController = GetComponent<CharacterController>(); // fill reference to CharacterController component dynamically
        mainCamera = Camera.main.gameObject.transform; // find the main camera in the scene

        // hide mouse cursor
        Cursor.lockState = CursorLockMode.Locked; // lock the cursor to the center of the screen
        Cursor.visible = false; // hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        GroundCheck();
        MovePlayer(); // works in Update as opposed to fixed update as it is not using rigid body
        RotatePlayer();
        JumpCheck();

        ShootBullet();
        ShootRocket();

        // Pick and Drop below not functioning as intended  
        PickAndDrop(); // lesson 3 2hours in - 1h50-2h07 - goes to unity for objects with IPickable interface

        Interact(); // lesson 3 2hours in - 1h50-2h07 - goes to unity for objects with ISelectable interface
        // lesson 4 - 0h16

    }

    private void GetInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        moveMultiplier = Input.GetButton("Sprint") ? sprintMultiplier : 1; // bool check using ? see below
    }

    private void MovePlayer()
    {
        characterController.Move((transform.forward * vertical + transform.right * horizontal) * moveSpeed * Time.deltaTime * moveMultiplier);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
        Debug.Log("Grounded: " + isGrounded); // debug log to check if grounded
    }

    void JumpCheck()
    {
        if (Input.GetButtonDown("Jump"))
        {
            playerVelocity.y = jumpVelocity;
        }
    }

    private void RotatePlayer()
    {
        // Player Turn movement
        transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime * mouseX);

        // Camera up/down movement
        cameraRotation += Time.deltaTime * mouseY * turnSpeed * -1f;
        cameraRotation = Mathf.Clamp(cameraRotation, -80f, 80f);
        mainCamera.localRotation = Quaternion.Euler(cameraRotation, 0, 0);
    }

    void ShootBullet()
    {
        if (Input.GetButtonDown("Fire1")) // f added as an alternate button - numbers not working
        {
            Rigidbody bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            bullet.AddForce(shootPoint.forward * shootForce);
            Destroy(bullet.gameObject, 5f); // destroys the bullet after 5 seconds

        }
    }

    void ShootRocket()
    {
        if (Input.GetButtonDown("Fire2")) // f added as an alternate button - numbers not working
        {
            Rigidbody bullet = Instantiate(rocketPrefab, shootPoint.position, shootPoint.rotation);
            bullet.AddForce(shootPoint.forward * shootForce);
            Destroy(bullet.gameObject, 5f); // destroys the bullet after 5 seconds
        }
    }

    //lesson 3 2hours in - 1h56
    //void PickAndDrop()
    //{
    //  Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    //if (Physics.Raycast(ray, out raycastHit, pickupDistance, pickupLayer))
    //{
    //  if (Input.GetKeyDown(KeyCode.E) && !isPicked)
    //{
    //  pickable = raycastHit.transform.GetComponent<IPickable>();
    //if (pickable == null) return;

    //pickable.OnPicked(attachTransform); // call the OnPicked method of the IPickable interface
    //isPicked = true;
    //return;
    //}
    //} //drop the picked object 
    //if (Input.GetKeyDown(KeyCode.E) && isPicked && pickable != null)
    //{
    //  pickable.OnDropped(); // call the OnDropped method of the IPickable interface
    //isPicked = false;
    //}
    //}

    void PickAndDrop()
    {
        // Handle input once at the start
        if (!Input.GetKeyDown(KeyCode.E)) return;

        // If already holding something, drop it
        if (isPicked && pickable != null)
        {
            pickable.OnDropped();
            isPicked = false;
            pickable = null;
            return;
        }

        // Try to pick up something new - IGNORE TRIGGERS
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, pickupDistance, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            IPickable newPickable = raycastHit.transform.GetComponent<IPickable>();
            if (newPickable != null)
            {
                newPickable.OnPicked(attachTransform);
                pickable = newPickable;
                isPicked = true;
            }
        }
    }
    void Interact()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, interactDistance, interactLayer, QueryTriggerInteraction.Ignore)) // trying QueryTriggerInteraction.Ignore
        {
            selectable = raycastHit.transform.GetComponent<ISelectable>(); // get the ISelectable component from the object hit by the raycast
            if (selectable != null)
            {
                selectable.OnHoverEnter(); // call the OnHoverEnter method of the ISelectable interface
                if (Input.GetKeyDown(KeyCode.E)) // check if the E key is pressed
                {
                    selectable.OnSelect(); // call the OnSelect method of the ISelectable interface
                }
            }
        }
        if (raycastHit.transform == null && selectable != null)
        {
            selectable.OnHoverExit(); // call the OnHoverExit method of the ISelectable interface
            selectable = null; // reset the selectable object
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance); // draw a wire sphere to visualize the ground check
        }
    }
}
// This script is attached to the player GameObject and is responsible for player-related functionality.
// Camera: experiment with field of view (FOV) and perspective
// 40 by 40 playfield
// Adding Static to Ground as it is not moving - implications for rendering, eg baking lighting etc.  
// using header for variable organisation 
// hide mouse
// adding a sprint button through the input manager - change 30 to 31 and add deets
// CONTROL and . (period) gives option to extract method - see GetInput method
// If Else shortcut ?
// if sprint button isPressed use sprintMultiplier of 1 - ? sprintMultiplier : 1;
// 1h56m Destroy(bullet) destroys the rigid body - use bullet.gameObject for the whole gO
// bullet prefab - use continuous dynamic

