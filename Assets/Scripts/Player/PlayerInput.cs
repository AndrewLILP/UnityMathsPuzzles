using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// updated in Lesson 7 of the Game Architecture course (1h 42min)
// updtaed in Lesson 8 of the Game Architecture course (1h 04min)

[DefaultExecutionOrder(-100)]

public class PlayerInput : MonoBehaviour
{
    public float horizontal { get; private set; }
    public float vertical { get; private set; }
    public float mouseX { get; private set; }
    public float mouseY { get; private set; }
    public bool sprintHeld { get; private set; }
    public bool jumpPressed { get; private set; }
    public bool activatePressed { get; private set; }
    public bool primaryShootPressed { get; private set; }
    public bool secondaryShootPressed { get; private set; }

    public bool weapon1Pressed { get; private set; }// added for Lesson 7
    public bool weapon2Pressed { get; private set; }// added for Lesson 7
    public bool commandPressed { get; private set; } // added for Lesson 8


    private bool clear;


    // Singleton
    private static PlayerInput instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
            return;
        }
        instance = this;
    }

    public static PlayerInput GetInstance() { return instance; }

    // End of Singleton

    // Update is called once per frame
    void Update()
    {
        ClearInputs();
        ProcessInputs();
    }

    void ProcessInputs()
    {
        weapon1Pressed = weapon1Pressed || Input.GetKeyDown(KeyCode.Alpha1);
        weapon2Pressed = weapon2Pressed || Input.GetKeyDown(KeyCode.Alpha2);

        horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Check if the input is cleared
        sprintHeld = sprintHeld || Input.GetButtonDown("Sprint");
        jumpPressed = jumpPressed || Input.GetButtonDown("Jump");
        activatePressed = activatePressed || Input.GetKeyDown(KeyCode.E);
        primaryShootPressed = primaryShootPressed || Input.GetButtonDown("Fire1");
        secondaryShootPressed = secondaryShootPressed || Input.GetButtonDown("Fire2");

        commandPressed = commandPressed || Input.GetKeyDown(KeyCode.C); // added for Lesson 8 - instructor uses F
    }

    private void FixedUpdate()
    {
        clear = true;
    }

    void ClearInputs()
    {
        if (!clear)
            return;

        horizontal = 0;
        vertical = 0;
        mouseX = 0;
        mouseY = 0;

        sprintHeld = false;
        jumpPressed = false;
        activatePressed = false;

        primaryShootPressed = false;
        secondaryShootPressed = false;
        commandPressed = false;

        weapon1Pressed = false;
        weapon2Pressed = false;
    }

}
