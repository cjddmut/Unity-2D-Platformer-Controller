using UnityEngine;

public class MessageOnCollision : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D o)
    {
        Debug.Log(gameObject.name + " OnTriggerEnter with " + o.name);
    }

    void OnTriggerStay2D(Collider2D o)
    {
        Debug.Log(gameObject.name + " OnTriggerStay with " + o.name);
    }

    void OnTriggerExit2D(Collider2D o)
    {
        Debug.Log(gameObject.name + " OnTriggerExit with " + o.name);
    }

    void OnCollisionEnter2D(Collision2D o)
    {
        Debug.Log(gameObject.name + " OnCollisionEnter with " + o.collider.name);
    }

    void OnCollisionStay2D(Collision2D o)
    {
        Debug.Log(gameObject.name + " OnCollisionStay with " + o.collider.name);
    }

    void OnCollisionExit2D(Collision2D o)
    {
        Debug.Log(gameObject.name + " OnCollisionExit with " + o.collider.name);
    }

}
