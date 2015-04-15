using UnityEngine;

namespace PC2D
{
    public class SimpleLeftRight : MonoBehaviour
    {
        public float leftRightAmount;
        public float speed;

        private Rigidbody2D _rigidbody2D;
        private float _startingX;
        private Vector2 _velocity;

        // Use this for initialization
        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _startingX = transform.position.x;
            _velocity = -Vector2.right * speed;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _rigidbody2D.position += _velocity * Time.fixedDeltaTime;

            if (_velocity.x < 0 && _startingX - _rigidbody2D.position.x >= leftRightAmount)
            {
                _rigidbody2D.position += Vector2.right * ((_startingX - _rigidbody2D.position.x) - leftRightAmount);
                _velocity = Vector2.right * speed;
            }
            else if (_velocity.x > 0 && _rigidbody2D.position.x - _startingX >= leftRightAmount)
            {
                _rigidbody2D.position += -Vector2.right * ((_rigidbody2D.position.x - _startingX) - leftRightAmount);
                _velocity = -Vector2.right * speed;
            }
        }
    }
}
