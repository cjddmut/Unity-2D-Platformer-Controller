using UnityEngine;

public class MovingPlatformMotor2D : MonoBehaviour
{
    public Vector2 velocity { get; set; }
    public System.Action<PlatformerMotor2D> onPlatformerMotorContact;

    public Vector2 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)velocity * Time.fixedDeltaTime;
    }
}
