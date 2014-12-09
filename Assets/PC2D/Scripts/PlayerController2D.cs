
using UnityEngine;
using System.Collections;

/// <summary>
/// This class is a simple example of how to build a controller that interacts with PlatformerMotor2D.
/// </summary>
[RequireComponent(typeof(PlatformerMotor2D))]
public class PlayerController2D : MonoBehaviour
{
    private PlatformerMotor2D motor;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlatformerMotor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(Input.GetAxis(PC2D.Input.HORIZONTAL)) > PC2D.Globals.INPUT_THRESHOLD)
        {
            motor.normalizedXMovement = Input.GetAxis(PC2D.Input.HORIZONTAL);
        }
        else
        {
            motor.normalizedXMovement = 0;
        }

        // Jump?
        if (Input.GetButtonDown(PC2D.Input.JUMP))
        {
            motor.Jump();
        }

        motor.jumpingHeld = Input.GetButton(PC2D.Input.JUMP);

        if (Input.GetAxis(PC2D.Input.VERTICAL) < -PC2D.Globals.FAST_FALL_THRESHOLD)
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
