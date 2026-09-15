using UnityEngine;

namespace Hackathon.Map
{
    public sealed class CodexRoboCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(3.2f, 3.4f, -4.5f);
        public float smoothTime = .16f;
        Vector3 velocity;
        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * .4f);
        }
    }
}
