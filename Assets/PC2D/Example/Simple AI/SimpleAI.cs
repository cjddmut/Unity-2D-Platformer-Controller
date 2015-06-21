using System.Collections;
using UnityEngine;

namespace  PC2D
{
    public class SimpleAI : MonoBehaviour
    {
        public float distanceCheckForJump;
        public float heightToFallFast;
        public float delayForWallJump;

        private PlatformerMotor2D _motor;

        public float movement { get; private set; }

        // Use this for initialization
        void Start()
        {
            _motor = GetComponent<PlatformerMotor2D>();
            movement = -1;

            // Find objects generally pretty bad but this is a demo :)
            SimpleAI[] ais = FindObjectsOfType<SimpleAI>();

            for (int i = 0; i < ais.Length; i++)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ais[i].GetComponent<Collider2D>());
            }

            _motor.onWallJump +=
                dir =>
                {
                    // Since the motor needs to be pressing into the wall to wall jump, we switch direction after the jump.
                    movement = Mathf.Sign(dir.x);
                };
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _motor.normalizedXMovement = movement;

            if (_motor.motorState == PlatformerMotor2D.MotorState.OnGround)
            {
                _motor.fallFast = false;
                Vector2 dir = Vector2.right;

                if (movement < 0)
                {
                    dir *= -1;
                }

                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    dir,
                    distanceCheckForJump,
                    Globals.ENV_MASK);

                if (hit.collider != null)
                {
                    _motor.Jump();
                }
            }

            if (_motor.motorState == PlatformerMotor2D.MotorState.WallSticking)
            {
                StartCoroutine(DelayWallJump());
            }

            if (_motor.motorState == PlatformerMotor2D.MotorState.Falling)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    -Vector2.up,
                    heightToFallFast,
                    Globals.ENV_MASK);

                Vector2 dir = Vector2.right;

                if (movement < 0)
                {
                    dir *= -1;
                }

                RaycastHit2D hit2 = Physics2D.Raycast(
                    transform.position,
                    dir,
                    distanceCheckForJump,
                    Globals.ENV_MASK);

                if (hit.collider == null && hit2.collider == null)
                {
                    _motor.fallFast = true;
                    
                }
            }
        }

        private IEnumerator DelayWallJump()
        {
            yield return new WaitForSeconds(delayForWallJump);
            _motor.Jump();
        }
    }
}
