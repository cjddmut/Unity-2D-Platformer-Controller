using UnityEngine;

public class MovingPlatformMotor2D : MonoBehaviour
{
    public Vector2 velocity
    {
        get
        {
            return _velocity;
        }
        set
        {
            _velocitySet = true;
            _velocity = value;
        }
    }
    public System.Action<PlatformerMotor2D> onPlatformerMotorContact;

    private bool _velocitySet;
    private Vector2 _velocity;

    public Vector2 position
    {
        get { return transform.position; }
        set
        {
            previousPosition = transform.position;
            transform.position = value;
            velocity = Vector2.zero;
            _velocitySet = false;
        }
    }

    public Vector2 previousPosition { get; private set; }

    private void FixedUpdate()
    {
        if (_velocitySet)
        {
            previousPosition = transform.position;
            transform.position += (Vector3)velocity * Time.fixedDeltaTime;
        }
    }
}
