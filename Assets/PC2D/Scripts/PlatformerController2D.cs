
using UnityEngine;
using System.Collections;


/**
 * This script is set up to work with the default Unity input settings to be an easy example. Alter this file
 * along with the input settings to change what keys do what. In fact, I'd say this is encouraged :).
 **/

[RequireComponent(typeof(PlayerMotor2D))]
public class PlatformerController2D : MonoBehaviour
{
    public bool canControl = true;

    private PlayerMotor2D motor;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlayerMotor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canControl)
        {
            return;
        }

        // Handle input communication with the motor. Since this is a side scrolling platformer, we do not want
        // any vertical movement. So only set the x.
        Vector2 moveDir = new Vector2();
        moveDir.x = Input.GetAxis(PC2D.Input.HORIZONTAL);
        motor.movementDir = moveDir;

        // Jump?
        if (Input.GetButtonDown(PC2D.Input.JUMP))
        {
            motor.Jump();
        }

        motor.jumpingHeld = Input.GetButton(PC2D.Input.JUMP);

        if (Input.GetAxis(PC2D.Input.VERTICAL) < PC2D.Globals.INPUT_THRESHOLD)
        {
            motor.fallFast = true;
        }

        if (Input.GetButtonDown(PC2D.Input.DASH))
        {
            motor.Dash();
        }
    }
}
