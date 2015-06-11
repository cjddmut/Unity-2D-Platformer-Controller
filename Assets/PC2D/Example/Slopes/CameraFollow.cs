using UnityEngine;
using System.Collections;

namespace  PC2D
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;

        // Update is called once per frame
        void Update()
        {
            Vector3 pos = transform.position;
            pos.x = target.position.x;
            pos.y = target.position.y;

            transform.position = pos;
        }
    }
}
