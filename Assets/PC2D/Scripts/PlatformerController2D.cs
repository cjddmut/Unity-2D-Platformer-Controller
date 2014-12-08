
using UnityEngine;
using System.Collections;


/**
 * This script is set up to work with the default Unity input settings to be an easy example. Alter this file
 * along with the input settings to change what keys do what. In fact, I'd say this is encouraged :).
 **/

[RequireComponent(typeof(PlatformerMotor2D))]
public class PlatformerController2D : MonoBehaviour
{
    public bool canControl = true;

    private PlatformerMotor2D motor;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlatformerMotor2D>();
        float a = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canControl)
        {
            return;
        }

        motor.normalizedXMovement = Input.GetAxis(PC2D.Input.HORIZONTAL); ;

        // Jump?
        if (Input.GetButtonDown(PC2D.Input.JUMP))
        {
            motor.Jump();
        }

        motor.jumpingHeld = Input.GetButton(PC2D.Input.JUMP);

        if (Input.GetAxis(PC2D.Input.VERTICAL) < -PC2D.Globals.INPUT_THRESHOLD)
        {
            motor.fallFast = true;
        }
        else
        {
            motor.fallFast = false;
        }

        if (Input.GetButtonDown(PC2D.Input.DASH))
        {
            motor.Dash();
        }
    }
}
