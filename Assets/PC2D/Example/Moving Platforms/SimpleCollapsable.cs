using UnityEngine;

namespace PC2D
{
    public class SimpleCollapsable : MonoBehaviour
    {
        public float gravityScaleWhenFalling;
        public float darkenDuration;
        public Color darkenColor;

        private float _darkenTime;
        private Color _originalColor;

        private enum State
        {
            None,
            Darken,
            Falling
        }

        private MovingPlatformMotor2D _mpMotor;
        private SpriteRenderer _renderer;
        private State _state;

        // Use this for initialization
        void Start()
        {
            _mpMotor = GetComponent<MovingPlatformMotor2D>();
            _mpMotor.onPlatformerMotorContact += PlayerContact;

            _renderer = GetComponent<SpriteRenderer>();
            _originalColor = _renderer.color;
        }

        private void FixedUpdate()
        {
            if (_state == State.Falling)
            {
                _mpMotor.velocity += Physics2D.gravity * gravityScaleWhenFalling * Time.fixedDeltaTime;
            }
            
            if (_state == State.Darken)
            {
                _darkenTime += Time.fixedDeltaTime;
                _renderer.color = Color.Lerp(_originalColor, darkenColor, Mathf.Clamp01(_darkenTime / darkenDuration));
            }

            if (_darkenTime >= darkenDuration)
            {
                _state = State.Falling;
            }
        }

        private void PlayerContact(PlatformerMotor2D player)
        {
            _mpMotor.onPlatformerMotorContact -= PlayerContact;
            _state = State.Darken;
        }
    }
}
