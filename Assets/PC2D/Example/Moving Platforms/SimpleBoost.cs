using UnityEngine;

namespace PC2D
{
    public class SimpleBoost : MonoBehaviour
    {
        public float heightReached;
        public float moveUpDuration;
        public EasingFunctions.Functions moveUpEase;

        public float moveDownDuration;
        public EasingFunctions.Functions moveDownEase;

        public float playerSpeedYAtApex;

        private enum State
        {
            None,
            Up,
            Down
        }

        private MovingPlatformMotor2D _mpMotor;
        private State _state;
        private float _originalY;
        private float _time;
        private PlatformerMotor2D _player;

        private EasingFunctions.EasingFunc _moveUpFunc;
        private EasingFunctions.EasingFunc _moveDownFunc;

        // Use this for initialization
        void Start()
        {
            _mpMotor = GetComponent<MovingPlatformMotor2D>();
            _mpMotor.onPlatformerMotorContact += PlayerContact;
            _originalY = transform.position.y;

            _moveUpFunc = EasingFunctions.GetEasingFunction(moveUpEase);
            _moveDownFunc = EasingFunctions.GetEasingFunction(moveDownEase);
        }

        private void FixedUpdate()
        {
            if (_state == State.Down)
            {
                _time += Time.fixedDeltaTime;

                _mpMotor.position = new Vector3(
                    _mpMotor.position.x,
                    _moveDownFunc(_originalY + heightReached, _originalY, Mathf.Clamp01(_time / moveDownDuration)),
                    transform.position.z);

                if (_time >= moveDownDuration)
                {
                    _state = State.None;

                    if (_player != null && _player.connectedPlatform == _mpMotor)
                    {
                        _state = State.Up;
                        _time = 0;
                    }
                    else
                    {
                        _player = null;
                    }
                }
            }

            if (_state == State.Up)
            {
                _time += Time.fixedDeltaTime;

                _mpMotor.position = new Vector3(
                    _mpMotor.position.x,
                    _moveUpFunc(_originalY, _originalY + heightReached, Mathf.Clamp01(_time / moveUpDuration)),
                    transform.position.z);

                if (_time >= moveUpDuration)
                {
                    _state = State.Down;
                    _time = 0;

                    if (_player.connectedPlatform == _mpMotor)
                    {
                        _player.DisconnectFromPlatform();
                        _player.velocity += Vector2.up * playerSpeedYAtApex;
                        _player = null;
                    }
                }
            }
        }

        private void PlayerContact(PlatformerMotor2D player)
        {
            if (_state == State.None)
            {
                _state = State.Up;
                _time = 0;

            }

            _player = player;
        }
    }
}
