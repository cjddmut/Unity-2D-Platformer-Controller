using UnityEngine;

namespace PC2D
{
    public class SimpleUpDown : MonoBehaviour
    {
        public float upDownAmount;
        public float speed;

        private Rigidbody2D _rigidbody2D;
        private float _startingY;
        private Vector2 _velocity;

        // Use this for initialization
        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _startingY = transform.position.y;
            _velocity = Vector2.up * speed;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _rigidbody2D.position += _velocity * Time.fixedDeltaTime;

            if (_velocity.y < 0 && _startingY - _rigidbody2D.position.y >= upDownAmount)
            {
                _rigidbody2D.position += Vector2.up * ((_startingY - _rigidbody2D.position.y) - upDownAmount);
                _velocity = Vector2.up * speed;
            }
            else if (_velocity.y > 0 && _rigidbody2D.position.y - _startingY >= upDownAmount)
            {
                _rigidbody2D.position += -Vector2.up * ((_rigidbody2D.position.y - _startingY) - upDownAmount);
                _velocity = -Vector2.up * speed;
            }
        }
    }
}
