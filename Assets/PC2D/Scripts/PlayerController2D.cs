using UnityEngine;
using System.Collections;


/**
 * This script is set up to work with the default Unity input settings to be an easy example. Alter this file
 * along with the input settings to change what keys do what. In fact, I'd say this is encouraged :).
 **/

[RequireComponent(typeof(PlayerMotor2D))]
public class PlayerController2D : MonoBehaviour
{
    public bool CanControl = true;

    private PlayerMotor2D _Motor;

    // Use this for initialization
    void Start()
    {
        _Motor = GetComponent<PlayerMotor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanControl)
        {
            return;
        }

        // Handle input communication with the motor. Since this is a side scrolling platformer, we do not want
        // any vertical movement. So only set the x.
        Vector2 moveDir = new Vector2();
        moveDir.x = Input.GetAxis(PC2D.Input.HORIZONTAL);
        _Motor.SetMovementDirection(moveDir);

        // Jump?
        if (Input.GetButtonDown(PC2D.Input.JUMP))
        {
            _Motor.Jump();
        }

        if (Input.GetButton(PC2D.Input.JUMP))
        {
            // Held down?
            _Motor.JumpHeld();
        }

        if (Input.GetButtonDown(PC2D.Input.DASH))
        {
            _Motor.Dash();
        }
    }
}
