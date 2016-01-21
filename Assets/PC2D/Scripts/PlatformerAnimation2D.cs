using UnityEngine;
using System.Collections.Generic;

namespace PC2D
{
    /// <summary>
    /// This is a very very very simple example of how an animation system could query information from the motor to set state.
    /// This can be done to explicitly play states, as is below, or send triggers, float, or bools to the animator. Most likely this
    /// will need to be written to suit your game's needs.
    /// </summary>

    public class PlatformerAnimation2D : MonoBehaviour
    {
        public float jumpRotationSpeed;
        public bool jumpPlayOnce;
        public GameObject visualChild;

        private PlatformerMotor2D _motor;
        private Animator _animator;
        private bool _isJumping;
        private bool _jumpPlayed = false;
        private bool _currentFacingLeft;

        private int _animationIdle;
        private int _animationWalk;
        private int _animationJump;
        private int _animationFall;
        private int _animationDash;
        private int _animationCling;
        private int _animationSlip;
        private int _animationOnCorner;

        private Dictionary<int, bool> _hasAnimation = new Dictionary<int, bool>();

        // Use this for initialization
        void Start()
        {
            _motor = GetComponent<PlatformerMotor2D>();
            _animator = visualChild.GetComponent<Animator>();

            _animationIdle = Animator.StringToHash("Idle");
            _animationWalk = Animator.StringToHash("Walk");
            _animationJump = Animator.StringToHash("Jump");
            _animationFall = Animator.StringToHash("Fall");
            _animationDash = Animator.StringToHash("Dash");
            _animationCling = Animator.StringToHash("Cling");
            _animationSlip = Animator.StringToHash("Slip");
            _animationOnCorner = Animator.StringToHash("On Corner");

            CheckAnimation(_animationIdle);
            CheckAnimation(_animationWalk);
            CheckAnimation(_animationJump);
            CheckAnimation(_animationFall);
            CheckAnimation(_animationDash);
            CheckAnimation(_animationCling);
            CheckAnimation(_animationSlip);
            CheckAnimation(_animationOnCorner);

            PlayAnimation(_animationIdle);

            _motor.onJump += SetCurrentFacingLeft;
            _motor.onJump += ResetJumpPlayed;
            _motor.onLanded += ResetJumpPlayed;
        }

        // Update is called once per frame
        void Update()
        {
            if (_motor.motorState == PlatformerMotor2D.MotorState.Jumping ||
                _isJumping &&
                    (_motor.motorState == PlatformerMotor2D.MotorState.Falling ||
                                 _motor.motorState == PlatformerMotor2D.MotorState.FallingFast))
            {
                _isJumping = true;
                if (!_jumpPlayed)
                {
                    PlayAnimation(_animationJump);
                }

                if (_motor.velocity.x <= -0.1f)
                {
                    _currentFacingLeft = true;
                }
                else if (_motor.velocity.x >= 0.1f)
                {
                    _currentFacingLeft = false;
                }

                Vector3 rotateDir = _currentFacingLeft ? Vector3.forward : Vector3.back;
                visualChild.transform.Rotate(rotateDir, jumpRotationSpeed * Time.deltaTime);
            }
            else
            {
                _isJumping = false;
                visualChild.transform.rotation = Quaternion.identity;

                if (_motor.motorState == PlatformerMotor2D.MotorState.Falling ||
                                 _motor.motorState == PlatformerMotor2D.MotorState.FallingFast)
                {
                    PlayAnimation(_animationFall);
                }
                else if (_motor.motorState == PlatformerMotor2D.MotorState.WallSliding ||
                         _motor.motorState == PlatformerMotor2D.MotorState.WallSticking)
                {
                    PlayAnimation(_animationCling);
                }
                else if (_motor.motorState == PlatformerMotor2D.MotorState.OnCorner)
                {
                    PlayAnimation(_animationOnCorner);
                }
                else if (_motor.motorState == PlatformerMotor2D.MotorState.Slipping)
                {
                    PlayAnimation(_animationSlip);
                }
                else if (_motor.motorState == PlatformerMotor2D.MotorState.Dashing)
                {
                    PlayAnimation(_animationDash);
                }
                else
                {
                    if (_motor.velocity.sqrMagnitude >= 0.1f * 0.1f)
                    {
                        PlayAnimation(_animationWalk);
                    }
                    else
                    {
                        PlayAnimation(_animationIdle);
                    }
                }
            }

            // Facing
            float valueCheck = _motor.normalizedXMovement;

            if (_motor.motorState == PlatformerMotor2D.MotorState.Slipping ||
                _motor.motorState == PlatformerMotor2D.MotorState.Dashing ||
                _motor.motorState == PlatformerMotor2D.MotorState.Jumping)
            {
                valueCheck = _motor.velocity.x;
            }

            if (valueCheck >= 0.1f)
            {
                visualChild.transform.localScale = Vector3.one;
            }
            else if (valueCheck <= -0.1f)
            {
                Vector3 newScale = Vector3.one;
                newScale.x = -1;
                visualChild.transform.localScale = newScale;
            }
        }

        private void SetCurrentFacingLeft()
        {
            _currentFacingLeft = _motor.facingLeft;
        }

        private void ResetJumpPlayed()
        {
            _jumpPlayed = false;
        }

        private void PlayAnimation(int animation)
        {
            if (HasAnimation(animation))
            {
                _animator.Play(animation);

                if (jumpPlayOnce && animation == _animationJump)
                {
                    _jumpPlayed = true;
                }
            }
        }

        private bool HasAnimation(int animation)
        {
            if (_hasAnimation.ContainsKey(animation))
            {
                return _hasAnimation[animation];
            }

            return false;
        }

        private void CheckAnimation(int animation)
        {
            if (!_hasAnimation.ContainsKey(animation))
            {
                _hasAnimation.Add(animation, _animator.HasState(0, animation));
            }
        }
    }
}
