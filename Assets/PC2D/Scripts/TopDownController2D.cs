using UnityEngine;
using System.Collections;


/**
 * This script is set up to work with the default Unity input settings to be an easy example. Alter this file
 * along with the input settings to change what keys do what. In fact, I'd say this is encouraged :).
 **/

[RequireComponent(typeof(PlayerMotor2D))]
public class TopDownController2D : MonoBehaviour
{
    public bool canControl = true;

    private PlayerMotor2D motor;
    private Vector2 lastHeading;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlayerMotor2D>();
        lastHeading = -Vector2.up;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canControl)
        {
            return;
        }

        if (Input.GetButtonDown(PC2D.Input.DASH))
        {
            motor.Dash(lastHeading);
        }
        else
        {
            Vector2 moveDir = new Vector2();
            moveDir.x = Input.GetAxisRaw(PC2D.Input.HORIZONTAL);
            moveDir.y = Input.GetAxisRaw(PC2D.Input.VERTICAL);

            if (Mathf.Abs(moveDir.x) < PC2D.Globals.INPUT_THRESHOLD)
            {
                moveDir.x = 0;
            }

            if (Mathf.Abs(moveDir.y) < PC2D.Globals.INPUT_THRESHOLD)
            {
                moveDir.y = 0;
            }

            // If not zero then normalize.

            if (moveDir != Vector2.zero)
            {
                moveDir.Normalize();
                lastHeading = moveDir;
            }

            motor.movementDir = moveDir;
        }
    }
}
